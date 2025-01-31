using OpenCvSharp;
using SharpAdbClient;
using System.Runtime.InteropServices;

namespace SpaceAdventureBot
{
    class Bot
    {
        private const double DefaultConfidenceThreshold = 0.8;
        private const int TapSleepDuration = 2000;
        private const int DefaultTimeout = 15000;

        private Device _device;
        private bool _isFuelEmpty;
        private bool _isShieldBroken;
        private DateTime _lastTimeForceFieldUsed;
        private DateTime _lastTimeSpinned;
        private int _spinCount;
        private Tasks _tasks;
        DateTime _nextSleepTime;
        DateTime _nextTaskResetTime;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        public Bot(AdbClient adbClient, DeviceData deviceData)
        {
            _device = new Device(adbClient, deviceData);
            _isFuelEmpty = false;
            _isShieldBroken = false;
            _lastTimeForceFieldUsed = DateTime.Now.AddMinutes(-60);
            _lastTimeSpinned = DateTime.Now.AddMinutes(-10);
            _spinCount = 0;
            _nextSleepTime = GetNextSleepTime();
            _nextTaskResetTime = DateTime.Today.AddHours(22).AddMinutes(10);
            _tasks = new Tasks()
            {
                IsWatch5AdsCompleted = true,
                IsAddRocketCompleted = true,
                IsSpin10TimesCompleted = false,
                IsAddBountyPlayCompleted = false
            };

            // Allocate a new console for this bot instance
            AllocConsole();
            Console.Title = $"Bot Console - {deviceData.Serial}";
        }

        private void Daily()
        {
            Utils.Log("DAILY: Checking for daily activity...", LogType.Info);
            if (IsMatchOnScreen(Constants.Daily))
            {
                Utils.Log("DAILY: Daily activity found !", LogType.Info);
                Utils.Log("DAILY: Checking for double reward...", LogType.Info);
                if (FindWhileScrollingWithTimeout(Constants.DailyAdDoubleReward, Constants.DailyActivityScrollRegion, DefaultTimeout))
                {
                    if (FindAndTap(Constants.DailyAdDoubleReward))
                    {
                        Utils.Log("DAILY: Double reward found, watching ad...", LogType.Info);

                        DateTime startTime = DateTime.Now;
                        while ((DateTime.Now - startTime).TotalMilliseconds < 32000)
                        {
                            if (IsMatchOnScreen(Constants.Daily) || FindAndTap(Constants.AdFreeClose))
                            {
                                Utils.Log("DAILY: Ad finished, double reward collected.", LogType.Success);
                                break;
                            }
                            Thread.Sleep(500);
                        }
                        if (!IsMatchOnScreen(Constants.Daily))
                            Utils.Log("DAILY: Failed to collect double reward.", LogType.Warning);
                    }
                }
                if (FindAndTap(Constants.DailyClose))
                    Utils.Log("DAILY: Daily activity closed.", LogType.Info);
                else
                    Utils.Log("DAILY: Failed to close daily activity.", LogType.Error);
            }
            else
                Utils.Log("DAILY: No daily activity found.", LogType.Info);
        }

