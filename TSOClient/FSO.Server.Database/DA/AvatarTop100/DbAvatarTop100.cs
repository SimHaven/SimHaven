using System;

namespace FSO.Server.Database.DA.AvatarTop100
{
    public enum DbAvatarTop100Category : byte
    {
        MostFamous = 0,
        BestKarma = 1,
        Friendliest = 2,
        MostInfamous = 3,
        Meanest = 4
    }

    public class DbAvatarTop100
    {
        public DbAvatarTop100Category category { get; set; }
        public byte rank { get; set; }
        public int shard_id { get; set; }
        public uint avatar_id { get; set; }
        public long score { get; set; }
        public DateTime date { get; set; }
        public string name { get; set; }
    }
}
