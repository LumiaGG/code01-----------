using System;
using System.Windows.Forms;

namespace SystemTrayApp
{
    public partial class AppWindow 
    {
        long lastCheckSensorTime = 0;
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
            this.SystemTrayIcon.Text = "System Tray App";
            this.SystemTrayIcon.Visible = true;

            // Modify the right-click menu of your system tray icon here
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Exit", ContextMenuExit);
            this.SystemTrayIcon.ContextMenu = menu;


        }

        private void SystemTrayIconDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void SystemTrayIconMouseMove(object sender, MouseEventArgs e)
        {
            if (Millis() - lastCheckSensorTime < 1000)
            {
                return;
            }
            lastCheckSensorTime = Millis();
            string text = hardwareMonitor.GetPerformanceData();
            if (text.Length > 63)
            {
                text = text.Substring(0, 63);
            }
            this.SystemTrayIcon.Text = text;
        }

        private void ContextMenuExit(object sender, EventArgs e)
        {
            this.SystemTrayIcon.Visible = false;
            Application.Exit();
            Environment.Exit(0);
        }

        private long Millis()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
    }
}
