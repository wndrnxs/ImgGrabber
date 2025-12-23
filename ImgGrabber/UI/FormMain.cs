using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace ImgGrabber
{
    public partial class FormMain : Form
    {
        private ILightControl lightControl;
        private MilControl grabControl;
        private Comm comm;
        private System.Windows.Forms.Timer timerPerformance;
        private NetConnector networkChecker;
        private readonly IniFile iniFileSetup = new IniFile();
        private readonly IniFile iniFileUI = new IniFile();
        //private FormWating formWating = new FormWating();

        private Logger logger;
        private Logger loggerEMAS;

        private readonly List<ImageViewer> listImageViewer = new List<ImageViewer>();
        private readonly List<Panel> listPanel = new List<Panel>();


        private readonly FormAuthority formAuthority = new FormAuthority();
        private FormSetup formSetup;
        private readonly DeleteFiles deleteFiles = new DeleteFiles();
        private readonly Recipe recipe = new Recipe();

        private List<string> listCell_ID = new List<string>();
        private PerformanceCounter PerformCtrCPU;
        private readonly PerformanceCounter PerformCtrMem;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private System.Timers.Timer StartTimeoutTimer;
        private ProcessState State = ProcessState.Idle;

        /// <summary>
        /// 101 : 측면 시야각 검사기(AngleView)
        /// 102 : 정면 시야각 검사기(Chipping)
        /// </summary>
        private string versionCode = "101";

        private bool useLogging = true;
        private bool useEMASLogging = false;
        private bool useUIEvnetLogging = true;

        private bool useCommunication = true;
        private bool useImageShare = true;
        private bool useImageBackup = true;
        private bool useCheckSharedFolder = true;

        private bool useGrabber = true;
        private bool useStartTimeout = true;
        private bool useGrabberSimulation = false;
        private bool useCameraSet = true;
        private bool useExternalSignal = false;

        private bool useLight = true;
        private bool useLightCheckTemp = false;

        private string logPath;
        private string logPathEMAS;
        private string iniPath;
        private string imagePath;
        private string recipePath;
        private string imageBackupPath = @"D:\Backup\Image";
        private string imageSharePath;
        private string passwordPath = @"C:\ProgramData\KEOC\Authority";

        private string serverIP = "192.168.0.160";
        private int serverPort = 15001;
        private string clientIP = "192.168.0.100";
        private string clientAccount = "Admin";
        private string clientPassword = "am12#oled";
        private readonly int netID = 1; //Clinet 1개 고정

        private int lightComPortNo = 1;
        private int lightMaxValue = 5000;
        private int lightChannelCount = 2;
        private string lightControllerName = "Eroom";

        private string dcfFileName = "Linea_4K.dcf";
        private string grabberBoardName = "RADIENTEVCL";

        private int grabStartTimeoutDelay = 5000;
        private bool bFlagGrabStart;
        private int ffcTimeout_ms = 10000;

        private int maxDisplayLogLine = 50;

        private double backupImageSavingDate = 3d;

        private string recipeDefaultFileName = "base.ini";

        private double limitMemoryPercent = 80.0d;

        internal string UIChange(string controlName)
        {
            return iniFileUI.GetValue("UIChange", controlName, controlName);
        }

        private double remmain_physical_memory;
        private bool useMemoryLimit = true;


        public enum ProcessState
        {
            Idle,
            Ready,
            Grabbing,
            End,
            Alarm
        }

        public string CellDatas { get; internal set; }

        public FormMain()
        {
            InitializeComponent();
            //formWating.TopLevel = false;
            //Controls.Add(formWating);
            //formWating.Parent = this;
        }

        public void AddUILoggingEvents(Control parent)
        {
            if (parent.Controls.Count < 1)
            {
                return;
            }

            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button)
                {
                    (ctrl as Button).Click -= CustomLogging;
                    (ctrl as Button).Click += CustomLogging;
                }
                else if (ctrl is TextBox)
                {
                    (ctrl as TextBox).TextChanged -= CustomLogging;
                    (ctrl as TextBox).TextChanged += CustomLogging;
                }
                else
                {
                    AddUILoggingEvents(ctrl);
                }
            }
        }

        private void CustomLogging(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;

            string data = ctrl.Name; //default

            if (ctrl is Button button)
            {
                data = $"Button Click <<{button.Name}>>";
            }
            else if (ctrl is TextBox textBox)
            {
                if (!textBox.ReadOnly)
                {
                    data = $"TextBox Changed <<{textBox.Text}>>";
                }
            }

            Logging("[Event] " + data);
        }

        private DateTime getBuildDateTime()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return File.GetLastWriteTime(assembly.Location);
        }

        public void MainImageViewerCreate(Recipe recipe)
        {
            foreach (ImageViewer imgViewer in listImageViewer)
            {
                Controls.Remove(imgViewer);
                imgViewer.Dispose();
            }
            listImageViewer.Clear();

            foreach (Panel panel in listPanel)
            {
                Controls.Remove(panel);
                panel.Dispose();
            }
            listPanel.Clear();

            int imagecount = recipe.ListChildImageRoi.Count;

            for (int i = 0; i < imagecount; i++)
            {
                Panel panel = new Panel();
                listPanel.Add(panel);
                Controls.Add(panel);
                panel.Parent = panelChildImages;

                ImageViewer viewer = new ImageViewer();
                listImageViewer.Add(viewer);
                Controls.Add(viewer);
                
            }

            listPanel.Reverse();

            foreach(var panel in listPanel)
            {
                listImageViewer[listPanel.IndexOf(panel)].Parent = panel;
            }

            

            ResizeChildImageViewer(recipe);

        }


        private void FormMain_Load(object sender, EventArgs e)
        {
            //formWating.Show();

            ///  1. ini 값 Load
            Initialize();
            //formWating.Step(10);

            ///  2. 인스턴스 생성
            Create();
            //formWating.Step(20);

            recipe.Load($@"{recipePath}\{recipeDefaultFileName}");

            //MainImageViewerCreate(recipe);

            ///  3. 로깅 초기화
            if (useLogging)
            {
                logger.Open(logPath);
            }

            if (useEMASLogging)
            {
                loggerEMAS.Open(logPathEMAS);
                EMASLogging("logging", false);
            }
            //formWating.Step(30);

            ///  4. 통신 초기화
            if (useCommunication)
            {
                comm.OpenServer(netID, serverIP, serverPort);

                if (useImageShare)
                {
                    if (!CheckSharedState())
                    {
                        Process(ProcessState.Alarm);
                    }
                }
            }
            else
            {
                Logging("Communication is not used.");
                groupBoxComm.Visible = false;
            }

            //formWating.Step(40);

            ///  5. 그래버 초기화
            if (useGrabber)
            {
                grabControl.Initialize(dcfFileName, grabberBoardName);

                if (useStartTimeout)
                {
                    StartTimeoutTimer = new System.Timers.Timer
                    {
                        Interval = grabStartTimeoutDelay
                    };
                    StartTimeoutTimer.Elapsed += new ElapsedEventHandler(StartTimeoutTimer_Elapsed);
                }
            }
            else
            {
                grabControl.Simulation = true;
            }

            //formWating.Step(50);

            ///  6. 조명 초기화
            if (useLight)
            {
                lightControl.CheckTemp = useLightCheckTemp;
                lightControl.InitValue = recipe.ListLightValue;

                if (!lightControl.InitLight())
                {
                    Logging("Light not connected.");
                }
            }
            else
            {
                Logging("Light is not used.");
            }

            //formWating.Step(60);

            ///  7. 백업 파일 자동 제거 스레드 초기화
            if (useImageBackup)
            {
                DirectoryInfo di = new DirectoryInfo(imageBackupPath);
                if (!di.Exists)
                {
                    di.Create();
                }
                deleteFiles.Start(imageBackupPath, backupImageSavingDate);
            }

            if (!useExternalSignal)
            {
                groupBoxStartTrigger.Visible = false;
            }

            ///  8. 계정 설정 초기화
            formAuthority.Initialize(passwordPath);
            formAuthority.ChangeAuthorityEvent += new FormAuthority.ChangeAuthority(FormAuthority_ChangeAuthorityEvent);
            SetAuthority(formAuthority.GetLevel());

            //formWating.Step(70);

            ///  9. Setup창 설정
            formSetup.Init(grabControl, lightControl, recipe);

            //formWating.Step(90);

            ///  10. 프로그램 버전 표시
            string version = $"Ver. {getBuildDateTime():yyyy.MMdd.HHmm.}{versionCode}";
            labelReleaseVersion.Text = version;
            Logging(version);

            ///  11. UI 조작에 대한 로깅 기능 On
            if (useLogging && useUIEvnetLogging)
            {
                AddUILoggingEvents(formSetup);
            }

            //formWating.Step(100);
        }

        private void Initialize()
        {
            logPath = Directory.GetCurrentDirectory() + @"\Log";
            DirectoryInfo LogInfo = new DirectoryInfo(logPath);
            if (!LogInfo.Exists) { LogInfo.Create(); }

            logPathEMAS = Directory.GetCurrentDirectory() + @"\EMAS";
            DirectoryInfo LogInfoEMAS = new DirectoryInfo(logPathEMAS);
            if (!LogInfoEMAS.Exists) { LogInfoEMAS.Create(); }

            iniPath = Directory.GetCurrentDirectory() + @"\Init";
            DirectoryInfo IniInfo = new DirectoryInfo(iniPath);
            if (!IniInfo.Exists) { IniInfo.Create(); }

            imagePath = Directory.GetCurrentDirectory() + @"\Image";
            DirectoryInfo ImageInfo = new DirectoryInfo(imagePath);
            if (!ImageInfo.Exists) { ImageInfo.Create(); }

            recipePath = Directory.GetCurrentDirectory() + @"\Recipe";
            DirectoryInfo RecipeInfo = new DirectoryInfo(recipePath);
            if (!RecipeInfo.Exists) { RecipeInfo.Create(); }

            string setupPath = iniPath + @"\Setup.ini";
            if (!File.Exists(setupPath))
            {
                using (FileStream fs = new FileStream(setupPath, FileMode.Create))
                {
                    fs.Close();
                }
            }
            iniFileSetup.Load(setupPath);

            string UIPath = iniPath + @"\UI.ini";
            if (!File.Exists(UIPath))
            {
                using (FileStream fs = new FileStream(UIPath, FileMode.Create))
                {
                    fs.Close();
                }
            }
            iniFileUI.Load(UIPath);

            versionCode = iniFileSetup.GetValue("Version", "Code", versionCode);

            useLogging = iniFileSetup.GetValue("Use", "Logging", useLogging);
            useEMASLogging = iniFileSetup.GetValue("Use", "EMASLogging", useEMASLogging);
            useCommunication = iniFileSetup.GetValue("Use", "Communication", useCommunication);
            useGrabber = iniFileSetup.GetValue("Use", "Grabber", useGrabber);
            useLight = iniFileSetup.GetValue("Use", "Light", useLight);
            useImageShare = iniFileSetup.GetValue("Use", "ImageShare", useImageShare);
            useImageBackup = iniFileSetup.GetValue("Use", "ImageBackup", useImageBackup);
            useCheckSharedFolder = iniFileSetup.GetValue("Use", "CheckSharedFolder", useCheckSharedFolder);
            useStartTimeout = iniFileSetup.GetValue("Use", "StartTimeout", useStartTimeout);
            useGrabberSimulation = iniFileSetup.GetValue("Use", "GrabberSimulation", useGrabberSimulation);
            useCameraSet = iniFileSetup.GetValue("Use", "CameraSet", useCameraSet);
            useMemoryLimit = iniFileSetup.GetValue("Use", "MemoryLimit", useMemoryLimit);
            useExternalSignal = iniFileSetup.GetValue("Use", "GrabExternalSignal", useExternalSignal);
            useUIEvnetLogging = iniFileSetup.GetValue("Use", "UIEvnetLogging", useUIEvnetLogging);
            useLightCheckTemp = iniFileSetup.GetValue("Use", "LightCheckTemp", useLightCheckTemp);

            dcfFileName = iniFileSetup.GetValue("FrameGrabber", "DcfFileName", dcfFileName);
            grabberBoardName = iniFileSetup.GetValue("FrameGrabber", "BoardName", grabberBoardName);

            serverIP = iniFileSetup.GetValue("Server", "IP", serverIP);
            serverPort = iniFileSetup.GetValue("Server", "Port", serverPort);

            clientIP = iniFileSetup.GetValue("Client", "IP", clientIP);
            clientAccount = iniFileSetup.GetValue("Client", "Account", clientAccount);
            clientPassword = iniFileSetup.GetValue("Client", "Password", clientPassword);

            imageSharePath = $@"\\{clientIP}\Shared";
            imageBackupPath = iniFileSetup.GetValue("Path", "Backup", imageBackupPath);
            imageSharePath = iniFileSetup.GetValue("Path", "Share", imageSharePath);
            passwordPath = iniFileSetup.GetValue("Path", "Password", passwordPath);

            lightComPortNo = iniFileSetup.GetValue("Light", "PortNo", lightComPortNo);
            lightMaxValue = iniFileSetup.GetValue("Light", "MaxValue", lightMaxValue);
            lightChannelCount = iniFileSetup.GetValue("Light", "ChannelCount", lightChannelCount);
            lightControllerName = iniFileSetup.GetValue("Light", "Controller", lightControllerName);

            backupImageSavingDate = iniFileSetup.GetValue("Etc", "BackupImageSavingDate", backupImageSavingDate);
            grabStartTimeoutDelay = iniFileSetup.GetValue("Etc", "StartTimeoutDelay", grabStartTimeoutDelay);
            maxDisplayLogLine = iniFileSetup.GetValue("Etc", "MaxDisplayLogLine", maxDisplayLogLine);
            ffcTimeout_ms = iniFileSetup.GetValue("Etc", "FFCTimeout", ffcTimeout_ms);
            limitMemoryPercent = iniFileSetup.GetValue("Etc", "LimitMemoryPercent", limitMemoryPercent);

            recipeDefaultFileName = iniFileSetup.GetValue("Recipe", "Default", recipeDefaultFileName);
        }

        internal ImageViewer GetMainViewer(int index)
        {
            return listImageViewer[index];
        }

        private void Create()
        {
            PerformCtrCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            //PerformCtrMem = new PerformanceCounter("Memory", "Available MBytes");

            formSetup = new FormSetup(this)
            {
                TopLevel = false,
                Parent = panelSetup,
                Dock = DockStyle.Fill,
                ImagePath = imagePath,
                RecipePath = recipePath
            };
            formSetup.Show();

            switch (lightControllerName)
            {
                case "Eroom":
                default:
                    {
                        lightControl = new EroomLightCtrl();
                    }
                    break;

                case "Dawoo":
                    {
                        lightControl = new DawooLightCtrl();
                    }
                    break;

                case "VitFc":
                    {
                        lightControl = new VitFcLightCtrl();
                    }
                    break;
            }

            lightControl.Parent = this;
            lightControl.ComPortNo = lightComPortNo;
            lightControl.MaxValue = lightMaxValue;
            lightControl.LightChannelCount = lightChannelCount;
            lightControl.ReceiveEvent += new DelLightReceive(LightControl_RecieveEvent);
            lightControl.ConnectEvent += new DelLightConnect(LightControl_ConnectEvent);

            grabControl = new MilControl(this)
            {
                Simulation = useGrabberSimulation,
                FFCTimeout_ms = ffcTimeout_ms,
                UseFeature = useCameraSet,
                UseExternalSignal = useExternalSignal
            };

            grabControl.GrabStartEvent += new GrabStartDelegate(GrabStart);
            grabControl.GrabEndEvent += new GrabEndDelegate(GrabEnd);
            grabControl.TriggerEvent += new TriggerDelegate(TriggerEvent);

            comm = new Comm(this);
            comm.ConnectEvent += new Comm.ConnectDelegate(ClientConnected);

            networkChecker = new NetConnector();

            logger = new Logger(LogMessage)
            {
                MaxDisplayLine = maxDisplayLogLine
            };

            loggerEMAS = new Logger
            {
                DateDisplay = false,
                DeleteDay = 7,
                FilesExtension = "csv"
            };

            timerPerformance = new System.Windows.Forms.Timer
            {
                Interval = 5000,
                Enabled = true
            };

            timerPerformance.Tick += new EventHandler(TimerPerformance_Tick);
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerPerformance.Enabled = false;
            EMASLogging("logging", true);

            if (useCommunication)
            {
                comm.ConnectEvent -= new Comm.ConnectDelegate(ClientConnected);
                comm.Dispose();
            }

            if (useGrabber)
            {
                grabControl.GrabStartEvent -= new GrabStartDelegate(GrabStart);
                grabControl.GrabEndEvent -= new GrabEndDelegate(GrabEnd);
                grabControl.TriggerEvent -= new TriggerDelegate(TriggerEvent);
                grabControl.GrabStop();
                grabControl.Free();
            }

            if (useLight)
            {
                lightControl.ReceiveEvent -= new DelLightReceive(LightControl_RecieveEvent);
                lightControl.ConnectEvent -= new DelLightConnect(LightControl_ConnectEvent);
                lightControl.Dispose();
            }

            if (useLogging)
            {
                logger.Close();
            }

            if (useEMASLogging)
            {
                loggerEMAS.Close();
            }

            if (useImageBackup)
            {
                deleteFiles.Close();
            }

            timerPerformance.Tick -= new EventHandler(TimerPerformance_Tick);

            formAuthority.ChangeAuthorityEvent -= new FormAuthority.ChangeAuthority(FormAuthority_ChangeAuthorityEvent);
        }

        public void ResizeChildImageViewer(Recipe recipe)
        {
            int index = 0;
            int imagecount = listImageViewer.Count;
            if (imagecount == recipe.ListChildImageRoi.Count)
            {
                foreach (ROI roi in recipe.ListChildImageRoi)
                {
                    listImageViewer[index].SetSize(new Size(roi.Width, roi.Height));
                    if (recipe.ListChildImageRoi[index].Width > recipe.ListChildImageRoi[index].Height)
                    {
                        listPanel[index].Dock = DockStyle.Top;
                        listPanel[index].Height = panelChildImages.Height / imagecount;
                    }
                    else
                    {
                        listPanel[index].Dock = DockStyle.Left;
                        listPanel[index].Width = panelChildImages.Width / imagecount;
                    }

                    ResizeViewer(index, roi.Width, roi.Height, listPanel[index].Width, listPanel[index].Height);
                    index++;
                }
            }
            
        }

        private void ResizeViewer(int index, float childWidth, float childHeight, float panelWidth, float panelHeight)
        {
            float Scale;
            float compareChildXYScale = childHeight / (float)childWidth;
            float comparepanelXYScale = panelHeight / (float)panelWidth;
            ImageViewer viewer = listImageViewer[index];

            if (compareChildXYScale > comparepanelXYScale) //panel의 가로가 더 큼
            {
                Scale = childWidth / (float)childHeight;

                viewer.Height = (int)panelHeight;
                viewer.Width = (int)(panelHeight * Scale);
                viewer.Location = new Point((int)(panelWidth / 2f - viewer.Width / 2f), 0);
            }
            else
            {
                Scale = childHeight / (float)childWidth;

                viewer.Width = (int)panelWidth;
                viewer.Height = (int)(panelWidth * Scale);
                viewer.Location = new Point(0, (int)(panelHeight / 2f - viewer.Height / 2f));
            }
        }

        public void Logging(string msg)
        {
            if (useLogging)
            {
                if(logger != null)
                {
                    logger.Logging(DateTime.Now.ToString("HH:mm:ss.fff | ") + msg);
                }
            }
        }

        public void EMASLogging(string action, bool end, string cell_ID = "")
        {
            if (useEMASLogging)
            {
                string start_end = "START";
                action = action.ToUpper();

                if (end)
                {
                    start_end = "END";
                }

                loggerEMAS.Logging(DateTime.Now.ToString("MMdd_HHmm_ss.fff") + $",IN01,A,,{cell_ID},,,{action}=IN01=IN01={start_end}");
            }
        }

        public void Process(ProcessState state)
        {
            switch (state)
            {
                case ProcessState.Ready:
                    {
                        labelProcessReady.Invoke(new Action(() => { labelProcessReady.BackColor = Color.SteelBlue; }));
                        labelProcessGrab.Invoke(new Action(() => { labelProcessGrab.BackColor = Color.White; }));
                        labelProcessEnd.Invoke(new Action(() => { labelProcessEnd.BackColor = Color.White; }));
                        State = state;
                    }
                    break;
                case ProcessState.Grabbing:
                    {
                        labelProcessReady.Invoke(new Action(() => { labelProcessReady.BackColor = Color.White; }));
                        labelProcessGrab.Invoke(new Action(() => { labelProcessGrab.BackColor = Color.SteelBlue; }));
                        labelProcessEnd.Invoke(new Action(() => { labelProcessEnd.BackColor = Color.White; }));
                        State = state;
                    }
                    break;
                case ProcessState.End:
                    {
                        labelProcessReady.Invoke(new Action(() => { labelProcessReady.BackColor = Color.White; }));
                        labelProcessGrab.Invoke(new Action(() => { labelProcessGrab.BackColor = Color.White; }));
                        labelProcessEnd.Invoke(new Action(() => { labelProcessEnd.BackColor = Color.SteelBlue; }));
                        State = state;
                    }
                    break;
                case ProcessState.Alarm:
                    {
                        labelProcessAlarm.Invoke(new Action(() => { labelProcessAlarm.BackColor = Color.Red; }));
                        State = state;
                    }
                    break;
                case ProcessState.Idle:
                    {
                        labelProcessAlarm.Invoke(new Action(() => { labelProcessAlarm.BackColor = Color.White; }));
                        State = state;
                    }
                    break;

            }
        }

        private void FormAuthority_ChangeAuthorityEvent(FormAuthority.Level level)
        {
            Logging($"Chagne Authority [{level}]");
            SetAuthority(level);
        }

        private void SetAuthority(FormAuthority.Level level)
        {
            formSetup.SetAuthority(level);

            switch (level)
            {
                case FormAuthority.Level.Operator:
                    {
                        labelAuthority.Invoke(new Action(() =>
                        {
                            labelAuthority.Text = "Operator";
                            labelAuthority.BackColor = Color.LightYellow;
                        }));

                        if (tabControlMain.TabPages.Contains(tabPageSetup))
                        {
                            tabControlMain.TabPages.Remove(tabPageSetup);
                        }
                    }
                    break;
                case FormAuthority.Level.Maintenance:
                    {
                        labelAuthority.Invoke(new Action(() =>
                        {
                            labelAuthority.Text = "Maintenance";
                            labelAuthority.BackColor = Color.LightGreen;
                        }));

                        if (!tabControlMain.TabPages.Contains(tabPageSetup))
                        {
                            tabControlMain.TabPages.Add(tabPageSetup);
                        }
                    }
                    break;
                case FormAuthority.Level.Engineer:
                    {
                        labelAuthority.Invoke(new Action(() =>
                        {
                            labelAuthority.Text = "Engineer";
                            labelAuthority.BackColor = Color.LightBlue;
                        }));

                        if (!tabControlMain.TabPages.Contains(tabPageSetup))
                        {
                            tabControlMain.TabPages.Add(tabPageSetup);
                        }
                    }
                    break;
            }
        }     

        private void TriggerEvent(bool isOn)
        {
            if (isOn)
            {
                labelTrigger.Invoke(new Action(() =>
                {
                    labelTrigger.BackColor = Color.LimeGreen;
                    labelTrigger.Text = "On";
                }));
            }
            else
            {
                labelTrigger.Invoke(new Action(() =>
                {
                    labelTrigger.BackColor = Color.LightGray;
                    labelTrigger.Text = "Off";
                }));
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logging("Closing the program");

            FormAuthority.Level beforeLevel = formAuthority.GetLevel();
            formAuthority.SetLevel(FormAuthority.Level.Operator);

            formAuthority.ShowDialog();

            if (formAuthority.GetLevel() < FormAuthority.Level.Maintenance)
            {
                e.Cancel = true;
                formAuthority.SetLevel(beforeLevel);
                SetAuthority(beforeLevel);
            }

            Logging("Terminate the program");
        }

        internal bool CheckReady()
        {
            bool ret = true;

            for (int i = 1; i <= lightControl.LightChannelCount; i++)
            {
                if (lightControl.GetLightTemp(i) > 70)
                {
                    Logging($"Light Temperature Alarm! [{i}]");
                    Process(ProcessState.Alarm);
                }
                else
                {
                    Process(ProcessState.Idle);
                }
            }
            return ret;
        }

        private bool CheckSharedState()
        {
            try
            {
                Ping ping = new Ping();
                PingOptions options = new PingOptions
                {
                    DontFragment = true
                };

                string data = "ping test";
                byte[] buffer = ASCIIEncoding.ASCII.GetBytes(data);
                int timeout = 120;

                PingReply reply = ping.Send(IPAddress.Parse(clientIP), timeout, buffer, options);
                if (reply.Status != IPStatus.Success)
                {
                    Logging($"ping test fail [{reply.Status}]");
                    return false;
                }

                if (clientAccount.Length > 0)
                {
                    int result = networkChecker.TryConnectNetwork(imageSharePath, clientAccount, clientPassword);
                    if (result != 0)
                    {
                        Logging($"{imageSharePath} {clientAccount} {clientPassword}");
                        Logging($"The shared folder is not accessible. [Error code:{result}]");
                    }
                }

                DirectoryInfo ImageShareInfo = new DirectoryInfo(imageSharePath);
                if (!ImageShareInfo.Exists)
                {
                    Logging(imageSharePath);
                    Logging("The shared folder does not exist.");
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }

        }

        private void TimerPerformance_Tick(object sender, EventArgs e)
        {
            ReadCPUResource();
            ReadDriveStorage();
        }


        private void ReadCPUResource()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        double total_physical_memeory = double.Parse(mo["TotalVisibleMemorySize"].ToString());
                        total_physical_memeory /= 1024;
                        double free_physical_memeory = double.Parse(mo["FreePhysicalMemory"].ToString());
                        free_physical_memeory /= 1024;
                        remmain_physical_memory = (total_physical_memeory - free_physical_memeory) / total_physical_memeory * 100;
                        labelMemory.Text = string.Format("{0:F1}%", remmain_physical_memory);
                    }
                }

                if (PerformCtrCPU != null)
                {
                    labelCPU.Text = string.Format("{0:F1}%", PerformCtrCPU.NextValue());
                }

                if (useMemoryLimit)
                {
                    if (State == ProcessState.End || State == ProcessState.Idle)
                    {
                        if (remmain_physical_memory > limitMemoryPercent)
                        {
                            FreeMemory.ClearFileSystemCache(ClearStandbyCache: true);
                            FreeMemory.EmptyWorkingSetFunction();
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
            catch
            {
                return;
            }
        }
        private void ReadDriveStorage()
        {
            DriveInfo[] driveInfo = DriveInfo.GetDrives();
            foreach (DriveInfo drive in driveInfo)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    if (drive.Name.Contains("C"))
                    {
                        double TotalSize = drive.TotalSize;
                        if (TotalSize > 0)
                        {
                            double FreeSpace = drive.AvailableFreeSpace;

                            double ee = (TotalSize - FreeSpace) / TotalSize;
                            progressBarCDrive.Maximum = 100;
                            progressBarCDrive.Value = (int)(ee * 100);
                            labelCDrive.Text = string.Format("{0:F1}%", ee * 100);
                        }
                        else
                        {
                            progressBarCDrive.Enabled = false;
                        }
                    }
                    else if (drive.Name.Contains("D"))
                    {
                        double TotalSize = drive.TotalSize;
                        if (TotalSize > 0)
                        {
                            double FreeSpace = drive.AvailableFreeSpace;

                            double ee = (TotalSize - FreeSpace) / TotalSize;
                            progressBarDDrive.Maximum = 100;
                            progressBarDDrive.Value = (int)(ee * 100);
                            labelDDrive.Text = string.Format("{0:F1}%", ee * 100);
                        }
                        else
                        {
                            progressBarDDrive.Enabled = false;
                            labelDDrive.Text = "Not Used";
                        }

                    }
                }
            }
        }

        // Communication ------------------------------------------------------------------------------------------------

        private void ClientConnected(bool bConnected)
        {
            labelClientConnected.Invoke(new Action(() =>
            {
                if (bConnected)
                {
                    labelClientConnected.BackColor = Color.LimeGreen;
                    labelClientConnected.Text = "Connected";
                    Logging("Client Connected");
                }
                else
                {
                    labelClientConnected.BackColor = Color.LightGray;
                    labelClientConnected.Text = "Disconnected";
                    Logging("Client Disconnected");
                }
            }
            ));
        }


        // Light ------------------------------------------------------------------------------------------------
        private string strValue = "";
        private string strTemp = "";
        

        private void LightControl_RecieveEvent(object command)
        {

            bool updateLabel = false;

            if (lightControl is DawooLightCtrl)
            {
                if (command is DawooLightCtrl.Command cmd)
                {
                    switch (cmd)
                    {
                        case DawooLightCtrl.Command.GetLightOnOffState:
                            {
                                for (int index = 1; index <= lightControl.LightChannelCount; index++)
                                {
                                    bool OnOff = lightControl.GetLightState(index);
                                    formSetup.UpdateLightState(index, OnOff);
                                }
                            }
                            break;
                        case DawooLightCtrl.Command.GetDimmingValue:
                            {
                                strValue = "Value : ";
                                for (int index = 1; index <= lightControl.LightChannelCount; index++)
                                {
                                    int value = lightControl.GetDimmingValue(index);
                                    //if (value > 0)
                                    {
                                        strValue += $"CH{index} [{value}]  ";
                                        formSetup.UpdateLightSetValue(index, value);
                                        updateLabel = true;
                                    }
                                }
                            }
                            break;
                        case DawooLightCtrl.Command.GetLightTemperature:
                            {
                                strTemp = "Temp : ";
                                for (int index = 1; index <= lightControl.LightChannelCount; index++)
                                {
                                    int temp = lightControl.GetLightTemp(index);
                                    //if (temp > 0)
                                    {
                                        strTemp += $"CH{index} [{temp}]  ";
                                        formSetup.UpdateLightTemp(index, temp);
                                        updateLabel = true;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            else if (lightControl is EroomLightCtrl)
            {
                if (command is EroomLightCtrl.Command cmd)
                {
                    switch (cmd)
                    {
                        case EroomLightCtrl.Command.GetOnOff:
                            {
                                for (int index = 1; index <= lightControl.LightChannelCount; index++)
                                {
                                    bool OnOff = lightControl.GetLightState(index);
                                    formSetup.UpdateLightState(index, OnOff);
                                }
                            }
                            break;
                        case EroomLightCtrl.Command.GetBright:
                            {
                                strValue = "Value : ";
                                for (int index = 1; index <= lightControl.LightChannelCount; index++)
                                {
                                    int value = lightControl.GetDimmingValue(index);
                                    //if (value > 0)
                                    {
                                        strValue += $"CH{index} [{value}]  ";
                                        formSetup.UpdateLightSetValue(index, value);
                                        updateLabel = true;
                                    }
                                }
                            }
                            break;
                        case EroomLightCtrl.Command.GetTemperature:
                            {
                                strTemp = "Temp : ";
                                for (int index = 1; index <= lightControl.LightChannelCount; index++)
                                {
                                    int temp = lightControl.GetLightTemp(index);
                                    //if (temp > 0)
                                    {
                                        strTemp += $"CH{index} [{temp}]  ";
                                        formSetup.UpdateLightTemp(index, temp);
                                        updateLabel = true;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            else if (lightControl is VitFcLightCtrl)
            {
                if (command is VitFcLightCtrl.Command cmd)
                {
                    switch (cmd)
                    {
                        case VitFcLightCtrl.Command.RequestState:
                            {
                                for (int index = 1; index <= lightControl.LightChannelCount; index++)
                                {
                                    bool OnOff = lightControl.GetLightState(index);
                                    formSetup.UpdateLightState(index, OnOff);
                                }
                            }
                            break;
                        case VitFcLightCtrl.Command.RequestValue:
                            {
                                strValue = "Value : ";
                                for (int index = 1; index <= lightControl.LightChannelCount; index++)
                                {
                                    int value = lightControl.GetDimmingValue(index);
                                    //if (value > 0)
                                    {
                                        strValue += $"CH{index} [{value}]  ";
                                        formSetup.UpdateLightSetValue(index, value);
                                        updateLabel = true;
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            if (updateLabel)
            {
                labelLightState.Invoke(new Action(() =>
                {
                    labelLightState.Text = $"{strValue} \n{strTemp}";
                }));
            }


        }

        private void LightControl_ConnectEvent(bool bConnected)
        {
            if (bConnected)
            {
                labelPortConnected.Text = "Connected";
                labelPortConnected.BackColor = Color.LimeGreen;
            }
            else
            {
                labelPortConnected.Text = "Disconnected";
                labelPortConnected.BackColor = Color.LightGray;
            }
        }

        // Grab ------------------------------------------------------------------------------------------------

        public bool GrabReady()
        {
            if (CellDatas.Length > 0)
            {
                Logging($"Start Grab Process [Cell : {CellDatas}]");
                EMASLogging("Grab_Process", false);

                listCell_ID.Clear();
                string[] StringArray = CellDatas.Split(new string[] { "," }, StringSplitOptions.None);
                listCell_ID = StringArray.ToList();

                if (useStartTimeout)
                {
                    StartTimeoutTimer.Enabled = true;
                    bFlagGrabStart = false;
                }

                Process(ProcessState.Ready);

                lightControl.LightAllOnOff(true);
                Logging("Light On");

                grabControl.SetTriggerReady();
                Logging("Trigger Standby");

                return true;
            }
            else
            {
                Logging($"Recieve data is wrong!!  [CellDatas : {CellDatas}]");
                return false;
            }
        }

        public void GrabStart()
        {
            if (State != ProcessState.Grabbing)
            {
                Process(ProcessState.Grabbing);
                Logging("Grab Start");
                EMASLogging("Grab", false);
                stopwatch.Restart();

                if (grabControl.Simulation)
                {
                    GrabEnd();
                }
            }
            else
            {
                Logging("Received a Start signal during the Grabbing state.");
                Process(ProcessState.Alarm);
            }
        }

        public void GrabEnd()
        {
            try
            {
                int totalImageCount = recipe.ListChildImageRoi.Count;
                Logging($"Grab End [{stopwatch.Elapsed:mm\\:ss\\.ff}]");
                EMASLogging("Grab", true);

                stopwatch.Restart();

                bool isNextJobReady = (State == ProcessState.Ready);

                if (useStartTimeout)
                {
                    if (!isNextJobReady)
                    {
                        StartTimeoutTimer.Enabled = false;
                        bFlagGrabStart = true;
                    } //네트워크 지연 후 조명이 Off될 때 사용
                }

                if (!isNextJobReady)
                {
                    lightControl.LightAllOnOff(false);
                    Logging("Light Off");
                }
                else
                {
                    Logging("Skip Light Off (Next Job is Ready)");
                } //네트워크 지연 후 조명이 Off될 때 사용

                bool ret = true;

                if (listCell_ID.Count == 0)
                {
                    for (int i = 0; i < totalImageCount; i++)
                    {
                        listCell_ID.Add("Manual_" + i);
                    }
                }

                List<string> currentCell_ID = new List<string>(listCell_ID);
                string safeCellDataString = string.Join(",", currentCell_ID);
                /*
                int childIndex = 0;
                foreach (ROI child in recipe.ListChildImageRoi)
                {
                    if (!grabControl.ChildSave($@"{imagePath}\{childIndex}.bmp", childIndex))
                    {
                        Logging($"The file was not saved to the shared folder.[index :{childIndex}]");
                        ret = false;
                    }

                    if (grabControl.Simulation)
                    {
                        Bitmap bitmap = new Bitmap(child.Width, child.Height);
                        bitmap.Save($@"{imagePath}\{childIndex}.bmp");
                    }

                    childIndex++;
                }
                */

                if (useImageShare)
                {
                    if (new DirectoryInfo(imageSharePath).Exists)
                    {
                        int index = 0;
                        foreach(var child in recipe.ListChildImageRoi)
                        {
                            int cellIndex = child.CellNo - 1;

                            if (currentCell_ID[cellIndex].Contains("NOCELL"))
                            {
                                continue;
                            }

                            string file = currentCell_ID.Count == recipe.ListChildImageRoi.Count
                                ? $@"{imageSharePath}\{currentCell_ID[index]}.bmp"
                                : $@"{imageSharePath}\{currentCell_ID[cellIndex]}{child.Name}.bmp";

                            //공유폴더 저장
                            grabControl.ChildSave(file, index);
                            //if (File.Exists(file)) { File.Delete(file); }
                            //File.Copy($@"{imagePath}\{index}.bmp", file);

                            Logging($"Save image {Path.GetFileName(file)} in Shared folder");

                            if (useCheckSharedFolder)
                            {
                                if (File.Exists(file))
                                {
                                    Logging($"Find Grab Image. [ {Path.GetFileName(file)} ]");
                                }
                                else
                                {
                                    ret = false;

                                    Logging($"Did not save Grab Image. [ {Path.GetFileName(file)} ]");
                                    break;
                                }
                            }
                            index++;
                        }  
                    }
                    else
                    {
                        Logging("Image Share Path is not Exist [GrabEnd()]");
                        ret = false;
                    }
                }

                if (useCommunication)
                {
                    comm.SendGrabEnd(safeCellDataString, ret);
                    //comm.SendGrabEnd(CellDatas, ret);

                    if (ret == false)
                    {
                        Logging("Image save fail [GrabEnd()]");
                        Process(ProcessState.Alarm);
                    }

                    Logging($"End Grab Process [Cell : {safeCellDataString}], [Shared : {ret}]");
                    //Logging($"End Grab Process [Cell : {CellDatas}], [Shared : {ret}]");
                    EMASLogging("Grab_Process", true);

                    //safeCellDataString = "";
                    //CellDatas = "";
                }

                if (!isNextJobReady)
                {
                    Process(ProcessState.End);
                } //네트워크 지연 후 조명이 Off될 때 사용

                Logging($"Save End [{stopwatch.Elapsed:mm\\:ss\\.ff}]");
                stopwatch.Restart();

                if (useImageBackup)
                {
                    if (new DirectoryInfo(imageBackupPath).Exists)
                    {
                        int index = 0;
                        foreach (var child in recipe.ListChildImageRoi)
                        { 
                            int cellIndex = child.CellNo - 1;

                            if (currentCell_ID[cellIndex].Contains("NOCELL"))
                            {
                                continue;
                            }

                            string path = $@"{imageBackupPath}\{DateTime.Now:d}";

                            DirectoryInfo BackupInfo = new DirectoryInfo(path);
                            if (!BackupInfo.Exists) { BackupInfo.Create(); }

                            string file = currentCell_ID.Count == recipe.ListChildImageRoi.Count
                                ? $@"{path}\{currentCell_ID[index]}.bmp"
                                : $@"{path}\{currentCell_ID[cellIndex]}{child.Name}.bmp";

                            //백업폴더 저장
                            grabControl.ChildSave(file, index);
                            //if (File.Exists(file)) { File.Delete(file); }
                            //File.Copy($@"{imagePath}\{index}.bmp", file);
                            Logging($"Save image {Path.GetFileName(file)} in Backup folder");

                            index++;
                        }
                    }
                }

                /*
                for (int i = 0; i < totalImageCount; i++)
                {
                    File.Delete($@"{imagePath}\{i}.bmp");
                }
                */

                GC.Collect();
                GC.WaitForPendingFinalizers();

                if (useMemoryLimit)
                {
                    if (remmain_physical_memory > limitMemoryPercent)
                    {
                        FreeMemory.ClearFileSystemCache(ClearStandbyCache: true);
                        FreeMemory.EmptyWorkingSetFunction();
                    }
                }

                if (formSetup.Enabled)
                {
                    Logging($"Setup Tab selected [GrabEnd()]");
                }

                Logging($"Process End [{stopwatch.Elapsed:mm\\:ss\\.ff}]");
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                Logging($"catch Msg : {ex.Message} [GrabEnd()]");
                Process(ProcessState.Alarm);
            }
        }

        private void StartTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StartTimeoutTimer.Enabled = false;

            if (bFlagGrabStart == false)
            {
                Logging("Start Timeout!!");

                Logging($"Check PC Memory [{labelMemory.Text}]");

                lightControl.LightAllOnOff(false);
                Logging("Light Off");

                Process(ProcessState.Alarm);
                stopwatch.Stop();
            }
        }

        private Image ResizeImage(Image image, Rectangle rectangle)
        {
            if (image != null)
            {
                Bitmap croppedBitmap = new Bitmap(image);
                croppedBitmap = croppedBitmap.Clone(rectangle, System.Drawing.Imaging.PixelFormat.DontCare);
                return croppedBitmap;
            }
            else
            {
                return image;
            }
        }


        private void labelProcessAlarm_Click(object sender, EventArgs e)
        {
            Process(ProcessState.Idle);
        }

        private void groupBoxGrabber_Resize(object sender, EventArgs e)
        {
            //ResizeChildImageViewer(recipe);
            //MainImageViewerCreate(recipe);
            ResizeChildImageViewer(recipe);
        }

        private void buttonLevelChange_Click(object sender, EventArgs e)
        {
            formAuthority.ShowDialog();
        }

        private void labelAuthority_DoubleClick(object sender, EventArgs e)
        {
            if (formAuthority.GetLevel() != FormAuthority.Level.Operator)
            {
                if (MessageBox.Show("Do you want to change user Authority to operator mode?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    formAuthority.SetLevel(FormAuthority.Level.Operator);
                    SetAuthority(FormAuthority.Level.Operator);
                }
            }
        }

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageSetup)
            {
                formSetup.Enabled = true;
            }
            else if (tabControlMain.SelectedTab == tabPageMain)
            {
                formSetup.Enabled = false;

                if (formSetup.LiveOn)
                {
                    formSetup.Live(false);
                }
            }
        }

        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (null != identity)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return false;
        }

        private void label5_Click(object sender, EventArgs e)
        {
            if (IsAdministrator())
            {
                FreeMemory.EmptyWorkingSetFunction();
                FreeMemory.ClearFileSystemCache(ClearStandbyCache: true);
            }
        }
    }
}
