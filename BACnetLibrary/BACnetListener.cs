using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace BACnetLibrary
{
    public class BACnetListener
    {
        BACnetManager _bnm;
        AppManager _apm;

        public int BACnet_port;

        public BACnetListener(AppManager apm, BACnetManager bnm, int port )
        {
            _bnm = bnm;
            _apm = apm;

            BACnet_port = port;

            _bnm.bacnet_listen_socket = new OurSocket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, port); 
        }


        public void BACnetListenerClose()
        {
            _bnm.bacnet_listen_socket.Close();
        }


        // This method that will be called when the thread is started
        public void BACnetListenerMethod()
        {
            Console.WriteLine("Thread starting for port " + Convert.ToString(BACnet_port));

            while (true)
            {
                Byte[] received = new Byte[2000];
                BACnetPacket incomingCRPpacket = new BACnetPacket(_apm, _bnm);

                // Create an IPEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                try
                {
                    incomingCRPpacket.length = _bnm.bacnet_listen_socket.ReceiveFrom(received, ref senderRemote);

                    Console.WriteLine("This message was sent from " + ((IPEndPoint)senderRemote).Address.ToString() + "  Port " + ((IPEndPoint)senderRemote).Port.ToString());

                    incomingCRPpacket.directlyConnectedIPEndPointOfDevice = new myIPEndPoint ( senderRemote ) ;
                    incomingCRPpacket.buffer = received;

                    // Decode the packet 
                    bool decodeOK = incomingCRPpacket.DecodeBACnet(received, incomingCRPpacket.length);

                    if (!decodeOK)
                    {
                        continue;
                    }

                    // log this data
                    PacketLog pkt = new PacketLog(false, (IPEndPoint)senderRemote, incomingCRPpacket);
                    pkt.BACnetPacket = (BACnetPacket)incomingCRPpacket;
                    _bnm.BACnetMessageLog.Enqueue(pkt);


                    // did packet Decode fail to create a device? If so, we are not interested. e.g. Who-Is.

                    if (incomingCRPpacket.srcDevice == null)
                    {
                        _apm.MessageTodo("m0039 - Device not created by packet decode");
                        continue;
                    }


                   _apm.pktQueueToApplication.Enqueue(incomingCRPpacket);

                }
                catch (SocketException)
                {
                    // we expect a socket exception when shutting down
                    System.Console.WriteLine("socket exception occurred");
                }
                catch (System.Threading.ThreadAbortException)
                {
                    System.Console.WriteLine("Thread abort exception occurred (shutting down)");
                }
                catch (Exception efe)
                {
                    // need to catch the inevitable exception when this blocking call is cancelled by the shutdown code
                    _apm.MessagePanic(efe.ToString());
                }
            }
        }
    }
}
