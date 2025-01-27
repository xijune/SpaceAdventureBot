using OpenCvSharp;
using SharpAdbClient;

namespace SpaceAdventureBot
{
    class Bot
    {
        private const int MinSleepMinutes = 10;
        private const int MaxSleepMinutes = 14;
        private const int SleepDurationMultiplier = 60 * 1000;
        private const double DefaultConfidenceThreshold = 0.8;
        private const double HighConfidenceThreshold = 0.98;
        private const int TapSleepDuration = 2000;
        private const int DefaultTimeout = 15000;

        private Device Device { get; set; }

        public Bot(AdbClient adbClient, DeviceData deviceData)
        {
            Device = new Device(adbClient, deviceData);
        }

        public void Start()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Starting Telegram...");
                    if (!Device.StartApp(Constants.TelegramPackageName, Constants.TelegramMainActivity))
                        break;

                    Console.WriteLine("Launching Space Adventure...");
                    if (!LaunchSpaceAdventure())
                    {
                        Console.WriteLine("Failed to launch Space Adventure");
                        Device.CloseApp(Constants.TelegramPackageName);
                        continue;
                    }
                    //Device.Screenshot();
                    //return;
                    
                    DailyActivity();
                    Fuel();
                    Shield();
                    CollectCoin();
                    Spin();

                    Console.WriteLine("Closing Telegram...");
                    Device.CloseApp(Constants.TelegramPackageName);

