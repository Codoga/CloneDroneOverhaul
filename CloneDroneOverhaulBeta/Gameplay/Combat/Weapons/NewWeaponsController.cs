﻿using ModLibrary;
using OverhaulAPI;
using System.Collections.Generic;
using UnityEngine;

namespace CDOverhaul.Gameplay.Combat
{
    public class NewWeaponsController : OverhaulGameplayController
    {
        private static readonly List<AddedWeaponModel> m_AddedWeaponModels = new List<AddedWeaponModel>();
        private static readonly ModelOffset m_BoomerangModelOffset = new ModelOffset(Vector3.zero, Vector3.zero, Vector3.one * 0.5f);

        public override void Initialize()
        {
            base.Initialize();

            m_AddedWeaponModels.Clear();

            BoomerangWeaponModel boomerang = WeaponsAdder.AddWeaponModel<BoomerangWeaponModel>(AssetController.GetAsset("P_WM_Boomerang_2", OverhaulAssetsPart.Part2).transform, m_BoomerangModelOffset);
            m_AddedWeaponModels.Add(boomerang);
        }

        public override void OnFirstPersonMoverSpawned(FirstPersonMover firstPersonMover, bool hasInitializedModel)
        {
            if (!hasInitializedModel || !OverhaulGamemodeManager.SupportsCombatOverhaul())
            {
                return;
            }

            firstPersonMover.gameObject.AddComponent<NewWeaponsRobotExpansion>();
            WeaponsAdder.AddWeaponModelsToFirstPersonMover(firstPersonMover, m_AddedWeaponModels);
        }
    }
}