using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Drawing;

namespace BACnetLibrary
{
    public class PacketLog
    {
        public bool sending;
        public IPEndPoint ipep;
        public BACnetPacket BACnetPacket;

        static int _count;

        public PacketLog(bool sending, IPEndPoint ipep)
        {
            this.sending = sending;
            this.ipep = ipep;  // ipep sending to, or receiving from
        }

        public PacketLog(bool sending, IPEndPoint ipep, BACnetPacket packet)
        {
            this.sending = sending;
            this.ipep = ipep;
            this.BACnetPacket = packet;
        }


        public PacketLog(bool sending, BACnetPacket packet)
        {
            this.sending = sending;
            this.BACnetPacket = packet;
        }

        public static void TreeViewUpdate(AppManager apm, BACnetManager bnm, TreeView treeViewMessageLog, bool logmessages )
        {
            if (bnm.BACnetMessageLog.Count > 0)
            {
                while (bnm.BACnetMessageLog.Count > 0)
                {
                    // remove the first treenode

                    _count++;

                    PacketLog pktlog = bnm.BACnetMessageLog.Dequeue();

                    if (!logmessages)
                    {
                        // discard the message, unless it has been logged as an error
                        if ( pktlog.BACnetPacket != null && ! pktlog.BACnetPacket.errorFlag)
                        {
                            continue;
                        }
                    }


                    TreeNode ntn = new TreeNode();
                    TreeNodeCollection addBACnetHere = treeViewMessageLog.Nodes ;

                    if (pktlog.BACnetPacket != null)
                    {
                        // bump the rest up a level
                        TreeNode nntn = new TreeNode();
                        nntn.Text = "BACnet";

                        if (pktlog.sending == true)
                        {
                            nntn.Text = _count.ToString() + " BACnet Sent";
                            nntn.BackColor = Color.LightPink;
                        }
                        else
                        {
                            nntn.Text = _count.ToString() + " BACnet Recd";
                            nntn.BackColor = Color.LightGreen;
                        }

                        // display some NPDU parameters

                        if (pktlog.BACnetPacket.npdu.isBroadcast)
                        {
                            nntn.Nodes.Add("To:   Broadcast");
                        }
                        else
                        {
                            if (pktlog.BACnetPacket.dadr != null)
                            {
                                nntn.Nodes.Add("To:   " + pktlog.BACnetPacket.dadr.ToString());
                            }
                        }

                        // From address

                        if (pktlog.BACnetPacket.directlyConnectedIPEndPointOfDevice != null)
                        {
                            nntn.Nodes.Add("From: " + pktlog.BACnetPacket.directlyConnectedIPEndPointOfDevice.ToString());
                        }


                        if (pktlog.BACnetPacket.npdu.expectingReply)
                        {
                            nntn.Nodes.Add("Expecting Reply");
                        }

                        if ( ! pktlog.BACnetPacket.npdu.isNPDUmessage)
                        {
                            switch (pktlog.BACnetPacket.pduType)
                            {
                                case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST:
                                    nntn.Text += " Unconfirmed Service Request -";
                                    switch (pktlog.BACnetPacket.unconfirmedServiceChoice)
                                    {
                                        case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_I_AM:
                                            nntn.Text += " I-Am";
                                            break;
                                        case BACnetEnums.BACNET_UNCONFIRMED_SERVICE.SERVICE_UNCONFIRMED_WHO_IS:
                                            nntn.Text += " Who-Is";
                                            break;
                                        default:
                                            nntn.Text += " m0019 Unknown BACnet unconfirmed service type - fix line 196 PacketLog.cs";
                                            break;
                                    }
                                    break;


                                case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_CONFIRMED_SERVICE_REQUEST:
                                    nntn.Text += " Confirmed Service Request";
                                    switch (pktlog.BACnetPacket.confirmedServiceChoice)
                                    {
                                        case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                                            nntn.Nodes.Add("Read Property " + pktlog.BACnetPacket.propertyID.ToString());
                                            break;
                                        case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROP_MULTIPLE:
                                            nntn.Nodes.Add("Read Property Multiple");
                                            break;
                                        default:
                                            nntn.Nodes.Add("m0014 Unknown BACnet confirmed service type " + pktlog.BACnetPacket.confirmedServiceChoice.ToString()) ;
                                            break;
                                    }
                                    break;

                                case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_COMPLEX_ACK:
                                    nntn.Text += " Complex ACK -" ;
                                    switch (pktlog.BACnetPacket.confirmedServiceChoice)
                                    {
                                        case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROPERTY:
                                            nntn.Text += " Read Property " + pktlog.BACnetPacket.propertyID.ToString();
                                            break;
                                        case BACnetEnums.BACNET_CONFIRMED_SERVICE.SERVICE_CONFIRMED_READ_PROP_MULTIPLE:
                                            nntn.Text += " Read Property Multiple";
                                            break;
                                        default:
                                            nntn.Text += " m0036 Unknown BACnet confirmed service type";
                                            break;
                                    }
                                    break;

                                case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_REJECT:
                                    nntn.Text += " Reject PDU - " + pktlog.BACnetPacket.pduRejectReason.ToString() + " [" + pktlog.BACnetPacket.invokeID.ToString() + "]" ;
                                    break;

                                case BACnetEnums.BACNET_PDU_TYPE.PDU_TYPE_ABORT:
                                    apm.MessageTodo("m0092 Resolve pktlog.bacnetpacket.pdutyp " + pktlog.BACnetPacket.pduType.ToString());
                                    break;

                                default:
                                    apm.MessageTodo("m0015 Resolve pktlog.bacnetpacket.pdutyp " + pktlog.BACnetPacket.pduType.ToString());
                                    break;
                            }
                        }
                        else if (pktlog.BACnetPacket.apdu_present)
                        {
                            apm.MessageTodo("m0018 I dont expect to see this");
                        }
                        else
                        {
                            // todo, clean up this flag one day
                            nntn.Nodes.Add("NPDU Function: " + pktlog.BACnetPacket.npdu.function.ToString());
                        }
                        addBACnetHere.Add(nntn);
                    }

                    treeViewMessageLog.ExpandAll();

                    // remove all the earlier nodes...
                    while (treeViewMessageLog.Nodes.Count > 150)
                    {
                        treeViewMessageLog.Nodes.RemoveAt(0);
                    }
                }
            }
        }
    }
}
