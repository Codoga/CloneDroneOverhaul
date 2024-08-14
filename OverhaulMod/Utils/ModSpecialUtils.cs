using InternalModBot;
using System;
using System.Runtime.InteropServices;

namespace OverhaulMod.Utils
{
    public static class ModSpecialUtils
    {
        private static class InnerModSpecialUtils
        {
            private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
            private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

            public const string INITIAL_TITLE_BAR_TEXT = "Clone Drone in the Danger Zone";

            private static string s_titleBarText;

            /// <summary>
            /// Find a window with name
            /// </summary>
            /// <param name="className"></param>
            /// <param name="windowName"></param>
            /// <returns></returns>
            [DllImport("user32.dll", EntryPoint = "FindWindow")]
            private static extern System.IntPtr FindWindow(string className, string windowName);

            /// <summary>
            /// Set property value of window
            /// </summary>
            /// <param name="hwnd"></param>
            /// <param name="attr"></param>
            /// <param name="attrValue"></param>
            /// <param name="attrSize"></param>
            /// <returns></returns>
            [DllImport("dwmapi.dll")]
            private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

            /// <summary>
            /// Set title bar text
            /// </summary>
            /// <param name="hwnd"></param>
            /// <param name="lpString"></param>
            /// <returns></returns>
            [DllImport("user32.dll", EntryPoint = "SetWindowText")]
            private static extern bool SetWindowText(System.IntPtr hwnd, string lpString);

            private static bool useImmersiveDarkMode(IntPtr handle, bool enabled)
            {
                if (IsWinNTCurrentBuildOrGreater(17763))
                {
                    int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                    if (IsWinNTCurrentBuildOrGreater(18985))
                    {
                        attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                    }

                    int useImmersiveDarkMode = enabled ? 1 : 0;
                    return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
                }
                return false;
            }

            public static bool IsWinNTCurrentBuildOrGreater(int build)
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Build >= build;
            }

            public static void SetTitleBarDarkModeEnabled(bool enabled)
            {
                IntPtr intPtr = FindWindow(null, s_titleBarText.IsNullOrEmpty() ? INITIAL_TITLE_BAR_TEXT : s_titleBarText);
                if (intPtr != null)
                {
                    _ = useImmersiveDarkMode(intPtr, enabled);
                }
            }

            public static void SetTitleBarText(string text)
            {
                if (text.IsNullOrEmpty())
                    return;

                string windowTitle = s_titleBarText.IsNullOrEmpty() ? INITIAL_TITLE_BAR_TEXT : s_titleBarText;

                IntPtr intPtr = FindWindow(null, windowTitle);
                if (intPtr != null)
                {
                    _ = SetWindowText(intPtr, text);
                    s_titleBarText = text;
                }
            }
        }

        public static bool SupportsTitleBarOverhaul()
        {
            bool result;
            try
            {
                result = InnerModSpecialUtils.IsWinNTCurrentBuildOrGreater(17763);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static void SetOverhauledTitleBarState()
        {
            SetTitleBarDarkModeEnabled(true);
            SetTitleBarText("Clone Drone with Overhaul mod");
        }

        public static void RestoreInitialTitleBarState()
        {
            SetTitleBarDarkModeEnabled(false);
            SetTitleBarText(InnerModSpecialUtils.INITIAL_TITLE_BAR_TEXT);
        }

        public static void SetTitleBarStateDependingOnSettings()
        {
            if (ModCore.EnableTitleBarOverhaul && ModCore.isEnabled)
                SetOverhauledTitleBarState();
            else
                RestoreInitialTitleBarState();
        }

        public static void SetTitleBarDarkModeEnabled(bool enabled)
        {
            try
            {
                InnerModSpecialUtils.SetTitleBarDarkModeEnabled(enabled);
            }
            catch
            {

            }
        }

        public static void SetTitleBarText(string text)
        {
            try
            {
                InnerModSpecialUtils.SetTitleBarText(text);
            }
            catch
            {

            }
        }

        public static bool IsModEnabled(string id)
        {
            foreach (ModLibrary.Mod mod in ModsManager.Instance.GetAllLoadedActiveMods())
            {
                if (mod != null && mod.ModInfo != null && mod.ModInfo.UniqueID == id && mod.ModInfo.IsModEnabled)
                    return true;
            }
            return false;
        }
    }
}
