using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using BACnetLibrary;

namespace BACnet_Server
{
    public partial class MainForm : Form
    {
        AppManager _apm = new AppManager();

        BACnetManager bnm ;


        Stopwatch _stopWatch = Stopwatch.StartNew();

        public MainForm()
        {
            InitializeComponent();

            bnm = new BACnetManager( _apm, 47809 );
            _apm.bnm = bnm;


            // Start our Application task
            backgroundWorkerApp.RunWorkerAsync();
        }

       private void ServerMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bnm.BAClistener_thread.Abort();

            // cancel our outstanding socket receives
            try
            {
                bnm.BAClistener_object.BACnetListenerClose();
            }
            catch (Exception fe)
            {
                Console.WriteLine(fe);
            }
            Application.Exit();
        }
		
		
       private void timerUpdateUI_Tick(object sender, EventArgs e)
        {



            if (_apm.DiagnosticLogMessage.Count > 0)
            {
                textBoxDiagnosticLog.AppendText(_apm.DiagnosticLogMessage.myDequeue());
            }

            if (_apm.DiagnosticLogTodo.Count > 0)
            {
                textBoxTodo.AppendText(_apm.DiagnosticLogTodo.myDequeue());
            }

            if (_apm.DiagnosticLogProtocol.Count > 0)
            {
                textBoxProtocol.AppendText(_apm.DiagnosticLogProtocol.myDequeue());
                textBoxProtocol.BackColor = Color.Pink;
                tabControlLogs.SelectTab(tabPageProtocol);
            }


           if (_apm.DiagnosticLogPanic.Count > 0)
            {
                textBoxPanics.AppendText(_apm.DiagnosticLogPanic.myDequeue());
                textBoxPanics.BackColor = Color.Pink;
                tabControlLogs.SelectTab(tabPageErrors);
            }


            // Update the message log treeview

            PacketLog.TreeViewUpdate(_apm, bnm, treeViewMessageLog, checkBoxLogMessages.Checked );

        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }




		
        private void buttonClearLog_Click(object sender, EventArgs e)
        {
            while (treeViewMessageLog.Nodes.Count > 0)
            {
                treeViewMessageLog.Nodes.RemoveAt(0);
            }
        }

		
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox( this );

            ab.Show();
        }

        private void buttonClearDiag_Click(object sender, EventArgs e)
        {
            textBoxDiagnosticLog.Clear();
        }


        private void backgroundWorkerApp_DoWork(object sender, DoWorkEventArgs e)
        {
            BACnetAppTask bt = new BACnetAppTask(_apm, bnm);

            bt.ServerApplication();
        }
		
		
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("http://www.bac-test.com");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("http://www.bacnetwiki.com");
        }

        private void linkLabelProgramWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://sourceforge.net/projects/bacnetserver/");
        }

    }
}
