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

        public List<DbAvatarTop100> GetTop100(int shard_id, DbAvatarTop100Category category, int limit)
        {
            string query;
            switch (category)
            {
                case DbAvatarTop100Category.MostFamous:
                    query = Top100CountQuery("r.to_id", "r.from_id", "r.value >= 60");
                    break;
                case DbAvatarTop100Category.BestKarma:
                    query = Top100ScoreQuery("r.to_id", "r.from_id", "r.value > 0");
                    break;
                case DbAvatarTop100Category.Friendliest:
                    query = Top100CountQuery("r.from_id", "r.to_id", "r.value >= 60");
                    break;
                case DbAvatarTop100Category.MostInfamous:
                    query = Top100CountQuery("r.to_id", "r.from_id", "r.value <= -60");
                    break;
                case DbAvatarTop100Category.Meanest:
                    query = Top100CountQuery("r.from_id", "r.to_id", "r.value <= -60");
                    break;
                default:
                    return new List<DbAvatarTop100>();
            }

            return Context.Connection.Query<DbAvatarTop100>(query, new
            {
                shard_id = shard_id,
                limit = limit
            }).ToList();
        }

        private string Top100CountQuery(string avatarColumn, string otherAvatarColumn, string scoreCondition)
        {
            return Top100Query(avatarColumn, otherAvatarColumn, "COUNT(*)", scoreCondition);
        }

        private string Top100ScoreQuery(string avatarColumn, string otherAvatarColumn, string scoreCondition)
        {
            return Top100Query(avatarColumn, otherAvatarColumn, "SUM(r.value)", scoreCondition);
        }

        private string Top100Query(string avatarColumn, string otherAvatarColumn, string scoreExpression, string scoreCondition)
        {
            return @"SELECT avatar.avatar_id, avatar.name, " + scoreExpression + @" AS score
                FROM fso_relationships r
                INNER JOIN fso_avatars avatar
                    ON avatar.avatar_id = " + avatarColumn + @"
                    AND avatar.shard_id = @shard_id
                INNER JOIN fso_avatars other_avatar
                    ON other_avatar.avatar_id = " + otherAvatarColumn + @"
                    AND other_avatar.shard_id = @shard_id
                WHERE r.`index` = 1
                    AND " + scoreCondition + @"
                    AND r.from_id <> r.to_id
                GROUP BY avatar.avatar_id, avatar.name
                ORDER BY score DESC, avatar.avatar_id ASC
                LIMIT @limit";
        }

        public int UpdateMany(List<DbRelationship> entries)
        {
            var date = Epoch.Now;
            var conn = (MySqlConnection)Context.Connection;
            int rows;
            using (MySqlCommand cmd = new MySqlCommand("", conn))
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
                    sCommand.Append(" ON DUPLICATE KEY UPDATE value = VALUES(`value`); ");

                    cmd.CommandTimeout = 300;
                    cmd.CommandText = sCommand.ToString();
                    rows = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return -1;
                }
                return rows;
            }
        }
    }
}
