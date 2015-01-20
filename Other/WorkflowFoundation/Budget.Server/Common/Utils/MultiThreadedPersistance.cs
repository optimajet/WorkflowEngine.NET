using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utils
{
    public class MultiThreadedPersistance <TItem>
    {
        List<TItem> _items = new List<TItem>();

        private volatile object _lock = new object();

        public void AddItem (TItem item)
        {
            lock (_lock)
            {
                _items.Add(item);
            }
            
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }

        public IEnumerable<TItem> Items
        {
            get
            {
                lock (_lock)
                {
                    foreach (var item in _items)
                        yield return item;
                }
            }
        }


    }
}
