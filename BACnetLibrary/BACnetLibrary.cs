using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace BACnetLibrary
{

    public class myIPEndPoint : IPEndPoint
    {

        public myIPEndPoint(EndPoint ep)
            : base( ((IPEndPoint)ep).Address, ((IPEndPoint)ep).Port)
        {
        }

        public myIPEndPoint(IPAddress address, int port)
            : base(address, port)
        {
        }

        public myIPEndPoint()
            : base(IPAddress.Any, 0)
        {
        }

        // public IPEndPoint ipep = new IPEndPoint(0,0);

        public void Decode(byte[] buffer, int offset)
        {
            int port = 0;

            byte[] bytearr = new byte[4];

            Buffer.BlockCopy(buffer, offset, bytearr, 0, 4);

            port = buffer[offset + 4];
            port = (port << 8) | buffer[offset + 5];

            base.Address = new IPAddress(bytearr);
            base.Port = port;

        }

        public void Encode(byte[] buffer, ref int optr)
        {
            byte[] addrb = base.Address.GetAddressBytes();

            for (int i = 0; i < 4; i++)
            {
                buffer[optr++] = addrb[i];
            }

            buffer[optr++] = (byte)((Port >> 8) & 0xff);
            buffer[optr++] = (byte)(Port & 0xff);
        }
    }

    public class BACnetSegmentation
    {

        public void Encode(byte[] buffer, ref int pos)
        {
            BACnetEncoding.EncodeApplicationTag(buffer, ref pos,
                BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED,
                (int)BACnetEnums.BACNET_SEGMENTATION.SEGMENTATION_NONE);
        }

        public void Decode(byte[] buffer, ref int pos)
        {
            pos += 2;
        }
    }


    public class Unsigned
    {
        uint value;

        public Unsigned(uint value)
        {
            this.value = value;
        }

        public uint Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public void Encode(byte[] buffer, ref int pos)
        {
            if (value <= 0xff)
            {
                buffer[pos++] = 0x21;
                buffer[pos++] = (byte)(value & 0xFF);
                return;
            }

            if (value > 0xff)
            {
                buffer[pos++] = 0x22;
                buffer[pos++] = (byte)(value / 0xFF & 0xFF);
                buffer[pos++] = (byte)(value & 0xFF);
                return;
            }
        }


        public void Decode(byte[] buffer, ref UInt16 pos)
        {
            byte tag = buffer[pos++];
            if (tag == 0x21)
            {
                value = buffer[pos++];
            }
            if (tag == 0x22)
            {
                value = (uint)buffer[pos++] + (uint)buffer[pos++] * 0x100;
            }

        }
    }


    public class MACaddress : IEquatable<MACaddress>
    {
        public uint length;
        public uint uintMACaddress;
        public myIPEndPoint ipMACaddress;

        public MACaddress()
        {
        }

        public MACaddress(uint madr)
        {
            length = 1;
            uintMACaddress = madr;
        }

        public MACaddress(myIPEndPoint madr)
        {
            length = 6;
            ipMACaddress = madr;
        }

        public bool Equals(MACaddress madr)
        {
            if (length != madr.length) return false;

            switch (length)
            {
                case 0:
                    break;
                case 1:
                    if (madr.uintMACaddress != uintMACaddress) return false;
                    break;
                case 6:
                    if (ipMACaddress.Equals(madr.ipMACaddress) != true) return false;
                    break;
                default:
                    BACnetLibraryCL.Panic("Illegal MAC address length??");
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            switch (length)
            {
                case 0:
                    return "Broadcast";
                case 1:
                    return uintMACaddress.ToString();
                case 6:
                    return ipMACaddress.ToString();
                default:
                    BACnetLibraryCL.Panic("Illegal MAC length");
                    return "Illegal MAC address";
            }
        }
    }


    public class ADR : IEquatable<ADR>
    {
        // todo - does a network number of 0 have any special meaning? how do we indicate a directly connected device?

        public uint networkNumber;
        public MACaddress MACaddress;
        public bool directlyConnected = false;

        public ADR()
        {
            MACaddress = new MACaddress();
        }

        public ADR(uint networkNumber, uint MACaddr)
        {
            MACaddress = new MACaddress(MACaddr);
            this.networkNumber = networkNumber;
        }

        public ADR(uint networkNumber, myIPEndPoint ipep)
        {
            this.networkNumber = networkNumber;

            if (networkNumber == 0)
            {
                // we can argue whether a network number of 0 indicates directly connected or not later... for now we will
                // assume no network number == directly connected
                directlyConnected = true;
            }

            this.MACaddress = new MACaddress(ipep);
        }

        public bool Equals(ADR adr)
        {
            // no, mac address equality does not depend on directly or indirectly connected status
            // if (this.directlyConnected != adr.directlyConnected) return false;

            if (this.networkNumber != adr.networkNumber) return false;
            if (this.MACaddress.Equals(adr.MACaddress) != true) return false;
            return true;
        }

        public void Decode(byte[] buffer, ref int pos)
        {
            networkNumber = (uint)buffer[pos++] << 8;
            networkNumber |= buffer[pos++];

            MACaddress.length = buffer[pos++];

            switch (MACaddress.length)
            {
                case 0:
                    // indicates a broadcast, perfectly legal.
                    break;
                case 1:
                    MACaddress.uintMACaddress = buffer[pos++];
                    break;
                case 6:
                    // extract the IP address
                    myIPEndPoint ipep = new myIPEndPoint();
                    ipep.Decode(buffer, pos);
                    MACaddress.ipMACaddress = ipep;
                    pos += 6;
                    break;
                default:
                    BACnetLibraryCL.Panic("Illegal MAC address length??");
                    break;
            }
        }

        public void Encode(byte[] buffer, ref int pos)
        {

            buffer[pos++] = (byte)(this.networkNumber >> 8);
            buffer[pos++] = (byte)(this.networkNumber & 0xff);

            buffer[pos++] = (byte)MACaddress.length;

            switch (MACaddress.length)
            {
                case 1:
                    buffer[pos++] = (byte)MACaddress.uintMACaddress;
                    break;
                case 6:
                    MACaddress.ipMACaddress.Encode(buffer, ref pos);
                    break;
                default:
                    BACnetLibraryCL.Panic("Illegal MAC address length??");
                    break;
            }
        }


        public override string ToString()
        {
            switch (MACaddress.length)
            {
                case 0:
                    return "Broadcast";
                case 1:
                    return networkNumber.ToString() + " / " + MACaddress.uintMACaddress.ToString();
                case 6:
                    // extract the IP address
                    return networkNumber.ToString() + " / " + MACaddress.ipMACaddress.ToString();
                default:
                    // todo
                    // ("Implement MAC addresses of other lengths");
                    return "Unimplemented";
            }
        }
    }


    public class BACnetObjectIdentifier : IEquatable<BACnetObjectIdentifier>
    {
        public BACnetEnums.BACNET_OBJECT_TYPE objectType;
        public uint objectInstance;

        public BACnetObjectIdentifier()
        {
        }

        public bool Equals(BACnetObjectIdentifier oid)
        {
            if (oid.objectType != this.objectType) return false;
            if (oid.objectInstance != this.objectInstance) return false;
            return true;
        }

        public BACnetObjectIdentifier(byte[] buf, ref int offset)
        {
            DecodeApplicationTag(buf, ref offset);
        }




        // Create a new Application Tag

        public BACnetObjectIdentifier(byte[] buf, ref int offset, BACnetEnums.TAG tagType, BACnetEnums.BACNET_APPLICATION_TAG appTag)
        {
            // is the next parameter even an application tag 
            if ((buf[offset] & 0x08) != 0x00)
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Not a context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }

            if ((BACnetEnums.BACNET_APPLICATION_TAG)(((buf[offset] & 0xf0) >> 4)) != appTag)
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Unexpected application tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }

            int contextTagSize = buf[offset] & 0x07;

            offset++;

            switch (appTag)
            {
                case BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID:
                    if (contextTagSize != 4)
                    {
                        // we dont have a legal object ID!
                        BACnetLibraryCL.Panic("Illegal length");
                        return;
                    }

                    this.objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buf[offset] << 2) | ((uint)buf[offset + 1] >> 6));

                    objectInstance = ((uint)buf[offset + 1] & 0x3f) << 16;
                    objectInstance |= ((uint)buf[offset + 2]) << 8;
                    objectInstance |= ((uint)buf[offset + 3]);

                    offset += 4;
                    return;
            }
        }





        // Create a new Context Tag

        public BACnetObjectIdentifier(byte[] buf, ref int offset, BACnetEnums.TAG tagType, int tagValue)
        {
            // is the next parameter even a context tag 
            if ((buf[offset] & 0x08) != 0x08)
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Not a context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }

            if ((buf[offset] & 0xf0) != (tagValue << 4))
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Unexpected context tag");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }

            int contextTagSize = buf[offset] & 0x07;

            // the length of a bacnet object identifier better be 4

            if (contextTagSize != 4)
            {
                // we have an unexpected context tag, sort this out
                BACnetLibraryCL.Panic("Unbelievable length of object identifier");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
                return;
            }


            objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buf[offset + 1] << 2) | ((uint)buf[offset + 2] >> 6));

            objectInstance = ((uint)buf[offset + 2] & 0x3f) << 16;
            objectInstance |= ((uint)buf[offset + 3]) << 8;
            objectInstance |= ((uint)buf[offset + 4]);

            offset += 5;
        }


        public void SetType(BACnetEnums.BACNET_OBJECT_TYPE objectType)
        {
            this.objectType = objectType;
        }


        public void SetInstance(uint objectInstance)
        {
            // todo, check for duplicates. (?)
            if (objectInstance >= (1 << 22)) // may have to be power of 22... (todo)
            {
                throw new Exception("Object Instance out of range " + objectInstance.ToString());
            }
            this.objectInstance = (objectInstance & 0x3fffff);
        }


        public void EncodeApplicationTag(byte[] buffer, ref int pos)
        {
            UInt32 objid = ((UInt32)objectType << 22) | (objectInstance & 0x3ffffff);
            BACnetLibraryCL.InsertApplicationTag(buffer, ref pos, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID, objid);
        }


        public void EncodeContextTag(byte[] buffer, ref int pos, int contextTagNumber)
        {
            UInt32 objid;

            objid = ((UInt32)objectType << 22) | objectInstance;

            buffer[pos++] = (byte)(0x0c | (contextTagNumber << 4));

            buffer[pos++] = (byte)((objid >> 24) & 0xff);
            buffer[pos++] = (byte)((objid >> 16) & 0xff);
            buffer[pos++] = (byte)((objid >> 8) & 0xff);
            buffer[pos++] = (byte)(objid & 0xff);
        }


        public void DecodeContextTag(byte[] buffer, ref int pos)
        {
            if ((buffer[pos++] & 0x0f) != (0x08 | 0x04))
            {
                throw new Exception("m0045 - Illegal context tag for Object Identifier");
            }
            this.objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buffer[pos] << 2) | ((uint)buffer[pos + 1] >> 6));

            objectInstance = ((uint)buffer[pos + 1] & 0x3f) << 16;
            objectInstance |= ((uint)buffer[pos + 2]) << 8;
            objectInstance |= ((uint)buffer[pos + 3]);

            pos += 4;
        }

        public void DecodeApplicationTag(byte[] buffer, ref int pos)
        {
            // get the tag class, length

            uint cl = buffer[pos++];

            if (cl != 0xc4)
            {
                throw new Exception("m0041 - Missing Application Tag for Object Identifier");
            }

            this.objectType = (BACnetEnums.BACNET_OBJECT_TYPE)(((uint)buffer[pos] << 2) | ((uint)buffer[pos + 1] >> 6));

            objectInstance = ((uint)buffer[pos + 1] & 0x3f) << 16;
            objectInstance |= ((uint)buffer[pos + 2]) << 8;
            objectInstance |= ((uint)buffer[pos + 3]);

            pos += 4;
        }
    }


    public class BACnetLibraryCL
    {
        public static byte[] StrToByteArray(string s)
        {
            List<byte> value = new List<byte>();
            // foreach (char c in s.ToCharArray()) value.Add(c.ToByte());
            foreach (char c in s.ToCharArray())
            {
                value.Add(Convert.ToByte(c));
            }
            return value.ToArray();
        }

        public static void Panic(String message)
        {
            throw new Exception("m0039 - Old style Panic");
        }

        static public int ExtractInt16(byte[] buffer, ref int iptr)
        {
            int tint = 0;
            tint = (int)buffer[iptr++] << 8;
            tint |= (int)buffer[iptr++];
            return tint;
        }

        static public uint ExtractUint16(byte[] buffer, ref int iptr)
        {
            uint tint = 0;
            tint = (uint)buffer[iptr++] << 8;
            tint |= (uint)buffer[iptr++];
            return tint;
        }

        static public uint ExtractUint32(byte[] buffer, ref int iptr)
        {
            uint tint = 0;
            tint = (uint)buffer[iptr++] << 24;
            tint |= (uint)buffer[iptr++] << 16;
            tint |= (uint)buffer[iptr++] << 8;
            tint |= (uint)buffer[iptr++];
            return tint;
        }

        static public void InsertInt16(byte[] buffer, ref int optr, int val)
        {
            buffer[optr++] = (byte)((val >> 8) & 0xff);
            buffer[optr++] = (byte)(val & 0xff);
        }

        static public void InsertUint16(byte[] buffer, ref int optr, uint val)
        {
            buffer[optr++] = (byte)((val >> 8) & 0xff);
            buffer[optr++] = (byte)(val & 0xff);
        }

        static public void InsertUint32(byte[] buffer, ref int optr, uint val)
        {
            buffer[optr++] = (byte)((val >> 24) & 0xff);
            buffer[optr++] = (byte)((val >> 16) & 0xff);
            buffer[optr++] = (byte)((val >> 8) & 0xff);
            buffer[optr++] = (byte)(val & 0xff);
        }


        static public void InsertApplicationTagUint16(byte[] buffer, ref int optr, UInt16 value)
        {
            // We need a slightly special case of UInt16 Application Tag because some BACnet Packets call out for UInt16 specifically. e.g. I-Am.
            // http://www.bacnetwiki.com/wiki/index.php?title=Application_Tags

            buffer[optr++] = (byte)(((int)BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT << 4) | 2);

            buffer[optr++] = (byte)((value >> 8) & 0xff);
            buffer[optr++] = (byte)(value & 0xff);
        }

        static public void InsertApplicationTagString(byte[] buffer, ref int optr, string strval)
        {
            buffer[optr++] = 0x75;
            buffer[optr++] = (byte)(strval.Length + 1);

            // character set - ascii
            buffer[optr++] = 0;

            // todo, handling ascii encoding only for now..
            byte[] tarray = StrToByteArray(strval);

            tarray.CopyTo(buffer, optr);

            optr += strval.Length;
        }


        static public void InsertContextTag(byte[] buffer, ref int optr, int contextNumber, int value)
        {
            if (value > 255) throw new Exception("m0056 - Not ready for context tag > 255");

            int len = 1;

            buffer[optr++] = (byte)((contextNumber << 4) | 0x08 | len);
            buffer[optr++] = (byte)(value & 0xff);
        }


        static public void InsertContextClosingTag(byte[] buffer, ref int optr, int contextNumber)
        {
            buffer[optr++] = (byte)((contextNumber << 4) | 0x0f);
        }


        static public void InsertContextOpeningTag(byte[] buffer, ref int optr, int contextNumber)
        {
            buffer[optr++] = (byte)((contextNumber << 4) | 0x0e);
        }


        static public void InsertApplicationTag(byte[] buffer, ref int optr, BACnetEnums.BACNET_APPLICATION_TAG apptag, UInt32 value)
        {
            int saveOptr = optr++;
            int length = 1;

            if (value >= (1 << 24))
            {
                buffer[optr++] = (byte)((value >> 24) & 0xff);
                length++;
            }
            if (value >= (1 << 16))
            {
                buffer[optr++] = (byte)((value >> 16) & 0xff);
                length++;
            }
            if (value >= (1 << 8))
            {
                buffer[optr++] = (byte)((value >> 8) & 0xff);
                length++;
            }
            buffer[optr++] = (byte)(value & 0xff);
            buffer[saveOptr] = (byte)(((int)apptag << 4) | length);
        }


        public static void ReadPropertyObjectList(BACnetManager bnm, Device device)
        {
            byte[] data = new byte[1024];
            int optr = 0;

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            data[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;  // 81
            data[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;  //0a

            int store_length_here = optr;
            optr += 2;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            data[optr++] = 0x01;            //  Version
            data[optr++] = 0x24;            //  NCPI - Dest present, expecting reply

            if (device.adr != null)
            {
                device.adr.Encode(data, ref optr);
            }
            else
            {
                // this means we have an ethernet/IP address for a MAC address. At present
                // we then dont know the network number
                // todo - resolve the network number issue
                ADR tempAdr = new ADR(0, device.directlyConnectedIPEndPointOfDevice);
                tempAdr.Encode(data, ref optr);
            }

            data[optr++] = 0xff;            // Hopcount


            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU


            // Unconfirmed Request
            // Structure described here http://www.bacnetwiki.com/wiki/index.php?title=BACnet-Confirmed-Request-PDU

            data[optr++] = 0x02;            //  PDU Type=0 and SA=1
            data[optr++] = 0x04;            //  Max Resp (Encoded)

            data[optr++] = 0x01;            //  Invoke ID

            data[optr++] = 0x0c;            //  Service Choice 12 = ReadProperty

            // Service Request 

            // Object type, instance (Device Object) (Encode as context tag 0)
            device.deviceObjectID.EncodeContextTag(data, ref optr, 0);

            // Property Identifier (Object List)
            InsertContextTag(data, ref optr, 1, (int)BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_LIST);

            // todo, why is this not context encoded?

            BACnetLibraryCL.InsertInt16(data, ref store_length_here, optr);

            // todo bnm.insidesocket.MySend(data, optr, device.packet.fromBIP);
        }



        public static void InsertPDU(byte[] outbuf, ref int optr, BACnetEnums.BACNET_PDU_TYPE pduType, byte invokeID, BACnetEnums.BACNET_CONFIRMED_SERVICE svcChoice)
        {
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU

            outbuf[optr++] = (byte)((pduType) | 0);   // no flags yet

            outbuf[optr++] = invokeID;

            // todo - segment #s only if segmented messages allowed

            outbuf[optr++] = (byte)svcChoice;

            // The rest of the packet is variable encoded

        }


        public static void InsertBitString(byte[] outbuf, int optr, int maxbytes, int bit)
        {
            if (bit / 8 + 1 > maxbytes) throw new Exception("m0057 - Bitstring range exceeded");

            outbuf[optr + bit / 8] |= (byte)(0x80 >> (bit % 8));
        }


        public static void RespondToAPDU(AppManager apm, BACnetManager bnm, BACnetPacket incomingPacket)
        {
            byte[] outbuf = new byte[2000];
            int optr;
            PacketLog pktlog;

            BACnetPacket outgoingBACpacket = new BACnetPacket(apm, bnm);

            // fill in some CRP parameters for packet log display
            outgoingBACpacket.confirmedServiceChoice = incomingPacket.confirmedServiceChoice;
            outgoingBACpacket.apduConfirmedServiceTypeFlag = incomingPacket.apduConfirmedServiceTypeFlag;
            outgoingBACpacket.propertyID = incomingPacket.propertyID;
            outgoingBACpacket.apduUnconfirmedServiceFlag = incomingPacket.apduUnconfirmedServiceFlag;
            outgoingBACpacket.pduType = incomingPacket.pduType;
            outgoingBACpacket.unconfirmedServiceChoice = incomingPacket.unconfirmedServiceChoice;
            outgoingBACpacket.directlyConnectedIPEndPointOfDevice = incomingPacket.directlyConnectedIPEndPointOfDevice;

            BACnetPacket incomingBACpacket = (BACnetPacket)incomingPacket;

            if (incomingBACpacket.apduConfirmedServiceTypeFlag == true)
            {
                switch (incomingBACpacket.confirmedServiceChoice)
                {
                    case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:

                        switch (incomingBACpacket.propertyID)
                        {
                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_NAME:
                                // supply the first (and only) (device) object
                                optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);
                                // Device Object ID
                                InsertContextOpeningTag(outbuf, ref optr, 3);
                                InsertApplicationTagString(outbuf, ref optr, apm.ourDeviceName);
                                InsertContextClosingTag(outbuf, ref optr, 3);
                                // Send it off
                                SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_OBJECT_TYPES_SUPPORTED:
                                optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);
                                BACnetLibraryCL.InsertContextOpeningTag(outbuf, ref optr, 3);

                                // hardcode the bitstring until we see where else it can be used, then generalized.

                                outbuf[optr++] = 0x83;  // Assume length = 3, including unused bits byte
                                // outbuf[optr++] = 0x03;
                                outbuf[optr++] = 0x06;  // trailing bits not used

                                InsertBitString(outbuf, optr, 2, (int)BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE);
                                optr += 2;

                                BACnetLibraryCL.InsertContextClosingTag(outbuf, ref optr, 3);

                                SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_SERVICES_SUPPORTED:

                                optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);

                                BACnetLibraryCL.InsertContextOpeningTag(outbuf, ref optr, 3);


                                //List<BACnetEnums.BACNET_SERVICES_SUPPORTED> servicesList = new List<BACnetEnums.BACNET_SERVICES_SUPPORTED>();

                                //servicesList.Add(BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_READ_PROPERTY);
                                //// servicesList.Add(BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_WRITE_PROPERTY);
                                //servicesList.Add(BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_DEVICE_COMMUNICATION_CONTROL);
                                ////servicesList.Add(BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_REINITIALIZE_DEVICE);
                                //servicesList.Add(BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_TIME_SYNCHRONIZATION);
                                //servicesList.Add(BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_WHO_IS);
                                //// todo, what about i-am ?
                                //BACnetLibraryCL.InsertApplicationTagString(outbuf, ref optr, servicesList );

                                // hardcode the bitstring until we see where else it can be used, then generalized.

                                outbuf[optr++] = 0x85;  // Assume length = 6
                                outbuf[optr++] = 0x06;  // Length, including the next byte
                                outbuf[optr++] = 0x05;  // 5 trailing bits not used

                                InsertBitString(outbuf, optr, 5, (int)BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_READ_PROPERTY);
                                InsertBitString(outbuf, optr, 5, (int)BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_DEVICE_COMMUNICATION_CONTROL);
                                InsertBitString(outbuf, optr, 5, (int)BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_TIME_SYNCHRONIZATION);
                                InsertBitString(outbuf, optr, 5, (int)BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_WHO_IS);

                                InsertBitString(outbuf, optr, 5, (int)BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_WRITE_PROPERTY);
                                InsertBitString(outbuf, optr, 5, (int)BACnetEnums.BACNET_SERVICES_SUPPORTED.SERVICE_SUPPORTED_REINITIALIZE_DEVICE);

                                optr += 5;

                                BACnetLibraryCL.InsertContextClosingTag(outbuf, ref optr, 3);

                                SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_LIST:
                                // Only respond if array index specified
                                if (!incomingPacket.arrayIndexDecoded) throw new Exception("m0058 - Not expecting open object list");

                                if (incomingPacket.arrayIndex == 0)
                                {
                                    optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);

                                    // arrayIndex 0
                                    InsertContextTag(outbuf, ref optr, 2, 0);

                                    // Object count
                                    InsertContextOpeningTag(outbuf, ref optr, 3);
                                    InsertApplicationTag(outbuf, ref optr, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT, 1);
                                    InsertContextClosingTag(outbuf, ref optr, 3);

                                    // Send it off
                                    SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                    break;

                                    // supply the number of objects
                                }
                                else if (incomingPacket.arrayIndex == 1)
                                {
                                    // supply the first (and only) (device) object
                                    optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);

                                    // arrayIndex 1
                                    InsertContextTag(outbuf, ref optr, 2, 1);

                                    // Device Object ID
                                    InsertContextOpeningTag(outbuf, ref optr, 3);
                                    incomingPacket.objectID.EncodeApplicationTag(outbuf, ref optr);
                                    InsertContextClosingTag(outbuf, ref optr, 3);

                                    // Send it off
                                    SendOffPacket(apm, outgoingBACpacket, outbuf, optr);

                                }
                                else
                                {
                                    throw new Exception("m0059 - Illegal object list index");
                                }
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_TYPE:
                                // supply the first (and only) (device) object
                                optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);
                                // Device Object ID
                                InsertContextOpeningTag(outbuf, ref optr, 3);
                                InsertApplicationTag(outbuf, ref optr, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED, (uint)BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE);
                                InsertContextClosingTag(outbuf, ref optr, 3);
                                // Send it off
                                SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_SYSTEM_STATUS:
                                // supply the first (and only) (device) object
                                optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);
                                // Device Object ID
                                InsertContextOpeningTag(outbuf, ref optr, 3);
                                InsertApplicationTag(outbuf, ref optr, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED, (uint)BACnetEnums.BACNET_DEVICE_STATUS.STATUS_OPERATIONAL);
                                InsertContextClosingTag(outbuf, ref optr, 3);
                                // Send it off
                                SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_IDENTIFIER:
                                // supply the first (and only) (device) object
                                optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);
                                // Device Object ID
                                InsertContextOpeningTag(outbuf, ref optr, 3);
                                incomingPacket.objectID.EncodeApplicationTag(outbuf, ref optr);
                                InsertContextClosingTag(outbuf, ref optr, 3);
                                // Send it off
                                SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_VENDOR_NAME:
                                // supply the first (and only) (device) object
                                optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);
                                // Device Object ID
                                InsertContextOpeningTag(outbuf, ref optr, 3);
                                InsertApplicationTagString(outbuf, ref optr, apm.ourVendorName);
                                InsertContextClosingTag(outbuf, ref optr, 3);
                                // Send it off
                                SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_MODEL_NAME:
                                // supply the first (and only) (device) object
                                optr = InsertReadPropertyResponse(apm, incomingBACpacket, outgoingBACpacket, outbuf, incomingBACpacket.propertyID);
                                // Device Object ID
                                InsertContextOpeningTag(outbuf, ref optr, 3);
                                InsertApplicationTagString(outbuf, ref optr, apm.ourModelName);
                                InsertContextClosingTag(outbuf, ref optr, 3);
                                // Send it off
                                SendOffPacket(apm, outgoingBACpacket, outbuf, optr);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_FIRMWARE_REVISION:
                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_APPLICATION_SOFTWARE_VERSION:
                                RespondReadPropertyWithString(apm, incomingBACpacket, outgoingBACpacket, outbuf, apm.ourFirmwareRevision);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_VENDOR_IDENTIFIER:
                                RespondReadPropertyWithUint(apm, incomingBACpacket, outgoingBACpacket, outbuf, apm.ourVendorID);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_VERSION:
                                RespondReadPropertyWithUint(apm, incomingBACpacket, outgoingBACpacket, outbuf, 1);
                                break;

                            case BACnetEnums.BACNET_PROPERTY_ID.PROP_PROTOCOL_REVISION:
                                RespondReadPropertyWithUint(apm, incomingBACpacket, outgoingBACpacket, outbuf, 4);
                                break;

                            default:
                                apm.MessageTodo("m0046 - Implement read property " + incomingBACpacket.propertyID.ToString());
                                break;
                        }
                        break;
                    default:
                        apm.MessageTodo("m0044 - Implement confirmed service " + incomingBACpacket.confirmedServiceChoice.ToString());
                        break;
                }
            }

            if (incomingBACpacket.apduUnconfirmedServiceFlag == true)
            {
                switch (incomingBACpacket.pduType)
                {
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
                        switch (incomingBACpacket.unconfirmedServiceChoice)
                        {
                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS:

                                // we need to respond

                                // ONLY if we are in the range, and if not, ignore.

                                if (incomingBACpacket.lowRange != 0 || incomingBACpacket.highRange != 0)
                                {
                                    if (apm.ourDeviceId < incomingBACpacket.lowRange || apm.ourDeviceId > incomingBACpacket.highRange)
                                    {
                                        // This packet not for us
                                        apm.MessageLog("m0034 - Ranged Who-Is not addressed to us, ignoring");
                                        break;
                                    }
                                }

                                // Compose the response (I-am)

                                outgoingBACpacket.BACnetPort = incomingPacket.BACnetPort;

                                outgoingBACpacket.npdu.isBroadcast = true;
                                outgoingBACpacket.hopcount = 256;

                                // destination address of the BACnet packet. (Must either be b'cast or directlyConnectedIPEP

                                optr = 0;

                                outgoingBACpacket.npdu.isNPDUmessage = false; // makes it an APDU
                                outgoingBACpacket.pduType = BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST;
                                outgoingBACpacket.unconfirmedServiceChoice = BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM;

                                outgoingBACpacket.EncodeBACnetNew(outbuf, ref optr);

                                // send the message

                                // if there is no available adapter, this try will throw
                                try
                                {
                                    apm.bnm.bacnet_listen_socket.SendTo(outbuf, optr, SocketFlags.None, incomingPacket.directlyConnectedIPEndPointOfDevice);

                                    pktlog = new PacketLog(true, incomingPacket.directlyConnectedIPEndPointOfDevice, outgoingBACpacket);
                                    pktlog.BACnetPacket = (BACnetPacket)outgoingBACpacket;
                                    apm.bnm.BACnetMessageLog.Enqueue(pktlog);
                                }
                                catch (SocketException)
                                {
                                    apm.MessageTodo("Either the network cable is unplugged, or there is no configured Ethernet Port on this computer");
                                }
                                break;

                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM:
                                break;

                            default:
                                apm.MessageTodo("Implement " + incomingBACpacket.unconfirmedServiceChoice.ToString());
                                break;
                        }
                        break;

                    default:
                        apm.MessageTodo("Implement " + incomingBACpacket.pduType.ToString());
                        break;
                }
            }
        }


        private static void RespondReadPropertyWithString(AppManager apm, BACnetPacket bacpkt, BACnetPacket outpkt, byte[] outbuf, string resp)
        {
            int optr;
            // supply the first (and only) (device) object
            optr = InsertReadPropertyResponse(apm, bacpkt, outpkt, outbuf, bacpkt.propertyID);
            // Device Object ID
            InsertContextOpeningTag(outbuf, ref optr, 3);
            InsertApplicationTagString(outbuf, ref optr, resp);
            InsertContextClosingTag(outbuf, ref optr, 3);
            // Send it off
            SendOffPacket(apm, outpkt, outbuf, optr);
        }


        private static void RespondReadPropertyWithUint(AppManager apm, BACnetPacket bacpkt, BACnetPacket outpkt, byte[] outbuf, uint val)
        {
            int optr;
            // supply the first (and only) (device) object
            optr = InsertReadPropertyResponse(apm, bacpkt, outpkt, outbuf, bacpkt.propertyID);
            // Device Object ID
            InsertContextOpeningTag(outbuf, ref optr, 3);
            InsertApplicationTag(outbuf, ref optr, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT, val);
            InsertContextClosingTag(outbuf, ref optr, 3);
            // Send it off
            SendOffPacket(apm, bacpkt, outbuf, optr);
        }


        private static void SendOffPacket(AppManager apm, BACnetPacket pkt, byte[] outbuf, int optr)
        {
            // store NPDU length.

            InsertUint16(outbuf, ref pkt.npduLengthOffset, (uint)optr);

            apm.bnm.bacnet_listen_socket.SendTo(outbuf, optr, SocketFlags.None, pkt.directlyConnectedIPEndPointOfDevice);

            PacketLog pktlog = new PacketLog(true, pkt);
            apm.bnm.BACnetMessageLog.Enqueue(pktlog);
        }


        // Build the 'header' part of the BACnet Packet
        private static int InsertReadPropertyResponse(AppManager apm, BACnetPacket requestBACpkt, BACnetPacket responseCRP, byte[] outbuf, BACnetEnums.BACNET_PROPERTY_ID pID)
        {
            int optr;

            optr = responseCRP.apdu_offset;

            // build APDU to provide services supported.

            responseCRP.EncodeNPDU(outbuf, ref optr);

            BACnetLibraryCL.InsertPDU(outbuf, ref optr, BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK, requestBACpkt.invokeID, requestBACpkt.confirmedServiceChoice);

            requestBACpkt.objectID.EncodeContextTag(outbuf, ref optr, 0);

            BACnetLibraryCL.InsertContextTag(outbuf, ref optr, 1, (int)pID);

            return optr;
        }

    }
}
