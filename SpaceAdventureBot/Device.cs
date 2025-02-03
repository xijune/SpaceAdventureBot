using OpenCvSharp;
using SharpAdbClient;

namespace SpaceAdventureBot
{
    public class Device
    {
        public AdbClient AdbClient { get; private set; }
        public DeviceData DeviceData { get; private set; }

        public Device(AdbClient adbClient, DeviceData deviceData)
        {
            AdbClient = adbClient;
            DeviceData = deviceData;
        }

        /// <summary>
        /// Taps on the screen at the specified coordinates.
        /// </summary>
        /// <param name="center">The coordinates to tap.</param>
        public void Tap(Point center) => ExecuteCommand($"input touchscreen tap {center.X} {center.Y}");

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The output receiver containing the command output.</returns>
        public ConsoleOutputReceiver ExecuteCommand(string command)
        {
            ConsoleOutputReceiver receiver = new();
            AdbClient.ExecuteRemoteCommand(command, DeviceData, receiver);
            return receiver;
        }

        /// <summary>
        /// Takes a screenshot of the device screen and checks for anti-bot measures.
        /// </summary>
        public void Screenshot()
        {
            const string remotePath = "/sdcard/screenshot.png";

            ExecuteCommand($"screencap -p {remotePath}");
            using (SyncService service = new SyncService(AdbClient, DeviceData))
            using (Stream stream = File.OpenWrite($"screenshot-{DeviceData.Serial}.png"))
            {
                service.Pull(remotePath, stream, null, CancellationToken.None);
            }

            // Check for anti-bot measures and tap if found
            CheckAndTapAntiBot(Constants.AntiBotClose);
            CheckAndTapAntiBot(Constants.AntiBotClose2);
        }

        /// <summary>
        /// Checks for the specified anti-bot measure and taps if found.
        /// </summary>
        /// <param name="antiBotImagePath">The path to the anti-bot image.</param>
        private void CheckAndTapAntiBot(string antiBotImagePath)
        {
            using (var mainImage = Cv2.ImRead($"screenshot-{DeviceData.Serial}.png", ImreadModes.Color))
            using (var templateImage = Cv2.ImRead(antiBotImagePath, ImreadModes.Color))
            {
                if (!mainImage.Empty() && !templateImage.Empty())
                {
                    using (var result = new Mat())
                    {
                        Cv2.MatchTemplate(mainImage, templateImage, result, TemplateMatchModes.CCoeffNormed);
                        Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

                        if (maxVal >= 0.9)
                        {
                            int centerX = maxLoc.X + templateImage.Width / 2;
                            int centerY = maxLoc.Y + templateImage.Height / 2;
                            Tap(new Point(centerX, centerY));
                            Thread.Sleep(2000);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the specified app is in the foreground.
        /// </summary>
        /// <param name="packageName">The package name of the app.</param>
        /// <returns>True if the app is in the foreground, otherwise false.</returns>
        public bool IsAppInForeground(string packageName)
        {
            var receiver = ExecuteCommand("dumpsys activity activities | grep 'mResumedActivity'");
            string output = receiver.ToString().Trim();
            return output.Contains(packageName);
        }

        /// <summary>
        /// Starts the specified app.
        /// </summary>
        /// <param name="packageName">The package name of the app.</param>
        /// <param name="activityName">The main activity name of the app.</param>
        /// <returns>True if the app started successfully, otherwise false.</returns>
        public bool StartApp(string packageName, string activityName)
        {
            if (IsAppInForeground(packageName))
                return true;

            var receiver = ExecuteCommand($"am start -n {packageName}/{activityName}");
            if (!receiver.ToString().Contains("Starting: Intent"))
                return false;

            return WaitForAppToStart(packageName);
        }

        /// <summary>
        /// Closes the specified app.
        /// </summary>
        /// <param name="packageName">The package name of the app.</param>
        public void CloseApp(string packageName)
        {
            if (IsAppInForeground(packageName))
            {
                ExecuteCommand($"am force-stop {packageName}");
                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Waits for the specified app to start.
        /// </summary>
        /// <param name="packageName">The package name of the app.</param>
        /// <returns>True if the app started successfully, otherwise false.</returns>
        private bool WaitForAppToStart(string packageName)
        {
            DateTime startTime = DateTime.Now;
            const int timeout = 8000;
            const int sleepInterval = 500;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeout)
            {
                if (IsAppInForeground(packageName))
                    return true;
                Thread.Sleep(sleepInterval);
            }

            return false;
        }

        /// <summary>
        /// Scrolls up within the specified region.
        /// </summary>
        /// <param name="region">The region to scroll within.</param>
        /// <param name="distance">The distance to scroll up.</param>
        public void ScrollUpInRegion(Rect region, int distance)
        {
            int startX = region.X + region.Width / 2;
            int startY = region.Y + region.Height - 1;
            int endX = startX;
            int endY = startY - distance;

            ExecuteCommand($"input swipe {startX} {startY} {endX} {endY}");
        }

        public void ScrollDownInRegion(Rect region, int distance)
        {
            int startX = region.X + region.Width / 2;
            int startY = region.Y;
            int endX = startX;
            int endY = startY + distance;

            ExecuteCommand($"input swipe {startX} {startY} {endX} {endY}");
        }
    }
}
