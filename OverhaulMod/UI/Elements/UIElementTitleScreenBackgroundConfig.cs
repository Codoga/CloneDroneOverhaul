using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementTitleScreenBackgroundConfig : OverhaulUIBehaviour
    {
        [UIElement("Label")]
        private readonly Text m_label;

        [UIElementAction(nameof(OnSetInGameLevelButtonClicked))]
        [UIElement("SetInGameLevelButton")]
        private readonly Button m_setInGameLevelButton;

        [UIElementAction(nameof(OnSetWorkshopLevelButtonClicked))]
        [UIElement("SetWorkshopLevelButton")]
        private readonly Button m_setWorkshopLevelButton;

        [UIElementAction(nameof(OnSetCustomLevelButtonClicked))]
        [UIElement("SetCustomLevelButton")]
        private readonly Button m_setCustomLevelButton;

        private TitleScreenBackgroundInfo m_backgroundInfo;
        public TitleScreenBackgroundInfo backgroundInfo
        {
            get
            {
                return m_backgroundInfo;
            }
            set
            {
                m_backgroundInfo = value;

                if (m_backgroundInfo != null && m_label)
                    m_label.text = m_backgroundInfo.Level == null ? "None" : m_backgroundInfo.Level.LevelID;
            }
        }

        public GameObject levelIsLoadingBG
        {
            get;
            set;
        }

        public bool refreshWhenEdited
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            refreshLabel();
        }

        public void OnSetInGameLevelButtonClicked()
        {
            ModUIUtils.LevelDescriptionBrowser(LevelManager.Instance._endlessLevels, onSetLevel);
        }

        public void OnSetWorkshopLevelButtonClicked()
        {
            ModUIUtils.LevelDescriptionBrowser(WorkshopLevelManager.Instance.GetAllWorkShopEndlessLevels(), onSetLevel);
        }

        public void OnSetCustomLevelButtonClicked()
        {
            ModUIUtils.FileExplorer(null, true, delegate (string levelPath)
            {
                if (!File.Exists(levelPath))
                {
                    onSetLevel(null);
                    ModUIUtils.MessagePopupOK("Error", "Could not find the level file you've specified");
                    return;
                }

                LevelDescription levelDescription = new LevelDescription()
                {
                    LevelTags = new List<LevelTags>() { },
                    LevelJSONPath = levelPath,
                    LevelEditorDifficultyIndex = 0,
                    LevelID = TitleScreenCustomizationManager.CUSTOM_LEVEL_ID
                };
                LevelManager.Instance._currentWorkshopLevelDifficultyIndex = 0;
                onSetLevel(levelDescription);
            }, Path.Combine(Application.persistentDataPath, "LevelEditorLevels"), "*.json");
        }

        private void onSetLevel(LevelDescription level)
        {
            backgroundInfo.Level = level;

            TitleScreenCustomizationManager titleScreenCustomizationManager = TitleScreenCustomizationManager.Instance;
            titleScreenCustomizationManager.SetLevelIsLoadingBG(levelIsLoadingBG);
            titleScreenCustomizationManager.SetStaticLevel(level, refreshWhenEdited);
            titleScreenCustomizationManager.SaveCustomizationInfo();
            refreshLabel();
        }

        private void refreshLabel()
        {
            TitleScreenBackgroundInfo info = TitleScreenCustomizationManager.Instance?.GetStaticBackgroundInfo();
            if (info == null || info.Level == null)
            {
                m_label.text = "None";
                return;
            }

            if (info.Level.WorkshopItem != null)
            {
                SteamWorkshopItem steamWorkshopItem = info.Level.WorkshopItem;
                m_label.text = $"{steamWorkshopItem.Title} (by {steamWorkshopItem.CreatorName})";
                return;
            }
            if (info.Level.LevelID == TitleScreenCustomizationManager.CUSTOM_LEVEL_ID)
            {
                m_label.text = Path.GetFileNameWithoutExtension(info.Level.LevelJSONPath);
                return;
            }
            m_label.text = StringUtils.AddSpacesToCamelCasedString(info.Level.PrefabName.Substring(info.Level.PrefabName.LastIndexOf("/") + 1));
        }
    }
}
