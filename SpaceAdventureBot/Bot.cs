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
        private const int TapSleepDuration = 2000;
        private const int DefaultTimeout = 15000;

        private Device _device;
        private bool _isFuelEmpty;
        private bool _isShieldBroken;
        private DateTime _lastTimeForceFieldUsed;
        private DateTime _lastTimeSpinned;
        private int _spinCount;

        public Bot(AdbClient adbClient, DeviceData deviceData)
        {
            _device = new Device(adbClient, deviceData);
            _isFuelEmpty = false;
            _isShieldBroken = false;
            _lastTimeForceFieldUsed = DateTime.Now.AddMinutes(-60);
            _lastTimeSpinned = DateTime.Now.AddMinutes(-10);
            _spinCount = 0;
        }

        private void Daily()
        {
            AntiBot();
            Utils.Log("Checking for daily activity...", LogType.Info);
            if (IsMatchOnScreen(Constants.Daily))
            {
                Utils.Log("Daily activity found !", LogType.Info);
                Utils.Log("Checking for double reward...", LogType.Info);
                if (FindWhileScrollingWithTimeout(Constants.DailyAdDoubleReward, Constants.DailyActivityScrollRegion, DefaultTimeout))
                {
                    if (FindAndTap(Constants.DailyAdDoubleReward))
                    {
                        Utils.Log("Double reward found, watching ad...", LogType.Info);

                        DateTime startTime = DateTime.Now;
                        while ((DateTime.Now - startTime).TotalMilliseconds < 32000)
                        {
                            if (IsMatchOnScreen(Constants.Daily) || FindAndTap(Constants.AdFreeClose))
                            {
                                Utils.Log("Ad finished, double reward collected.", LogType.Success);
                                break;
                            }
                            Thread.Sleep(500);
                        }
                        if (!IsMatchOnScreen(Constants.Daily))
                            Utils.Log("Failed to collect double reward.", LogType.Error);
                    }
                }
                if (FindAndTap(Constants.DailyClose))
                    Utils.Log("Daily activity closed.", LogType.Info);
                else
                    Utils.Log("Failed to close daily activity.", LogType.Error);
            }
            else
                Utils.Log("No daily activity found.", LogType.Info);
        }

        public void Start()
        {
            while (true)
            {
                try
                {
                    Utils.Log("Launching Telegram...", LogType.Info);
                    if (!_device.StartApp(Constants.TelegramPackageName, Constants.TelegramMainActivity))
                    {
                        Utils.Log("Failed to launch Telegram.", LogType.Error);
                        continue;
                    }

                    if (FindMatchOnScreen(Constants.SpaceAdventure) == null)
                    {
                        if (!LaunchSpaceAdventure())
                        {
                            Utils.Log("Closing Telegram...", LogType.Warning);
                            _device.CloseApp(Constants.TelegramPackageName);
                            continue;
                        }
                    }

                    Daily();

                    if (!IsMatchOnScreen(Constants.HomeLanguage))
                    {
                        Utils.Log("Screen is not in home page.", LogType.Warning);
                        if (!FindAndTap(Constants.NavHome))
                        {
                            Utils.Log("Failed to navigate to home page.", LogType.Error);
                            _device.CloseApp(Constants.TelegramPackageName);
                            continue;
                        }
                    }

                    Spin();

                    _isFuelEmpty = IsMatchOnScreen(Constants.HomeEmptyFuel, Constants.FuelRegion);
                    _isShieldBroken = IsMatchOnScreen(Constants.HomeBrokenShield, Constants.ShieldRegion);

                    CollectCoin();

                    // TODO: Add check with lastTimeForceFieldUsed
                    if (_isFuelEmpty || _isShieldBroken)

                        Fuel();
                    Shield();

                    Utils.Log("Closing Telegram...", LogType.Info);
                    _device.CloseApp(Constants.TelegramPackageName);

                    int sleepDuration = GetRandomSleepDuration();
                    Console.WriteLine($"Bot is sleeping for {sleepDuration / SleepDurationMultiplier} minutes.");
                    Thread.Sleep(sleepDuration);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    _device.CloseApp(Constants.TelegramPackageName);
                }
            }
        }

        private void AntiBot()
        {
            if (FindAndTap(Constants.AntiBotClose))
                Utils.Log("Anti-bot measure bypassed.", LogType.Success);
        }

        private bool LaunchSpaceAdventure()
        {
            Utils.Log("Launching Space Adventure...", LogType.Info);
            Rect? spaceAdventureRegion = FindRegionOnScreenWithTimeout(Constants.SpaceAdventureRegion, DefaultTimeout);
            if (spaceAdventureRegion != null)
            {
                if (FindAndTap(Constants.SpaceAdventureOpen, spaceAdventureRegion))
                {
                    Utils.Log("Space Adventure game opened.", LogType.Info);
                    if (FindAndTapWithTimeout(Constants.SpaceAdventureStart, DefaultTimeout))
                    {
                        Utils.Log("Game started.", LogType.Success);
                        return true;
                    }
                }
            }
            Utils.Log("Failed to launch Space Adventure.", LogType.Error);
            return false;
        }

        private void CollectCoin()
        {
            AntiBot();
            if (FindAndTap(Constants.HomeCollect))
                Utils.Log("Coin collected.", LogType.Success);
        }

        private void Fuel()
        {
            AntiBot();

            Console.WriteLine("Fuel is empty, filling...");
            if (FindAndTap("Shop"))
            {
                Console.WriteLine("Opened Shop.");
                _device.Screenshot();
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

        private void Shield()
        {
            AntiBot();

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

        private void Spin()
        {
            AntiBot();

            if (FindAndTap(Constants.HomeSpin))
            {
                if (IsMatchOnScreen(Constants.Spin))
                {
                    Utils.Log("Spin button tapped.", LogType.Info);
                    if (FindAndTap(Constants.SpinAdFree))
                    {
                        _lastTimeSpinned = DateTime.Now;
                        Utils.Log("Spin ad found, watching ad...", LogType.Info);

                        DateTime startTime = DateTime.Now;
                        while ((DateTime.Now - startTime).TotalMilliseconds < 32000)
                        {
                            if (IsMatchOnScreen(Constants.SpinSpin) || FindAndTap(Constants.AdFreeClose))
                            {
                                Utils.Log("Ad finished.", LogType.Info);
                                break;
                            }
                            Thread.Sleep(500);
                        }
                        if (FindAndTap(Constants.SpinSpin))
                        {
                            Utils.Log("Spin button tapped, waiting for spin to complete...", LogType.Info);
                            if (FindMatchOnScreenWithTimeout(Constants.Spin, DefaultTimeout) != null)
                            {
                                Utils.Log("Spin completed.", LogType.Success);
                                _spinCount++;
                            }
                            else
                                Utils.Log("Failed to complete spin.", LogType.Error);
                        }
                    }
                    if (FindAndTap(Constants.SpinClose))
                        Utils.Log("Spin page closed.", LogType.Info);
                    else
                        Utils.Log("Failed to close spin page.", LogType.Error);
                }
            }
        }

        private bool FindAndTap(string templateImagePath, Rect? region = null)
        {
            Point? match = FindMatchOnScreen(templateImagePath, DefaultConfidenceThreshold, region);
            if (match == null)
                return false;

            Console.WriteLine($"Tapping on {templateImagePath}...");
            _device.Tap(match.Value);
            Thread.Sleep(TapSleepDuration);
            return true;
        }

        private bool FindAndTapWithTimeout(string templateImagePath, int timeoutMilliseconds, Rect? region = null)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMilliseconds)
            {
                AntiBot();
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
                    _device.Screenshot();

                    using (var mainImage = Cv2.ImRead("screenshot.png", ImreadModes.Color))
                    using (var templateImage = Cv2.ImRead(imagePath, ImreadModes.Color))
                    {
                        if (mainImage.Empty() || templateImage.Empty())
                        {
                            Utils.Log("Could not load images.", LogType.Error);
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

                            //Utils.Log($"Match found at: Top-Left={maxLoc}, Center=({centerX}, {centerY}) with confidence {maxVal}", LogType.Info);
                            return new Point(centerX, centerY);
                        }
                    }
                }
                catch (IOException ex)
                {
                    Utils.Log(ex.Message, LogType.Warning);
                    if (attempt < maxRetries - 1)
                    {
                        Utils.Log("Retrying...", LogType.Warning);
                        Thread.Sleep(delayBetweenRetries);
                    }
                    else
                    {
                        Utils.Log("Max retries reached. Unable to access the file.", LogType.Error);
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

            using var templateImage = Cv2.ImRead(referenceImagePath, ImreadModes.Color);
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
                AntiBot();
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
                AntiBot();
                Point? match = FindMatchOnScreen(templateImagePath);
                if (match != null)
                {
                    Utils.Log($"Found {templateImagePath} while scrolling.", LogType.Success);
                    return true;
                }

                Utils.Log($"Scrolling to find {templateImagePath}...", LogType.Info);
                _device.ScrollUpInRegion(scrollRegion, scrollDistance);
                Thread.Sleep(500);
            }
            Utils.Log($"Failed to find {templateImagePath} within timeout.", LogType.Warning);
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
