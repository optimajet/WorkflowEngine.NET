using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Common.Utils
{
    public sealed class DictionaryCache<TKey, TValue>
    {

        private Timer _timer;

        private ReaderWriterLock _lock = new ReaderWriterLock();

        private Dictionary<TKey, TValue> _cachedDictionary;

        private readonly TimeSpan _refreshInterval;

        private readonly Func<Dictionary<TKey, TValue>> _refreshMethod;

        private static int _timeout = 60000;

        public DictionaryCache(TimeSpan refreshInterval, Func<Dictionary<TKey, TValue>> refreshMethod)
        {
            _timer = new Timer(ResetOnTimer);
            _refreshInterval = refreshInterval;
            _refreshMethod = refreshMethod;
        }

        private void ResetOnTimer(object state)
        {
            _lock.AcquireWriterLock(_timeout);
            try
            {
                _cachedDictionary = null;
            }
            finally
            {

                _lock.ReleaseWriterLock();
            }
        }

        private Dictionary<TKey, TValue> CachedDictionary
        {
            get
            {
                if (_cachedDictionary == null || _cachedDictionary.Count == 0)
                {
                    LockCookie lc = _lock.UpgradeToWriterLock(_timeout);
                    try
                    {
                        _cachedDictionary = _refreshMethod.Invoke();
                        _timer.Change(_refreshInterval, new TimeSpan(-1));

                    }
                    finally
                    {
                        _lock.DowngradeFromWriterLock(ref lc);
                    }
                }

                return _cachedDictionary;

            }
        }

        public TValue GetValue(TKey key)
        {
            TValue template = default(TValue);

            _lock.AcquireReaderLock(_timeout);
            try
            {
                template = CachedDictionary[key];
            }
            catch(KeyNotFoundException)
            {}
            finally
            {
                _lock.ReleaseReaderLock();
            }

            return template;
        }

        public void AddValue (TKey key, TValue value)
        {
            _lock.AcquireWriterLock(_timeout);
            try
            {
                if (!CachedDictionary.ContainsKey(key))
                    CachedDictionary.Add(key,value);
                else
                {
                    CachedDictionary[key] = value;
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void RemoveValue(TKey key)
        {
            _lock.AcquireWriterLock(_timeout);
            try
            {
                if (CachedDictionary.ContainsKey(key))
                    CachedDictionary.Remove(key);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public IEnumerable<TValue> GetValues()
        {
            _lock.AcquireReaderLock(_timeout);
            try
            {
                foreach (var value in CachedDictionary.Values)
                    yield return value;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        public IEnumerable<KeyValuePair<TKey,TValue>> GetDictionary()
        {
            _lock.AcquireReaderLock(_timeout);
            try
            {
                foreach (var kvp in CachedDictionary)
                    yield return kvp;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

    }
}
