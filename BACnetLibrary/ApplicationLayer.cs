using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BACnetLibrary
{
    public class ApplicationLayer : Device
    {
        public List<RouterPort> listOfRouterPorts = new List<RouterPort>();

        public ApplicationLayer(Device d)
        {
            type = BACnetEnums.DEVICE_TYPE.Router;
            adr = d.adr;
            directlyConnectedIPEndPointOfDevice = d.directlyConnectedIPEndPointOfDevice;
        }

        public RouterPort EstablishRouterPort(uint networkNumber)
        {
            // destinationDev = _bnm.deviceList.Find(delegate(Device d) { return d.adr.Equals(incomingCRPpacket.dadr); });
            RouterPort frp = listOfRouterPorts.Find(delegate(RouterPort drp) { return drp.networkNumber == networkNumber; });

            if (frp == null)
            {
                // create a new routerport and add to the list
                frp = new RouterPort();
                frp.networkNumber = networkNumber;
                // todo we run the risk of partially populating this object..... 
                listOfRouterPorts.Add(frp);
            }
            return frp;
        }
    }

    public class BACnetAppTask
    {
        AppManager _apm;
        BACnetManager _bnm;

        public BACnetAppTask(AppManager apm, BACnetManager bnm)
        {
            _apm = apm;
            _bnm = bnm;
        }


        public void ServerApplication()
        {

            while (true)
            {
                // check to see if we have any live Incoming messages..

                while (_apm.pktQueueToApplication.Count > 0)
                {
                    try
                    {
                        // process the incoming queue

                        BACnetPacket bacpkt = _apm.pktQueueToApplication.Dequeue();

                        if (bacpkt.npdu.isNPDUmessage)
                        {
                            switch (bacpkt.npdu.function)
                            {
                                case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK:
                                    // find the site, establish the device, make sure that it is marked as a router, save the information
                                    _apm.MessageTodo("I am router received");
                                    break;

                                case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_INIT_RT_TABLE_ACK:
                                    // find the site, establish the device, make sure that it is marked as a router, save the information
                                    _apm.MessageTodo("Init rt table ack received");
                                    break;

                                case BACnetEnums.BACNET_NETWORK_MESSAGE_TYPE.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK:
                                    _apm.MessageTodo("m0053 - ----- Implementing  " + bacpkt.npdu.function.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            // todo - a certain amount of redundancy exists below, resolve.
                            BACnetLibraryCL.RespondToAPDU( _apm, _bnm, bacpkt);
                        }
                    }
                    catch (Exception ex)
                    {
                        _apm.MessagePanic("m0052 - Router application layer fault" + ex.ToString() );
                    }
                }

                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
