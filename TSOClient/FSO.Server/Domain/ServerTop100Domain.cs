using FSO.Common.DatabaseService.Model;
using FSO.Common.Domain.Top100;
using FSO.Common.Enum;
using FSO.Server.Database.DA;
using FSO.Server.Database.DA.Relationships;
using FSO.Server.Servers.City;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace FSO.Server.Domain
{
    public class ServerTop100Domain : Top100Domain
    {
        private MemoryCache Cache;
        private IDAFactory DAFactory;
        private CityServerContext Context;

        public ServerTop100Domain(IDAFactory DAFactory, CityServerContext context, MemoryCache cache)
        {
            Cache = cache;
            this.DAFactory = DAFactory;
            this.Context = context;
        }

        public List<Top100Entry> Query(Top100Category category)
        {
            lock (Cache)
            {
                var key = "top100/" + Context.ShardId + "/" + category.ToString();
                var value = Cache.GetCacheItem(key);

                if (value != null)
                {
                    return (List<Top100Entry>)value.Value;
                }

                using (var db = DAFactory.Get())
                {
                    List<Top100Entry> results;

                    if (category.IsLotCategory())
                    {
                        results = db.LotTop100.GetByCategory(Context.ShardId, category.ToLotCategory()).Select(x => new Top100Entry()
                        {
                            Rank = x.rank,
                            //Data service uses lot location as the ID
                            TargetId = (uint?)x.lot_location,
                            TargetName = x.lot_name
                        }).ToList();
                    }
                    else
                    {
                        var avatarCategory = ToAvatarCategory(category);
                        if (avatarCategory.HasValue)
                        {
                            results = db.Relationships.GetTop100(Context.ShardId, avatarCategory.Value, 100)
                                .Select((x, index) => new Top100Entry()
                                {
                                    Rank = (byte)(index + 1),
                                    TargetId = x.avatar_id,
                                    TargetName = x.name
                                }).ToList();
                        }
                        else
                        {
                            results = new List<Top100Entry>();
                        }
                    }

                    Cache.Add(key, results, new CacheItemPolicy()
                    {
                        AbsoluteExpiration = DateTime.Now.Add(TimeSpan.FromMinutes(5))
                    });
                    return results;
                }
            }
        }

        private DbAvatarTop100Category? ToAvatarCategory(Top100Category category)
        {
            switch (category)
            {
                case Top100Category.avatar_most_famous:
                    return DbAvatarTop100Category.MostFamous;
                case Top100Category.avatar_best_karma:
                    return DbAvatarTop100Category.BestKarma;
                case Top100Category.avatar_friendliest:
                    return DbAvatarTop100Category.Friendliest;
                case Top100Category.avatar_most_infamous:
                    return DbAvatarTop100Category.MostInfamous;
                case Top100Category.avatar_meanest:
                    return DbAvatarTop100Category.Meanest;
                default:
                    return null;
            }
        }
    }
}
