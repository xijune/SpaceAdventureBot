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

        public static void Log(string message, LogType messageType)
        {
            string prefix = string.Empty;
            switch (messageType)
            {
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    prefix = "[INFO]";
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    prefix = "[WARNING]";
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    prefix = "[ERROR]";
                    break;
                case LogType.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    prefix = "[SUCCESS]";
                    break;
            }
            Console.WriteLine($"{DateTime.Now.ToString("T")} | {prefix} {message}");
            Console.ResetColor();
        }

        public static int GetRandomSleepDuration(int min, int max)
        {
            Random random = new Random();;
            int randomMillisecond = random.Next(min * Constants.SleepDurationMultiplier, max * Constants.SleepDurationMultiplier);
            return randomMillisecond;
        }
    }
}
