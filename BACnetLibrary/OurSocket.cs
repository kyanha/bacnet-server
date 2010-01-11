using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BACnetLibrary
{
    public class OurSocket : Socket 
    {
        public int OurSocketPort;

        public List<BACnetPacket> outgoing_buffer_copy_queue = new List<BACnetPacket>();

        public OurSocket(AddressFamily af, SocketType st, ProtocolType pt, int port ) : base ( af, st, pt)
        {
            IPEndPoint local_ipep = new IPEndPoint(0, port);

//            base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
//            base.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            // bind the local end of the connection to BACnet port number
            try
            {
                base.Bind(local_ipep);
            }
            catch (SocketException sockex )
            {
                //System.Console.WriteLine(sockex.ToString());
                if (sockex.ErrorCode == 0x2740)
                {
                    // todo
                }
            }
            OurSocketPort = port;
        }


        public bool detect_echo_packet( BACnetManager bnm, BACnetPacket packet)
        {
            foreach (IPAddress ipa in bnm.OurIPAddressList)
            {
                if (packet.directlyConnectedIPEndPointOfDevice.Address.Equals(ipa))
                {
                    // when the sent IP address matches one of ours, check the contents of the packet against the packets stored in the outbound copy queue

                    // remove all expired packets
                    foreach (BACnetPacket pkt in outgoing_buffer_copy_queue)
                    {
                        if (pkt.timestamp + 5000 < bnm._stopWatch.ElapsedMilliseconds)
                        {
                            // drop it
                            outgoing_buffer_copy_queue.Remove(pkt);
                            // and quit from this loop, since foreach may fail...
                            // todo, find a better way to remove all > 5000 ms items
                            break;
                        }
                    }

                    if (outgoing_buffer_copy_queue.Count > 100)
                    {
                        // time to panic
                        Console.WriteLine("Outbound copy queue overflow");
                        outgoing_buffer_copy_queue.Clear();
                        return false;
                    }

                    if ( outgoing_buffer_copy_queue.Contains ( packet ) )
                    {
                        Console.WriteLine("This message is from ourselves");
                        
                        // inform that the packet was a match
                        return true ;
                    }
                }
            }
            return false;
        }
    }
}
