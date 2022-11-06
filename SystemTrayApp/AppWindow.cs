using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace SystemTrayApp
{
    public partial class AppWindow 
    {

        HardwareMonitorServer hardwareMonitor = new HardwareMonitorServer();

        public AppWindow()
        {
            InitializeComponent();

            // To provide your own custom icon image, go to:
            //   1. Project > Properties... > Resources
            //   2. Change the resource filter to icons
            //   3. Remove the Default resource and add your own
            //   4. Modify the next line to Properties.Resources.<YourResource>
            //this.Icon = Properties.Resources.Default;
            this.SystemTrayIcon.Icon = Properties.Resources.Default;

            // Change the Text property to the name of your application
            this.SystemTrayIcon.Visible = true;

            // Modify the right-click menu of your system tray icon here
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("性能表现：", ContextMenuNone);
            menu.MenuItems.Add("内存占用：", ContextMenuNone);
            menu.MenuItems.Add("温度(CPU)：", ContextMenuNone);
            menu.MenuItems.Add("功率(CPU)：", ContextMenuNone);
            menu.MenuItems.Add("Exit", ContextMenuExit);
            this.SystemTrayIcon.ContextMenu = menu;
        }

        private void SystemTrayIconMouceDown(object sender, MouseEventArgs e)
        {
            showMenu();
        }

        private void SystemTrayIconMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void showMenu()
        {
            SensorData sensorData = hardwareMonitor.GetPerformanceData();
            SystemTrayIcon.ContextMenu.MenuItems[0].Text = "性能表现：" + hardwareMonitor.GetPerformanceMode();
            SystemTrayIcon.ContextMenu.MenuItems[1].Text = "内存占用：" + sensorData.usedMemProcentage + " %";
            SystemTrayIcon.ContextMenu.MenuItems[2].Text = "温度(CPU)：" + sensorData.temp + " ℃";
            SystemTrayIcon.ContextMenu.MenuItems[3].Text = "功率(CPU)：" + sensorData.power + " W";
            MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(SystemTrayIcon, null);
        }


        private double PointDistance(Point a, Point b)
        {
            return Math.Sqrt((Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)));
        }

        private void ContextMenuExit(object sender, EventArgs e)
        {
            this.SystemTrayIcon.Visible = false;
            Application.Exit();
            Environment.Exit(0);
        }
        private void ContextMenuNone(object sender, EventArgs e)
        {
        }

        private long Millis()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
    }
}
