﻿using Steamworks;
using UnityEngine;

namespace OverhaulMod.Utils
{
    public static class ModParseUtils
    {
        public static int TryParseToInt(string @string, int defaultValue)
        {
            return !int.TryParse(@string, out int result) ? defaultValue : result;
        }

        public static float TryParseToFloat(string @string, float defaultValue)
        {
            return string.IsNullOrEmpty(@string) || !float.TryParse(@string.Replace('.', ','), out float result) ? defaultValue : result;
        }

        public static bool TryParseToBool(string @string, bool defaultValue)
        {
            return !bool.TryParse(@string, out bool result) ? defaultValue : result;
        }

        public static ulong TryParseToULong(string @string, ulong defaultValue)
        {
            return !ulong.TryParse(@string, out ulong result) ? defaultValue : result;
        }

        public static Color TryParseToColor(string @string, Color defaultValue)
        {
            return !ColorUtility.TryParseHtmlString(@string, out Color result) ? defaultValue : result;
        }

        public static CSteamID TryParseToSteamID(string @string)
        {
            return !ulong.TryParse(@string, out ulong result) ? default : (CSteamID)result;
        }
    }
}
