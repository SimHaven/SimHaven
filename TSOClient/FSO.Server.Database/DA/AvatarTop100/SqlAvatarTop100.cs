using Dapper;
using System.Collections.Generic;

namespace FSO.Server.Database.DA.AvatarTop100
{
    public class SqlAvatarTop100 : AbstractSqlDA, IAvatarTop100
    {
        public SqlAvatarTop100(ISqlContext context) : base(context)
        {
        }

        public void Calculate(int shard_id)
        {
            Context.Connection.Execute(
                "CALL fso_avatar_top_100_calc_all(@shard_id);",
                new { shard_id = shard_id },
                commandTimeout: 300
            );
        }

        public IEnumerable<DbAvatarTop100> GetByCategory(int shard_id, DbAvatarTop100Category category)
        {
            return Context.Connection.Query<DbAvatarTop100>(@"
                SELECT top.*, avatar.name
                FROM fso_avatar_top_100 top
                INNER JOIN fso_avatars avatar ON avatar.avatar_id = top.avatar_id
                WHERE top.shard_id = @shard_id AND top.category = @category
                ORDER BY top.rank ASC", new
            {
                shard_id = shard_id,
                category = (byte)category
            });
        }
    }
}
