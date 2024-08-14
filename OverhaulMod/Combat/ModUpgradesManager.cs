using InternalModBot;
using ModLibrary;
using OverhaulMod.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Combat
{
    public class ModUpgradesManager : Singleton<ModUpgradesManager>, IGameLoadListener
    {
        public const UpgradeType SCYTHE_UNLOCK_UPGRADE = (UpgradeType)500;
        public const UpgradeType SCYTHE_FIRE_UPGRADE = (UpgradeType)501;
        public const UpgradeType SCYTHE_BLADE_UPGRADE = (UpgradeType)502;

        public const UpgradeType DOUBLE_JUMP_UPGRADE = (UpgradeType)580;

        private List<UpgradeDescription> m_upgrades;

        private Transform m_upgradesObjectTransform;

        public override void Awake()
        {
            base.Awake();

            m_upgrades = new List<UpgradeDescription>();

            Transform transform = new GameObject("Upgrades").transform;
            transform.SetParent(base.transform);
            m_upgradesObjectTransform = transform;

            createUpgrades();
        }

        private void OnDestroy()
        {
            if (!m_upgrades.IsNullOrEmpty())
                foreach (UpgradeDescription upgradeDescription in m_upgrades)
                    if (upgradeDescription)
                    {
                        _ = UpgradeManager.Instance.UpgradeDescriptions.Remove(upgradeDescription);

                        if (upgradeDescription.Icon)
                            Destroy(upgradeDescription.Icon);

                        Destroy(upgradeDescription);
                    }
        }

        public void OnGameLoaded()
        {
            AddUpgrades();
        }

        private void createUpgrades()
        {
            UpgradeDescription scythe = CreateUpgrade<UpgradeDescription>("scythe_unlock",
                "scythe_unlock_desc",
                SCYTHE_UNLOCK_UPGRADE,
                1,
                AssetBundleConstants.UPGRADES,
                "Scythe-128x128");
            _ = CreateUpgrade<UpgradeDescription>("fire_scythe",
                  "fire_scythe_desc",
                  SCYTHE_FIRE_UPGRADE,
                  1,
                  AssetBundleConstants.UPGRADES,
                  "FireScythe-128x128",
                  scythe);
            _ = CreateUpgrade<UpgradeDescription>("sharp_blade",
                  "sharp_blade_desc",
                  SCYTHE_BLADE_UPGRADE,
                  1,
                  AssetBundleConstants.UPGRADES,
                  "ScytheBlade-128x128",
                  scythe);

            _ = CreateUpgrade<UpgradeDescription>("double_jump",
                "double_jump_desc",
                DOUBLE_JUMP_UPGRADE,
                1,
                AssetBundleConstants.UPGRADES,
                "DoubleJump-128x128");
        }

        public void DeleteLocalizationKeysOfUpgrades(Dictionary<string, string> keys)
        {
            foreach (UpgradeDescription upgrade in m_upgrades)
            {
                _ = keys.Remove(upgrade.UpgradeName);
                _ = keys.Remove(upgrade.Description);
            }
        }

        public void AddUpgrades()
        {
            Mod mod = ModCore.instance;
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            foreach (UpgradeDescription upgrade in m_upgrades)
                if (!upgradeManager.HasUpgrade(upgrade.UpgradeType, upgrade.Level))
                {
                    if (!upgradeManager.IsUpgradeTypeAndLevelUsed(upgrade.UpgradeType, upgrade.Level))
                        upgradeManager.UpgradeDescriptions.Add(upgrade);

                    UpgradePagesManager.AddUpgrade(upgrade.UpgradeType, upgrade.Level, mod.ModInfo.UniqueID);
                    if (upgrade is AbilityUpgrade)
                    {
                        Dictionary<UpgradeType, bool> abilityUpgradeTypes = upgradeManager._abilityUpgradeTypes;
                        if (abilityUpgradeTypes != null)
                        {
                            abilityUpgradeTypes[upgrade.UpgradeType] = true;
                        }
                    }
                }
        }

        public T CreateUpgrade<T>(string displayName, string description, UpgradeType upgradeType, int level = 1, string iconBundle = null, string iconAsset = null, UpgradeDescription r1 = null, UpgradeDescription r2 = null) where T : UpgradeDescription
        {
            GameObject gameObject = new GameObject($"{displayName} {level}");
            gameObject.transform.SetParent(m_upgradesObjectTransform);

            UpgradeDescription upgradeDescription = gameObject.AddComponent<T>();
            upgradeDescription.UpgradeName = displayName;
            upgradeDescription.Description = description;
            upgradeDescription.UpgradeType = upgradeType;
            upgradeDescription.Level = level;
            upgradeDescription.Requirement = r1;
            upgradeDescription.Requirement2 = r2;
            if (!iconBundle.IsNullOrEmpty() && !iconAsset.IsNullOrEmpty())
                upgradeDescription.Icon = ModResources.Sprite(iconBundle, iconAsset);

            m_upgrades.Add(upgradeDescription);
            return (T)upgradeDescription;
        }
    }
}
