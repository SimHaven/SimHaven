using System.Collections.Generic;

namespace FSO.Server.Database.DA.AvatarTop100
{
    public interface IAvatarTop100
    {
        void Calculate(int shard_id);
        IEnumerable<DbAvatarTop100> GetByCategory(int shard_id, DbAvatarTop100Category category);
    }
}
