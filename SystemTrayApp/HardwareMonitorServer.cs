using System;
using System.Management;
using OpenHardwareMonitor.Hardware;
using System.Threading;

namespace SystemTrayApp
{
    public class SensorData
    {
        public int temp;
        public int power;
        public int usedMemProcentage;
    }
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
        public class PerformanceMode
        {
            public int index = -1;
            public string name = "未知";
        }
        public SensorData GetPerformanceData()
        {            
            SensorData sensorData = new SensorData();
            double usedMem = 0;
            double availableMem = 1;
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.CPU)
                {
                    hardware.Update(); //use hardware.Name to get CPU model
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && sensor.Name == "CPU Package")
                        {
                            sensorData.temp = (int)sensor.Value.GetValueOrDefault();
                            continue;
                        }
                        if (sensor.SensorType == SensorType.Power && sensor.Value.HasValue && sensor.Name == "CPU Package")
                        {
                            sensorData.power = (int)sensor.Value.GetValueOrDefault();
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
            sensorData.usedMemProcentage = (int)(usedMem * 100 / (usedMem + availableMem));
            return sensorData;
        }

        public PerformanceMode GetPerformanceMode()
        {
            string text = "ACPI\\PNP0C14\\0_3";
            PerformanceMode mode = new PerformanceMode();
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
                            mode.index = 0;
                            mode.name = "静音";
                            return mode;
                        }
                        if (num != 2U)
                        {
                            mode.index = 1;
                            mode.name = "平衡";
                            return mode;
                        }
                        mode.index = 2;
                        mode.name = "性能";
                        return mode;
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
