using OpenCvSharp;

namespace SpaceAdventureBot
{
    public static class Constants
    {
        public const string TelegramPackageName = "org.telegram.messenger";
        public const string TelegramMainActivity = "org.telegram.ui.LaunchActivity";
        public static readonly Rect ShieldRegion = new Rect { X = 1072, Y = 383, Width = 63, Height = 222 };
        public static readonly Rect FuelRegion = new Rect { X = 479, Y = 371, Width = 116, Height = 245 };
        public static readonly Rect DailyActivityScrollRegion = new Rect { X = 567, Y = 538, Width = 419, Height = 289 };
    }
}
