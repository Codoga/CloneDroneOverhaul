﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CDOverhaul.Gameplay.Editors.Personalization
{
    public class PersonalizationPlayerInfoEditor : PersonalizationEditorUIElement
    {
        [ObjectDefaultVisibility(false)]
        [ObjectReference("Shading")]
        private readonly GameObject m_Shading;

        [ObjectReference("Dropdown2")]
        private readonly Dropdown m_TypesDropdown;

        [ObjectReference("UserID")]
        private readonly InputField m_IDInputField;

        [ActionReference(nameof(OnAddSelfIDClicked))]
        [ObjectReference("AddSelfID")]
        private readonly Button m_AddSelfIDButton;

        [ActionReference(nameof(OnDeleteClicked))]
        [ObjectReference("DeleteEntry")]
        private readonly Button m_DeleteButton;

        private bool m_HasInitialized;

        public List<string> EditingList
        {
            get;
            set;
        }

        public int TargetIndex
        {
            get;
            set;
        }

        public bool AddNew
        {
            get;
            set;
        }

        public System.Action<string> CallBack
        {
            get;
            set;
        }

        protected override bool AssignVariablesAutomatically() => false;

        public void Show(List<string> list, int index, bool addNewEntry, System.Action<string> callback = null)
        {
            if (!m_HasInitialized)
            {
                OverhaulUIVer2.AssignValues(this);
                OverhaulUIVer2.AssignActionToButton(GetComponent<ModdedObject>(), "BackButton", Hide);
                OverhaulUIVer2.AssignActionToButton(GetComponent<ModdedObject>(), "Done", OnDoneClicked);
                m_HasInitialized = true;
            }

            base.gameObject.SetActive(true);
            m_Shading.SetActive(true);

            EditingList = list;
            TargetIndex = index;
            AddNew = addNewEntry;
            CallBack = callback;

            m_DeleteButton.interactable = !addNewEntry;
            m_TypesDropdown.interactable = AllowUsingManyIDTypes();
            m_TypesDropdown.value = 0;

            if (list == null)
                return;

            string text = PersonalizationEditor.GetOnlyID(EditingList[index], out byte type);
            m_IDInputField.text = text;
            m_TypesDropdown.value = type - 1;
        }

        public void Hide()
        {
            base.gameObject.SetActive(false);
            m_Shading.SetActive(false);

            EditingList = null;
            TargetIndex = -1;
            AddNew = false;
            CallBack = null;
        }

        public void OnDoneClicked()
        {
            string text = string.Empty;
            switch (m_TypesDropdown.value)
            {
                case 0:
                    text = "steam ";
                    break;
                case 1:
                    text = "discord ";
                    break;
                case 2:
                    text = "playfab ";
                    break;
            }
            text += m_IDInputField.text.Replace("https://steamcommunity.com/profiles/", string.Empty).Replace("/", string.Empty);

            if (EditingList != null)
                EditingList[TargetIndex] = text;

            CallBack?.Invoke(text);
            Hide();
        }

        public void OnDeleteClicked()
        {
            EditingList.RemoveAt(TargetIndex);
            CallBack?.Invoke("delete");
            Hide();
        }

        public void OnAddSelfIDClicked()
        {
            string text = string.Empty;
            switch (m_TypesDropdown.value)
            {
                case 0:
                    text = OverhaulPlayerIdentifier.GetLocalSteamID();
                    break;
                case 1:
                    return;
                case 2:
                    text = OverhaulPlayerIdentifier.GetLocalPlayFabID();
                    break;
            }
            m_IDInputField.text = text;
        }

        public static bool AllowUsingManyIDTypes() => OverhaulVersion.IsDebugBuild;
    }
}
