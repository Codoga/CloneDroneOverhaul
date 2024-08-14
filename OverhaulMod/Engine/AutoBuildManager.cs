using OverhaulMod.UI;
using OverhaulMod.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class AutoBuildManager : Singleton<AutoBuildManager>
    {
        [ModSetting(ModSettingsConstants.AUTO_BUILD_KEY_BIND, KeyCode.U)]
        public static KeyCode AutoBuildKeyBind;

        /*[ModSetting(ModSettingsConstants.AUTO_BUILD_ACTIVATION_ON_MATCH_START, false)]
        public static bool AutoBuildActivationOnMatchStart;*/

        [ModSetting(ModSettingsConstants.AUTO_BUILD_INDEX_TO_USE_ON_MATCH_START, -1)]
        public static int AutoBuildIndexToUseOnMatchStart;

        private bool m_hasSelectedUpgradesForMatch;

        private float m_timeLeftBeforeAutoActivationReset;

        private UIAutoBuildSelectionMenu m_autoBuildSelectionMenu;
        public UIAutoBuildSelectionMenu autoBuildSelectionMenu
        {
            get
            {
                if (!m_autoBuildSelectionMenu)
                {
                    m_autoBuildSelectionMenu = ModUIConstants.ShowAutoBuildSelectionMenu();
                }
                return m_autoBuildSelectionMenu;
            }
        }

        public bool isInAutoBuildConfigurationMode
        {
            get;
            set;
        }

        public AutoBuildListInfo buildList
        {
            get;
            set;
        }

        private bool m_isApplyingBuild;

        private void Start()
        {
            LoadBuildList();
        }

        private void Update()
        {
            if (!GameModeManager.Is(GameMode.BattleRoyale) && !GameModeManager.Is(GameMode.MultiplayerDuel))
                return;

            if (Input.GetKeyDown(AutoBuildKeyBind))
            {
                UpgradeUI upgradeUI = ModCache.gameUIRoot.UpgradeUI;
                if (upgradeUI.gameObject.activeSelf)
                {
                    autoBuildSelectionMenu.Show();
                }
            }
            else if (Input.GetKeyUp(AutoBuildKeyBind))
            {
                autoBuildSelectionMenu.Hide();
            }

            if (m_hasSelectedUpgradesForMatch)
            {
                m_timeLeftBeforeAutoActivationReset -= Time.deltaTime;
                if (m_timeLeftBeforeAutoActivationReset <= 0f)
                    m_hasSelectedUpgradesForMatch = false;
                else
                    return;
            }

            if (AutoBuildIndexToUseOnMatchStart <= -1)
                return;

            BattleRoyaleManager battleRoyaleManager = BattleRoyaleManager.Instance;
            if (battleRoyaleManager)
            {
                UpgradeUI upgradeUI = ModCache.gameUIRoot.UpgradeUI;
                if (!upgradeUI || !upgradeUI.gameObject.activeInHierarchy)
                    return;

                int secondsLeft = battleRoyaleManager.GetSecondsToGameStart();
                if (!m_hasSelectedUpgradesForMatch && secondsLeft > 7 && secondsLeft < 10)
                {
                    m_hasSelectedUpgradesForMatch = true;
                    m_timeLeftBeforeAutoActivationReset = 15f;
                    ApplyBuild(AutoBuildIndexToUseOnMatchStart);
                }
            }
        }

        public void LoadBuildList()
        {
            string oldPath = Path.Combine(ModDataManager.userDataFolder, "AutoBuildInfo.json");
            AutoBuildInfo oldAutoBuildInfo;
            if (File.Exists(oldPath))
            {
                try
                {
                    oldAutoBuildInfo = ModDataManager.Instance.DeserializeFile<AutoBuildInfo>("AutoBuildInfo.json", false);
                    oldAutoBuildInfo.FixValues();

                    File.Delete(oldPath);
                }
                catch
                {
                    oldAutoBuildInfo = null;
                }
            }
            else
            {
                oldAutoBuildInfo = null;
            }

            AutoBuildListInfo autoBuildListInfo;
            try
            {
                autoBuildListInfo = ModDataManager.Instance.DeserializeFile<AutoBuildListInfo>("AutoBuilds.json", false);
                autoBuildListInfo.FixValues();
            }
            catch
            {
                autoBuildListInfo = new AutoBuildListInfo();
                autoBuildListInfo.FixValues();
            }

            if (oldAutoBuildInfo != null)
            {
                autoBuildListInfo.Builds.Add(oldAutoBuildInfo);
                SaveBuildsInfo();
            }
            buildList = autoBuildListInfo;
        }

        public void SaveBuildsInfo()
        {
            ModDataManager.Instance.SerializeToFile("AutoBuilds.json", buildList, false);
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

        public void ApplyBuild(int index)
        {
            if (m_isApplyingBuild)
                return;

            FirstPersonMover firstPersonMover = CharacterTracker.Instance?.GetPlayerRobot();
            if (!firstPersonMover)
                return;

            UpgradeUI upgradeUI = ModCache.gameUIRoot?.UpgradeUI;
            if (!upgradeUI || !upgradeUI.gameObject.activeInHierarchy)
                return;

            List<AutoBuildInfo> builds = buildList.Builds;
            if (builds.IsNullOrEmpty() || index < 0 || index >= builds.Count)
                return;

            AutoBuildInfo autoBuildInfo = builds[index];
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

        public static string GetBuildDisplayName(string name)
        {
            return name.IsNullOrEmpty() ? "[No name]" : name;
        }
    }
}
