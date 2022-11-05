using System;
using System.Management;
using OpenHardwareMonitor.Hardware;
using System.Threading;

namespace SystemTrayApp
{
    internal sealed class HardwareMonitorServer : IDisposable
    {
        private readonly Computer _computer;
           
        public HardwareMonitorServer()
        {
            _computer = new Computer { CPUEnabled = true, RAMEnabled = true };
            _computer.Open();
        }
        public void Dispose()
        {
            try
            {
                _computer.Close();
            }
            catch (Exception)
            {
                //ignore closing errors
            }
        }

        public string GetPerformanceData()
        {            
            int temp = 0;
            int power = 0;
            double usedMem = 0;
            double availableMem = 1;
            int usedMegProcentage = 0;
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.CPU)
                {
                    hardware.Update(); //use hardware.Name to get CPU model
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && sensor.Name == "CPU Package")
                        {
                            temp = (int)sensor.Value.GetValueOrDefault();
                            continue;
                        }
                        if (sensor.SensorType == SensorType.Power && sensor.Value.HasValue && sensor.Name == "CPU Package")
                        {
                            power = (int)sensor.Value.GetValueOrDefault();
                            continue;
                        }
                    }
                } else if (hardware.HardwareType == HardwareType.RAM)
                {
                    hardware.Update(); //use hardware.Name to get CPU model
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Data && sensor.Value.HasValue && sensor.Name == "Used Memory")
                        {
                            usedMem = sensor.Value.GetValueOrDefault();
                            continue;
                        }
                        if (sensor.SensorType == SensorType.Data && sensor.Value.HasValue  && sensor.Name == "Available Memory")
                        {
                            availableMem = sensor.Value.GetValueOrDefault();
                            continue;
                        }
                    }
                }
            }
            usedMegProcentage = (int)(usedMem * 100 / (usedMem + availableMem));
            return "性能表现："+GetPerformanceMode() 
                + "\n内存占用：" + usedMegProcentage + " %" 
                + "\n温度(CPU)：" + temp + " ℃\n功率(CPU)：" + power + " W";
        }

        public string GetPerformanceMode()
        {
            string text = "ACPI\\PNP0C14\\0_3";
            string mode = "未知";
            try
            {
                ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM AcpiTest_QULong");
                if (managementObjectSearcher == null) return mode;
                ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
                if (managementObjectCollection == null || managementObjectCollection.Count == 0) return mode;
                foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
                {
                    ManagementObject managementObject = (ManagementObject)managementBaseObject;
                    if (managementObject.Properties["InstanceName"] == null || !text.Equals((string)managementObject.Properties["InstanceName"].Value) || managementObject.Properties["ULong"] == null)
                        continue;
                    PropertyData propertyData = managementObject.Properties["ULong"];
                    if (propertyData != null && propertyData.Value != null)
                    {
                        uint num = (uint)propertyData.Value;
                        if (num == 1U)
                        {
                            return "静音";
                        }
                        if (num != 2U)
                        {
                            return "平衡";
                        }
                        return "性能";
                    }
                }
            }
            catch (Exception)
            {
            }
            return mode;
        }

    }
}
