using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace SystemTrayApp
{
    public partial class AppWindow
    {

        HardwareMonitorServer hardwareMonitor = new HardwareMonitorServer();
        SensorData sensorData = new SensorData();
        HardwareMonitorServer.PerformanceMode performanceMode = new HardwareMonitorServer.PerformanceMode();
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        System.Timers.Timer timerFnX = new System.Timers.Timer(100);
        Icon mode0 = null;
        Icon mode1 = null;
        Icon mode2 = null;
        Icon mode3 = null;

        KeyboardHook keyboardHook = new KeyboardHook();


        public AppWindow()
        {
            InitializeComponent();

            mode0 = GetIcon("Mode0");
            mode1 = GetIcon("Mode1");
            mode2 = GetIcon("Mode2");
            mode3 = GetIcon("Mode3");

            initTrayIcon();

            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerEvent);
            timer.Enabled = true;

            timerFnX.Elapsed += new System.Timers.ElapsedEventHandler(TimerFnXEvent);
            timerFnX.AutoReset = false;

            keyboardHook.FNXEvent += new FNXEventHandler(FnXEvent);
            keyboardHook.Start();

            UpdateIcon(true);
        }

        private void initTrayIcon()
        {
            this.SystemTrayIcon.Icon = mode0;
            this.SystemTrayIcon.Visible = true;
            this.SystemTrayIcon.ContextMenuStrip = new ContextMenuStrip();
            this.SystemTrayIcon.ContextMenuStrip.Items.Add("性能表现", null, ContextMenuNone);
            this.SystemTrayIcon.ContextMenuStrip.Items.Add("内存占用", null, ContextMenuNone);
            this.SystemTrayIcon.ContextMenuStrip.Items.Add("温度(CPU)", null, ContextMenuNone);
            this.SystemTrayIcon.ContextMenuStrip.Items.Add("功率(CPU)", null, ContextMenuNone);
            ToolStripMenuItem settingsItem = new ToolStripMenuItem();
            settingsItem.Text = "设置";
            settingsItem.DropDownItems.Add("Exit", null, ContextMenuExit);
            this.SystemTrayIcon.ContextMenuStrip.Items.Add(settingsItem);
        }

        private void FnXEvent()
        {
            timerFnX.Start();
        }

        public void TimerEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("timer timeout");
            if (this.SystemTrayIcon.ContextMenuStrip.Visible)
            {
                UpdateMenu();
                UpdateIcon(false);
            }
            else
            {
                timer.Stop();
            }
        }

        public void TimerFnXEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("TimerFnXEvent");
            timerFnX.Stop();
            UpdateIcon(true);
        }

        private void SystemTrayIconMouceDown(object sender, MouseEventArgs e)
        {
            ShowMenu();
            timer.Start();
        }

        private void ShowMenu()
        {
            UpdateMenu();
            MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(SystemTrayIcon, null);

            UpdateIcon(false);
        }

        private void UpdateMenu()
        {
            sensorData = hardwareMonitor.GetPerformanceData();
            performanceMode = hardwareMonitor.GetPerformanceMode();
            (SystemTrayIcon.ContextMenuStrip.Items[0] as ToolStripMenuItem).Text = "性能表现：" + performanceMode.name;
            (SystemTrayIcon.ContextMenuStrip.Items[1] as ToolStripMenuItem).Text = "内存占用：" + sensorData.usedMemProcentage + " %";
            (SystemTrayIcon.ContextMenuStrip.Items[2] as ToolStripMenuItem).Text = "温度(CPU)：" + sensorData.temp + " ℃";
            (SystemTrayIcon.ContextMenuStrip.Items[3] as ToolStripMenuItem).Text = "功率(CPU)：" + sensorData.power + " W";
        }

        private void UpdateIcon(bool updatePerformanceMode)
        {
            if (updatePerformanceMode)
            {
                performanceMode = hardwareMonitor.GetPerformanceMode();
            }
            switch (performanceMode.index)
            {
                case 0:
                    this.SystemTrayIcon.Icon = mode1;
                    break;
                case 1:
                    this.SystemTrayIcon.Icon = mode2;
                    break;
                case 2:
                    this.SystemTrayIcon.Icon = mode3;
                    break;
                default:
                    this.SystemTrayIcon.Icon = mode0;
                    break;
            }
        }

        private void ContextMenuExit(object sender, EventArgs e)
        {
            keyboardHook.Stop();
            timer.Stop();
            this.SystemTrayIcon.Visible = false;
            Application.Exit();
            Environment.Exit(0);
        }
        private void ContextMenuNone(object sender, EventArgs e)
        {

        }

        private System.Drawing.Icon GetIcon(string name)
        {
            object obj = Properties.Resources.ResourceManager.GetObject(name);
            return ((System.Drawing.Icon)(obj));
        }
        private long Millis()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
    }
}
