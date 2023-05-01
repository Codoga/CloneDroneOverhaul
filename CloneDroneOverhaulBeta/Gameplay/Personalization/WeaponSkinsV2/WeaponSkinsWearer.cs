﻿using CDOverhaul.Gameplay.Multiplayer;
using CDOverhaul.HUD;
using ModLibrary;
using OverhaulAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace CDOverhaul.Gameplay
{
    public class WeaponSkinsWearer : OverhaulCharacterExpansion
    {
        /// <summary>
        /// This VFX appears when user change skins
        /// Todo: Make better VFX
        /// </summary>
        public const bool AllowSwitchSkinVFX = false;

        private WeaponSkinsController m_Controller;

        public OverhaulModdedPlayerInfo PlayerInformation
        {
            get;
            private set;
        }
        public bool IsMultiplayerControlled { get; private set; }

        #region Spawned skins collection

        /// <summary>
        /// The collection of all instantiated skins
        /// </summary>
        public readonly List<WeaponSkinSpawnInfo> SpawnedSkins = new List<WeaponSkinSpawnInfo>();

        /// <summary>
        /// Get spawn info knowing only the <see cref="WeaponType"/>
        /// </summary>
        /// <param name="weaponType"></param>
        /// <returns></returns>
        public WeaponSkinSpawnInfo GetWeaponSkinSpawnInfo(WeaponType weaponType)
        {
            if (SpawnedSkins.IsNullOrEmpty())
            {
                return null;
            }

            foreach(WeaponSkinSpawnInfo info in SpawnedSkins)
            {
                if(info.Type == weaponType)
                {
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// Get spawn info knowing only the <see cref="WeaponModel"/>
        /// </summary>
        /// <param name="weaponModel"></param>
        /// <returns></returns>
        public WeaponSkinSpawnInfo GetWeaponSkinSpawnInfo(WeaponModel weaponModel)
        {
            if (weaponModel == null)
            {
                return null;
            }
            return GetWeaponSkinSpawnInfo(weaponModel.WeaponType);
        }

        /// <summary>
        /// Get spawn info knowing only the <see cref="IWeaponSkinItemDefinition"/>
        /// </summary>
        /// <param name="weaponSkinItemDefinition"></param>
        /// <returns></returns>
        public WeaponSkinSpawnInfo GetWeaponSkinSpawnInfo(IWeaponSkinItemDefinition weaponSkinItemDefinition)
        {
            if (weaponSkinItemDefinition == null || SpawnedSkins.IsNullOrEmpty())
            {
                return null;
            }

            foreach (WeaponSkinSpawnInfo info in SpawnedSkins)
            {
                if (info.Item == weaponSkinItemDefinition)
                {
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// Get skin spawn info of currently equipped weapon
        /// </summary>
        /// <returns></returns>
        public WeaponSkinSpawnInfo GetEquippedWeaponSkinSpawnInfo()
        {
            if(Owner == null)
            {
                return null;
            }
            return GetWeaponSkinSpawnInfo(Owner.GetEquippedWeaponType());
        }

        /// <summary>
        /// Static variant of <see cref="GetEquippedWeaponSkinSpawnInfo"/>, Get skin spawn info of currently equipped weapon
        /// </summary>
        /// <param name="mover"></param>
        /// <returns></returns>
        public static WeaponSkinSpawnInfo GetEquippedWeaponSkinSpawnInfoDirectly(FirstPersonMover mover)
        {
            if (mover == null)
            {
                return null;
            }

            WeaponSkinsWearer wearer = mover.GetComponent<WeaponSkinsWearer>();
            if (wearer == null)
            {
                return null;
            }

            return wearer.GetEquippedWeaponSkinSpawnInfo();
        }

        /// <summary>
        /// Get skin item definition of currently equipped weapon
        /// </summary>
        /// <param name="mover"></param>
        /// <returns></returns>
        public static WeaponSkinItemDefinitionV2 GetEquippedWeaponSkinItemDirectly(FirstPersonMover mover)
        {
            WeaponSkinSpawnInfo info = GetEquippedWeaponSkinSpawnInfoDirectly(mover);
            if(info == null)
            {
                return null;
            }
            return info.Item as WeaponSkinItemDefinitionV2;
        }

        public bool HasSpawnedSkin(WeaponType weaponType)
        {
            return GetWeaponSkinSpawnInfo(weaponType) != null;
        }
        public bool HasSpawnedSkin(WeaponModel weaponModel)
        {
            return GetWeaponSkinSpawnInfo(weaponModel) != null;
        }
        public bool HasSpawnedSkin(IWeaponSkinItemDefinition weaponSkinItemDefinition)
        {
            return GetWeaponSkinSpawnInfo(weaponSkinItemDefinition) != null;
        }

        public void RemoveSpawnedSkin(IWeaponSkinItemDefinition weaponSkinItemDefinition)
        {
            if (weaponSkinItemDefinition == null || SpawnedSkins.IsNullOrEmpty())
            {
                return;
            }

            int indexToRemove = 0;
            foreach (WeaponSkinSpawnInfo info in SpawnedSkins)
            {
                if (info.Item == weaponSkinItemDefinition)
                {
                    break;
                }
                indexToRemove++;
            }
            SpawnedSkins.RemoveAt(indexToRemove);
        }

        #endregion

        #region Weapon infos

        public bool IsFireVariant(WeaponModel model)
        {
            return model != null && IsFireVariant(model.WeaponType);
        }

        public bool IsFireVariant(WeaponType type)
        {
            if (type == WeaponType.Sword)
            {
                if (Owner.HasUpgrade(UpgradeType.FireSword))
                {
                    return true;
                }
            }
            else if (type == WeaponType.Hammer)
            {
                if (Owner.HasUpgrade(UpgradeType.FireHammer))
                {
                    return true;
                }
            }
            else if (type == WeaponType.Spear)
            {
                if (Owner.HasUpgrade(UpgradeType.FireSpear))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        private bool m_WaitingToSpawnSkins;
        private bool m_HasEverSpawnedSkins;

        private bool m_HasAddedListeners;


        public override void Start()
        {
            base.Start();
            m_Controller = OverhaulController.GetController<WeaponSkinsController>();

            DelegateScheduler.Instance.Schedule(delegate
            {
                if (Owner != null && MultiplayerPlayerInfoManager.Instance != null)
                {
                    MultiplayerPlayerInfoState pInfo = MultiplayerPlayerInfoManager.Instance.GetPlayerInfoState(Owner.GetPlayFabID());
                    if (pInfo != null)
                    {
                        PlayerInformation = pInfo.GetComponent<OverhaulModdedPlayerInfo>();
                        if (PlayerInformation != null)
                        {
                            onGetPlayerInfo(PlayerInformation.GetHashtable());

                            m_HasAddedListeners = true;
                            _ = OverhaulEventsController.AddEventListener<Hashtable>(OverhaulModdedPlayerInfo.InfoReceivedEventString, onGetPlayerInfo);
                        }
                    }
                }
            }, 0.5f);
            SpawnSkins();
        }

        protected override void OnDisposed()
        {
            base.OnDisposed();

            if (!m_HasAddedListeners)
            {
                return;
            }
            OverhaulEventsController.RemoveEventListener<Hashtable>(OverhaulModdedPlayerInfo.InfoReceivedEventString, onGetPlayerInfo);
        }

        protected override void OnRefresh()
        {
            SpawnSkins();
        }

        protected override void OnDeath()
        {
            WeaponSkinBehaviour b = GetSpecialBehaviourInEquippedWeapon<WeaponSkinBehaviour>();
            if (b == null)
            {
                return;
            }

            b.OnDeath();
        }

        private void onGetPlayerInfo(Hashtable hash)
        {
            m_HasEverSpawnedSkins = false;
            IsMultiplayerControlled = true;
            OnRefresh();
        }

        public bool IsOutdatedModel(WeaponSkinSpawnInfo info)
        {
            bool isFire = IsFireVariant(info.Type) && info.Type != WeaponType.Bow;
            bool isMultiplayer = GameModeManager.UsesMultiplayerSpeedMultiplier() && info.Type == WeaponType.Sword;

            switch (info.Variant)
            {
                case WeaponVariant.Default:
                    return isFire || isMultiplayer;

                case WeaponVariant.DefaultMultiplayer:
                    return isFire || !isMultiplayer;

                case WeaponVariant.Fire:
                    return !isFire || isMultiplayer;

                case WeaponVariant.FireMultiplayer:
                    return !isFire || !isMultiplayer;
            }
            return true;
        }

        public bool HasToRespawnSkins()
        {
            if (IsOwnerMainPlayer())
            {
                foreach (WeaponSkinSpawnInfo info in SpawnedSkins)
                {
                    if (IsOutdatedModel(info))
                    {
                        return true;
                    }
                }
                return !m_HasEverSpawnedSkins || WeaponSkinsController.SkinsDataIsDirty;
            }
            return true;
        }

        public T GetSpecialBehaviourInEquippedWeapon<T>() where T : WeaponSkinBehaviour
        {
            if (Owner == null)
            {
                return null;
            }

            WeaponSkinSpawnInfo m = GetEquippedWeaponSkinSpawnInfo();
            return m == null ? null : m.Model.GetComponent<T>();
        }

        public void SpawnSkins()
        {
            if (IsDisposedOrDestroyed())
            {
                return;
            }
            if (m_WaitingToSpawnSkins || !HasToRespawnSkins())
            {
                return;
            }

            WeaponSkinsController.SkinsDataIsDirty = false;
            m_HasEverSpawnedSkins = true;
            m_WaitingToSpawnSkins = true;
            DelegateScheduler.Instance.Schedule(delegate
            {
                spawnSkins();
            }, 0.2f);
        }

        private void spawnSkins()
        {
            m_WaitingToSpawnSkins = false;
            if (!WeaponSkinsController.IsFirstPersonMoverSupported(Owner))
            {
                return;
            }

            bool isMultiplayer = GameModeManager.IsMultiplayer();

            SetDefaultModelsActive();
            if (!OverhaulGamemodeManager.SupportsPersonalization())
            {
                return;
            }
            if (!IsOwnerPlayer() && !WeaponSkinsController.AllowEnemiesWearSkins)
            {
                return;
            }
            if (isMultiplayer && (PlayerInformation == null || !PlayerInformation.HasReceivedData || string.IsNullOrEmpty(PlayerInformation.GetData("ID"))))
            {
                return;
            }

            WeaponSkinsController controller = OverhaulController.GetController<WeaponSkinsController>();
            IWeaponSkinItemDefinition[] skins;
            if (IsMultiplayerControlled)
            {
                skins = new IWeaponSkinItemDefinition[4];
                skins[0] = controller.Interface.GetSkinItem(WeaponType.Sword, PlayerInformation.GetData("Skin.Sword"), ItemFilter.Everything, out _);
                skins[1] = controller.Interface.GetSkinItem(WeaponType.Bow, PlayerInformation.GetData("Skin.Bow"), ItemFilter.Everything, out _);
                skins[2] = controller.Interface.GetSkinItem(WeaponType.Hammer, PlayerInformation.GetData("Skin.Hammer"), ItemFilter.Everything, out _);
                skins[3] = controller.Interface.GetSkinItem(WeaponType.Spear, PlayerInformation.GetData("Skin.Spear"), ItemFilter.Everything, out _);
            }
            else
            {
                skins = controller.Interface.GetSkinItems(Owner);
            }
            if (skins == null)
            {
                return;
            }

            if (!SpawnedSkins.IsNullOrEmpty())
            {
                List<IWeaponSkinItemDefinition> toDelete = new List<IWeaponSkinItemDefinition>();
                foreach (WeaponSkinSpawnInfo info in SpawnedSkins)
                {
                    if (!IsOutdatedModel(info) && info.Type == WeaponType.Bow && !OverhaulGamemodeManager.SupportsBowSkins())
                    {
                        continue;
                    }

                    SetDefaultModelsActive(info.Model.transform);

                    if (info.Type == WeaponType.Bow)
                    {
                        ModdedObject m = info.Model.GetComponent<ModdedObject>();
                        if (m != null)
                        {
                            Destroy(m.GetObject<Transform>(0).gameObject);
                            Destroy(m.GetObject<Transform>(1).gameObject);
                        }
                    }
                    info.DestroyModel();
                    toDelete.Add(info.Item);
                }

                foreach (IWeaponSkinItemDefinition itemDef in toDelete)
                {
                    RemoveSpawnedSkin(itemDef);
                }
            }

            foreach (IWeaponSkinItemDefinition skin in skins)
            {
                if (!HasSpawnedSkin(skin))
                {
                    SpawnSkin(skin);
                }
            }

            if (AllowSwitchSkinVFX && Owner == WeaponSkinsController.RobotToPlayAnimationOn)
            {
                WeaponSkinsController.RobotToPlayAnimationOn = null;
                WeaponModel model = Owner.GetEquippedWeaponModel();
                if (model == null)
                {
                    return;
                }

                PooledPrefabController.SpawnObject<VFXWeaponSkinSwitch>(WeaponSkinsController.VFX_ChangeSkinID, model.transform.position + new Vector3(0, 0.25f, 0f), Vector3.zero);
            }
        }

        public void SetDefaultModelsActive(Transform transformToRemove = null)
        {
            if (!Owner.HasCharacterModel())
            {
                return;
            }
            CharacterModel model = Owner.GetCharacterModel();

            WeaponModel weaponModel1 = model.GetWeaponModel(WeaponType.Sword);
            if (weaponModel1 != null)
            {
                if (transformToRemove != null)
                {
                    if (weaponModel1.PartsToDrop != null)
                    {
                        List<Transform> t1 = weaponModel1.PartsToDrop.ToList();
                        _ = t1.Remove(transformToRemove);
                        weaponModel1.PartsToDrop = t1.ToArray();
                    }
                }
                else
                {
                    SetDefaultModelsVisible(true, weaponModel1);
                }
            }
            WeaponModel weaponModel2 = model.GetWeaponModel(WeaponType.Bow);
            if (weaponModel2 != null && OverhaulGamemodeManager.SupportsBowSkins())
            {
                if (transformToRemove != null)
                {
                    if (weaponModel2.PartsToDrop != null)
                    {
                        List<Transform> t1 = weaponModel2.PartsToDrop.ToList();
                        _ = t1.Remove(transformToRemove);
                        weaponModel2.PartsToDrop = t1.ToArray();
                    }
                }
                else
                {
                    SetDefaultModelsVisible(true, weaponModel2);
                }
            }
            WeaponModel weaponModel3 = model.GetWeaponModel(WeaponType.Hammer);
            if (weaponModel3 != null)
            {
                if (transformToRemove != null)
                {
                    if (weaponModel3.PartsToDrop != null)
                    {
                        List<Transform> t1 = weaponModel3.PartsToDrop.ToList();
                        _ = t1.Remove(transformToRemove);
                        weaponModel3.PartsToDrop = t1.ToArray();
                    }
                }
                else
                {
                    SetDefaultModelsVisible(true, weaponModel3);
                }
            }
            WeaponModel weaponModel4 = model.GetWeaponModel(WeaponType.Spear);
            if (weaponModel4 != null)
            {
                if (transformToRemove != null)
                {
                    if (weaponModel4.PartsToDrop != null)
                    {
                        List<Transform> t1 = weaponModel4.PartsToDrop.ToList();
                        _ = t1.Remove(transformToRemove);
                        weaponModel4.PartsToDrop = t1.ToArray();
                    }
                }
                else
                {
                    SetDefaultModelsVisible(true, weaponModel4);
                }
            }
        }

        public void SpawnSkin(IWeaponSkinItemDefinition item)
        {
            if (item == null || Owner == null || !Owner.HasCharacterModel())
            {
                return;
            }

            WeaponModel weaponModel = Owner.GetCharacterModel().GetWeaponModel(item.GetWeaponType());
            if (weaponModel == null || (weaponModel.WeaponType.Equals(WeaponType.Bow) && !OverhaulGamemodeManager.SupportsBowSkins()))
            {
                return;
            }

            SetDefaultModelsVisible(false, weaponModel);
            if (item.GetItemName() == "Default" || weaponModel.WeaponType != item.GetWeaponType())
            {
                SetDefaultModelsVisible(true, weaponModel);
                return;
            }
            bool fire = IsFireVariant(weaponModel) && item.GetWeaponType() != WeaponType.Bow;
            bool multiplayer = GameModeManager.UsesMultiplayerSpeedMultiplier() && item.GetWeaponType() == WeaponType.Sword;
            WeaponVariant variant = WeaponSkinsController.GetVariant(fire, multiplayer);
            WeaponSkinItemDefinitionV2 itemDefinition = item as WeaponSkinItemDefinitionV2;

            WeaponSkinModel newModel = item.GetModel(fire, multiplayer, 0);
            if (newModel != null && newModel.Model != null)
            {
                bool reparented = false;
                Transform toParent = weaponModel.transform;
                if (!string.IsNullOrEmpty(itemDefinition.ReparentToBodypart))
                {
                    toParent = TransformUtils.FindChildRecursive(Owner.GetCharacterModel().transform, itemDefinition.ReparentToBodypart);
                    if (toParent == null)
                    {
                        SetDefaultModelsVisible(true, weaponModel);
                        return;
                    }
                    reparented = true;
                }

                Transform spawnedModel = Instantiate(newModel.Model, toParent).transform;
                spawnedModel.localPosition = newModel.Offset.OffsetPosition;
                spawnedModel.localEulerAngles = newModel.Offset.OffsetEulerAngles;
                spawnedModel.localScale = newModel.Offset.OffsetLocalScale;
                spawnedModel.gameObject.layer = Layers.BodyPart;
                spawnedModel.gameObject.SetActive(!reparented);

                bool shouldApplyFavouriteColor = (fire && !itemDefinition.DontUseCustomColorsWhenFire) || (!fire && !itemDefinition.DontUseCustomColorsWhenNormal);
                if (shouldApplyFavouriteColor)
                {
                    Color? forcedColor = null;
                    forcedColor = Owner.GetCharacterModel().GetFavouriteColor();
                    if (fire && (item as WeaponSkinItemDefinitionV2).IndexOfForcedFireVanillaColor != -1)
                    {
                        int indexOfFireColor = (item as WeaponSkinItemDefinitionV2).IndexOfForcedFireVanillaColor;
                        forcedColor = indexOfFireColor == 5
                            ? new Color(3.552548f, 1.296873f, 0.5021926f, 1f)
                            : HumanFactsManager.Instance.GetFavColor(indexOfFireColor).ColorValue;
                    }
                    else if (!fire && (item as WeaponSkinItemDefinitionV2).IndexOfForcedNormalVanillaColor != -1)
                    {
                        forcedColor = HumanFactsManager.Instance.GetFavColor((item as WeaponSkinItemDefinitionV2).IndexOfForcedNormalVanillaColor).ColorValue;
                    }

                    SetModelColor(spawnedModel.gameObject, fire, (item as WeaponSkinItemDefinitionV2).Saturation, forcedColor, (item as WeaponSkinItemDefinitionV2).Multiplier);
                }
                WeaponSkinSpawnInfo newInfo = new WeaponSkinSpawnInfo
                {
                    Model = spawnedModel.gameObject,
                    Type = item.GetWeaponType(),
                    Variant = variant,
                    IsReparented = reparented,
                    Item = item
                };
                SpawnedSkins.Add(newInfo);

                BoxCollider collider = spawnedModel.gameObject.AddComponent<BoxCollider>();
                collider.size *= 0.5f;

                List<Transform> t1 = weaponModel.PartsToDrop.ToList();
                t1.Add(spawnedModel);
                weaponModel.PartsToDrop = t1.ToArray();

                if (weaponModel.WeaponType == WeaponType.Bow)
                {
                    ModdedObject m = spawnedModel.GetComponent<ModdedObject>();
                    Transform bowStringUpper = TransformUtils.FindChildRecursive(weaponModel.transform, "BowStringUpper");
                    Transform bowStringLower = TransformUtils.FindChildRecursive(weaponModel.transform, "BowStringLower");
                    if (bowStringLower != null && bowStringUpper != null)
                    {
                        bowStringLower.GetChild(0).localScale = new Vector3(0.1f, 1.3f, 0.1f);
                        bowStringUpper.GetChild(0).localScale = new Vector3(0.1f, 1.3f, 0.1f);
                        if ((item as WeaponSkinItemDefinitionV2).UseVanillaBowStrings)
                        {
                            m.GetObject<Transform>(0).gameObject.SetActive(false);
                            m.GetObject<Transform>(1).gameObject.SetActive(false);
                            bowStringLower.GetChild(0).gameObject.SetActive(true);
                            bowStringLower.GetChild(0).localScale = new Vector3(0.05f, 1.3f, 0.05f);
                            bowStringUpper.GetChild(0).gameObject.SetActive(true);
                            bowStringUpper.GetChild(0).localScale = new Vector3(0.05f, 1.3f, 0.05f);
                        }
                        else
                        {
                            m.GetObject<Transform>(0).SetParent(bowStringUpper, true);
                            m.GetObject<Transform>(1).SetParent(bowStringLower, true);
                        }
                    }
                }
            }
            else
            {
                SetDefaultModelsVisible(true, weaponModel);
            }
        }

        public void SetDefaultModelsVisible(bool value, WeaponModel model)
        {
            if (model == null)
            {
                return;
            }

            Transform[] partsToDropArray = model.PartsToDrop;
            if (!partsToDropArray.IsNullOrEmpty())
            {
                foreach (Transform part in partsToDropArray)
                {
                    if (part != null)
                    {
                        part.gameObject.SetActive(value);

                        ReplaceVoxelColor[] cols = part.GetComponents<ReplaceVoxelColor>();
                        if (!cols.IsNullOrEmpty())
                        {
                            foreach (ReplaceVoxelColor col in cols)
                            {
                                col.ReplaceColorOnStart = true;
                                col.CallPrivateMethod("Start");
                            }
                        }
                    }
                }
            }
        }

        public void SetModelColor(GameObject model, bool fire, float saturation, Color? forceColor = null, float multiplier = 1f)
        {
            Renderer renderer = model.GetComponent<Renderer>();
            if (renderer == null || renderer.material == null)
            {
                return;
            }

            HSBColor hsbcolor2 = new HSBColor(forceColor.Value)
            {
                b = 1f,
                s = saturation
            };
            renderer.material.SetColor("_EmissionColor", hsbcolor2.ToColor() * (2.5f * multiplier));

            WeaponSkinBehaviour[] behaviours = model.GetComponents<WeaponSkinBehaviour>();
            if (behaviours.IsNullOrEmpty())
            {
                return;
            }
            foreach(WeaponSkinBehaviour behaviour in behaviours)
            {
                if (behaviour != null)
                {
                    behaviour.OnSetColor(hsbcolor2.ToColor());
                }
            }
        }

        private void Update()
        {
            if (Owner != null && !SpawnedSkins.IsNullOrEmpty())
            {
                if (Time.frameCount % 3 == 0)
                {
                    int i = 0;
                    do
                    {
                        WeaponSkinSpawnInfo info = SpawnedSkins[i];
                        if (info != null && info.IsReparented && info.Model != null)
                        {
                            info.Model.SetActive(Owner.GetEquippedWeaponType() == info.Type);
                        }
                        i++;
                    } while (i < SpawnedSkins.Count);
                }
            }

#if DEBUG
            if (!IsOwnerMainPlayer())
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftAlt))
            {
                Transform model = GetTransform();
                if (model == null)
                {
                    return;
                }
                CopyVector(model.localPosition);
            }
            if (Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.LeftAlt))
            {
                Transform model = GetTransform();
                if (model == null)
                {
                    return;
                }
                CopyVector(model.localEulerAngles);
            }
            if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftAlt))
            {
                Transform model = GetTransform();
                if (model == null)
                {
                    return;
                }
                CopyVector(model.localScale);
            }
#endif
        }

        public void CopyVector(Vector3 vector)
        {
            string toCopy = vector[0].ToString().Replace(',', '.') + "f, " + vector[1].ToString().Replace(',', '.') + "f, " + vector[2].ToString().Replace(',', '.') + "f";
            TextEditor editor = new TextEditor
            {
                text = toCopy
            };
            editor.SelectAll();
            editor.Copy();
        }

        public Transform GetTransform()
        {
            if (Owner == null || SpawnedSkins.IsNullOrEmpty())
            {
                return null;
            }

            WeaponType weaponType = Owner.GetEquippedWeaponType();
            if (!WeaponSkinsController.IsWeaponSupported(weaponType))
            {
                return null;
            }

            WeaponSkinsController controller = OverhaulController.GetController<WeaponSkinsController>();
            if (controller == null || controller.Interface == null)
            {
                return null;
            }

            string w = null;
            switch (weaponType)
            {
                case WeaponType.Sword:
                    w = WeaponSkinsController.EquippedSwordSkin;
                    break;
                case WeaponType.Hammer:
                    w = WeaponSkinsController.EquippedHammerSkin;
                    break;
                case WeaponType.Bow:
                    w = WeaponSkinsController.EquippedBowSkin;
                    break;
                case WeaponType.Spear:
                    w = WeaponSkinsController.EquippedSpearSkin;
                    break;
            }
            if (string.IsNullOrEmpty(w))
            {
                return null;
            }

            WeaponSkinSpawnInfo model = GetWeaponSkinSpawnInfo(weaponType);
            return model == null ? null : model.Model.transform;
        }
    }
}