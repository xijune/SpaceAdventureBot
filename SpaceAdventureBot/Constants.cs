using OpenCvSharp;

namespace SpaceAdventureBot
{
    public static class Constants
    {
        public const int SleepDurationMultiplier = 60 * 1000;

        public const string TelegramPackageName = "org.telegram.messenger";
        public const string TelegramMainActivity = "org.telegram.ui.LaunchActivity";
        public static readonly Rect ShieldRegion = new Rect { X = 1060, Y = 380, Width = 80, Height = 50 };
        public static readonly Rect FuelRegion = new Rect { X = 460, Y = 380, Width = 120, Height = 50 };
        public static readonly Rect DailyActivityScrollRegion = new Rect { X = 567, Y = 538, Width = 419, Height = 289 };
        public static readonly Rect TasksScrollRegion = new Rect { X = 549, Y = 203, Width = 446, Height = 447 };

        public static readonly Rect ActivityVerificationCodeRegion = new Rect { X = 760, Y = 500, Width = 80, Height = 25 };

        // Image paths
        public const string SpaceAdventureRegion = "images/SpaceAdventureChat.png";
        public const string SpaceAdventure = "images/SpaceAdventureLbl.png";
        public const string SpaceAdventureOpen = "images/SpaceAdventureOpenBtn.png";
        public const string SpaceAdventureStart = "images/SpaceAdventureStartBtn.png";

        public const string AdFreeClose = "images/AdFree/CloseBtn.png";

        public const string AntiBotClose = "images/AntiBot/CloseBtn.png";
        public const string AntiBotClose2 = "images/AntiBot/CloseBtn2.png";
        public const string AntiBotClose3 = "images/AntiBot/CloseBtn3.png";
        public const string AntiBotActivityVerification = "images/AntiBot/ActivityVerificationLbl.png";
        public const string AntiBotActivityVerificationConfirm = "images/AntiBot/ActivityVerificationConfirmBtn.png";
        public const string AntiBotActivityVerificationInput = "images/AntiBot/ActivityVerificationInputRegion.png";

        public const string DailyAdDoubleReward = "images/DailyActivity/AdDoubleRewardBtn.png";
        public const string DailyClose = "images/DailyActivity/CloseBtn.png";
        public const string Daily = "images/DailyActivity/DailyActivityLogo.png";

        public const string HomeBrokenShield = "images/Home/BrokenShieldLbl.png";
        public const string HomeCollect = "images/Home/CollectBtn.png";
        public const string HomeEmptyFuel = "images/Home/EmptyFuelLbl.png";
        public const string HomeLanguage = "images/Home/LanguageBtn.png";
        public const string HomePayout = "images/Home/PayoutBtn.png";
        public const string HomeSpin = "images/Home/SpinLbl.png";
        public const string HomeLevel = "images/Home/YourLevelLbl.png";

        public const string NavHome = "images/NavBar/HomeBtn.png";
        public const string NavShop = "images/NavBar/ShopBtn.png";
        public const string NavTasks = "images/NavBar/TasksBtn.png";

        public const string ShopAdFree = "images/Shop/AdFreeBtn.png";
        public const string ShopClose = "images/Shop/CloseBtn.png";
        public const string ShopForceField = "images/Shop/ForceFieldLogo.png";
        public const string ShopFuel = "images/Shop/FuelLogo.png";
        public const string ShopLeft = "images/Shop/LeftBtn.png";
        public const string ShopRepairShield = "images/Shop/RepairShieldLogo.png";
        public const string ShopRight = "images/Shop/RightBtn.png";
        public const string Shop = "images/Shop/ShopLbl.png";

        public const string SpinAdFree = "images/Spin/AdFreeBtn.png";
        public const string SpinClose = "images/Spin/CloseBtn.png";
        public const string SpinSpin = "images/Spin/SpinBtn.png";
        public const string Spin = "images/Spin/SpinLogo.png";

        public const string TasksCheck = "images/Tasks/CheckBtn.png";
        public const string TasksClose = "images/Tasks/CloseBtn.png";
        public const string TasksCompletedSection = "images/Tasks/CompletedTasksLbl.png";
        public const string TasksEnterTask = "images/Tasks/EnterTaskBtn.png";

        public const string TasksSponsorsWatch5AdsRegion = "images/Tasks/Sponsors/Watch5AdsRegion.png";
        public const string TasksSponsorsWatch = "images/Tasks/Sponsors/WatchBtn.png";

        public const string TasksOurAddBountyPlayRegion = "images/Tasks/Our/AddBountyPlayToNameRegion.png";
        public const string TasksOurAddRocketRegion = "images/Tasks/Our/AddRocketToNameRegion.png";
        public const string TasksOurReward = "images/Tasks/Our/OurRewardsBtn.png";
        public const string TasksOurSpin10TimesRegion = "images/Tasks/Our/Spin10TimesRegion.png";
    }
}
