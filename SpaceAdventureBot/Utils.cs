using SharpAdbClient;
using System.Diagnostics;

namespace SpaceAdventureBot
{
    static class Utils
    {
        public static List<DeviceData> GetDevices()
        {
            AdbClient adbClient = new();
            return adbClient.GetDevices();
        }

        public static async void StartAdbServer(bool restartServer = false)
        {
            AdbServer server = new AdbServer();
            if(server.GetStatus().IsRunning && restartServer)
            {
                var process = StopServer();
                process.WaitForExit();
            }
            StartServerResult result = server.StartServer(@"C:\adb\adb.exe", false);
                server.RestartServer();

            while (result != StartServerResult.AlreadyRunning)
            {
                await Task.Delay(1);
            }
        }

        static Process StopServer()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = @"C:\adb\adb.exe",
                Arguments = "kill-server",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            return process;
        }
    }
}
