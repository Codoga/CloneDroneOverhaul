using OverhaulMod.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPersonalizationEditorAuthorsEditMenu : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnAddButtonClicked))]
        [UIElement("AddButton")]
        private readonly Button m_sendButton;

        [UIElementAction(nameof(OnAddSelfButtonClicked))]
        [UIElement("AddSelfButton")]
        private readonly Button m_sendSelfButton;

        [UIElement("ItemDisplayPrefab", false)]
        private readonly ModdedObject m_authorDisplayPrefab;

        [UIElement("Content")]
        private readonly Transform m_container;

        public List<string> referenceList
        {
            get;
            set;
        }

        public Action callback
        {
            get;
            set;
        }

        public override void Hide()
        {
            base.Hide();
            callback?.Invoke();
            callback = null;
        }

        public void Populate(List<string> list, Action doneCallback)
        {
            referenceList = list;
            callback = doneCallback;
            populateContainer();
        }

        private void populateContainer()
        {
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            List<string> list = referenceList;
            if (list.IsNullOrEmpty())
                return;

            int index = 0;
            foreach (string author in list)
            {
                int i = index;
                ModdedObject moddedObject = Instantiate(m_authorDisplayPrefab, m_container);
                moddedObject.gameObject.SetActive(true);
                moddedObject.GetObject<Text>(0).text = author;
                moddedObject.GetObject<Button>(1).onClick.AddListener(delegate
                {
                    list.RemoveAt(i);
                    populateContainer();
                });
                moddedObject.GetObject<Button>(2).onClick.AddListener(delegate
                {
                    ModUIUtils.InputFieldWindow("Edit author", "Type name", author, 30, 125f, delegate (string value)
                    {
                        list[i] = value;
                        moddedObject.GetObject<Text>(0).text = value;
                    });
                });
                index++;
            }
        }

        public void OnAddButtonClicked()
        {
            ModUIUtils.InputFieldWindow("Add author", "Type new name", null, 30, 125f, delegate (string value)
            {
                List<string> list = referenceList;
                if (list == null || list.Contains(value))
                    return;

                list.Add(value);
                populateContainer();
            });
        }

        public void OnAddSelfButtonClicked()
        {
            string value = SteamFriends.GetPersonaName();

            List<string> list = referenceList;
            if (list == null || list.Contains(value))
                return;

            list.Add(value);
            populateContainer();
        }
    }
}
