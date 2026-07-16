namespace FSO.Server.Database.DA.Relationships
{
    public enum DbAvatarTop100Category
    {
        MostFamous,
        BestKarma,
        Friendliest,
        MostInfamous,
        Meanest
    }

    public class DbAvatarTop100
    {
        public uint avatar_id { get; set; }
        public string name { get; set; }
        public long score { get; set; }
    }

    public class DbRelationship
    {
        public uint from_id { get; set; }
        public uint to_id { get; set; }
        public int value { get; set; }
        public uint index { get; set; }
        public uint? comment_id { get; set; }
        public uint date { get; set; }
    }
}
