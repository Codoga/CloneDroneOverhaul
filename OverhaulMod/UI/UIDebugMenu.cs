﻿using OverhaulMod.Utils;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIDebugMenu : OverhaulUIBehaviour
    {
        public override bool enableUIOverLogoMode => true;

        public override bool enableCursor => true;

        [UIElementAction(nameof(OnUpdateBuildCompilationInfoButtonClicked))]
        [UIElement("UpdateBuildInfoButton")]
        private readonly Button m_buildCompilationInfoButton;

        [UIElementAction(nameof(OnSkipToNightmariumButtonClicked))]
        [UIElement("SkipToNightmariumButton")]
        private readonly Button m_skipToNightmariumButton;

        [UIElementAction(nameof(OnSkillPointsButtonClicked))]
        [UIElement("MoreSkillPointsButton")]
        private readonly Button m_skillPointsButton;

        public void OnSkillPointsButtonClicked()
        {
            GameDataManager.Instance.SetAvailableSkillPoints(1000);
        }

        public void OnSkipToNightmariumButtonClicked()
        {
            if (!GameModeManager.Is(GameMode.Endless))
            {
                ModUIUtils.MessagePopupOK("This button only works in endless mode.", "This button only works in endless mode.");
                return;
            }

            int levelsToBeat = 46 - GameDataManager.Instance.GetNumberOfLevelsWon();
            if (levelsToBeat < 1)
                return;

            GameData currentGameData = GameDataManager.Instance._endlessModeData;
            for(int i = 0; i < levelsToBeat; i++)
            {
                LevelManager.Instance.PickNextLevel();
                currentGameData.LevelIDsBeatenThisPlaythrough.Add(currentGameData.CurentLevelID);
                currentGameData.LevelPrefabsBeatenThisPlaythrough.Add(LevelManager.Instance.GetCurrentLevelDescription().PrefabName);
            }
            DebugManager.Instance.KillAllEnemies();
            currentGameData.SetDirty(true);
        }

        public void OnUpdateBuildCompilationInfoButtonClicked()
        {
            ModBuildInfo.GenerateExtraInfo();
            ModUIUtils.MessagePopupOK("Successfully saved compilation date.", "Successfully saved compilation date.");
        }
    }
}
