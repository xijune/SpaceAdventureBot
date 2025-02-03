using SharpAdbClient;
using SpaceAdventureBot;
using System.Diagnostics;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--device" && args.Length > 1)
        {
            string deviceSerial = args[1];
            DeviceData device = Utils.GetDeviceBySerial(deviceSerial);
            if (device != null)
            {
                Bot bot = new(new(), device);
                Utils.Log($"Starting bot for device {device.Serial}", LogType.Info);
                bot.Start();
            }
            else
            {
                Utils.Log($"Device with serial {deviceSerial} not found.", LogType.Error);
            }
        }
        else
        {
            Utils.StartAdbServer();

            List<DeviceData> devices = Utils.GetDevices();
            if (devices.Count == 0)
            {
                Utils.Log("No devices found.", LogType.Warning);
                Utils.Log("Stopping program", LogType.Error);
                return;
            }

            string executablePath = Process.GetCurrentProcess().MainModule.FileName;

            devices.ForEach(o =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = $"--device {o.Serial}",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process.Start(startInfo);
                Utils.Log($"Started new console window for device {o.Serial}", LogType.Info);
            });
        }
    }
}
