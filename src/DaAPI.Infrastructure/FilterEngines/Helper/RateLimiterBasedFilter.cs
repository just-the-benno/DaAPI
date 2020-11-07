using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DaAPI.Infrastructure.FilterEngines.Helper
{
    public class RateLimitBasedFilter<T>
    {
        private class RateLimitEntry
        {
            public UInt32 Seconds { get; set; }
            public UInt16 BucketsCount { get; set; }

            public RateLimitEntry(UInt32 seconds)
            {
                this.Reset(seconds);
            }

            public void Reset(UInt32 seconds)
            {
                Seconds = seconds;
                BucketsCount = 1;
            }
        }

        #region Fields

        private static readonly ConcurrentDictionary<T, RateLimitEntry> _entries;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        static RateLimitBasedFilter()
        {
            _entries = new ConcurrentDictionary<T, RateLimitEntry>();

            Timer timer = new Timer((state) => CleanUp(), null, TimeSpan.FromSeconds(60), TimeSpan.FromMinutes(1));
        }

        public RateLimitBasedFilter()
        {
        }

        #endregion

        #region Methods

        private static void CleanUp()
        {
            IEnumerable<KeyValuePair<T, RateLimitEntry>> copy = _entries.ToList();
            UInt32 seconds = GetCurrentSeconds();
            foreach (var item in copy)
            {
                if (item.Value.Seconds < seconds)
                {
                    _entries.TryRemove(item.Key, out RateLimitEntry _);
                }
            }
        }

        private static UInt32 GetCurrentSeconds()
        {
            DateTime now = DateTime.Now;
            UInt32 result = (UInt32)(((now.Hour * 60) + now.Minute) * 60 + now.Second);
            return result;
        }

        public bool FilterByRateLimit(T item, Int32 maxBuckets)
        {
            UInt32 seconds = GetCurrentSeconds();

            try
            {
                if (_entries.ContainsKey(item) == false)
                {
                    _entries.GetOrAdd(item, new RateLimitEntry(seconds));
                    return false;
                }
                else
                {
                    if (_entries.TryGetValue(item, out RateLimitEntry entry) == true)
                    {
                        if (entry.Seconds != seconds)
                        {
                            entry.Reset(seconds);
                            return false;
                        }
                        else
                        {
                            if (entry.BucketsCount > maxBuckets)
                            {
                                return true;
                            }
                            else
                            {
                                entry.BucketsCount += 1;

                                return false;
                            }
                        }
                    }
                    else
                    {
                        _entries.TryAdd(item, new RateLimitEntry(seconds));
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                /// becase of async nature of sockets it is possible that something like KeyExists happend
                /// also the cleanup Method can causing this issue
                /// if so, return false
                /// nevertheless a client will retransmit a package

                return false;
            }
        }

        #endregion
    }
}
