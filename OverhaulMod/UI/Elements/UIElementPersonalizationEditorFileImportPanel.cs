﻿using OverhaulMod.Content.Personalization;
using OverhaulMod.Utils;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementPersonalizationEditorFileImportPanel : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(OnHelpButtonClicked))]
        [UIElement("HelpButton")]
        private readonly Button m_helpButton;

        [UIElementAction(nameof(OnImportVoxButtonClicked))]
        [UIElement("ImportVoxButton")]
        private readonly Button m_importVoxButton;

        [UIElement("FileDisplayPrefab", false)]
        private readonly ModdedObject m_fileDisplayPrefab;

        [UIElement("Content")]
        private readonly Transform m_fileDisplayContainer;

        private PersonalizationItemInfo m_itemInfo;
        public PersonalizationItemInfo itemInfo
        {
            get
            {
                return m_itemInfo;
            }
            set
            {
                m_itemInfo = value;
                Populate();
            }
        }

        public void Populate()
        {
            if (m_fileDisplayContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_fileDisplayContainer);

            foreach (var file in itemInfo.importedFiles)
            {
                ModdedObject moddedObject = Instantiate(m_fileDisplayPrefab, m_fileDisplayContainer);
                moddedObject.gameObject.SetActive(true);
                moddedObject.GetObject<Text>(1).text = file;
            }
        }

        public void OnHelpButtonClicked()
        {
            ModUIConstants.ShowPersonalizationEditorHelpMenu(UIPersonalizationEditor.instance.transform);
        }

        public void OnImportVoxButtonClicked()
        {
            var item = itemInfo;
            ModUIUtils.FileExplorer(UIPersonalizationEditor.instance.transform, true, delegate (string path)
            {
                if (!File.Exists(path))
                    return;

                string fn = Path.GetFileName(path);
                string dest = item.importedFilesFolder + fn;
                if (File.Exists(dest))
                {
                    ModUIUtils.MessagePopup(true, "File with the same name is already imported!", "Replace the file?", 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Replace", "No", null, delegate
                    {
                        File.Delete(dest);
                        File.Copy(path, dest);
                        Populate();
                    });
                }
                else
                {
                    File.Copy(path, dest);
                }

                Populate();
            }, InternalModBot.ModsManager.Instance.ModFolderPath, "*.vox");
        }
    }
}
