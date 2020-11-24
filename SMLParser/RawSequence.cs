using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class RawSequence : RawRecord
    {
        private List<RawRecord> items = null;
        internal RawSequence()
        {

        }
        internal RawSequence(List<RawRecord> Items)
        {
            items = Items;
            Length = Items.Count;
            Type = RawType.List;
        }
        public List<RawRecord> Items { 
            get {
                if (items == null)
                    items = new List<RawRecord>();
                return items;
            }
        }
    }
}
