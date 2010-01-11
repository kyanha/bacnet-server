// AppManager is our catch all for 'Global Objects' as well as the interface between Application Runtime code and the various User Interface forms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BACnetLibrary
{
    public class AppManager
    {
        public Stopwatch _stopWatch = Stopwatch.StartNew();

        public DeviceTreeView treeViewUpdater;

        // temporary until debug messages migrated
        public BACnetManager bnm;

        public myQueue<String> DiagnosticLogMessage = new myQueue<string>();
        public myQueue<String> DiagnosticLogTodo = new myQueue<string>();
        public myQueue<String> DiagnosticLogProtocol = new myQueue<string>();
        public myQueue<String> DiagnosticLogPanic = new myQueue<string>();

        public myQueue<BACnetPacket> pktQueueToApplication = new myQueue<BACnetPacket>();

        // Our device's parameters

        public UInt16 ourVendorID = 343;
        public string ourDeviceName = "BACnet Server";
        public string ourVendorName = "BACnet Interoperability Testing Services, Inc.";
        public string ourModelName = "BNC-BNS";
        public string ourFirmwareRevision ;

        public UInt32 ourDeviceId = 12345;


        public AppManager()
        {
            ourFirmwareRevision = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor + "." +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build + "." +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision;
        }


        // verbosity - 0 always, 9 never
        public void MessageLog(string msg, int verbosity)
        {
            if (verbosity == 9) return;
            MessageLog(msg);
        }


        public void MessageLog(string msg)
        {
            DiagnosticLogMessage.myEnqueue(msg + Environment.NewLine);
        }


        public void MessageProtocolError(string msg)
        {
            // These messages indicate an error in the protocol..
            DiagnosticLogProtocol.myEnqueue(msg + Environment.NewLine);
        }


        List<String> todoList = new List<String>();
        List<String> panicList = new List<String>();

        public void MessageTodo(string msg)
        {
            // do we already have this message, if so, ignore it
            if (todoList.Contains(msg)) return;
            todoList.Add(msg);

            DiagnosticLogTodo.myEnqueue(msg + Environment.NewLine);
        }


        public void MessagePanic(string panicmessage)
        {
            // do we already have this message, if so, ignore it
            if (panicList.Contains(panicmessage)) return;

            panicList.Add(panicmessage);
            DiagnosticLogPanic.myEnqueue(panicmessage + Environment.NewLine + Environment.NewLine);
        }
    }
}
