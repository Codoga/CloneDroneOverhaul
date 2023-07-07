﻿using OverhaulAPI;
using System.Collections.Generic;
using UnityEngine;

namespace CDOverhaul.Gameplay
{
    public static class WeaponSkinsCustomVFXController
    {
        private static readonly Dictionary<WeaponSkinItemDefinitionV2, string> m_AllVFX = new Dictionary<WeaponSkinItemDefinitionV2, string>();

        public static void PrepareCustomVFXForSkin(WeaponSkinItemDefinitionV2 itemDefinition)
        {
            if (SkinHasCustomVFX(itemDefinition))
                return;

            if (OverhaulAssetsController.TryGetAsset<GameObject>(itemDefinition.CollideWithEnvironmentVFXAssetName, itemDefinition.OverrideAssetBundle, out GameObject vfx))
            {
                if (vfx == null)
                    return;

                PooledPrefabController.CreateNewEntry<WeaponSkinCustomVFXInstance>(vfx.transform, 5, itemDefinition.CollideWithEnvironmentVFXAssetName);
                m_AllVFX.Add(itemDefinition, itemDefinition.CollideWithEnvironmentVFXAssetName);
            }
        }

        public static bool SkinHasCustomVFX(WeaponSkinItemDefinitionV2 itemDefinition) => itemDefinition != null && m_AllVFX.ContainsKey(itemDefinition);

        public static void SpawnVFX(Vector3 position, Vector3 eulerAngles, WeaponSkinItemDefinitionV2 itemDefinition)
        {
            if (!SkinHasCustomVFX(itemDefinition))
                return;

            Debug.Log((itemDefinition as IWeaponSkinItemDefinition).GetItemName() + " custom VFX!!");
            _ = PooledPrefabController.SpawnEntry<WeaponSkinCustomVFXInstance>(itemDefinition.CollideWithEnvironmentVFXAssetName, position, eulerAngles);
        }

        public static void RemoveAllVFX() => m_AllVFX.Clear();
    }
}
