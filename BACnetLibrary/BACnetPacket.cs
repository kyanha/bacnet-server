using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BACnetLibrary
{
    public class BACnetPacket : IEquatable<BACnetPacket>, ICloneable
    {
        AppManager _apm;
        BACnetManager lbnm;         // Local BACnet Manager, per Karl's discussion Saturday

        public int Source_Port;
        public byte[] buffer;
        public int length;           // for incoming messages
        public int optr;             // for buildingoutgoing messages

        public int npdu_offset;
        public int nsdu_offset;
        public int npduLengthOffset;    // This is where the length of the whole packet - NPDU (NCPI+NSDU) and APDU (if it exists) will eventually be stored

        // APDU Parameters
        // See: http://www.bacnetwiki.com/wiki/index.php?title=PDU_Type

        public bool errorFlag;
        public int apdu_offset;
        public int apdu_length;
        public BACnetEnums.BACNET_BACNET_REJECT_REASON pduRejectReason;

        public uint lowRange;
        public uint highRange;

        public BACnetEnums.BACNET_PDU_TYPE pduType;
        public BACnetEnums.BACNET_UNCONFIRMED_SERVICE unconfirmedServiceChoice;
        public BACnetEnums.BACNET_CONFIRMED_SERVICE confirmedServiceChoice;
        public bool apduUnconfirmedServiceFlag;
        public bool apduConfirmedServiceTypeFlag;

        public BACnetObjectIdentifier objectID;
        public BACnetEnums.BACNET_PROPERTY_ID propertyID;
        public List<BACnetObjectIdentifier> objectList;
        public int arrayIndex;
        public bool arrayIndexDecoded;

        public byte[] apdu_buf;
        public byte invokeID;


        // NPDU parameters
        public NPDU npdu = new NPDU();
        public List<uint> numberList;
        public List<RouterPort> routerPortList;

        // various objects that will be created depending on the packet received
        public ADR dadr;
        public Device srcDevice = new Device();

        // todo add hopcount to npdu class
        public uint hopcount;
        public long timestamp;

        // public bool apdu_present, is_broadcast, expecting_reply;
        public bool apdu_present;


        public myIPEndPoint directlyConnectedIPEndPointOfDevice;
        public uint BACnetPort;


        public BACnetPacket(AppManager apm, BACnetManager bnm)
        {
            lbnm = bnm;
            _apm = apm;

            apdu_present = false;
            npdu.isBroadcast = false;
        }

        public bool Equals(BACnetPacket packetToCompare)
        {
            if (this.length == packetToCompare.length)
            {
                // well, lengths are the same, is the data??

                int i;

                for (i = 0; i < this.length; i++)
                {
                    if (this.buffer[i] != packetToCompare.buffer[i])
                    {
                        // mismatch
                        return false;
                    }
                }
            }
            return true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool DecodeBACnet(byte[] buffer, int totalMessageLength)
        {
            int offset = 0 ;

            try
            {
                // this whole section http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
                ADR sadr = null;

                if (buffer[offset] != BACnetEnums.BACNET_BVLC_TYPE_BIP)
                {
                    // todo3, panic log
                    _apm.MessageProtocolError("m0013 - Not a BACnet/IP message");
                    return false;
                }

                // we could receive an original broadcast, a unicast, a forwarded here...
                // BVLC Function Types. See http://www.bacnetwiki.com/wiki/index.php?title=BVLC_Function

                switch ((BACnetEnums.BACNET_BVLC_FUNCTION)buffer[offset + 1])
                {
                    case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_FORWARDED_NPDU:
                        npdu_offset = offset + 10;
                        break;
                    case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU:
                    case BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU:
                        // all these OK
                        npdu_offset = offset + 4;
                        break;
                    default:
                        BACnetLibraryCL.Panic("m0012 nosuch bvlc function");
                        break;
                }


                // Investigate the NPDU
                // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

                if (buffer[npdu_offset] != BACnetEnums.BACNET_PROTOCOL_VERSION)
                {
                    // we have a major problem, microprotocol version has changed. http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control
                    BACnetLibraryCL.Panic("m0011 BVLC microprotocol problem");
                    return false;
                }

                // expecting reply?

                if ((buffer[npdu_offset + 1] & 0x04) == 0x04)
                {
                    npdu.expectingReply = true;
                }


                // destination address present?
                // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

                if ((buffer[npdu_offset + 1] & 0x20) == 0x20)
                {

                    //da_present = true;
                    dadr = new ADR();

                    // dnet, dadr and hop count present

                    int dAddrOffset = npdu_offset + 2;
                    dadr.Decode(buffer, ref dAddrOffset);

                    if (dadr.MACaddress.length == 0)
                    {
                        npdu.isBroadcast = true;

                        // broadcast, but check the DNET
                        if (dadr.networkNumber != 0xffff)
                        {
                            throw new Exception ("m0010 - Broadcast according to DLEN, but DNET not 0xffff");
                            // todo, this means directed broadcast, need to deal with this still
                        }
                    }
                    if (dadr.MACaddress.length != 1 && dadr.MACaddress.length != 6 && dadr.MACaddress.length != 0)
                    {
                        // panic
                        throw new Exception("m0009 - Unexpected DADR len");
                    }
                    // todo, pick up variable length destination address
                }


                // Source address present?
                // http://www.bacnetwiki.com/wiki/index.php?title=NPCI

                if ((buffer[npdu_offset + 1] & 0x08) == 0x08)
                {
                    //sa_present = true;

                    sadr = new ADR();

                    int sa_offset = npdu_offset + 2;

                    // however, if there is a destination address, move the sa_offset up appropriate amount

                    if (dadr != null)
                    {
                        sa_offset = npdu_offset + 2 + 3 + (int)dadr.MACaddress.length;
                    }

                    // means SADR, SNET present

                    sadr.Decode(buffer, ref sa_offset);

                    // SA exists (included MAC and so this means the received IP address needs to be the fromBIP

                }
                else
                {
                    // at this point, if SADR not discovered within the NPDU, then the SADR MAC address is the Ethernet/IP fromaddress
                    // and the device can (must) be considered 'direcly connected'
                    if (directlyConnectedIPEndPointOfDevice != null)
                    {
                        // and even though the device is directly connected, because we are a router, we have an allocated network number to provide
                        // the network number is available outside this class, and will be filled in by the calling function if it is relevant.
                        srcDevice.adr = new ADR(0, directlyConnectedIPEndPointOfDevice);
                    }
                    else
                    {
                        throw new Exception("m0063 - No From-Address can be determined");
                    }
                }



                if (dadr != null)
                {
                    if (sadr != null)
                    {
                        hopcount = (uint)(buffer[npdu_offset + 2 + 2 + 1 + dadr.MACaddress.length + 2 + 1 + sadr.MACaddress.length]);
                    }
                    else
                    {
                        hopcount = (uint)(buffer[npdu_offset + 2 + 2 + 1 + dadr.MACaddress.length]);
                    }
                    // true broadcast, but check the hopcount

                    if (hopcount == 0)
                    {
                        // out of hops, should never happen to us, so sound a warning
                        // todo, remove this for a functioning systems
                        System.Windows.Forms.MessageBox.Show("m0008 Hopcount of 0 detected");
                        return false;
                    }
                }

                // finished resolving sadr, and dadr. Now populate our devices as required

                if (sadr != null)
                {
                    if (srcDevice.adr != null)
                    {
                        // means this adr was partially populated elsewhere.
                        srcDevice.adr.networkNumber = sadr.networkNumber;
                        srcDevice.adr.MACaddress = sadr.MACaddress;
                    }
                    else
                    {
                        srcDevice.adr = sadr;
                    }
                }




                if ((buffer[npdu_offset + 1] & 0x80) == 0x80)  // todo magic number
                {
                    // NSDU contains Network Layer Message
                    // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                    apdu_present = false;
                    npdu.isNPDUmessage = true;

                    // calculate offset to the NSDU

                    nsdu_offset = npdu_offset + 2;

                    if (sadr != null) nsdu_offset += 2 + 1 + (int)sadr.MACaddress.length;
                    if (dadr != null) nsdu_offset += 2 + 1 + (int)dadr.MACaddress.length + 1;

                    npdu.function = (BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE)buffer[nsdu_offset];

                    switch (npdu.function)
                    {
                        case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                            // there may be a list of network numbers after the header

                            if ((length - nsdu_offset >= 3))
                            {
                                numberList = new List<uint>();
                            }

                            for (int i = 0; i < (length - nsdu_offset - 1) / 2; i++)
                            {
                                int tref = nsdu_offset + 1 + i * 2;
                                numberList.Add(BACnetLibraryCL.ExtractUint16(buffer, ref tref));
                            }
                            break;

                        case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:
                            int tiptr = nsdu_offset + 1;
                            int numPorts = buffer[tiptr++];
                            routerPortList = new List<RouterPort>();

                            for (int i = 0; i < numPorts; i++)
                            {
                                RouterPort rp = new RouterPort();
                                rp.Decode(buffer, ref tiptr);
                                routerPortList.Add(rp);
                            }
                            break;

                        default:
                            // todo
                            break;
                    }
                }
                else
                {
                    // NSDU contains APDU
                    // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Protocol_Control_Information

                    // determine if SNET, SLEN, SADR present

                    apdu_present = true;

                    apdu_offset = npdu_offset + 2;

                    if (sadr != null) apdu_offset += 2 + 1 + (int)sadr.MACaddress.length;
                    if (dadr != null) apdu_offset += 2 + 1 + (int)dadr.MACaddress.length + 1;

                    apdu_length = length - apdu_offset;
                    if (apdu_length < 0)
                    {
                        _apm.MessagePanic("m0006 Illegal APDU length");
                        return false;
                    }

                    // todo - need to extract the apdu for others to refer to. However, APDUs may be customer specific, so extract this as a buffer
                    // only for now, and do some spot checks for relevant functions such as I-Am and Who-Is.

                    apdu_buf = new byte[2000];

                    Buffer.BlockCopy(buffer, apdu_offset, apdu_buf, 0, apdu_length);

                    // the offset here is the APDU. Start parsing APDU.
                    // todo, decided to leave the enum values unshifted today 11/27/09
                    pduType = (BACnetEnums.BACNET_PDU_TYPE)(buffer[apdu_offset] & 0xf0);

                    // make sure that we can handle the rest of the packet

                    //if ((buffer[apdu_offset] & 0x0f) != 0 )
                    //{
                    //    throw new Exception("m0056 - Cannot handle segmented messages yet");
                    //}

                    int tptr = apdu_offset + 1 ;

                    if (pduType == BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST)
                    {
                        // some PDUs have max segs here
                        tptr++;
                    }

                    //_apm.MessageTodo("Remove apdu++");
                    //apdu_offset++;
                    //apdu_offset++;

                    // now the next byte is the invoke ID

                    invokeID = buffer[tptr++];

                    switch (pduType)
                    {
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
                            // todo, offset is not always 1, depends on the flags in the first byte.
                            DecodeUnconfirmedService(apdu_buf, apdu_length );
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST:
                            DecodeConfirmedService(apdu_buf);
                            break;

                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_SIMPLE_ACK:
                            break;

                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK:
                            confirmedServiceChoice = (BACnetEnums.BACNET_CONFIRMED_SERVICE)buffer[apdu_offset + 2];
                            DecodeComplexACK(buffer, apdu_offset);
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_SEGMENT_ACK:
                            _apm.MessageTodo("m0093 - Segment ACK");
                            errorFlag = true;
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ERROR:
                            _apm.MessageTodo("m0064 - Error");
                            errorFlag = true;
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_REJECT:
                            pduRejectReason = (BACnetEnums.BACNET_BACNET_REJECT_REASON) buffer[apdu_offset++];
                            errorFlag = true;
                            break;
                        case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ABORT:
                            _apm.MessageTodo("m0066 - PDU abort");
                            errorFlag = true;
                            break;
                        default:
                            throw new Exception ("m0003 - Illegal PDU type");
                    }
                }
            }
            catch ( Exception ex )
            {
                _apm.MessagePanic("m0001 - BACnet Decode Failed " + ex.ToString() );
            }
            return true; 
        }



        private void DecodeConfirmedService(byte[] apdu_buf)
        {
            if ((apdu_buf[0] & 0x08) != 0)
            {
                _apm.MessageTodo("m0002 Need to implement confirmed service types with seg=1 still");
                return;
            }

            int iptr = 3 ;

            confirmedServiceChoice = (BACnetEnums.BACNET_CONFIRMED_SERVICE)apdu_buf[iptr++] ;
            apduConfirmedServiceTypeFlag = true;

            switch (confirmedServiceChoice)
            {
                case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY :
                    // Expecting 2-3 context tags. 
                    // First, mandatory, context 0, object ID
                    objectID = new BACnetObjectIdentifier();
                    objectID.DecodeContextTag ( apdu_buf, ref iptr ) ;

                    // Second, mandatory, Property ID
                    propertyID = (BACnetEnums.BACNET_PROPERTY_ID) BACnetEncoding.DecodeTagContextUint(apdu_buf, ref iptr, 1 );

                    // Third, Array Index, Optional
                    if (iptr < apdu_length)
                    {
                        arrayIndex = (int) BACnetEncoding.DecodeTagContextUint(apdu_buf, ref iptr, 2);
                        arrayIndexDecoded = true;
                    }
                    break;

                default:
                    _apm.MessageTodo("m0024 all the other service types");
                    break;
            }

           
        }


        private bool DecodeUnconfirmedService(byte[] apdu_buf, int apduLen )
        {
            // todo, this code assumes that the service is encoded in postion number one. This is not always the case. Resolve

            if ((apdu_buf[0] & 0x0f) != 0)
            {
                _apm.MessageProtocolError("m0025 this nibble should be zero for Unconfirmed services");
                return false;
            }

            if (apdu_buf[1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS)
            {
                // http://www.bacnetwiki.com/wiki/index.php?title=Who-Is

                apduUnconfirmedServiceFlag = true;
                unconfirmedServiceChoice = BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS;

                if (apduLen != 2)
                {
                    int offset = 2 ;

                    // means we must have a low and high range..
                    lowRange = BACnetEncoding.DecodeTagContextUint ( apdu_buf, ref offset, 0 ) ;
                    highRange = BACnetEncoding.DecodeTagContextUint(apdu_buf, ref offset, 1);
                }

                // and for now, we will assume only workstations issue who-is messages.
                this.srcDevice.type = BACnetEnums.DEVICE_TYPE.Workstation;
            }
            else if (apdu_buf[1] == (byte)BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM)
            {
                apduUnconfirmedServiceFlag = true;
                unconfirmedServiceChoice = BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM;

                // I-Am described right here: http://www.bacnetwiki.com/wiki/index.php?title=I-Am

                // first encoded entity is the Device Identifier...
                // Encoding described here: http://www.bacnetwiki.com/wiki/index.php?title=Encoding

                // Decode Device Identifier

                int offset = 2;

                srcDevice.deviceObjectID.DecodeApplicationTag(apdu_buf, ref offset);
                Console.WriteLine("This is device: " + srcDevice.deviceObjectID);

                // todo, for now, we will ignore device insance xxx if received 
                // and we are the client (bacnet browser )

                srcDevice.packet = this;

                uint maxAPDULen;

                offset += BACnetEncoding.BACnetDecode_uint(apdu_buf, offset, out maxAPDULen);

                Console.WriteLine("Max APDU length accepted: " + maxAPDULen);

                uint segmentation_supported;

                offset += BACnetEncoding.BACnetDecode_uint(apdu_buf, offset, out segmentation_supported);

                Console.WriteLine("Segmentation Supported: " + segmentation_supported);

                srcDevice.SegmentationSupported = (BACnetEnums.BACNET_SEGMENTATION)segmentation_supported;



                uint vendorId;

                offset += BACnetEncoding.BACnetDecode_uint(apdu_buf, offset, out vendorId);

                Console.WriteLine("VendorId: " + vendorId);

                srcDevice.VendorId = vendorId;
            }
            return true;
        }


        public void EncodeBACnetResponse(byte[] outbuf, ref int optr, BACnetPacket request )
        {
            // Encodes a BACnet response to the supplied packet
        }


        public void EncodeBACnetNew(byte[] outbuf, ref int optr)
        {
            int startBACnetPacket = optr;

            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            outbuf[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;

            if (npdu.isBroadcast)
            {
                outbuf[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;
            }
            else
            {
                outbuf[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;
            }

            int store_length_here = optr;
            optr += 2;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            outbuf[optr++] = 0x01;        // Always 1

            int store_NPCI = optr;
            outbuf[optr++] = 0x00;        // Control

            if (npdu.isNPDUmessage)
            {
                outbuf[store_NPCI] |= 0x80;     // Indicating Network Layer Message
            }

            if (npdu.expectingReply)
            {
                outbuf[store_NPCI] |= 0x04;     // todo- magic number
            }

            if (npdu.isBroadcast)
            {
                outbuf[store_NPCI] |= 0x20;     // Control byte - indicate DADR present 
                outbuf[optr++] = 0xff;          // DNET Network - B'cast
                outbuf[optr++] = 0xff;
                outbuf[optr++] = 0x00;          // DLEN
            }
            else
            {
                // insert dadr - but only if the device is NOT directly coupled. See page 59. If the device is directly coupled
                // then the ethernet address in the packet will suffice.
                if (!dadr.directlyConnected)
                {
                    // need to insert destination DADR here
                    outbuf[store_NPCI] |= 0x20;         // Control byte - indidate DADR present 
                    dadr.Encode(outbuf, ref optr);
                }
            }

            // we are a router, so we need to add source address under most circumstances. (not broadcast who-is)
            if (srcDevice.adr != null)
            {
                outbuf[store_NPCI] |= 0x08;                 // Control byte - indidate SADR present 
                srcDevice.adr.Encode(outbuf, ref optr);
            }

            if (npdu.isBroadcast || !dadr.directlyConnected)
            {
                // insert hopcount here. 
                hopcount -= 10;
                outbuf[optr++] = (byte)hopcount;
            }

            // APDU start
            // http://www.bacnetwiki.com/wiki/index.php?title=APDU

            if (apdu_present)
            {
                // APDU start
                // http://www.bacnetwiki.com/wiki/index.php?title=APDU

                for (int i = 0; i < apdu_length; i++)
                {
                    outbuf[optr++] = buffer[apdu_offset + i];        // Encoded APDU type == 01 == Unconfirmed Request
                }
            }
            else if (npdu.isNPDUmessage)
            {
                // Build the Nsdu
                // http://www.bacnetwiki.com/wiki/index.php?title=Network_Layer_Message

                outbuf[optr++] = (byte)npdu.function;

                switch (npdu.function)
                {
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK:
                        break;

                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE:
                        outbuf[optr++] = 0x00;        // Number of port mappings
                        break;
                        
                    case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                        if (numberList != null)
                        {
                            foreach (uint i in numberList)
                            {
                                BACnetLibraryCL.InsertUint16(outbuf, ref optr, i);
                            }
                        }
                        break;

                    default:
                        _apm.MessageTodo("m0023 Implement " + npdu.function.ToString());
                        break;
                }
            }
            else
            {
                // build an APDU.
                switch (this.pduType)
                {
                    case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
                        switch (this.unconfirmedServiceChoice)
                        {
                            case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM:
                                // APDU start
                                // http://www.bacnetwiki.com/wiki/index.php?title=APDU

                                outbuf[optr++] = 0x10;        // Encoded APDU type == 01 == Unconfirmed Request
                                outbuf[optr++] = 0x00;        // Unconfirmed Service Choice: I-Am

                                // object identifier, device object 

                                BACnetObjectIdentifier bnoid = new BACnetObjectIdentifier();

                                bnoid.SetType(BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE);
                                _apm.MessageTodo("m0038 - Establish a mechanism to determine our OWN Device ID");
                                bnoid.SetInstance( _apm.ourDeviceId );
                                bnoid.EncodeApplicationTag(outbuf, ref optr );

                                // Maximum APDU length (Application Tag, Integer)
                                Unsigned apdulen = new Unsigned(1476);
                                apdulen.Encode(outbuf, ref optr);

                                // Segmentation supported, (Application Tag, Enum)
                                BACnetSegmentation bsg = new BACnetSegmentation();

                                bsg.Encode(outbuf, ref optr);

                                // Vendor ID, (Application Tag, Integer)
                                BACnetLibraryCL.InsertApplicationTagUint16(outbuf, ref optr, _apm.ourVendorID );
                                break;

                            default:
                                _apm.MessageTodo("m0022 Build missing service type");
                                break;
                        }
                        break;

                    default:
                        _apm.MessageTodo("m0021 Build missing PDU type");
                        break;
                }
            }
            outbuf[store_length_here] = (byte)(((optr - startBACnetPacket) >> 8) & 0xff);
            outbuf[store_length_here + 1] = (byte)((optr - startBACnetPacket) & 0xff);
        }


        public void EncodeNPDU(byte[] outbuf, ref int optr)
        {
            // BVLC Part
            // http://www.bacnetwiki.com/wiki/index.php?title=BACnet_Virtual_Link_Control

            outbuf[optr++] = BACnetEnums.BACNET_BVLC_TYPE_BIP;

            if (npdu.isBroadcast)
            {
                outbuf[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_BROADCAST_NPDU;
            }
            else
            {
                outbuf[optr++] = (byte)BACnetEnums.BACNET_BVLC_FUNCTION.BVLC_ORIGINAL_UNICAST_NPDU;
            }

            npduLengthOffset = optr;
            optr += 2;

            // Start of NPDU
            // http://www.bacnetwiki.com/wiki/index.php?title=NPDU

            outbuf[optr++] = 0x01;        // Always 1

            int store_NPCI = optr;
            outbuf[optr++] = 0x00;        // Control

            if (npdu.isNPDUmessage)
            {
                outbuf[store_NPCI] |= 0x80;     // Indicating Network Layer Message
            }

            if (npdu.expectingReply)
            {
                outbuf[store_NPCI] |= 0x04;     // todo- magic number
            }

            if (npdu.isBroadcast)
            {
                outbuf[store_NPCI] |= 0x20;     // Control byte - indicate DADR present 
                outbuf[optr++] = 0xff;          // DNET Network - B'cast
                outbuf[optr++] = 0xff;
                outbuf[optr++] = 0x00;          // DLEN
            }
            else
            {
                // insert dadr - but only if the device is NOT directly coupled. See page 59. If the device is directly coupled
                // then the ethernet address in the packet will suffice.
                if ( dadr != null && !dadr.directlyConnected)
                {
                    // need to insert destination DADR here
                    outbuf[store_NPCI] |= 0x20;         // Control byte - indidate DADR present 
                    dadr.Encode(outbuf, ref optr);
                }
            }

            // we are a router, so we need to add source address under most circumstances. (not broadcast who-is)
            if (srcDevice.adr != null)
            {
                outbuf[store_NPCI] |= 0x08;                 // Control byte - indidate SADR present 
                srcDevice.adr.Encode(outbuf, ref optr);
            }

            if (npdu.isBroadcast || ( dadr != null && !dadr.directlyConnected ) )
            {
                // insert hopcount here. 
                hopcount--;
                outbuf[optr++] = (byte)hopcount;
            }
        }


        void DecodeComplexACK(byte[] buf, int offset)
        {
            if ((buf[offset] & 0x0f) != 0)
            {
                throw new Exception (  "m0020 - Not ready to handle segmented messages yet");
            }

            // invoke ID - ignoring for now

            // Service ACK choice

            BACnetEnums.BACNET_CONFIRMED_SERVICE sc = (BACnetEnums.BACNET_CONFIRMED_SERVICE)buf[offset + 2] ;
            switch ( sc )
            {
                case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                    offset += 3;

                    // Decode Object ID of the object whos property we are reading

                    BACnetObjectIdentifier oid = new BACnetObjectIdentifier(buf, ref offset, BACnetEnums.TAG.CONTEXT, 0);

                    // Decode the property ID
                    propertyID = (BACnetEnums.BACNET_PROPERTY_ID)BACnetEncoding.DecodeTagContextUint(buf, ref offset, 1);

                    // Now decode the Property Value itself. Variable encoding, variable length, etc....

                    switch (oid.objectType)
                    {
                        case BACnetEnums.BACNET_OBJECT_TYPE.OBJECT_DEVICE:
                            switch (propertyID)
                            {
                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_LIST:
                                    // decode the list of objects
                                    // process the opening context tag, 0x3e
                                    if (buffer[offset++] != 0x3e)
                                    {
                                        throw new Exception (  "m0033 - Opening context tag not found " + buffer[offset-1].ToString() );
                                    }

                                    objectList = new List<BACnetObjectIdentifier>();

                                    // now loop until closing tag found
                                    while (buffer[offset] != 0x3f)
                                    {
                                        // we should get a list of object IDs, add these to our backnet packet object as they are discovered.

                                        objectList.Add(new BACnetObjectIdentifier(buffer, ref offset, BACnetEnums.TAG.APPLICATION, BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID));
                                    }
                                    break;

                                case BACnetEnums.BACNET_PROPERTY_ID.PROP_OBJECT_NAME:
                                    _apm.MessageTodo("m0032 - Decode object name");
                                    break;

                                default:
                                    _apm.MessageTodo ( "m0026 Unimplemented Property ID " + propertyID.ToString () ) ;
                                    break;
                            }
                            break;

                        default:
                            _apm.MessageTodo("m0061 Unhandled object type " + oid.objectType.ToString());
                            break;
                    }
                    break;
                default:
                    _apm.MessageTodo("m0028 - Not ready to deal with this service yet " + sc.ToString() );
                    return ;
            }
        }
    }
}
