namespace BACnet_Server
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonQuit = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.treeViewMessageLog = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDiagnosticLog = new System.Windows.Forms.TextBox();
            this.backgroundWorkerApp = new System.ComponentModel.BackgroundWorker();
            this.buttonClearDiag = new System.Windows.Forms.Button();
            this.tabControlLogs = new System.Windows.Forms.TabControl();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.tabPageConfig = new System.Windows.Forms.TabPage();
            this.textBoxConfigChanges = new System.Windows.Forms.TextBox();
            this.tabPageErrors = new System.Windows.Forms.TabPage();
            this.textBoxPanics = new System.Windows.Forms.TextBox();
            this.tabPageProtocol = new System.Windows.Forms.TabPage();
            this.textBoxProtocol = new System.Windows.Forms.TextBox();
            this.tabPageTodo = new System.Windows.Forms.TabPage();
            this.textBoxTodo = new System.Windows.Forms.TextBox();
            this.checkBoxLogMessages = new System.Windows.Forms.CheckBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.menuStrip1.SuspendLayout();
            this.tabControlLogs.SuspendLayout();
            this.tabPageLog.SuspendLayout();
            this.tabPageConfig.SuspendLayout();
            this.tabPageErrors.SuspendLayout();
            this.tabPageProtocol.SuspendLayout();
            this.tabPageTodo.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonQuit
            // 
            this.buttonQuit.Location = new System.Drawing.Point(861, 331);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(75, 23);
            this.buttonQuit.TabIndex = 5;
            this.buttonQuit.Text = "Quit";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(388, 350);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(95, 13);
            this.linkLabel1.TabIndex = 0;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "www.bac-test.com";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1020, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // timerUpdateUI
            // 
            this.timerUpdateUI.Enabled = true;
            this.timerUpdateUI.Tick += new System.EventHandler(this.timerUpdateUI_Tick);
            // 
            // treeViewMessageLog
            // 
            this.treeViewMessageLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeViewMessageLog.Location = new System.Drawing.Point(26, 41);
            this.treeViewMessageLog.Name = "treeViewMessageLog";
            this.treeViewMessageLog.Size = new System.Drawing.Size(488, 284);
            this.treeViewMessageLog.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(356, 351);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Visit";
            // 
            // textBoxDiagnosticLog
            // 
            this.textBoxDiagnosticLog.Location = new System.Drawing.Point(0, 0);
            this.textBoxDiagnosticLog.Multiline = true;
            this.textBoxDiagnosticLog.Name = "textBoxDiagnosticLog";
            this.textBoxDiagnosticLog.Size = new System.Drawing.Size(400, 711);
            this.textBoxDiagnosticLog.TabIndex = 0;
            // 
            // buttonClearDiag
            // 
            this.buttonClearDiag.Location = new System.Drawing.Point(158, 727);
            this.buttonClearDiag.Name = "buttonClearDiag";
            this.buttonClearDiag.Size = new System.Drawing.Size(75, 23);
            this.buttonClearDiag.TabIndex = 17;
            this.buttonClearDiag.Text = "Clear";
            this.buttonClearDiag.UseVisualStyleBackColor = true;
            this.buttonClearDiag.Click += new System.EventHandler(this.buttonClearDiag_Click);
            // 
            // tabControlLogs
            // 
            this.tabControlLogs.Controls.Add(this.tabPageLog);
            this.tabControlLogs.Controls.Add(this.tabPageConfig);
            this.tabControlLogs.Controls.Add(this.tabPageErrors);
            this.tabControlLogs.Controls.Add(this.tabPageProtocol);
            this.tabControlLogs.Controls.Add(this.tabPageTodo);
            this.tabControlLogs.Location = new System.Drawing.Point(537, 41);
            this.tabControlLogs.Name = "tabControlLogs";
            this.tabControlLogs.SelectedIndex = 0;
            this.tabControlLogs.Size = new System.Drawing.Size(412, 284);
            this.tabControlLogs.TabIndex = 19;
            // 
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.textBoxDiagnosticLog);
            this.tabPageLog.Controls.Add(this.buttonClearDiag);
            this.tabPageLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLog.Size = new System.Drawing.Size(404, 258);
            this.tabPageLog.TabIndex = 0;
            this.tabPageLog.Text = "Log";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // tabPageConfig
            // 
            this.tabPageConfig.Controls.Add(this.textBoxConfigChanges);
            this.tabPageConfig.Location = new System.Drawing.Point(4, 22);
            this.tabPageConfig.Name = "tabPageConfig";
            this.tabPageConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConfig.Size = new System.Drawing.Size(395, 258);
            this.tabPageConfig.TabIndex = 1;
            this.tabPageConfig.Text = "Config Changes";
            this.tabPageConfig.UseVisualStyleBackColor = true;
            // 
            // textBoxConfigChanges
            // 
            this.textBoxConfigChanges.Location = new System.Drawing.Point(0, 0);
            this.textBoxConfigChanges.Multiline = true;
            this.textBoxConfigChanges.Name = "textBoxConfigChanges";
            this.textBoxConfigChanges.Size = new System.Drawing.Size(396, 711);
            this.textBoxConfigChanges.TabIndex = 0;
            // 
            // tabPageErrors
            // 
            this.tabPageErrors.Controls.Add(this.textBoxPanics);
            this.tabPageErrors.Location = new System.Drawing.Point(4, 22);
            this.tabPageErrors.Name = "tabPageErrors";
            this.tabPageErrors.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageErrors.Size = new System.Drawing.Size(395, 258);
            this.tabPageErrors.TabIndex = 2;
            this.tabPageErrors.Text = "Errors";
            this.tabPageErrors.UseVisualStyleBackColor = true;
            // 
            // textBoxPanics
            // 
            this.textBoxPanics.Location = new System.Drawing.Point(0, 0);
            this.textBoxPanics.Multiline = true;
            this.textBoxPanics.Name = "textBoxPanics";
            this.textBoxPanics.Size = new System.Drawing.Size(396, 711);
            this.textBoxPanics.TabIndex = 0;
            // 
            // tabPageProtocol
            // 
            this.tabPageProtocol.Controls.Add(this.textBoxProtocol);
            this.tabPageProtocol.Location = new System.Drawing.Point(4, 22);
            this.tabPageProtocol.Name = "tabPageProtocol";
            this.tabPageProtocol.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageProtocol.Size = new System.Drawing.Size(395, 258);
            this.tabPageProtocol.TabIndex = 3;
            this.tabPageProtocol.Text = "Protocol";
            this.tabPageProtocol.UseVisualStyleBackColor = true;
            // 
            // textBoxProtocol
            // 
            this.textBoxProtocol.Location = new System.Drawing.Point(0, 0);
            this.textBoxProtocol.Multiline = true;
            this.textBoxProtocol.Name = "textBoxProtocol";
            this.textBoxProtocol.Size = new System.Drawing.Size(396, 721);
            this.textBoxProtocol.TabIndex = 0;
            // 
            // tabPageTodo
            // 
            this.tabPageTodo.Controls.Add(this.textBoxTodo);
            this.tabPageTodo.Location = new System.Drawing.Point(4, 22);
            this.tabPageTodo.Name = "tabPageTodo";
            this.tabPageTodo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTodo.Size = new System.Drawing.Size(395, 258);
            this.tabPageTodo.TabIndex = 4;
            this.tabPageTodo.Text = "Todo";
            this.tabPageTodo.UseVisualStyleBackColor = true;
            // 
            // textBoxTodo
            // 
            this.textBoxTodo.Location = new System.Drawing.Point(0, 0);
            this.textBoxTodo.Multiline = true;
            this.textBoxTodo.Name = "textBoxTodo";
            this.textBoxTodo.Size = new System.Drawing.Size(400, 722);
            this.textBoxTodo.TabIndex = 0;
            // 
            // checkBoxLogMessages
            // 
            this.checkBoxLogMessages.AutoSize = true;
            this.checkBoxLogMessages.Checked = true;
            this.checkBoxLogMessages.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLogMessages.Location = new System.Drawing.Point(26, 350);
            this.checkBoxLogMessages.Name = "checkBoxLogMessages";
            this.checkBoxLogMessages.Size = new System.Drawing.Size(95, 17);
            this.checkBoxLogMessages.TabIndex = 20;
            this.checkBoxLogMessages.Text = "Log Messages";
            this.checkBoxLogMessages.UseVisualStyleBackColor = true;
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(388, 372);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(108, 13);
            this.linkLabel2.TabIndex = 21;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "www.bacnetwiki.com";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 414);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.checkBoxLogMessages);
            this.Controls.Add(this.tabControlLogs);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeViewMessageLog);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.buttonQuit);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "BACnet Server";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControlLogs.ResumeLayout(false);
            this.tabPageLog.ResumeLayout(false);
            this.tabPageLog.PerformLayout();
            this.tabPageConfig.ResumeLayout(false);
            this.tabPageConfig.PerformLayout();
            this.tabPageErrors.ResumeLayout(false);
            this.tabPageErrors.PerformLayout();
            this.tabPageProtocol.ResumeLayout(false);
            this.tabPageProtocol.PerformLayout();
            this.tabPageTodo.ResumeLayout(false);
            this.tabPageTodo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonQuit;
        private System.Windows.Forms.Timer timerUpdateUI;
        private System.Windows.Forms.TreeView treeViewMessageLog;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDiagnosticLog;
        private System.ComponentModel.BackgroundWorker backgroundWorkerApp;
        private System.Windows.Forms.Button buttonClearDiag;
        private System.Windows.Forms.TabControl tabControlLogs;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.TabPage tabPageConfig;
        private System.Windows.Forms.TabPage tabPageErrors;
        private System.Windows.Forms.TextBox textBoxConfigChanges;
        private System.Windows.Forms.TextBox textBoxPanics;
        private System.Windows.Forms.CheckBox checkBoxLogMessages;
        private System.Windows.Forms.TabPage tabPageProtocol;
        private System.Windows.Forms.TextBox textBoxProtocol;
        private System.Windows.Forms.TabPage tabPageTodo;
        private System.Windows.Forms.TextBox textBoxTodo;
        private System.Windows.Forms.LinkLabel linkLabel2;
    }
}

