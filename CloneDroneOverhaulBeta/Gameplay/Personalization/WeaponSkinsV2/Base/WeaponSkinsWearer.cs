﻿using ModLibrary;
using System.Collections.Generic;
using UnityEngine;

namespace CDOverhaul.Gameplay
{
    public class WeaponSkinsWearer : FirstPersonMoverExtention
    {
        public readonly Dictionary<IWeaponSkinItemDefinition, WeaponSkinSpawnInfo> WeaponSkins = new Dictionary<IWeaponSkinItemDefinition, WeaponSkinSpawnInfo>();
        private bool m_WaitingToSpawnSkins;

        public override void OnUpgradesRefreshed(UpgradeCollection upgrades)
        {
            SpawnSkins();
        }

        public void SpawnSkins()
        {
            if (m_WaitingToSpawnSkins)
            {
                return;
            }

            m_WaitingToSpawnSkins = true;
            DelegateScheduler.Instance.Schedule(delegate
            {
                spawnSkins();
                m_WaitingToSpawnSkins = false;
            }, 0.2f);
        }

        private void spawnSkins()
        {
            IWeaponSkinItemDefinition[] skins = OverhaulController.GetController<WeaponSkinsControllerV2>().Interface.GetSkinItems(Owner);
            if (skins.IsNullOrEmpty())
            {
                return;
            }
            foreach (IWeaponSkinItemDefinition skin in skins)
            {
                SpawnSkin(skin);
            }
        }

        public void SpawnSkin(IWeaponSkinItemDefinition item)
        {
            if (item == null)
            {
                return;
            }
            CharacterModel model = Owner.GetCharacterModel();
            if(model == null)
            {
                return;
            }
            WeaponModel weaponModel = Owner.GetEquippedWeaponModel();
            if(weaponModel == null)
            {
                return;
            }
            SetDefaultModelsVisible(false, weaponModel);
            if (item.GetItemName() == "Default" || weaponModel.WeaponType != item.GetWeaponType())
            {
                SetDefaultModelsVisible(true, weaponModel);
                return;
            }
            bool fire = IsFireVariant(weaponModel);
            bool multiplayer = GameModeManager.UsesMultiplayerSpeedMultiplier();
            WeaponVariant variant = WeaponSkinsControllerV2.GetVariant(fire, multiplayer);

            foreach(WeaponSkinSpawnInfo wsInfo in WeaponSkins.Values)
            {
                if (wsInfo.Type == item.GetWeaponType() && wsInfo != null)
                {
                    wsInfo.DestroyModel();
                    break;
                }
            }
            _ = WeaponSkins.Remove(item);
            
            WeaponSkinModel newModel = item.GetModel(fire, multiplayer);            
            /*OverhaulDebugController.Print("Fire: " + fire);
            OverhaulDebugController.Print("Multiplayer: " + multiplayer);
            OverhaulDebugController.Print((newModel == null).ToString());*/
            if (newModel != null)
            {
                Transform spawnedModel = Instantiate(newModel.Model, weaponModel.transform).transform;
                spawnedModel.localPosition = newModel.Offset.OffsetPosition;
                spawnedModel.localEulerAngles = newModel.Offset.OffsetEulerAngles;
                spawnedModel.localScale = newModel.Offset.OffsetLocalScale;
                SetModelColor(spawnedModel.gameObject, fire);
                WeaponSkinSpawnInfo newInfo = new WeaponSkinSpawnInfo
                {
                    Model = spawnedModel.gameObject,
                    Type = item.GetWeaponType(),
                    Variant = variant
                };
                WeaponSkins.Add(item, newInfo);
            }
            else
            {
                SetDefaultModelsVisible(true, weaponModel);
            }
        }

        public void SetDefaultModelsVisible(bool value, WeaponModel model)
        {
            Transform[] partsToDropArray = model.PartsToDrop;
            if (!partsToDropArray.IsNullOrEmpty())
            {
                foreach (Transform part in partsToDropArray)
                {
                    part.gameObject.SetActive(value);
                }
            }
        }

        public void SetModelColor(GameObject model, bool fire)
        {
            if (!fire)
            {
                Renderer renderer = model.GetComponent<Renderer>();
                if(renderer == null || renderer.material == null)
                {
                    return;
                }
                Material material = renderer.material;
                HSBColor hsbcolor2 = new HSBColor(Owner.GetCharacterModel().GetFavouriteColor())
                {
                    b = 1f,
                    s = 0.75f
                };
                material.SetColor("_EmissionColor", hsbcolor2.ToColor() * 2.5f);
            }
        }

        public bool IsFireVariant(WeaponModel model)
        {
            if(model.WeaponType == WeaponType.Sword)
            {
                if (Owner.HasUpgrade(UpgradeType.FireSword))
                {
                    return true;
                }
            }
            else if (model.WeaponType == WeaponType.Hammer)
            {
                if (Owner.HasUpgrade(UpgradeType.FireHammer))
                {
                    return true;
                }
            }
            else if (model.WeaponType == WeaponType.Spear)
            {
                if (Owner.HasUpgrade(UpgradeType.FireSpear))
                {
                    return true;
                }
            }
            return false;
        }

    }
}