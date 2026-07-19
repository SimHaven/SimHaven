using System;
using System.Collections.Generic;

namespace FSO.Server.Database.DA.Relationships
{
    public interface IRelationships
    {
        int UpdateMany(List<DbRelationship> entries);
        List<DbRelationship> GetOutgoing(uint entity_id);
        List<DbRelationship> GetBidirectional(uint entity_id);
        void MarkAvatarActive(uint avatar_id, DateTime activity_day);
        DbRelationshipDecayResult Decay(int shard_id, DateTime activity_day, int grace_active_days, int ltr_active_day_interval);
        int Delete(uint entity_id);
    }
}