        public void Start()
        {


            while (true)
            {
                try
                {
                    // Check if it's time to reset tasks
                    if (DateTime.Now >= _nextTaskResetTime)
                    {
                        Utils.Log("Bot is going to reset tasks...", LogType.Info);
                        ResetTasks();
                        _nextTaskResetTime = GetNextTaskResetTime();
                        Utils.Log("Tasks have been reset.", LogType.Success);
                    }

                    Utils.Log("LAUNCH: Launching Telegram...", LogType.Info);
                    if (!_device.StartApp(Constants.TelegramPackageName, Constants.TelegramMainActivity))
                    {
                        Utils.Log("LAUNCH: Failed to launch Telegram.", LogType.Error);
                        continue;
                    }

                    if (FindMatchOnScreen(Constants.SpaceAdventure) == null)
                    {
                        if (!LaunchSpaceAdventure())
                        {
                            Utils.Log("LAUNCH: Failed to launch Space Adventure.", LogType.Error);
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
                        else
                            Utils.Log("Navigated to home page.", LogType.Success);
                    }

                    if (!Spin())
                    {
                        _device.CloseApp(Constants.TelegramPackageName);
                        continue;
                    }

                    _isFuelEmpty = IsMatchOnScreen(Constants.HomeEmptyFuel, Constants.FuelRegion, 0.9);
                    _isShieldBroken = IsMatchOnScreen(Constants.HomeBrokenShield, Constants.ShieldRegion, 0.91);

                    CollectCoin();

                    if (!Shop())
                    {
                        _device.CloseApp(Constants.TelegramPackageName);
                        continue;
                    }

                    //Tasks();

                    Utils.Log("Closing Telegram...", LogType.Info);
                    _device.CloseApp(Constants.TelegramPackageName);

                    //if (DateTime.Now >= _nextSleepTime)
                    //{
                    //    Utils.Log("Bot is going to sleep...", LogType.Info);
                    //    int sleepDuration = Utils.GetRandomSleepDuration(240, 360);
                    //    CountdownSleep(sleepDuration);
                    //    _nextSleepTime = GetNextSleepTime();
                    //}

                    CountdownSleep(Utils.GetRandomSleepDuration(10, 14));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    _device.CloseApp(Constants.TelegramPackageName);
                }
            }
        }

        private bool Watch5Ads()
        {
            if (!_tasks.IsWatch5AdsCompleted)
            {
                if (FindWhileScrollingWithTimeout(Constants.TasksSponsorsWatch5AdsRegion, Constants.TasksScrollRegion, DefaultTimeout))
                {
                    Rect? watch5AdsRegion = FindRegionOnScreen(Constants.TasksSponsorsWatch5AdsRegion);
                    if (watch5AdsRegion != null)
                    {
                        if (FindAndTap(Constants.TasksEnterTask, watch5AdsRegion))
                        {
                            int watch5AdsCount = 0;
                            while (watch5AdsCount < 5)
                            {
                                if (FindAndTapWithTimeout(Constants.TasksSponsorsWatch, 2000))
                                {
                                    Utils.Log("TASKS: Watch button tapped.", LogType.Info);
                                    DateTime startTime = DateTime.Now;
                                    while ((DateTime.Now - startTime).TotalMilliseconds < 32000)
                                    {
                                        if (IsMatchOnScreen(Constants.TasksSponsorsWatch) || FindAndTap(Constants.AdFreeClose))
                                        {
                                            watch5AdsCount++;
                                            Utils.Log($"TASKS: Ad finished. ({watch5AdsCount}/5)", LogType.Info);
                                            break;
                                        }
                                        Thread.Sleep(500);
                                    }
                                    if (!IsMatchOnScreen(Constants.TasksSponsorsWatch))
                                        Utils.Log("TASKS: Failed to watch the ad.", LogType.Warning);
                                }
                                else
                                {
                                    Utils.Log("TASKS: Failed to tap watch button.", LogType.Warning);
                                    break;
                                }
                            }

                            _tasks.IsWatch5AdsCompleted = watch5AdsCount == 5;
                            if (_tasks.IsWatch5AdsCompleted)
                            {
                                Utils.Log("TASKS: Watch 5 ads task completed.", LogType.Success);
                            }
                            else
                                Utils.Log("TASKS: Failed to complete watch 5 ads task.", LogType.Warning);

                            if (FindAndTap(Constants.TasksClose))
                            {
                                Utils.Log("TASKS: Task closed.", LogType.Info);
                            }
                            else
                            {
                                Utils.Log("TASKS: Failed to close the task.", LogType.Warning);
                                return false;
                            }
                        }
                        else
                        {
                            Utils.Log("TASKS: Failed to enter the task.", LogType.Warning);
                            return false;
                        }
                    }
                    else
                    {
                        Utils.Log("TASKS: Failed to find watch 5 ads task.", LogType.Warning);
                        return false;
                    }
                }
                else
                {
                    Utils.Log("TASKS: Failed to find watch 5 ads task.", LogType.Warning);
                    return false;
                }
            }
            return true;
        }

        private bool OurReward()
        {
            if (!_tasks.IsSpin10TimesCompleted || !_tasks.IsAddRocketCompleted || !_tasks.IsAddBountyPlayCompleted)
            {
                if (FindWhileScrollingWithTimeout(Constants.TasksOurReward, Constants.TasksScrollRegion, DefaultTimeout, scrollType: ScrollType.Down))
                {
                    if (FindAndTap(Constants.TasksOurReward))
                    {
                        if (!CollectAddRocketTask())
                            return false;


                        if (!_tasks.IsSpin10TimesCompleted)
                        {

                        }

                        if (!_tasks.IsAddBountyPlayCompleted)
                        {

                        }
                    }
                }
            }
            return true;
        }

        private bool CollectAddRocketTask()
        {
            if (!_tasks.IsAddRocketCompleted)
            {
                if (FindWhileScrollingWithTimeout(Constants.TasksOurAddRocketRegion, Constants.TasksScrollRegion, DefaultTimeout))
                {
                    Rect? addRocketRegion = FindRegionOnScreen(Constants.TasksOurAddRocketRegion);
                    if (addRocketRegion != null)
                    {
                        if (FindAndTap(Constants.TasksEnterTask, addRocketRegion))
                        {
                            if (FindWhileScrollingWithTimeout(Constants.TasksCheck, Constants.TasksScrollRegion, DefaultTimeout))
                            {
                                if (FindAndTap(Constants.TasksCheck))
                                {
                                    Utils.Log("TASKS: Reward successfuly collected !.", LogType.Success);
                                    return true;
                                }
                                else
                                {
                                    Utils.Log("TASKS: Failed to check the task.", LogType.Warning);
                                    if (!FindAndTap(Constants.TasksClose))
                                    {
                                        Utils.Log("TASKS: Failed to close the task.", LogType.Warning);
                                    }
                                    return false;
                                }

                            }
                            else
                            {
                                Utils.Log("TASKS: Failed to find check button.", LogType.Warning);
                                return false;
                            }
                        }
                        else
                        {
                            Utils.Log("TASKS: Failed to enter the task.", LogType.Warning);
                            return false;
                        }
                    }
                    else
                    {
                        Utils.Log("TASKS: Failed to find add rocket task.", LogType.Warning);
                        return false;
                    }
                }
                else
                {
                    Utils.Log("TASKS: Failed to find add rocket task.", LogType.Warning);
                    return false;
                }
            }
            return true;
        }

        private bool Tasks()
        {
            if (!_tasks.IsSpin10TimesCompleted || !_tasks.IsAddRocketCompleted || !_tasks.IsAddBountyPlayCompleted || !_tasks.IsWatch5AdsCompleted)
            {
                if (FindAndTap(Constants.NavTasks))
                {
                    Utils.Log("TASKS: Navigated to tasks.", LogType.Info);

                    Watch5Ads();

                    OurReward();

                    //// Add bounty play
                    //if (!_tasks.IsAddBountyPlayCompleted)
                    //{
                    //    if (FindWhileScrollingWithTimeout(Constants.TasksOurAddBountyPlayRegion, TODO, DefaultTimeout))
                    //    {
                    //        if (FindAndTap(Constants.TasksEnterTask, TODO))
                    //        {
                    //        }
                    //        else
                    //        {
                    //            Utils.Log("TASKS: Failed to enter the task.", LogType.Warning);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Utils.Log("TASKS: Failed to find watch 5 ads task.", LogType.Warning);
                    //    }
                    //}


                    if (FindAndTap(Constants.NavHome))
                        Utils.Log("TASKS: Navigated to home.", LogType.Info);
                    else
                    {
                        Utils.Log("TASKS: Failed to navigate to home.", LogType.Warning);
                        return false;
                    }
                }
                else
                {
                    Utils.Log("TASKS: Failed to navigate to tasks", LogType.Warning);
                    return false;
                }
            }
            return true;
        }

        private bool Shop()
        {
            if (_isFuelEmpty || _isShieldBroken || DateTime.Now > _lastTimeForceFieldUsed.AddMinutes(60))
            {
                if (FindAndTap(Constants.NavShop))
                {
                    Utils.Log("SHOP: Navigated to shop.", LogType.Info);

                    // Filling Fuel
                    if (_isFuelEmpty)
                    {
                        if (IsMatchOnScreen(Constants.ShopFuel))
                        {
                            Utils.Log("FUEL: Fuel is empty, filling...", LogType.Info);
                            if (FindAndTap(Constants.ShopAdFree))
                            {
                                Utils.Log("FUEL: Ad-free found, watching ad...", LogType.Info);
                                DateTime startTime = DateTime.Now;
                                while ((DateTime.Now - startTime).TotalMilliseconds < 32000)
                                {
                                    if (IsMatchOnScreen(Constants.ShopFuel) || FindAndTap(Constants.AdFreeClose))
                                    {
                                        Utils.Log("FUEL: Ad finished, fuel filled.", LogType.Success);
                                        break;
                                    }
                                    Thread.Sleep(500);
                                }
                                if (!IsMatchOnScreen(Constants.ShopFuel))
                                    Utils.Log("FUEL: Failed to collect double reward.", LogType.Warning);
                            }
                        }
                    }

                    // Repairing Shield
                    if (_isShieldBroken)
                    {
                        if (FindAndTap(Constants.ShopRight))
                        {
                            Utils.Log("SHIELD: Navigated to the right in the Shop.", LogType.Info);
                            if (IsMatchOnScreen(Constants.ShopRepairShield))
                            {
                                Utils.Log("SHIELD: Shield is broken, repairing...", LogType.Info);
                                if (FindAndTap(Constants.ShopAdFree))
                                {
                                    Utils.Log("SHIELD: Ad-free found, watching ad...", LogType.Info);
                                    DateTime startTime = DateTime.Now;
                                    while ((DateTime.Now - startTime).TotalMilliseconds < 32000)
                                    {
                                        if (IsMatchOnScreen(Constants.ShopRepairShield) || FindAndTap(Constants.AdFreeClose))
                                        {
                                            Utils.Log("SHIELD: Ad finished, shield repaired.", LogType.Success);
                                            break;
                                        }
                                        Thread.Sleep(500);
                                    }
                                    if (!IsMatchOnScreen(Constants.ShopRepairShield))
                                        Utils.Log("SHIELD: Failed to repair shield.", LogType.Warning);
                                }
                            }
                        }
                    }

                    // Using Force Field
                    if (DateTime.Now > _lastTimeForceFieldUsed.AddMinutes(60))
                    {
                        Utils.Log("FORCEFIELD: Force field is ready, navigating to it", LogType.Info);
                        if (IsMatchOnScreen(Constants.ShopFuel))
                            if (FindAndTap(Constants.ShopRight))
                                Utils.Log("FORCEFIELD: Navigated to the right in the Shop.", LogType.Info);

                        if (IsMatchOnScreen(Constants.ShopRepairShield))
                            if (FindAndTap(Constants.ShopRight))
                                Utils.Log("FORCEFIELD: Navigated to the right in the Shop.", LogType.Info);

                        if (IsMatchOnScreen(Constants.ShopForceField))
                        {
                            if (FindAndTap(Constants.ShopAdFree))
                            {
                                Utils.Log("FORCEFIELD: Ad-free found, watching ad...", LogType.Info);
                                DateTime startTime = DateTime.Now;
                                while ((DateTime.Now - startTime).TotalMilliseconds < 32000)
                                {
                                    if (IsMatchOnScreen(Constants.ShopForceField) || FindAndTap(Constants.AdFreeClose))
                                    {
                                        Utils.Log("FORCEFIELD: Ad finished, force field used.", LogType.Success);
                                        _lastTimeForceFieldUsed = DateTime.Now;
                                        break;
                                    }
                                    Thread.Sleep(500);
                                }
                                if (!IsMatchOnScreen(Constants.ShopForceField))
                                    Utils.Log("FORCEFIELD: Failed to use force field.", LogType.Warning);
                            }
                        }
                    }

                    if (FindAndTapWithTimeout(Constants.ShopClose, DefaultTimeout))
                        Utils.Log("SHOP: Shop closed.", LogType.Info);
                    else
                    {
                        Utils.Log("SHOP: Failed to close shop.", LogType.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        private bool LaunchSpaceAdventure()
        {
            Utils.Log("LAUNCH: Launching Space Adventure...", LogType.Info);
            Rect? spaceAdventureRegion = FindRegionOnScreenWithTimeout(Constants.SpaceAdventureRegion, DefaultTimeout);
            if (spaceAdventureRegion != null)
            {
                if (FindAndTap(Constants.SpaceAdventureOpen, spaceAdventureRegion, 4000))
                {
                    Utils.Log("LAUNCH: Space Adventure launched.", LogType.Info);
                    if (FindAndTapWithTimeout(Constants.SpaceAdventureStart, DefaultTimeout))
                    {
                        Utils.Log("LAUNCH: Game started.", LogType.Success);
                        return true;
                    }
                }
            }
            return false;
        }

        private void CollectCoin()
        {
            if (FindAndTap(Constants.HomeCollect))
                Utils.Log("COIN: Coin collected.", LogType.Success);
        }

        private bool Spin()
        {
            if (DateTime.Now > _lastTimeSpinned.AddMinutes(10))
            {
                if (FindAndTap(Constants.HomeSpin))
                {
                    if (IsMatchOnScreen(Constants.Spin))
                    {
                        Utils.Log("SPIN: Spin button tapped.", LogType.Info);
                        if (FindAndTap(Constants.SpinAdFree))
                        {
                            Utils.Log("SPIN: Spin ad found, watching ad...", LogType.Info);

                            DateTime startTime = DateTime.Now;
                            while ((DateTime.Now - startTime).TotalMilliseconds < 32000)
                            {
                                if (IsMatchOnScreen(Constants.SpinSpin) || FindAndTap(Constants.AdFreeClose))
                                {
                                    Utils.Log("SPIN: Ad finished.", LogType.Info);
                                    break;
                                }
                                Thread.Sleep(500);
                            }
                            if (FindAndTap(Constants.SpinSpin))
                            {
                                Utils.Log("SPIN: Spin button tapped, waiting for spin to complete...", LogType.Info);
                                if (FindMatchOnScreenWithTimeout(Constants.Spin, DefaultTimeout) != null)
                                {
                                    Utils.Log("SPIN: Spin completed.", LogType.Success);
                                    _lastTimeSpinned = DateTime.Now;
                                    _spinCount++;
                                }
                                else
                                {
                                    Utils.Log("SPIN: Failed to complete spin.", LogType.Error);
                                    return false;
                                }
                            }
                        }
                        else
                            Utils.Log("SPIN: No ad found.", LogType.Info);
                        if (FindAndTap(Constants.SpinClose))
                            Utils.Log("SPIN: Spin page closed.", LogType.Info);
                        else
                        {
                            Utils.Log("SPIN: Failed to close spin page.", LogType.Error);
                            return false;
                        }
                    }
                }
                else
                {
                    Utils.Log("SPIN: Failed to tap spin button.", LogType.Error);
                    return false;
                }
            }
            return true;
        }

        private bool FindAndTap(string templateImagePath, Rect? region = null, int tapSleepDuration = TapSleepDuration)
        {
            Point? match = FindMatchOnScreen(templateImagePath, DefaultConfidenceThreshold, region);
            if (match == null)
                return false;

            _device.Tap(match.Value);
            Thread.Sleep(tapSleepDuration);
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
                Point? match = FindMatchOnScreen(imagePath, confidenceThreshold, region);
                if (match != null)
                    return match;

                Thread.Sleep(500);
            }
            return null;
        }

        private bool FindWhileScrollingWithTimeout(string templateImagePath, Rect scrollRegion, int timeoutMilliseconds, int scrollDistance = 500, ScrollType scrollType = ScrollType.Up)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMilliseconds)
            {
                Point? match = FindMatchOnScreen(templateImagePath);
                if (match != null)
                {
                    Utils.Log($"Found {templateImagePath} while scrolling.", LogType.Info);
                    return true;
                }

                Utils.Log($"Scrolling to find {templateImagePath}...", LogType.Info);
                switch (scrollType)
                {
                    case ScrollType.Up:
                        _device.ScrollUpInRegion(scrollRegion, scrollDistance);
                        break;
                    case ScrollType.Down:
                        _device.ScrollDownInRegion(scrollRegion, scrollDistance);
                        break;
                }
                Thread.Sleep(500);
            }
            Utils.Log($"Failed to find {templateImagePath} within timeout.", LogType.Warning);
            return false;
        }

        private void CountdownSleep(int sleepDuration)
        {
            DateTime endTime = DateTime.Now.AddMilliseconds(sleepDuration);
            while (DateTime.Now < endTime)
            {
                if (DateTime.Now >= _nextTaskResetTime)
                {
                    Utils.Log("Bot is going to reset tasks...", LogType.Info);
                    ResetTasks();
                    _nextTaskResetTime = GetNextTaskResetTime();
                }

                TimeSpan remainingTime = endTime - DateTime.Now;
                Utils.Log($"Bot is sleeping for {remainingTime.Hours} hours, {remainingTime.Minutes} minutes, and {remainingTime.Seconds} seconds.", LogType.Info);
                Thread.Sleep(1000);
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
        }

        private void ResetTasks()
        {
            _tasks = new Tasks()
            {
                IsWatch5AdsCompleted = false,
                IsAddBountyPlayCompleted = false,
                IsAddRocketCompleted = false,
                IsSpin10TimesCompleted = false
            };
            Utils.Log("TASKS: Tasks have been reset.", LogType.Info);
        }

        private DateTime GetNextTaskResetTime() => DateTime.Today.AddDays(1).AddHours(22).AddMinutes(10);

        private DateTime GetNextSleepTime()
        {
            Random random = new Random();
            int sleepHour = random.Next(0, 24);
            int sleepMinute = random.Next(0, 60);
            return DateTime.Today.AddHours(sleepHour).AddMinutes(sleepMinute);
        }
    }
}
