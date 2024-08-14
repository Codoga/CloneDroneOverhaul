using OverhaulMod.Utils;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationMultiplayerPlayerInfo
    {
        public string DataVersion, PlayFabID, SteamID, SwordSkin, BowSkin, HammerSkin, SpearSkin, ShieldSkin, ScytheSkin;

        public bool Error;

        public PersonalizationMultiplayerPlayerInfo()
        {

        }

        public PersonalizationMultiplayerPlayerInfo(string rawData)
        {
            SetData(rawData);
        }

        public void SetData(string rawData)
        {
            if (rawData.IsNullOrEmpty())
            {
                Error = true;
                return;
            }

            string[] split = rawData.Split(PersonalizationMultiplayerManager.Separator);
            if (split == null || split.Length < 3)
            {
                Error = true;
                return;
            }

            string dataVersion = split[2];
            DataVersion = dataVersion;
            if (!PersonalizationMultiplayerManager.CompareDataVersion(dataVersion))
            {
                Error = true;
                return;
            }
            Error = false;

            for (int i = 0; i < split.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        PlayFabID = split[i];
                        break;
                    case 1:
                        SteamID = split[i];
                        break;
                    case 3:
                        SwordSkin = split[i];
                        break;
                    case 4:
                        BowSkin = split[i];
                        break;
                    case 5:
                        HammerSkin = split[i];
                        break;
                    case 6:
                        SpearSkin = split[i];
                        break;
                    case 7:
                        ShieldSkin = split[i];
                        break;
                    case 8:
                        ShieldSkin = split[i];
                        break;
                }
            }
        }
    }
}
