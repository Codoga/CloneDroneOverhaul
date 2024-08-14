using OverhaulMod.Combat.Weapons;
using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OverhaulMod.Combat
{
    public class ModWeaponsManager : Singleton<ModWeaponsManager>
    {
        public const WeaponType SCYTHE_TYPE = (WeaponType)51;

        private Dictionary<WeaponType, (GameObject, Type)> m_Weapons;
        private List<WeaponType> m_MeleeWeapons;

        public override void Awake()
        {
            base.Awake();

            m_Weapons = new Dictionary<WeaponType, (GameObject, Type)>();
            m_MeleeWeapons = new List<WeaponType>();

            AddWeapon<ScytheWeaponModel>(SCYTHE_TYPE, true, AssetBundleConstants.WEAPONS, "OverhaulScythe");
        }

        public void AddWeapon<T>(WeaponType weaponType, bool melee, string assetBundle, string assetName) where T : ModWeaponModel
        {
            if (m_Weapons.ContainsKey(weaponType))
                return;

            GameObject model = null;
            if (!assetBundle.IsNullOrEmpty() && !assetName.IsNullOrEmpty())
                model = ModResources.Prefab(assetBundle, assetName);

            m_Weapons.Add(weaponType, (model, typeof(T)));

            if (melee)
                m_MeleeWeapons.Add(weaponType);
        }

        public bool IsMeleeWeapon(WeaponType weaponType)
        {
            return m_MeleeWeapons.Contains(weaponType);
        }

        public void AddWeaponsToRobot(FirstPersonMover firstPersonMover)
        {
            if (GameModeManager.IsMultiplayer() || !firstPersonMover || !ModFeatures.IsEnabled(ModFeatures.FeatureType.NewWeapons))
                return;

            CharacterModel characterModel = firstPersonMover.GetCharacterModel();
            if (!characterModel)
                return;

            if (characterModel.WeaponModels == null)
                characterModel.WeaponModels = new WeaponModel[] { };

            Transform targetTransform = TransformUtils.FindChildRecursive(characterModel.transform, "HandR");
            if (!targetTransform)
            {
                targetTransform = TransformUtils.FindChildRecursive(characterModel.transform, "Head");
                if (!targetTransform)
                    return;
            }

            List<WeaponModel> list = characterModel.WeaponModels.ToList();
            foreach (WeaponType weaponType in m_Weapons.Keys)
            {
                (GameObject, Type) tuple = m_Weapons[weaponType];

                GameObject model = tuple.Item1;
                GameObject gameObject;
                if (model)
                {
                    gameObject = Instantiate(model, targetTransform, false);
                }
                else
                {
                    gameObject = new GameObject(weaponType.ToString());
                    gameObject.transform.SetParent(targetTransform, false);
                }
                gameObject.SetActive(false);

                ModWeaponModel modWeaponModel = (ModWeaponModel)gameObject.AddComponent(tuple.Item2);
                modWeaponModel.WeaponType = weaponType;
                modWeaponModel.SetOwner(firstPersonMover);
                modWeaponModel.OnInstantiated(firstPersonMover);
                list.Add(modWeaponModel);
            }
            characterModel.WeaponModels = list.ToArray();
            firstPersonMover.RefreshModWeaponModels();
        }
    }
}
