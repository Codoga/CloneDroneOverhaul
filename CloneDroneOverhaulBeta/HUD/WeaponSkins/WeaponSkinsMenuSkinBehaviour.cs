﻿using CDOverhaul.Gameplay;
using CDOverhaul.Gameplay.Multiplayer;
using CDOverhaul.Gameplay.Outfits;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CDOverhaul.HUD
{
    public class WeaponSkinsMenuSkinBehaviour : OverhaulBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
    {
        public const string Normal = "#1C6BFF";
        public const string Exclusive = "#1C6BFF";

        #region Static

        private static readonly List<WeaponSkinsMenuSkinBehaviour> m_InstantiatedButtons = new List<WeaponSkinsMenuSkinBehaviour>();

        public static void SelectSpecific(bool isOutfitSelection = false)
        {
            if (m_InstantiatedButtons.IsNullOrEmpty())
            {
                return;
            }

            foreach (WeaponSkinsMenuSkinBehaviour b in m_InstantiatedButtons)
            {
                if (isOutfitSelection && b.IsOutfitSelection)
                {
                    b.TrySelect();
                }
                else if (!isOutfitSelection && !b.IsOutfitSelection)
                {
                    b.TrySelect();
                }
            }
        }

        #endregion

        private GameObject m_SelectedImage;
        private WeaponSkinsMenu m_SkinsMenu;
        private string m_Skin;
        private WeaponType m_WeaponType;

        private GameObject m_ExclusiveIcon;
        private InputField m_Author;
        private Image m_Cooldown;
        private bool m_IsMouseOverElement;

        public bool IsSelected { get; private set; }

        public bool IsOutfitSelection;

        public void Initialize()
        {
            if (IsDisposedOrDestroyed())
            {
                return;
            }

            ModdedObject m = GetComponent<ModdedObject>();
            m_SelectedImage = m.GetObject<Transform>(0).gameObject;
            m_ExclusiveIcon = m.GetObject<Transform>(3).gameObject;
            m_Author = m.GetObject<InputField>(2);
            m_Cooldown = m.GetObject<Image>(4);
            m_Cooldown.fillAmount = WeaponSkinsMenu.GetSkinChangeCooldown();
            m_InstantiatedButtons.Add(this);

            Button b = GetComponent<Button>();
            b.onClick.AddListener(SelectThis);
        }

        protected override void OnDisposed()
        {
            _ = m_InstantiatedButtons.Remove(this);
            m_SelectedImage = null;
            m_SkinsMenu = null;
            m_Skin = null;
            m_Author = null;
            m_ExclusiveIcon = null;
            m_Cooldown = null;
        }

        private void Update()
        {
            if (Time.frameCount % 2 == 0)
            {
                m_Cooldown.fillAmount = WeaponSkinsMenu.GetSkinChangeCooldown();
            }
        }

        public void SetMenu(WeaponSkinsMenu menu)
        {
            if (IsDisposedOrDestroyed())
            {
                return;
            }

            if (m_SkinsMenu != null)
            {
                return;
            }
            m_SkinsMenu = menu;
        }

        public void SetSkin(string skin, string author, bool exclusive)
        {
            if (IsDisposedOrDestroyed())
            {
                return;
            }

            if (m_Skin != null)
            {
                return;
            }
            m_Skin = skin;
            m_Author.text = string.IsNullOrEmpty(author) ? "Original game" : "By " + author;
            m_ExclusiveIcon.SetActive(exclusive);
        }

        public void SetWeaponType(WeaponType weaponType)
        {
            if (IsDisposedOrDestroyed())
            {
                return;
            }

            if (m_WeaponType != default)
            {
                return;
            }
            m_WeaponType = weaponType;
        }

        public void SetSelected(bool value, bool initializing = false)
        {
            if (IsDisposedOrDestroyed() || (value == IsSelected && !initializing) || m_SelectedImage == null)
            {
                return;
            }

            m_SelectedImage.SetActive(value);
            IsSelected = value;
        }

        public void TrySelect()
        {
            IsSelected = false;
            if (IsOutfitSelection)
            {
                SetSelected(OutfitsController.EquippedAccessories.Contains(m_Skin), true);
                WeaponSkinsMenu.StartCooldown();

                FirstPersonMover mover = CharacterTracker.Instance.GetPlayerRobot();
                if (mover != null)
                {
                    OutfitsWearer outfits = mover.GetComponent<OutfitsWearer>();
                    if (outfits != null)
                    {
                        outfits.SpawnAccessories();
                    }
                }

                OverhaulModdedPlayerInfo info = OverhaulModdedPlayerInfo.GetLocalPlayerInfo();
                if (info != null && info.HasReceivedData)
                {
                    info.RefreshData();
                }

                return;
            }

            switch (m_WeaponType)
            {
                case WeaponType.Sword:
                    SetSelected(WeaponSkinsController.EquippedSwordSkin == m_Skin, true);
                    break;
                case WeaponType.Bow:
                    SetSelected(WeaponSkinsController.EquippedBowSkin == m_Skin, true);
                    break;
                case WeaponType.Hammer:
                    SetSelected(WeaponSkinsController.EquippedHammerSkin == m_Skin, true);
                    break;
                case WeaponType.Spear:
                    SetSelected(WeaponSkinsController.EquippedSpearSkin == m_Skin, true);
                    break;
            }
        }

        public void SelectThis()
        {
            if (IsDisposedOrDestroyed() || m_SkinsMenu == null || !WeaponSkinsMenu.AllowChangingSkins())
            {
                return;
            }

            if (IsOutfitSelection)
            {
                OutfitsController.SetAccessoryEquipped(m_Skin, !IsSelected);
                SelectSpecific(true);
                if (OverhaulVersion.IsDebugBuild && IsSelected)
                {
                    FirstPersonMover mover = CharacterTracker.Instance.GetPlayerRobot();
                    if (mover == null || !mover.HasCharacterModel())
                    {
                        return;
                    }

                    AccessoryItem item = OutfitsController.GetAccessoryItem(m_Skin, false);
                    if (!item.Offsets.ContainsKey(mover.GetCharacterModel().gameObject.name))
                    {
                        return;
                    }

                    OutfitsController.EditingItem = item;
                    OutfitsController.EditingCharacterModel = mover.GetCharacterModel().gameObject.name;
                    WeaponSkinsMenu.OutfitSelection.DebugSetInputFieldsValues(item.Offsets[mover.GetCharacterModel().gameObject.name]);
                }
                return;
            }
            else
            {
                if (IsSelected)
                {
                    return;
                }
            }

            m_SkinsMenu.SelectSkin(m_WeaponType, m_Skin);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsDisposedOrDestroyed() || IsSelected || m_SkinsMenu == null)
            {
                return;
            }

            m_SkinsMenu.ShowDescriptionTooltip(m_WeaponType, m_Skin);
            m_IsMouseOverElement = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (IsDisposedOrDestroyed() || IsSelected || m_SkinsMenu == null)
            {
                return;
            }

            m_SkinsMenu.ShowDescriptionTooltip(WeaponType.None, null);
            m_IsMouseOverElement = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerExit(null);
        }
    }
}