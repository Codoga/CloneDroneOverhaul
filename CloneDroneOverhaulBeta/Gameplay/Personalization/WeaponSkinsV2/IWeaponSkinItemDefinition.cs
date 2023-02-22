﻿using OverhaulAPI;
using System.Collections;
using UnityEngine;

namespace CDOverhaul.Gameplay
{
    public interface IWeaponSkinItemDefinition : IOverhaulItemDefinition, IEqualityComparer
    {
        /// <summary>
        /// Set model for specific variant of weapon skin
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="fire"></param>
        /// <param name="multiplayer"></param>
        void SetModel(GameObject prefab, WeaponSkinModelOffset offset, bool fire, bool multiplayer);
        /// <summary>
        /// Get model
        /// </summary>
        /// <param name="fire"></param>
        /// <param name="multiplayer"></param>
        /// <returns></returns>
        WeaponSkinModel GetModel(bool fire, bool multiplayer);

        /// <summary>
        /// TBA
        /// </summary>
        /// <param name="filter"></param>
        void SetFilter(WeaponSkinItemFilter filter);
        /// <summary>
        /// TBA
        /// </summary>
        /// <returns></returns>
        WeaponSkinItemFilter GetFilter();

        /// <summary>
        /// TBA
        /// </summary>
        /// <param name="weaponType"></param>
        void SetWeaponType(WeaponType weaponType);
        /// <summary>
        /// TBA
        /// </summary>
        /// <returns></returns>
        WeaponType GetWeaponType();
    }
}
