using SharpAdbClient;
using SpaceAdventureBot;
using System.Net;

class Program
{
    static void Main()
    {
        Utils.StartAdbServer();
        
        List<DeviceData> devices = Utils.GetDevices();
        if (devices.Count == 0)
        {
            Utils.Log("No devices found.", LogType.Warning);
            Utils.Log("Stopping program", LogType.Error);
            return;
        }

        devices.ForEach(o =>
        {
            Bot bot = new(new(), o);
            Utils.Log($"Starting bot for device {o.Serial}", LogType.Info);
            bot.Start();
        });
    }
}