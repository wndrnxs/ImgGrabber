
namespace ImgGrabber
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.groupBoxGrabber = new System.Windows.Forms.GroupBox();
            this.panelChildImages = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBoxLog = new System.Windows.Forms.GroupBox();
            this.LogMessage = new System.Windows.Forms.RichTextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.labelReleaseVersion = new System.Windows.Forms.Label();
            this.groupBoxStartTrigger = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.labelTrigger = new System.Windows.Forms.Label();
            this.groupBoxPCMonitor = new System.Windows.Forms.GroupBox();
            this.labelDDrive = new System.Windows.Forms.Label();
            this.labelCDrive = new System.Windows.Forms.Label();
            this.progressBarDDrive = new System.Windows.Forms.ProgressBar();
            this.progressBarCDrive = new System.Windows.Forms.ProgressBar();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelMemory = new System.Windows.Forms.Label();
            this.labelCPU = new System.Windows.Forms.Label();
            this.groupBoxProcess = new System.Windows.Forms.GroupBox();
            this.labelProcessAlarm = new System.Windows.Forms.Label();
            this.labelProcessEnd = new System.Windows.Forms.Label();
            this.labelProcessGrab = new System.Windows.Forms.Label();
            this.labelProcessReady = new System.Windows.Forms.Label();
            this.groupBoxLightTemp = new System.Windows.Forms.GroupBox();
            this.labelLightState = new System.Windows.Forms.Label();
            this.labelPortConnected = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBoxComm = new System.Windows.Forms.GroupBox();
            this.labelClientConnected = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxAuthority = new System.Windows.Forms.GroupBox();
            this.buttonLevelChange = new System.Windows.Forms.Button();
            this.labelAuthority = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.tabPageSetup = new System.Windows.Forms.TabPage();
            this.panelSetup = new System.Windows.Forms.Panel();
            this.tabControlMain.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.groupBoxGrabber.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBoxLog.SuspendLayout();
            this.panel4.SuspendLayout();
            this.groupBoxStartTrigger.SuspendLayout();
            this.groupBoxPCMonitor.SuspendLayout();
            this.groupBoxProcess.SuspendLayout();
            this.groupBoxLightTemp.SuspendLayout();
            this.groupBoxComm.SuspendLayout();
            this.groupBoxAuthority.SuspendLayout();
            this.tabPageSetup.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageMain);
            this.tabControlMain.Controls.Add(this.tabPageSetup);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabControlMain.ItemSize = new System.Drawing.Size(50, 25);
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1255, 891);
            this.tabControlMain.TabIndex = 432;
            this.tabControlMain.SelectedIndexChanged += new System.EventHandler(this.tabControlMain_SelectedIndexChanged);
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.groupBoxGrabber);
            this.tabPageMain.Controls.Add(this.panel3);
            this.tabPageMain.Location = new System.Drawing.Point(4, 29);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(1247, 858);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Main";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // groupBoxGrabber
            // 
            this.groupBoxGrabber.Controls.Add(this.panelChildImages);
            this.groupBoxGrabber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxGrabber.Location = new System.Drawing.Point(3, 3);
            this.groupBoxGrabber.Name = "groupBoxGrabber";
            this.groupBoxGrabber.Size = new System.Drawing.Size(880, 852);
            this.groupBoxGrabber.TabIndex = 459;
            this.groupBoxGrabber.TabStop = false;
            this.groupBoxGrabber.Text = "Grab Image";
            this.groupBoxGrabber.Resize += new System.EventHandler(this.groupBoxGrabber_Resize);
            // 
            // panelChildImages
            // 
            this.panelChildImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelChildImages.Location = new System.Drawing.Point(3, 23);
            this.panelChildImages.Name = "panelChildImages";
            this.panelChildImages.Size = new System.Drawing.Size(874, 826);
            this.panelChildImages.TabIndex = 453;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.groupBoxLog);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Controls.Add(this.groupBoxStartTrigger);
            this.panel3.Controls.Add(this.groupBoxPCMonitor);
            this.panel3.Controls.Add(this.groupBoxProcess);
            this.panel3.Controls.Add(this.groupBoxLightTemp);
            this.panel3.Controls.Add(this.groupBoxComm);
            this.panel3.Controls.Add(this.groupBoxAuthority);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(883, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(361, 852);
            this.panel3.TabIndex = 458;
            // 
            // groupBoxLog
            // 
            this.groupBoxLog.Controls.Add(this.LogMessage);
            this.groupBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxLog.Location = new System.Drawing.Point(0, 502);
            this.groupBoxLog.Name = "groupBoxLog";
            this.groupBoxLog.Size = new System.Drawing.Size(361, 327);
            this.groupBoxLog.TabIndex = 467;
            this.groupBoxLog.TabStop = false;
            this.groupBoxLog.Text = "Log";
            // 
            // LogMessage
            // 
            this.LogMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogMessage.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogMessage.Location = new System.Drawing.Point(3, 23);
            this.LogMessage.Name = "LogMessage";
            this.LogMessage.Size = new System.Drawing.Size(355, 301);
            this.LogMessage.TabIndex = 442;
            this.LogMessage.Text = "";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.labelReleaseVersion);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 829);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(361, 23);
            this.panel4.TabIndex = 466;
            // 
            // labelReleaseVersion
            // 
            this.labelReleaseVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelReleaseVersion.AutoSize = true;
            this.labelReleaseVersion.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelReleaseVersion.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.labelReleaseVersion.Location = new System.Drawing.Point(210, 4);
            this.labelReleaseVersion.Name = "labelReleaseVersion";
            this.labelReleaseVersion.Size = new System.Drawing.Size(150, 15);
            this.labelReleaseVersion.TabIndex = 459;
            this.labelReleaseVersion.Text = "Ver. yyyy.MMdd.HHmm.000";
            this.labelReleaseVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxStartTrigger
            // 
            this.groupBoxStartTrigger.Controls.Add(this.label11);
            this.groupBoxStartTrigger.Controls.Add(this.labelTrigger);
            this.groupBoxStartTrigger.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxStartTrigger.Location = new System.Drawing.Point(0, 454);
            this.groupBoxStartTrigger.Name = "groupBoxStartTrigger";
            this.groupBoxStartTrigger.Size = new System.Drawing.Size(361, 48);
            this.groupBoxStartTrigger.TabIndex = 464;
            this.groupBoxStartTrigger.TabStop = false;
            this.groupBoxStartTrigger.Text = "Start Trigger";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(121, 17);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(59, 17);
            this.label11.TabIndex = 454;
            this.label11.Text = "Sensor:";
            // 
            // labelTrigger
            // 
            this.labelTrigger.BackColor = System.Drawing.Color.White;
            this.labelTrigger.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelTrigger.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTrigger.Location = new System.Drawing.Point(206, 17);
            this.labelTrigger.Name = "labelTrigger";
            this.labelTrigger.Size = new System.Drawing.Size(142, 21);
            this.labelTrigger.TabIndex = 453;
            this.labelTrigger.Text = "On";
            this.labelTrigger.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxPCMonitor
            // 
            this.groupBoxPCMonitor.Controls.Add(this.labelDDrive);
            this.groupBoxPCMonitor.Controls.Add(this.labelCDrive);
            this.groupBoxPCMonitor.Controls.Add(this.progressBarDDrive);
            this.groupBoxPCMonitor.Controls.Add(this.progressBarCDrive);
            this.groupBoxPCMonitor.Controls.Add(this.label7);
            this.groupBoxPCMonitor.Controls.Add(this.label6);
            this.groupBoxPCMonitor.Controls.Add(this.label5);
            this.groupBoxPCMonitor.Controls.Add(this.label4);
            this.groupBoxPCMonitor.Controls.Add(this.labelMemory);
            this.groupBoxPCMonitor.Controls.Add(this.labelCPU);
            this.groupBoxPCMonitor.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxPCMonitor.Location = new System.Drawing.Point(0, 332);
            this.groupBoxPCMonitor.Name = "groupBoxPCMonitor";
            this.groupBoxPCMonitor.Size = new System.Drawing.Size(361, 122);
            this.groupBoxPCMonitor.TabIndex = 463;
            this.groupBoxPCMonitor.TabStop = false;
            this.groupBoxPCMonitor.Text = "PC Monitor";
            // 
            // labelDDrive
            // 
            this.labelDDrive.AutoSize = true;
            this.labelDDrive.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDDrive.Location = new System.Drawing.Point(175, 91);
            this.labelDDrive.Name = "labelDDrive";
            this.labelDDrive.Size = new System.Drawing.Size(68, 17);
            this.labelDDrive.TabIndex = 447;
            this.labelDDrive.Text = "Not Used";
            // 
            // labelCDrive
            // 
            this.labelCDrive.AutoSize = true;
            this.labelCDrive.BackColor = System.Drawing.Color.Transparent;
            this.labelCDrive.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCDrive.Location = new System.Drawing.Point(175, 63);
            this.labelCDrive.Name = "labelCDrive";
            this.labelCDrive.Size = new System.Drawing.Size(68, 17);
            this.labelCDrive.TabIndex = 446;
            this.labelCDrive.Text = "Not Used";
            // 
            // progressBarDDrive
            // 
            this.progressBarDDrive.Location = new System.Drawing.Point(58, 88);
            this.progressBarDDrive.Name = "progressBarDDrive";
            this.progressBarDDrive.Size = new System.Drawing.Size(287, 23);
            this.progressBarDDrive.TabIndex = 440;
            // 
            // progressBarCDrive
            // 
            this.progressBarCDrive.Location = new System.Drawing.Point(58, 60);
            this.progressBarCDrive.Name = "progressBarCDrive";
            this.progressBarCDrive.Size = new System.Drawing.Size(287, 22);
            this.progressBarCDrive.TabIndex = 439;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(23, 89);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(27, 17);
            this.label7.TabIndex = 438;
            this.label7.Text = "D :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(23, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 17);
            this.label6.TabIndex = 437;
            this.label6.Text = "C :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(178, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 17);
            this.label5.TabIndex = 436;
            this.label5.Text = "Memory :";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(27, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 17);
            this.label4.TabIndex = 435;
            this.label4.Text = "CPU :";
            // 
            // labelMemory
            // 
            this.labelMemory.BackColor = System.Drawing.Color.White;
            this.labelMemory.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMemory.Location = new System.Drawing.Point(252, 25);
            this.labelMemory.Name = "labelMemory";
            this.labelMemory.Size = new System.Drawing.Size(92, 29);
            this.labelMemory.TabIndex = 434;
            this.labelMemory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelCPU
            // 
            this.labelCPU.BackColor = System.Drawing.Color.White;
            this.labelCPU.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCPU.Location = new System.Drawing.Point(80, 25);
            this.labelCPU.Name = "labelCPU";
            this.labelCPU.Size = new System.Drawing.Size(92, 29);
            this.labelCPU.TabIndex = 433;
            this.labelCPU.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxProcess
            // 
            this.groupBoxProcess.Controls.Add(this.labelProcessAlarm);
            this.groupBoxProcess.Controls.Add(this.labelProcessEnd);
            this.groupBoxProcess.Controls.Add(this.labelProcessGrab);
            this.groupBoxProcess.Controls.Add(this.labelProcessReady);
            this.groupBoxProcess.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxProcess.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxProcess.Location = new System.Drawing.Point(0, 259);
            this.groupBoxProcess.Name = "groupBoxProcess";
            this.groupBoxProcess.Size = new System.Drawing.Size(361, 73);
            this.groupBoxProcess.TabIndex = 462;
            this.groupBoxProcess.TabStop = false;
            this.groupBoxProcess.Text = "Process";
            // 
            // labelProcessAlarm
            // 
            this.labelProcessAlarm.BackColor = System.Drawing.Color.White;
            this.labelProcessAlarm.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelProcessAlarm.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProcessAlarm.Location = new System.Drawing.Point(282, 21);
            this.labelProcessAlarm.Name = "labelProcessAlarm";
            this.labelProcessAlarm.Size = new System.Drawing.Size(74, 40);
            this.labelProcessAlarm.TabIndex = 430;
            this.labelProcessAlarm.Text = "Alarm";
            this.labelProcessAlarm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelProcessAlarm.Click += new System.EventHandler(this.labelProcessAlarm_Click);
            // 
            // labelProcessEnd
            // 
            this.labelProcessEnd.BackColor = System.Drawing.Color.White;
            this.labelProcessEnd.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelProcessEnd.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProcessEnd.Location = new System.Drawing.Point(188, 21);
            this.labelProcessEnd.Name = "labelProcessEnd";
            this.labelProcessEnd.Size = new System.Drawing.Size(80, 40);
            this.labelProcessEnd.TabIndex = 429;
            this.labelProcessEnd.Text = "End";
            this.labelProcessEnd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelProcessGrab
            // 
            this.labelProcessGrab.BackColor = System.Drawing.Color.White;
            this.labelProcessGrab.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelProcessGrab.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProcessGrab.Location = new System.Drawing.Point(100, 21);
            this.labelProcessGrab.Name = "labelProcessGrab";
            this.labelProcessGrab.Size = new System.Drawing.Size(74, 40);
            this.labelProcessGrab.TabIndex = 428;
            this.labelProcessGrab.Text = "Grab";
            this.labelProcessGrab.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelProcessReady
            // 
            this.labelProcessReady.BackColor = System.Drawing.Color.White;
            this.labelProcessReady.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelProcessReady.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProcessReady.Location = new System.Drawing.Point(6, 21);
            this.labelProcessReady.Name = "labelProcessReady";
            this.labelProcessReady.Size = new System.Drawing.Size(80, 40);
            this.labelProcessReady.TabIndex = 427;
            this.labelProcessReady.Text = "Ready";
            this.labelProcessReady.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxLightTemp
            // 
            this.groupBoxLightTemp.Controls.Add(this.labelLightState);
            this.groupBoxLightTemp.Controls.Add(this.labelPortConnected);
            this.groupBoxLightTemp.Controls.Add(this.label12);
            this.groupBoxLightTemp.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxLightTemp.Location = new System.Drawing.Point(0, 134);
            this.groupBoxLightTemp.Name = "groupBoxLightTemp";
            this.groupBoxLightTemp.Size = new System.Drawing.Size(361, 125);
            this.groupBoxLightTemp.TabIndex = 461;
            this.groupBoxLightTemp.TabStop = false;
            this.groupBoxLightTemp.Text = "Light";
            // 
            // labelLightState
            // 
            this.labelLightState.BackColor = System.Drawing.Color.White;
            this.labelLightState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelLightState.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLightState.Location = new System.Drawing.Point(23, 61);
            this.labelLightState.Name = "labelLightState";
            this.labelLightState.Size = new System.Drawing.Size(322, 50);
            this.labelLightState.TabIndex = 25;
            // 
            // labelPortConnected
            // 
            this.labelPortConnected.BackColor = System.Drawing.Color.LightGray;
            this.labelPortConnected.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPortConnected.Location = new System.Drawing.Point(216, 22);
            this.labelPortConnected.Name = "labelPortConnected";
            this.labelPortConnected.Size = new System.Drawing.Size(133, 29);
            this.labelPortConnected.TabIndex = 23;
            this.labelPortConnected.Text = "Disconnected";
            this.labelPortConnected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(20, 28);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(117, 17);
            this.label12.TabIndex = 22;
            this.label12.Text = "Port Connection:";
            // 
            // groupBoxComm
            // 
            this.groupBoxComm.Controls.Add(this.labelClientConnected);
            this.groupBoxComm.Controls.Add(this.label1);
            this.groupBoxComm.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxComm.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxComm.Location = new System.Drawing.Point(0, 67);
            this.groupBoxComm.Name = "groupBoxComm";
            this.groupBoxComm.Size = new System.Drawing.Size(361, 67);
            this.groupBoxComm.TabIndex = 460;
            this.groupBoxComm.TabStop = false;
            this.groupBoxComm.Text = "Communication";
            // 
            // labelClientConnected
            // 
            this.labelClientConnected.BackColor = System.Drawing.Color.LightGray;
            this.labelClientConnected.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelClientConnected.Location = new System.Drawing.Point(217, 27);
            this.labelClientConnected.Name = "labelClientConnected";
            this.labelClientConnected.Size = new System.Drawing.Size(133, 29);
            this.labelClientConnected.TabIndex = 20;
            this.labelClientConnected.Text = "Disconnected";
            this.labelClientConnected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(21, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "AMO Connection :";
            // 
            // groupBoxAuthority
            // 
            this.groupBoxAuthority.Controls.Add(this.buttonLevelChange);
            this.groupBoxAuthority.Controls.Add(this.labelAuthority);
            this.groupBoxAuthority.Controls.Add(this.label15);
            this.groupBoxAuthority.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxAuthority.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxAuthority.Location = new System.Drawing.Point(0, 0);
            this.groupBoxAuthority.Name = "groupBoxAuthority";
            this.groupBoxAuthority.Size = new System.Drawing.Size(361, 67);
            this.groupBoxAuthority.TabIndex = 459;
            this.groupBoxAuthority.TabStop = false;
            this.groupBoxAuthority.Text = "Authority";
            // 
            // buttonLevelChange
            // 
            this.buttonLevelChange.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLevelChange.Location = new System.Drawing.Point(261, 20);
            this.buttonLevelChange.Name = "buttonLevelChange";
            this.buttonLevelChange.Size = new System.Drawing.Size(89, 35);
            this.buttonLevelChange.TabIndex = 22;
            this.buttonLevelChange.Text = "Change";
            this.buttonLevelChange.UseVisualStyleBackColor = true;
            this.buttonLevelChange.Click += new System.EventHandler(this.buttonLevelChange_Click);
            // 
            // labelAuthority
            // 
            this.labelAuthority.BackColor = System.Drawing.Color.LightYellow;
            this.labelAuthority.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAuthority.Location = new System.Drawing.Point(88, 23);
            this.labelAuthority.Name = "labelAuthority";
            this.labelAuthority.Size = new System.Drawing.Size(163, 29);
            this.labelAuthority.TabIndex = 21;
            this.labelAuthority.Text = "Operator";
            this.labelAuthority.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelAuthority.DoubleClick += new System.EventHandler(this.labelAuthority_DoubleClick);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(27, 29);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(50, 17);
            this.label15.TabIndex = 4;
            this.label15.Text = "Level :";
            // 
            // tabPageSetup
            // 
            this.tabPageSetup.Controls.Add(this.panelSetup);
            this.tabPageSetup.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabPageSetup.Location = new System.Drawing.Point(4, 29);
            this.tabPageSetup.Name = "tabPageSetup";
            this.tabPageSetup.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSetup.Size = new System.Drawing.Size(1247, 858);
            this.tabPageSetup.TabIndex = 1;
            this.tabPageSetup.Text = "Setup";
            this.tabPageSetup.UseVisualStyleBackColor = true;
            // 
            // panelSetup
            // 
            this.panelSetup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSetup.Location = new System.Drawing.Point(3, 3);
            this.panelSetup.Name = "panelSetup";
            this.panelSetup.Size = new System.Drawing.Size(1241, 852);
            this.panelSetup.TabIndex = 1;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1255, 891);
            this.Controls.Add(this.tabControlMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ImageGrabber";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.groupBoxGrabber.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBoxLog.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.groupBoxStartTrigger.ResumeLayout(false);
            this.groupBoxStartTrigger.PerformLayout();
            this.groupBoxPCMonitor.ResumeLayout(false);
            this.groupBoxPCMonitor.PerformLayout();
            this.groupBoxProcess.ResumeLayout(false);
            this.groupBoxLightTemp.ResumeLayout(false);
            this.groupBoxLightTemp.PerformLayout();
            this.groupBoxComm.ResumeLayout(false);
            this.groupBoxComm.PerformLayout();
            this.groupBoxAuthority.ResumeLayout(false);
            this.groupBoxAuthority.PerformLayout();
            this.tabPageSetup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.TabPage tabPageSetup;
        private System.Windows.Forms.Panel panelSetup;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label labelReleaseVersion;
        private System.Windows.Forms.GroupBox groupBoxStartTrigger;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label labelTrigger;
        private System.Windows.Forms.GroupBox groupBoxPCMonitor;
        private System.Windows.Forms.Label labelDDrive;
        private System.Windows.Forms.Label labelCDrive;
        private System.Windows.Forms.ProgressBar progressBarDDrive;
        private System.Windows.Forms.ProgressBar progressBarCDrive;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelMemory;
        private System.Windows.Forms.Label labelCPU;
        private System.Windows.Forms.GroupBox groupBoxProcess;
        private System.Windows.Forms.Label labelProcessAlarm;
        private System.Windows.Forms.Label labelProcessEnd;
        private System.Windows.Forms.Label labelProcessGrab;
        private System.Windows.Forms.Label labelProcessReady;
        private System.Windows.Forms.GroupBox groupBoxLightTemp;
        private System.Windows.Forms.Label labelPortConnected;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBoxComm;
        private System.Windows.Forms.Label labelClientConnected;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxAuthority;
        private System.Windows.Forms.Button buttonLevelChange;
        private System.Windows.Forms.Label labelAuthority;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.GroupBox groupBoxLog;
        private System.Windows.Forms.RichTextBox LogMessage;
        private System.Windows.Forms.GroupBox groupBoxGrabber;
        private System.Windows.Forms.Panel panelChildImages;
        private System.Windows.Forms.Label labelLightState;
    }
}