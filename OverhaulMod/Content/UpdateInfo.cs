using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace OverhaulMod.Content
{
    public class UpdateInfo
    {
        public Version ModVersion;
        public string Changelog;

        public bool IsGoogleDriveLink;
        public string DownloadLink;

        public List<string> AllowedUsers;
        //public ExclusivePerkType RequireExclusivePerk;

        public void FixValues()
        {
            if(AllowedUsers == null)
                AllowedUsers = new List<string>();

            if(ModVersion == null)
                ModVersion = new Version(ModBuildInfo.versionMajor, ModBuildInfo.versionMinor, ModBuildInfo.versionBuild, ModBuildInfo.versionRevision);
        }

        public override string ToString()
        {
            return $"Overhaul mod {ModVersion}";
        }

        public bool CanBeInstalledByLocalUser()
        {
            /*if(RequireExclusivePerk != ExclusivePerkType.None)
            {
                if(ExclusivePerkManager.Instance.HasUnlockedPerk(RequireExclusivePerk))
                    return true;
            }*/
            return AllowedUsers == null || AllowedUsers.Contains(ModUserInfo.localPlayerPlayFabID) || AllowedUsers.Contains(ModUserInfo.localPlayerSteamID.ToString());
        }

        public bool IsCurrentBuild()
        {
            return ModBuildInfo.version == ModVersion;
        }

        public bool IsNewerBuild()
        {
            return ModBuildInfo.version < ModVersion;
        }

        public bool IsOlderBuild()
        {
            return ModBuildInfo.version > ModVersion;
        }
    }
}
