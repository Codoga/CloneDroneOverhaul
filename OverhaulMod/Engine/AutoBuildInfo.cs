using OverhaulMod.Utils;
using System.Collections.Generic;

namespace OverhaulMod.Engine
{
    public class AutoBuildInfo
    {
        public string Name;

        public List<UpgradeTypeAndLevel> Upgrades;

        public int SkillPoints;

        public void FixValues()
        {
            if (Upgrades == null)
                Upgrades = new List<UpgradeTypeAndLevel>();

            if (Name.IsNullOrEmpty())
                Name = "Unnamed build";
        }

        public void SetUpgradesFromData(int skillPoints)
        {
            SkillPoints = skillPoints;
            List<UpgradeTypeAndLevel> list = Upgrades;
            list.Clear();

            GameData gameData = GameDataManager.Instance?._tempTitleScreenData;
            if (gameData == null || gameData.PlayerUpgrades.IsNullOrEmpty())
                return;

            foreach (KeyValuePair<UpgradeType, int> kv in gameData.PlayerUpgrades)
            {
                UpgradeType upgradeType = kv.Key;
                int level = kv.Value;

                if (level > 1)
                    for (int i = 1; i <= level; i++)
                        list.Add(new UpgradeTypeAndLevel() { UpgradeType = upgradeType, Level = i });
                else
                    list.Add(new UpgradeTypeAndLevel() { UpgradeType = upgradeType, Level = level });
            }
        }

        public Dictionary<UpgradeType, int> GetUpgradesFromData()
        {
            Dictionary<UpgradeType, int> kv = new Dictionary<UpgradeType, int>();
            foreach (UpgradeTypeAndLevel ul in Upgrades)
            {
                if (kv.ContainsKey(ul.UpgradeType))
                {
                    if (ul.Level > kv[ul.UpgradeType])
                        kv[ul.UpgradeType] = ul.Level;
                }
                else
                    kv.Add(ul.UpgradeType, ul.Level);
            }
            return kv;
        }
    }
}
