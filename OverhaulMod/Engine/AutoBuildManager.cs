﻿using OverhaulMod.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class AutoBuildManager : Singleton<AutoBuildManager>
    {
        [ModSetting(ModSettingsConstants.AUTO_BUILD_KEY_BIND, KeyCode.U)]
        public static KeyCode AutoBuildKeyBind;

        public bool isInAutoBuildConfigurationMode
        {
            get;
            set;
        }

        public AutoBuildInfo buildInfo
        {
            get;
            set;
        }

        private bool m_isApplyingBuild;

        private void Start()
        {
            LoadBuildInfo();
        }

        private void Update()
        {
            if (Input.GetKeyDown(AutoBuildKeyBind))
                ApplyBuild();
        }

        public void LoadBuildInfo()
        {
            AutoBuildInfo autoBuildInfo;
            try
            {
                autoBuildInfo = ModDataManager.Instance.DeserializeFile<AutoBuildInfo>("AutoBuildInfo.json", false);
                autoBuildInfo.FixValues();
            }
            catch
            {
                autoBuildInfo = new AutoBuildInfo
                {
                    SkillPoints = 4
                };
                autoBuildInfo.FixValues();
            }
            buildInfo = autoBuildInfo;
        }

        public void SaveBuildInfo()
        {
            ModDataManager.Instance.SerializeToFile("AutoBuildInfo.json", buildInfo, false);
        }

        public void ResetUpgrades(Dictionary<UpgradeType, int> dictionary = null, int skillPoints = 4)
        {
            GameDataManager gameDataManager = GameDataManager.Instance;
            GameData gameData = gameDataManager._tempTitleScreenData;
            gameData.PlayerUpgrades.Clear();
            if (dictionary != null)
                foreach (KeyValuePair<UpgradeType, int> kv in dictionary)
                    gameData.PlayerUpgrades.Add(kv.Key, kv.Value);

            gameData.AvailableSkillPoints = skillPoints;

            GlobalEventManager.Instance.Dispatch("UpgradesReset");
            GlobalEventManager.Instance.Dispatch("AvailableSkillPointsChanged");
        }

        public void ApplyBuild()
        {
            if (m_isApplyingBuild)
                return;

            FirstPersonMover firstPersonMover = CharacterTracker.Instance?.GetPlayerRobot();
            if (!firstPersonMover)
                return;

            UpgradeUI upgradeUI = ModCache.gameUIRoot?.UpgradeUI;
            if (!upgradeUI || !upgradeUI.gameObject.activeInHierarchy)
                return;

            AutoBuildInfo autoBuildInfo = buildInfo;
            if (autoBuildInfo == null || autoBuildInfo.Upgrades.IsNullOrEmpty())
                return;

            m_isApplyingBuild = true;
            _ = base.StartCoroutine(applyBuildCoroutine(autoBuildInfo));
        }

        private IEnumerator applyBuildCoroutine(AutoBuildInfo autoBuildInfo)
        {
            string playFabId = MultiplayerLoginManager.Instance.GetLocalPlayFabID();
            UpgradeUI upgradeUI = ModCache.gameUIRoot.UpgradeUI;

            List<UpgradeTypeAndLevel> list = autoBuildInfo.Upgrades;
            for (int i = 0; i < list.Count; i++)
            {
                UpgradeTypeAndLevel ul = list[i];
                UpgradeUIIcon icon = upgradeUI.GetUpgradeUIIcon(ul.UpgradeType, ul.Level);
                if (icon && icon.GetCanUpgradeRightNow(playFabId))
                {
                    icon.OnButtonClicked();

                    float timeOut = Time.unscaledTime + 2f;
                    while (icon.GetCanUpgradeRightNow(playFabId) && Time.unscaledTime < timeOut)
                        yield return null;
                }
            }
            m_isApplyingBuild = false;
            yield break;
        }
    }
}
