using OverhaulMod.Utils;
using System;
using System.IO;

namespace OverhaulMod
{
    public static class ModBuildInfo
    {
        public const bool ENABLE_V5 = false;

        internal const string EXTRA_INFO_FILE_PATH = "buildInfo.json";

        private static bool s_loaded;

        public static ExtraInfo extraInfo { get; private set; }

        public static bool extraInfoError
        {
            get
            {
                return extraInfo == null;
            }
        }

        public static int versionMajor
        {
            get;
            private set;
        }

        public static int versionMinor
        {
            get;
            private set;
        }

        public static int versionBuild
        {
            get;
            private set;
        }

        public static int versionRevision
        {
            get;
            private set;
        }

        private static Version s_version;
        public static Version version
        {
            get
            {
                if (s_version == null)
                {
                    s_version = new Version(versionMajor, versionMinor, versionBuild, versionRevision);
                }
                return s_version;
            }
        }

        private static string s_versionString;
        public static string versionString
        {
            get
            {
                if (s_versionString == null)
                {
                    s_versionString = $"{versionMajor}.{versionMinor}.{versionBuild}.{versionRevision}";
                }
                return s_versionString;
            }
        }

        private static string s_fullVersionString;
        public static string fullVersionString
        {
            get
            {
                if (s_fullVersionString == null)
                {
                    string buildRelease = debug ? "internal" : "public";
                    if (extraInfo != null && !extraInfoError)
                    {
                        string date = extraInfo.CompileTime.ToShortDateString();
                        s_fullVersionString = $"{versionString} ({date}, {buildRelease})";
                    }
                    else
                    {
                        s_fullVersionString = $"{versionString} (na, {buildRelease})";
                    }
                }
                return s_fullVersionString;
            }
        }

        public static bool debug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static bool enableV5
        {
            get
            {
                return ENABLE_V5;
            }
        }

        public static bool isPrereleaseBuild
        {
            get
            {
                return true;
            }
        }

        internal static void Load()
        {
            if (s_loaded)
                return;

            loadVersion();
            loadExtraInfo();
            s_loaded = true;
        }

        private static void loadVersion()
        {
            Version version = ModCache.modAssemblyName.Version;
            int major = version.Major;
            int minor = version.Minor;
            int build = version.Build;
            int revision = version.Revision;

            versionMajor = major;
            versionMinor = minor;
            versionBuild = build;
            versionRevision = revision;
        }

        private static void loadExtraInfo()
        {
            extraInfo = null;
            try
            {
                string filePath = Path.Combine(ModCore.dataFolder, EXTRA_INFO_FILE_PATH);

                ExtraInfo loadedExtraInfo = ModJsonUtils.DeserializeStream<ExtraInfo>(filePath);
                extraInfo = loadedExtraInfo;
            }
            catch { }
        }

        public static void GenerateExtraInfo()
        {
            string filePath = Path.Combine(ModCore.dataFolder, EXTRA_INFO_FILE_PATH);
            string content = ModJsonUtils.Serialize(new ExtraInfo()
            {
                CompileTime = DateTime.UtcNow
            });
            ModIOUtils.WriteText(content, filePath);
        }

        public class ExtraInfo
        {
            public DateTime CompileTime;
        }
    }
}