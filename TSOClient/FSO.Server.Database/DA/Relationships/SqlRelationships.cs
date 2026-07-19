using Dapper;
using FSO.Server.Common;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSO.Server.Database.DA.Relationships
{
    public class SqlRelationships : AbstractSqlDA, IRelationships
    {
        public SqlRelationships(ISqlContext context) : base(context)
        {
        }

        public int Delete(uint entity_id)
        {
            return Context.Connection.ExecuteScalar<int>(
                "DELETE FROM fso_relationships WHERE from_id = @entity_id OR to_id = @entity_id",
                new { entity_id = entity_id }
            );
        }

        public List<DbRelationship> GetBidirectional(uint entity_id)
        {
            return Context.Connection.Query<DbRelationship>(
                "SELECT * FROM fso_relationships WHERE (from_id = @entity_id AND to_id < 16777216) OR (to_id = @entity_id AND from_id < 16777216)",
                new { entity_id = entity_id }
            ).ToList();
        }

        public List<DbRelationship> GetOutgoing(uint entity_id)
        {
            return Context.Connection.Query<DbRelationship>(
                "SELECT * FROM fso_relationships WHERE from_id = @entity_id",
                new { entity_id = entity_id }
            ).ToList();
        }

        public void MarkAvatarActive(uint avatar_id, DateTime activity_day)
        {
            Context.Connection.Execute(
                "INSERT INTO fso_avatar_activity (avatar_id, activity_day) VALUES (@avatar_id, @activity_day) " +
                "ON DUPLICATE KEY UPDATE avatar_id = VALUES(avatar_id)",
                new { avatar_id = avatar_id, activity_day = activity_day.Date }
            );
        }

        public DbRelationshipDecayResult Decay(int shard_id, DateTime activity_day, int grace_active_days, int ltr_active_day_interval)
        {
            if (grace_active_days < 0) grace_active_days = 0;
            if (ltr_active_day_interval < 1) ltr_active_day_interval = 1;

            var result = new DbRelationshipDecayResult();
            var day = activity_day.Date;
            var connection = Context.Connection;

            using (var transaction = connection.BeginTransaction())
            {
                var candidates = connection.Query<DbRelationshipDecayCandidate>(
                    "SELECT state.from_id, state.to_id, state.active_days_without_contact, " +
                    "MAX(CASE WHEN rel.`index` = 0 THEN rel.value ELSE NULL END) AS str, " +
                    "MAX(CASE WHEN rel.`index` = 1 THEN rel.value ELSE NULL END) AS ltr, " +
                    "MAX(CASE WHEN rel.comment_id IS NULL THEN 0 ELSE 1 END) AS has_comment " +
                    "FROM fso_relationship_decay state " +
                    "INNER JOIN fso_avatar_activity activity ON activity.avatar_id = state.from_id AND activity.activity_day = @day " +
                    "INNER JOIN fso_avatars source_avatar ON source_avatar.avatar_id = state.from_id AND source_avatar.shard_id = @shard_id " +
                    "INNER JOIN fso_avatars target_avatar ON target_avatar.avatar_id = state.to_id AND target_avatar.shard_id = @shard_id " +
                    "LEFT JOIN fso_relationships rel ON rel.from_id = state.from_id AND rel.to_id = state.to_id " +
                    "WHERE state.last_contact_day < @day AND (state.last_processed_day IS NULL OR state.last_processed_day < @day) " +
                    "GROUP BY state.from_id, state.to_id, state.active_days_without_contact",
                    new { shard_id = shard_id, day = day }, transaction
                ).ToList();

                foreach (var candidate in candidates)
                {
                    var activeDays = candidate.active_days_without_contact + 1;
                    var str = candidate.str ?? 0;
                    var ltr = candidate.ltr ?? 0;
                    var newStr = str;
                    var newLtr = ltr;

                    if (activeDays > grace_active_days)
                    {
                        newStr = MoveTowardZero(str);

                        if ((activeDays - grace_active_days) % ltr_active_day_interval == 0)
                        {
                            newLtr = MoveTowardZero(ltr);
                        }
                    }

                    var stateUpdated = connection.Execute(
                        "UPDATE fso_relationship_decay SET active_days_without_contact = @active_days, last_processed_day = @day " +
                        "WHERE from_id = @from_id AND to_id = @to_id " +
                        "AND last_contact_day < @day AND (last_processed_day IS NULL OR last_processed_day < @day)",
                        new
                        {
                            active_days = activeDays,
                            day = day,
                            from_id = candidate.from_id,
                            to_id = candidate.to_id
                        }, transaction
                    );

                    // A relationship update may have reset this row after the
                    // candidate query. In that race, contact always wins.
                    if (stateUpdated != 1) continue;

                    if (newStr != str)
                    {
                        connection.Execute(
                            "UPDATE fso_relationships SET value = @value WHERE from_id = @from_id AND to_id = @to_id AND `index` = 0",
                            new { value = newStr, from_id = candidate.from_id, to_id = candidate.to_id }, transaction
                        );
                    }

                    if (newLtr != ltr)
                    {
                        connection.Execute(
                            "UPDATE fso_relationships SET value = @value WHERE from_id = @from_id AND to_id = @to_id AND `index` = 1",
                            new { value = newLtr, from_id = candidate.from_id, to_id = candidate.to_id }, transaction
                        );
                    }

                    result.processed++;
                    if (newStr != str || newLtr != ltr) result.changed++;

                    if (activeDays > grace_active_days && newStr == 0 && newLtr == 0 && candidate.has_comment == 0)
                    {
                        connection.Execute(
                            "DELETE FROM fso_relationships WHERE from_id = @from_id AND to_id = @to_id; " +
                            "DELETE FROM fso_relationship_decay WHERE from_id = @from_id AND to_id = @to_id",
                            new { from_id = candidate.from_id, to_id = candidate.to_id }, transaction
                        );
                        result.deleted++;
                    }
                }

                // Only the current and immediately previous UTC activity days can be
                // consumed by this non-catch-up task. Keep those rows and discard
                // older bookkeeping so the activity table remains bounded.
                connection.Execute(
                    "DELETE FROM fso_avatar_activity WHERE activity_day < @day",
                    new { day = day }, transaction
                );

                transaction.Commit();
            }

            return result;
        }

        private static int MoveTowardZero(int value)
        {
            if (value > 0) return value - 1;
            if (value < 0) return value + 1;
            return 0;
        }

        public int UpdateMany(List<DbRelationship> entries)
        {
            if (entries == null || entries.Count == 0) return 0;

            var date = Epoch.Now;
            var contactDay = DateTime.UtcNow.Date;
            var conn = (MySqlConnection)Context.Connection;
            int rows;
            using (var transaction = conn.BeginTransaction())
            using (MySqlCommand cmd = new MySqlCommand("", conn, transaction))
            {
                try
                {
                    StringBuilder sCommand = new StringBuilder("INSERT INTO fso_relationships (from_id, to_id, value, `index`, `date`) VALUES ");

                    bool first = true;
                    foreach (var item in entries)
                    {
                        if (!first) sCommand.Append(",");
                        first = false;
                        sCommand.Append("(");
                        sCommand.Append(item.from_id);
                        sCommand.Append(",");
                        sCommand.Append(item.to_id);
                        sCommand.Append(",");
                        sCommand.Append(item.value);
                        sCommand.Append(",");
                        sCommand.Append(item.index);
                        sCommand.Append(",");
                        sCommand.Append(date);
                        sCommand.Append(")");
                    }
                    sCommand.Append(" ON DUPLICATE KEY UPDATE value = VALUES(`value`), `date` = VALUES(`date`); ");

                    cmd.CommandTimeout = 300;
                    cmd.CommandText = sCommand.ToString();
                    rows = cmd.ExecuteNonQuery();

                    var avatarPairs = entries
                        .Where(x => x.from_id != x.to_id && x.from_id < 16777216 && x.to_id < 16777216)
                        .GroupBy(x => new { x.from_id, x.to_id })
                        .Select(x => x.Key);

                    foreach (var pair in avatarPairs)
                    {
                        conn.Execute(
                            "INSERT INTO fso_relationship_decay (from_id, to_id, last_contact_day, active_days_without_contact, last_processed_day) " +
                            "SELECT @from_id, @to_id, @contact_day, 0, NULL FROM DUAL " +
                            "WHERE EXISTS (SELECT 1 FROM fso_avatars WHERE avatar_id = @from_id) " +
                            "AND EXISTS (SELECT 1 FROM fso_avatars WHERE avatar_id = @to_id) " +
                            "ON DUPLICATE KEY UPDATE last_contact_day = VALUES(last_contact_day), active_days_without_contact = 0; " +
                            "UPDATE fso_relationship_decay SET last_contact_day = @contact_day, active_days_without_contact = 0 " +
                            "WHERE from_id = @to_id AND to_id = @from_id",
                            new { from_id = pair.from_id, to_id = pair.to_id, contact_day = contactDay }, transaction
                        );
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return -1;
                }
                return rows;
            }
        }

        private class DbRelationshipDecayCandidate
        {
            public uint from_id { get; set; }
            public uint to_id { get; set; }
            public int active_days_without_contact { get; set; }
            public int? str { get; set; }
            public int? ltr { get; set; }
            public int has_comment { get; set; }
        }
    }
}
