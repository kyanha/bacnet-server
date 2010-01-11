using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Diagnostics;


namespace BACnetLibrary
{
    public class BACnetManager
    {
        AppManager _apm;

        public BACnetListener BAClistener_object;
        public Thread BAClistener_thread;

        public List<Device> deviceList = new List<Device>();
        public Queue<Device> NewDeviceQueue = new Queue<Device>();

        public Queue<PacketLog> BACnetMessageLog = new Queue<PacketLog>();
        public Queue<BACnetPacket> DeviceUpdateQueue = new Queue<BACnetPacket>();
        public Queue<String> DiagnosticLogMessage = new Queue<string>();

        public int BACnetManagerPort;
//        public Socket bacnet_listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public OurSocket bacnet_listen_socket ;

        public IPHostEntry OurIPAddressEntry;
        public IPAddress[] OurIPAddressList;

        public bool logPings = false;

        public Stopwatch _stopWatch = Stopwatch.StartNew();

        public DeviceTreeView refRouterTreeView;

        public void BACnetManagerClose()
        {
            // here we have a conundrum; the thread we are about to abort is most likely blocked waiting for an ethernet packet to arrive.
            // if we destroy the socket that the thread is using, the thread will do something nasty.
            // if we abort the thread, it will possibly never end since it is waiting for that packet to arrive..
            // ... so we destroy the socket, and catch the thread exception.

            BAClistener_object.BACnetListenerClose();

            BAClistener_thread.Abort();
            
        }

        public BACnetManager( AppManager apm, int BACnetManagerPort )
        {
            _apm = apm;

            this.BACnetManagerPort = BACnetManagerPort;

            // Establish our own IP address, port

            String strHostName = Dns.GetHostName();
            this.OurIPAddressEntry = Dns.GetHostEntry(strHostName);
            this.OurIPAddressList = this.OurIPAddressEntry.AddressList;

            // fire up a thread to watch for incoming packets

            BAClistener_object = new BACnetListener( _apm, this, BACnetManagerPort ) ;

            BAClistener_thread = new Thread(new ThreadStart(BAClistener_object.BACnetListenerMethod));
            BAClistener_thread.Start();
        }

        public bool IsThisUs(IPEndPoint ipep)
        {
            foreach (IPAddress ipa in OurIPAddressList)
            {
                if (ipep.Address.Equals(ipa))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
