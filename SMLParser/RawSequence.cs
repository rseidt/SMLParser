using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class RawSequence : RawRecord
    {
        private List<RawRecord> items = null;
        public uint? StartIndexInStream;
        internal RawSequence()
        {

        }

        public override int Length { get { return Items.Count; } }
        internal RawSequence(List<RawRecord> Items)
        {
            items = Items;
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