                    int sleepDuration = GetRandomSleepDuration();
                    Console.WriteLine($"Bot is sleeping for {sleepDuration / SleepDurationMultiplier} minutes.");
                    Thread.Sleep(sleepDuration);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Device.CloseApp(Constants.TelegramPackageName);
                }
            }
        }

        private bool DailyActivity()
        {
            AntiBot();
            Console.WriteLine("Checking for daily activity...");
            if (FindMatchOnScreenWithTimeout("DailyActivity", 5000) != null)
            {
                Console.WriteLine("Daily activity is available");
                if (FindWhileScrollingWithTimeout("DailyActivityDoubleReward", Constants.DailyActivityScrollRegion, DefaultTimeout))
                {
                    while (!IsMatchOnScreen("DailyActivity"))
                        continue;
                    return true;
                }
            }
            Console.WriteLine("No daily activity found");
            return false;
        }

        private void AntiBot()
        {
            Rect? adPopUp = FindRegionOnScreen("AdPopUp");
            if (adPopUp != null)
            {
                Console.WriteLine("Anti-bot measure detected, bypassing...");
                FindAndTap("AdBypass", adPopUp);
            }
        }

        private bool LaunchSpaceAdventure()
        {
            if (IsMatchOnScreen("SpaceAdventureLaunched"))
            {
                Console.WriteLine("Space Adventure already launched");
                return true;
            }

            Rect? spaceAdventureRegion = FindRegionOnScreenWithTimeout("SpaceAdventureBot", DefaultTimeout);
            if (spaceAdventureRegion != null)
            {
                Console.WriteLine("Opening Space Adventure...");
                if (FindAndTapWithTimeout("OpenSpaceAdventureBot", DefaultTimeout, spaceAdventureRegion))
                {
                    Console.WriteLine("Starting Space Adventure...");
                    return FindAndTapWithTimeout("StartButton", DefaultTimeout);
                }
            }
            Console.WriteLine("Failed to launch Space Adventure");
            return false;
        }

        private void CollectCoin()
        {
            AntiBot();
            if (FindAndTap("CollectButton"))
                Console.WriteLine("Collecting coin...");
        }

        private void Fuel()
        {
            AntiBot();
            if (!IsFuelEmpty())
            {
                Console.WriteLine("Fuel is not empty, no need to fill.");
                return;
            }

            Console.WriteLine("Fuel is empty, filling...");
            if (FindAndTap("Shop"))
            {
                Console.WriteLine("Opened Shop.");
                Device.Screenshot();
                //var test = IsMatchOnScreen();
                if (IsMatchOnScreen("FuelLogo"))
                {
                    Console.WriteLine("Fuel logo found, attempting to watch ad for filling.");
                    if (FindAndTap("FuelAd"))
                    {
                        Console.WriteLine("Watching ad for repair...");
                        while (!IsMatchOnScreen("FuelLogo"))
                            FindAndTap("AdClose");

                        Console.WriteLine("Ad finished, repair completed.");
                    }
                }
                Thread.Sleep(5000);
                Console.WriteLine("Closing Shop...");
                FindAndTap("ShopClose");
            }
            else
                Console.WriteLine("Failed to open Shop.");
        }

        private bool IsFuelEmpty()
        {
            Console.WriteLine("Checking if fuel is empty...");
            return IsMatchOnScreen("Fuel", Constants.FuelRegion);
        }

        private void Shield()
        {
            AntiBot();
            if (!IsShielBroken())
            {
                Console.WriteLine("Shield is not broken, no need to repair.");
                return;
            }

            Console.WriteLine("Shield is broken, repairing...");
            if (FindAndTap("Shop"))
            {
                Console.WriteLine("Opened Shop.");
                if (FindAndTap("RightArrow"))
                {
                    Console.WriteLine("Navigated to the right in the Shop.");
                    if (IsMatchOnScreen("RepairLogo"))
                    {
                        Console.WriteLine("Repair logo found, attempting to watch ad for repair.");
                        if (FindAndTap("RepairAd"))
                        {
                            Console.WriteLine("Watching ad for repair...");
                            while (!IsMatchOnScreen("RepairLogo"))
                                FindAndTap("AdClose");

                            Console.WriteLine("Ad finished, repair completed.");
                        }
                    }
                }
                Thread.Sleep(5000);
                Console.WriteLine("Closing Shop...");
                FindAndTap("ShopClose");
            }
            else
                Console.WriteLine("Failed to open Shop.");
        }

        private bool IsShielBroken()
        {
            Console.WriteLine("Checking if shield is broken...");
            return IsMatchOnScreen("Shield", Constants.ShieldRegion);
        }

        private void Spin()
        {
            AntiBot();
            if (FindAndTap("Spin"))
            {
                Console.WriteLine("Spin button tapped.");
                if (FindAndTap("SpinAd"))
                {
                    Console.WriteLine("Spin ad found, watching ad...");
                    while (!FindAndTap("SpinButton"))
                        FindAndTap("AdClose");

                    Console.WriteLine("Spin button tapped, waiting for spin to complete...");
                    while (!IsMatchOnScreen("SpinLogo"))
                    {
                        AntiBot();
                        continue;
                    }
                    Console.WriteLine("Spin completed.");
                }
                else
                    Console.WriteLine("No spin ad found");

                FindAndTap("CloseSpin");
                Console.WriteLine("Spin closed.");
            }
            else
                Console.WriteLine("Failed to find spin button.");
        }

        private bool FindAndTap(string templateImagePath, Rect? region = null)
        {
            Point? match = FindMatchOnScreen(templateImagePath, DefaultConfidenceThreshold, region);
            if (match == null)
                return false;

            Console.WriteLine($"Tapping on {templateImagePath}...");
            Device.Tap(match.Value);
            Thread.Sleep(TapSleepDuration);
            return true;
        }

        private bool FindAndTapWithTimeout(string templateImagePath, int timeoutMilliseconds, Rect? region = null)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMilliseconds)
            {
                if (FindAndTap(templateImagePath, region))
                    return true;

                Thread.Sleep(500);
            }
            return false;
        }

        private Point? FindMatchOnScreen(string imagePath, double confidenceThreshold = DefaultConfidenceThreshold, Rect? region = null)
        {
            const int maxRetries = 3;
            const int delayBetweenRetries = 500;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    Device.Screenshot();

                    using (var mainImage = Cv2.ImRead("screenshot.png", ImreadModes.Color))
                    using (var templateImage = Cv2.ImRead($"images/{imagePath}.png", ImreadModes.Color))
                    {
                        if (mainImage.Empty() || templateImage.Empty())
                        {
                            Console.WriteLine("Error: Could not load images.");
                            return null;
                        }

                        using (var searchImage = region.HasValue ? new Mat(mainImage, region.Value) : mainImage)
                        using (var result = new Mat())
                        {
                            Cv2.MatchTemplate(searchImage, templateImage, result, TemplateMatchModes.CCoeffNormed);
                            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

                            if (maxVal < confidenceThreshold)
                                return null;

                            int centerX = (region?.X ?? 0) + maxLoc.X + templateImage.Width / 2;
                            int centerY = (region?.Y ?? 0) + maxLoc.Y + templateImage.Height / 2;

                            Console.WriteLine($"Match found at: Top-Left={maxLoc}, Center=({centerX}, {centerY}) with confidence {maxVal}");
                            return new Point(centerX, centerY);
                        }
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IO Error: {ex.Message}");
                    if (attempt < maxRetries - 1)
                    {
                        Console.WriteLine("Retrying...");
                        Thread.Sleep(delayBetweenRetries);
                    }
                    else
                    {
                        Console.WriteLine("Max retries reached. Unable to access the file.");
                        return null;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Access Error: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    return null;
                }
            }

            return null;
        }

        private bool IsMatchOnScreen(string imagePath, Rect? region = null, double confidenceThreshold = DefaultConfidenceThreshold) => FindMatchOnScreen(imagePath, confidenceThreshold, region) != null;

        private Rect? FindRegionOnScreen(string referenceImagePath)
        {
            Point? match = FindMatchOnScreen(referenceImagePath);
            if (match == null) return null;

            using var templateImage = Cv2.ImRead($"images/{referenceImagePath}.png", ImreadModes.Color);
            return new Rect(match.Value.X - templateImage.Width / 2, match.Value.Y - templateImage.Height / 2, templateImage.Width, templateImage.Height);
        }

        private Rect? FindRegionOnScreenWithTimeout(string referenceImagePath, int timeoutMilliseconds)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMilliseconds)
            {
                Rect? region = FindRegionOnScreen(referenceImagePath);
                if (region != null)
                    return region;

                Thread.Sleep(500);
            }
            return null;
        }

        private Point? FindMatchOnScreenWithTimeout(string imagePath, int timeoutMilliseconds, double confidenceThreshold = DefaultConfidenceThreshold, Rect? region = null)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMilliseconds)
            {
                Point? match = FindMatchOnScreen(imagePath, confidenceThreshold, region);
                if (match != null)
                    return match;

                Thread.Sleep(500);
            }
            return null;
        }

        private bool FindWhileScrollingWithTimeout(string templateImagePath, Rect scrollRegion, int timeoutMilliseconds, int scrollDistance = 300)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMilliseconds)
            {
                Point? match = FindMatchOnScreen(templateImagePath, DefaultConfidenceThreshold, scrollRegion);
                if (match != null)
                {
                    Console.WriteLine($"Found {templateImagePath} while scrolling.");
                    return true;
                }

                Console.WriteLine($"Scrolling to find {templateImagePath}...");
                Device.ScrollUpInRegion(scrollRegion, scrollDistance);
                Thread.Sleep(500);
            }
            Console.WriteLine($"Failed to find {templateImagePath} within timeout.");
            return false;
        }

        private int GetRandomSleepDuration()
        {
            Random random = new Random();
            int randomMinutes = random.Next(MinSleepMinutes, MaxSleepMinutes);
            return randomMinutes * SleepDurationMultiplier;
        }
    }
}
