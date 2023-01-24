﻿using UnityEngine;
using UnityEngine.UI;

namespace CDOverhaul.HUD
{
    public class UIWeaponEntry : MonoBehaviour
    {
        private GameObject _selectedBG;
        private UISkinSelection _uiQuickReference;

        public WeaponType Weapon { get; set; }

        public void Initialize(in Transform selectedTransform, in Button button, in UISkinSelection ui, in WeaponType weapon)
        {
            _selectedBG = selectedTransform.gameObject;
            _uiQuickReference = ui;
            button.onClick.AddListener(OnClick);
            VisualizeDeselect();

            Weapon = weapon;
        }

        public void OnClick()
        {
            _uiQuickReference.OnSelectWeapon(this);
        }

        public void VisualizeSelect()
        {
            _selectedBG.gameObject.SetActive(true);
        }

        public void VisualizeDeselect()
        {
            _selectedBG.gameObject.SetActive(false);
        }
    }
}