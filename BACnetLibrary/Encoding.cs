using System;
using System.Collections.Generic;
using System.Text;

namespace BACnetLibrary
{
    class BACnetEncoding
    {
        public static uint DecodeTagContextUint(byte[] buf, ref int offset, int expectedTagValue)
        {
            // is the next parameter even a context tag 
            if ((buf[offset] & 0x08) != 0x08)
            {
                throw new Exception("m0060 - Expecting a context tag, but none found");
            }

            if ((buf[offset] & 0xf0) != (expectedTagValue << 4))
            {
                // we have an unexpected context tag, sort this out
                throw new Exception ("m0035 - Unexpected context tag index");
                // todo, now is there a way to avoid creating the object? Have to flag it at least...
            }

            int contextTagSize = buf[offset] & 0x07;
            int a = 0;

            switch (contextTagSize )
            {
                case 1:
                    a = buf[offset+1];
                    offset += 2;
                    return (uint) a;

                case 2:
                    a = buf[offset + 1] << 8 ;
                    a |= buf[offset + 2] ;
                    offset += 3;
                    return (uint)a;

                case 3:
                    a = buf[offset + 1] << 16;
                    a |= buf[offset + 2] << 8;
                    a |= buf[offset + 3];
                    offset += 4;
                    return (uint) a;

                case 4:
                    a = buf[offset + 1] << 24;
                    a |= buf[offset + 2] << 16;
                    a |= buf[offset + 3] << 8 ;
                    a |= buf[offset + 4] ;
                    offset += 5;
                    return (uint)a;

                default:
                    // we have an unexpected context tag, sort this out
                    throw new Exception ("m0035 - Unbelievable length of object identifier");
            }
        }



        public static void EncodeApplicationTag(byte[] buf, ref int optr, BACnetEnums.BACNET_APPLICATION_TAG appTag, int value)
        {
            // http://www.bacnetwiki.com/wiki/index.php?title=Application_Tags

            // establish the number of bytes required to encode the tag (i.e. length)

            if (value > 15) throw new Exception("Todo-encode values greater than one byte");

            buf[optr++] = (byte)((int)appTag << 4 | 1);
            buf[optr++] = (byte)(value & 0xff);
        }


        //todo, rename this decodeApplicationTag...
        public static int BACnetDecode_uint( byte[] buffer, int offset, out uint value)
        {
            // take a look at the first octet, this will indicate what type of encoded entity (Tag) we need to decode.
            // See: http://www.bacnetwiki.com/wiki/index.php?title=Encoding

            int len = buffer[offset] & 0x07;

            if ((buffer[offset] & 0x08) == 0x08)
            {
                // we have a Context Tag, todo
                //throw;
            }

            // See: http://www.bacnetwiki.com/wiki/index.php?title=Tag_Number

            switch ((buffer[offset] & 0xf0) >> 4)
            {
                case 0:

                case 1:

                case (int) BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_UNSIGNED_INT:
                case (int)BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_ENUMERATED:
                    // this means the lower nibble is the length - which we prepared when we created the variable..
                    switch (len)
                    {
                        case 1:
                            value = buffer[offset+1];
                            return 2;
                        case 2:
                            value = (uint) ( buffer[offset + 1] * 256 + buffer[offset + 2] );
                            return 3;
                        default:
                        //todo - panic
                            break;
                    }
                    break;

                case (int) BACnetEnums.BACNET_APPLICATION_TAG.BACNET_APPLICATION_TAG_OBJECT_ID:
                    // todo - should split into a type and an instance. return an int for now (4byte)
                    // this means the lower nibble is the length - which we prepared when we created the variable..
                    switch (len)
                    {
                        case 4:
                            value = (uint)( (buffer[offset + 1] << 24) + (buffer[offset + 2] << 16) + (buffer[offset + 3] * 256) + buffer[offset + 4]);
                            return 5;
                        default:
                            //todo - panic
                            break;
                    }
                    break;
                 
                default:
                    break;
            }
            value = 0;
            return 0;
        }
    }
}