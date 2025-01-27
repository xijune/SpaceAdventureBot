using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public static class Constants
    {
        public const string TelegramPackageName = "org.telegram.messenger";
        public const string TelegramMainActivity = "org.telegram.ui.LaunchActivity";
        public static readonly Rect FuelRegion = new Rect { X = 1072, Y = 383, Width = 63, Height = 222 };
        public static readonly Rect DailyActivityScrollRegion = new Rect { X = 567, Y = 538, Width = 419, Height = 289 };
    }
}
