using OverhaulMod.Content.Personalization;
using OverhaulMod.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPersonalizationEditorExclusivityEditMenu : OverhaulUIBehaviour
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

        [UIElementAction(nameof(OnCopyButtonClicked))]
        [UIElement("CopyButton")]
        private readonly Button m_copyButton;

        [UIElement("ItemDisplayPrefab", false)]
        private readonly ModdedObject m_authorDisplayPrefab;

        [UIElement("Content")]
        private readonly Transform m_container;

        public List<PersonalizationItemLockInfo> referenceList
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

        public void Populate(List<PersonalizationItemLockInfo> list, Action doneCallback)
        {
            referenceList = list;
            callback = doneCallback;
            populateContainer();
        }

        private void populateContainer()
        {
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            List<PersonalizationItemLockInfo> list = referenceList;
            if (list.IsNullOrEmpty())
                return;

            int index = 0;
            foreach (PersonalizationItemLockInfo lockInfo in list)
            {
                int i = index;

                bool hasPlayFabId = !lockInfo.PlayerPlayFabID.IsNullOrEmpty() && !lockInfo.PlayerPlayFabID.IsNullOrWhiteSpace();

                ModdedObject moddedObject = Instantiate(m_authorDisplayPrefab, m_container);
                moddedObject.gameObject.SetActive(true);

                InputField playFabIdField = moddedObject.GetObject<InputField>(0);
                playFabIdField.text = lockInfo.PlayerPlayFabID;
                playFabIdField.onEndEdit.AddListener(delegate (string value)
                {
                    lockInfo.PlayerPlayFabID = value;
                });
                playFabIdField.interactable = !hasPlayFabId;

                InputField nickNameField = moddedObject.GetObject<InputField>(1);
                nickNameField.text = lockInfo.PlayerNickname;
                nickNameField.onEndEdit.AddListener(delegate (string value)
                {
                    lockInfo.PlayerNickname = value;
                });

                Button deleteButton = moddedObject.GetObject<Button>(2);
                deleteButton.onClick.AddListener(delegate
                {
                    list.RemoveAt(i);
                    populateContainer();
                });

                Button revealButton = moddedObject.GetObject<Button>(3);
                revealButton.onClick.AddListener(delegate
                {
                    playFabIdField.interactable = true;
                    revealButton.gameObject.SetActive(false);
                });
                revealButton.gameObject.SetActive(hasPlayFabId);

                index++;
            }
        }

        public void OnAddButtonClicked()
        {
            List<PersonalizationItemLockInfo> list = referenceList;
            if (list == null)
                return;

            list.Add(new PersonalizationItemLockInfo());
            populateContainer();
        }

        public void OnAddSelfButtonClicked()
        {
            _ = SteamFriends.GetPersonaName();

            List<PersonalizationItemLockInfo> list = referenceList;
            if (list == null)
                return;

            list.Add(new PersonalizationItemLockInfo()
            {
                PlayerNickname = SteamFriends.GetPersonaName(),
                PlayerPlayFabID = ModUserInfo.localPlayerPlayFabID,
                PlayerSteamID = ModUserInfo.localPlayerSteamID.m_SteamID,
            });
            populateContainer();
        }

        public void OnCopyButtonClicked()
        {
            GUIUtility.systemCopyBuffer = "getplayfabids true";
            m_copyButton.interactable = false;
            DelegateScheduler.Instance.Schedule(delegate
            {
                if (m_copyButton)
                    m_copyButton.interactable = true;
            }, 2f);
        }
    }
}
