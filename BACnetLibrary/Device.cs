using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using BACnetLibrary;


namespace BACnetLibrary
{
    public class Device : IEquatable<Device>
    {
        #region Fields & Constants

        public BACnetEnums.DEVICE_TYPE type;

        public ADR adr;

        public myIPEndPoint directlyConnectedIPEndPointOfDevice;    // this is the BACnet Router on the way to the final device,
                                                                    // or the IP address (MAC address) of a directly connected device

        public uint I_Am_Count = 0;

        private uint vendorId;
        public BACnetObjectIdentifier deviceObjectID = new BACnetObjectIdentifier();            // note! This is NOT the same as the MAC address stored in dadr.


        public BACnetEnums.BACNET_SEGMENTATION SegmentationSupported;

        private int maxAPDULength;

        public BACnetPacket packet;   // a place to store the source IP address and port number

        #endregion

        #region Properties

        // Todo, make this return manufacturer names.
        public uint VendorId
        {
            get { return vendorId; }
            set { vendorId = value; }
        }

        #endregion


        public bool Equals(Device d)
        {
            if (this.adr == null && d.adr != null) return false;
            if (this.adr != null && d.adr == null) return false;
            if (this.adr != null && d.adr != null)
            {
                if (!this.adr.Equals(d.adr)) return false;
            }
            return true;
            //todo, add check that device instances (Device Object IDs) are equal here too... ( as a sanity check)
        }

        public void parse(byte[] bytes)
        {
            byte[] temp = new byte[4];
            temp[0] = bytes[2];
            temp[1] = bytes[1];
            temp[2] = bytes[0];
            temp[3] = 0x00;
            // todo this.deviceId = BitConverter.ToInt32(temp, 0);

            //bytes[17];
            temp = new byte[2];
            temp[0] = bytes[19];
            temp[1] = bytes[18];
            this.maxAPDULength = BitConverter.ToInt16(temp, 0);

            //bytes[19];
            //bytes[20];

            //bytes[21];
            this.vendorId = bytes[22];
        }

    }
}
