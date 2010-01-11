using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetLibrary
{
    public class RouterPort
    {
        public uint networkNumber;
        public uint ID;
        public uint portInfoLen;
        public byte[] portInfo;

        public bool seeOnWhoIsRouter;           // Any Network Number seen on Router Init, but NOT on Who Is, is the directly connected Network Number.
        public bool seenOnRouterInit;           // These flags help us establish that.

        public bool Decode ( byte[] buf, ref int iptr )
        {
            networkNumber = BACnetLibraryCL.ExtractUint16(buf, ref iptr);
            ID = buf[iptr++];
            portInfoLen = buf[iptr++];
            if (portInfoLen != 0)
            {
                // we are not ready to handle this.
                BACnetLibraryCL.Panic("todo");
            }
            iptr += (int) portInfoLen;
            return true;
        }
    }


    public class NPDU
    {
        public bool isNPDUmessage;
        public bool isBroadcast;
        public bool expectingReply;

        public BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE function;


        public void Copy(NPDU src)
        {
            isNPDUmessage = src.isNPDUmessage; // todo, reverse this, it should be at the packet level (like I reversed the APDU)
            isBroadcast = src.isBroadcast;
            expectingReply = src.expectingReply;
        }
    }
}
