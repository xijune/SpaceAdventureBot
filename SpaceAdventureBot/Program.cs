using SharpAdbClient;
using SpaceAdventureBot;

class Program
{
    static void Main()
    {
        Utils.StartAdbServer();
        
        List<DeviceData> devices = Utils.GetDevices();
        if (devices.Count == 0)
            return;

        devices.ForEach(o =>
        {
            Bot bot = new(new(), o);
            bot.Start();
        });
    }
}