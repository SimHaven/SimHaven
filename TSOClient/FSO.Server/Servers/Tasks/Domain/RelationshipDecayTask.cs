using FSO.Server.Database.DA;
using FSO.Server.Database.DA.LotVisits;
using FSO.Server.Database.DA.Tasks;
using NLog;
using System;

namespace FSO.Server.Servers.Tasks.Domain
{
    public class RelationshipDecayTask : ITask
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();
        private IDAFactory DAFactory;
        private TaskTuning Tuning;

        public RelationshipDecayTask(IDAFactory daFactory, TaskTuning tuning)
        {
            DAFactory = daFactory;
            Tuning = tuning;
        }

        public void Run(TaskContext context)
        {
            if (!context.ShardId.HasValue)
            {
                throw new Exception("Relationship decay must be given a shard_id to process");
            }

            var tuning = Tuning?.RelationshipDecay ?? new RelationshipDecayTaskTuning();
            var activityDay = LotVisitUtils.Midnight().AddDays(-1);

            using (var db = DAFactory.Get())
            {
                var result = db.Relationships.Decay(
                    context.ShardId.Value,
                    activityDay,
                    tuning.grace_active_days,
                    tuning.ltr_active_day_interval
                );

                LOG.Info(
                    "Relationship decay for shard {0} and activity day {1:yyyy-MM-dd}: {2} processed, {3} changed, {4} deleted.",
                    context.ShardId.Value,
                    activityDay,
                    result.processed,
                    result.changed,
                    result.deleted
                );
            }
        }

        public void Abort()
        {
        }

        public DbTaskType GetTaskType()
        {
            return DbTaskType.relationship_decay;
        }
    }

    public class RelationshipDecayTaskTuning
    {
        public int grace_active_days { get; set; } = 7;
        public int ltr_active_day_interval { get; set; } = 7;
    }
}
