using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using System;

namespace BoxOptions.Common.Extensions
{
    public static class Extensions
    {
        public static string ToTimeString(this DateTime date)
        {
            return date.ToString("HH:mm:ss.fff");
        }
        public static string ToDateTimeString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        public static string ToTimeString(this TimeSpan timespan)
        {
            return timespan.ToString("HH:mm:ss.fff");
        }

        public static BoxSize ToDto(this IBoxSize src)
        {
            return new BoxSize
            {
                AssetPair = src.AssetPair,
                BoxesPerRow = src.BoxesPerRow,
                BoxHeight = src.BoxHeight,
                BoxWidth = src.BoxWidth,
                TimeToFirstBox = src.TimeToFirstBox,
                SaveHistory = src.SaveHistory,
                GameAllowed = src.GameAllowed,
                ScaleK = src.ScaleK
            };
        }

    }
}
