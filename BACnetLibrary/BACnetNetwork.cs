using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetLibrary
{
    public class BACnetNetwork : IComparable<BACnetNetwork>, IEquatable<BACnetNetwork>
    {
        public uint NetworkNumber;

        public int CompareTo(BACnetNetwork d)
        {

            // sort order is relevant...
            if (this.NetworkNumber > d.NetworkNumber) return 1;

            if (this.NetworkNumber < d.NetworkNumber) return -1;

            // Networks must be equal
            return 0;
        }

        public bool Equals(BACnetNetwork d)
        {
            if (this.NetworkNumber == d.NetworkNumber ) return true;
            return false;
        }

    }
}
