using OverhaulMod.Combat;
using OverhaulMod.Combat.Weapons;
using OverhaulMod.Engine;
using OverhaulMod.UI;
using OverhaulMod.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationController : MonoBehaviour
    {
        [ModSetting(ModSettingsConstants.SWORD_SKIN, null)]
        public static string SwordSkin;

        [ModSetting(ModSettingsConstants.BOW_SKIN, null)]
        public static string BowSkin;

        [ModSetting(ModSettingsConstants.HAMMER_SKIN, null)]
        public static string HammerSkin;

        [ModSetting(ModSettingsConstants.SPEAR_SKIN, null)]
        public static string SpearSkin;

        [ModSetting(ModSettingsConstants.SHIELD_SKIN, null)]
        public static string ShieldSkin;

        [ModSetting(ModSettingsConstants.SCYTHE_SKIN, null)]
        public static string ScytheSkin;

        [ModSetting(ModSettingsConstants.ALLOW_ENEMIES_USE_WEAPON_SKINS, true)]
        public static bool AllowEnemiesUseSkins;

        private FirstPersonMover m_owner;
        public FirstPersonMover owner
        {
            get
            {
                if (!m_owner)
                {
                    m_owner = base.GetComponent<FirstPersonMover>();
                }
                return m_owner;
            }
        }

        private CharacterModel m_ownerModel;
        public CharacterModel ownerModel
        {
            get
            {
                if (!m_ownerModel)
                {
                    m_ownerModel = owner?.GetCharacterModel();
                }
                return m_ownerModel;
            }
        }

        private PersonalizationMultiplayerPlayerInfo m_playerInfo;
        public PersonalizationMultiplayerPlayerInfo playerInfo
        {
            get
            {
                FirstPersonMover firstPersonMover = owner;
                if (!firstPersonMover || !firstPersonMover.IsAlive() || !m_hasInitialized || !m_isMultiplayer || !m_isPlayer)
                    return null;

                if (m_playerInfo == null)
                {
                    m_playerInfo = PersonalizationMultiplayerManager.Instance.GetPlayInfo(owner.GetPlayFabID());
                }
                return m_playerInfo;
            }
        }

        private Dictionary<WeaponType, Transform[]> m_weaponTypeToParts;

        private Dictionary<PersonalizationItemInfo, PersonalizationEditorObjectBehaviour> m_spawnedItems;

        private Dictionary<WeaponType, WeaponVariant> m_weaponTypeToVariant;

        private bool m_spawnSkinsNextFrame;

        private bool m_isEnemy;

        private bool m_isPlayer, m_isMainPlayer, m_isMindSpace;

        private bool m_isMultiplayer;

        private bool m_hasInitialized, m_hasAddedEventListeners;

        private float m_timeLeftToRefreshWeaponSkinAndParts, m_timeLeftToRefreshSkinVisibility;

        private bool m_hasRefreshedAfterDeath;

        private void Awake()
        {
            m_weaponTypeToParts = new Dictionary<WeaponType, Transform[]>();
            m_weaponTypeToVariant = new Dictionary<WeaponType, WeaponVariant>();
            m_spawnedItems = new Dictionary<PersonalizationItemInfo, PersonalizationEditorObjectBehaviour>();

            if (PersonalizationEditorManager.IsInEditor())
                PersonalizationEditorManager.Instance.currentPersonalizationController = this;
        }

        private void OnEnable()
        {
            FirstPersonMover firstPersonMover = owner;
            if (!firstPersonMover || !firstPersonMover.IsAlive())
            {
                base.enabled = false;
                return;
            }

            if (!m_hasInitialized)
            {
                _ = base.StartCoroutine(initializeCoroutine(firstPersonMover));
            }
        }

        private void OnDestroy()
        {
            if (m_hasAddedEventListeners)
            {
                GlobalEventManager.Instance.RemoveEventListener<string>(PersonalizationMultiplayerManager.PLAYER_INFO_UPDATED_EVENT, onPlayerInfoUpdated);
                m_hasAddedEventListeners = false;
            }
        }

        private void Update()
        {
            if (m_hasRefreshedAfterDeath)
                return;

            if (m_spawnSkinsNextFrame)
            {
                m_spawnSkinsNextFrame = false;
                SpawnEquippedSkins();
            }

            if (!m_hasInitialized)
                return;

            FirstPersonMover firstPersonMover = owner;
            if (!firstPersonMover)
                return;

            float d = Time.deltaTime;

            m_timeLeftToRefreshWeaponSkinAndParts -= d;
            if (m_timeLeftToRefreshWeaponSkinAndParts <= 0f)
            {
                m_timeLeftToRefreshWeaponSkinAndParts = 0.5f;

                if (!firstPersonMover.IsAlive())
                    m_hasRefreshedAfterDeath = true;

                if (!m_isMindSpace)
                {
                    WeaponType weaponType = firstPersonMover.GetEquippedWeaponType();
                    string skin = GetWeaponSkinDependingOnOwner(weaponType);
                    bool noSkin = skin.IsNullOrEmpty() || skin == "_";

                    PersonalizationItemInfo personalizationItemInfo = null;
                    bool hasSpawnedSkinForWeapon = false;
                    foreach (PersonalizationItemInfo key in m_spawnedItems.Keys)
                        if (key.Weapon == weaponType)
                        {
                            personalizationItemInfo = key;
                            hasSpawnedSkinForWeapon = true;
                        }

                    if (!noSkin && !hasSpawnedSkinForWeapon)
                    {
                        //Debug.Log("Spawned an item because we didnt earlier");

                        PersonalizationEditorObjectBehaviour behaviour = SpawnItem(skin);
                        if (behaviour)
                        {
                            personalizationItemInfo = behaviour.ControllerInfo?.ItemInfo;
                            hasSpawnedSkinForWeapon = true;
                        }
                        else
                        {
                            hasSpawnedSkinForWeapon = false;
                        }
                    }

                    bool forceEnable = PersonalizationEditorManager.IsInEditor() && PersonalizationEditorManager.Instance.originalModelsEnabled;
                    SetWeaponPartsVisible(weaponType, forceEnable || !hasSpawnedSkinForWeapon, personalizationItemInfo != null && personalizationItemInfo.HideBowStrings);
                }
            }

            m_timeLeftToRefreshSkinVisibility -= d;
            if (m_timeLeftToRefreshSkinVisibility <= 0f)
            {
                m_timeLeftToRefreshSkinVisibility = 0.2f;

                WeaponType equippedWeaponType = firstPersonMover.GetEquippedWeaponType();
                foreach (KeyValuePair<PersonalizationItemInfo, PersonalizationEditorObjectBehaviour> keyValue in m_spawnedItems)
                {
                    PersonalizationEditorObjectBehaviour behaviour = keyValue.Value;
                    if (!behaviour)
                        continue;

                    PersonalizationItemInfo info = keyValue.Key;
                    if (info.Category != PersonalizationCategory.WeaponSkins || info.Weapon != WeaponType.Bow || info.OverrideParent.IsNullOrEmpty())
                        continue;

                    behaviour.gameObject.SetActive(firstPersonMover.IsAlive() && info.Weapon == equippedWeaponType);
                }
            }
        }

        private void onPlayerInfoUpdated(string playFabId)
        {
            if (playFabId == owner.GetPlayFabID())
            {
                SpawnEquippedSkinsNextFrame();
            }
        }

        private void refreshWeaponSkinAndParts()
        {
            m_timeLeftToRefreshWeaponSkinAndParts = 0f;
        }

        private IEnumerator initializeCoroutine(FirstPersonMover firstPersonMover)
        {
            yield return null;

            while (firstPersonMover && firstPersonMover.IsAlive() && !firstPersonMover.IsInitialized())
                yield return null;

            if (!firstPersonMover || !firstPersonMover.IsAlive())
            {
                Destroy(this);
                yield break;
            }

            if (GameModeManager.IsMultiplayer())
            {
                m_isMultiplayer = true;
                if (firstPersonMover.state.IsAIControlled)
                {
                    m_isEnemy = true;
                    m_isPlayer = false;
                }
                else
                {
                    m_isEnemy = false;
                    m_isPlayer = true;
                    m_isMainPlayer = firstPersonMover.IsMainPlayer();

                    float timeOut = Time.time + 1f;
                    while (Time.time < timeOut && firstPersonMover.GetPlayFabID().IsNullOrEmpty())
                        yield return null;
                }
            }
            else
            {
                m_isMultiplayer = false;
                m_isEnemy = !firstPersonMover.IsMainPlayer();
                m_isPlayer = !m_isEnemy;
                m_isMainPlayer = !m_isEnemy;
                m_isMindSpace = firstPersonMover.IsMindSpaceCharacter;
            }

            m_hasInitialized = true;
            GlobalEventManager.Instance.AddEventListener<string>(PersonalizationMultiplayerManager.PLAYER_INFO_UPDATED_EVENT, onPlayerInfoUpdated);
            m_hasAddedEventListeners = true;
            refreshWeaponSkinAndParts();
            SpawnEquippedSkinsNextFrame();
            yield break;
        }

        public bool ShouldRefreshSkinOfWeapon(WeaponType weaponType)
        {
            return GetWeaponVariantOfSpawnedSkin(weaponType) != GetWeaponVariant(weaponType);
        }

        public void RefreshWeaponVariantOfSpawnedSkin(WeaponType weaponType)
        {
            Dictionary<WeaponType, WeaponVariant> d = m_weaponTypeToVariant;
            WeaponVariantManager.GetWeaponVariant(owner, weaponType, out WeaponVariant weaponVariant);

            if (d.ContainsKey(weaponType))
                d[weaponType] = weaponVariant;
            else
                d.Add(weaponType, weaponVariant);
        }

        public WeaponVariant GetWeaponVariantOfSpawnedSkin(WeaponType weaponType)
        {
            Dictionary<WeaponType, WeaponVariant> d = m_weaponTypeToVariant;
            if (d.ContainsKey(weaponType))
                return d[weaponType];

            return WeaponVariant.None;
        }

        public WeaponVariant GetWeaponVariant(WeaponType weaponType)
        {
            WeaponVariantManager.GetWeaponVariant(owner, weaponType, out WeaponVariant weaponVariant);
            return weaponVariant;
        }

        public void SetWeaponPartsVisible(WeaponType weaponType, bool value, bool hideBowStrings)
        {
            if (weaponType == ModWeaponsManager.SCYTHE_TYPE)
            {
                WeaponModel weaponModel = ownerModel.GetWeaponModel(ModWeaponsManager.SCYTHE_TYPE);
                if (weaponModel && weaponModel is ModWeaponModel modWeaponModel)
                {
                    modWeaponModel.SetIsModelActive(value);
                }
                return;
            }

            if (!m_weaponTypeToParts.IsNullOrEmpty() && m_weaponTypeToParts.TryGetValue(weaponType, out Transform[] parts))
            {
                foreach (Transform transform in parts)
                    if (transform)
                    {
                        if (!value && weaponType == WeaponType.Bow && transform.parent && (transform.parent.name == "BowStringUpper" || transform.parent.name == "BowStringLower"))
                        {
                            transform.gameObject.SetActive(!hideBowStrings);
                        }
                        else
                        {
                            transform.gameObject.SetActive(value);
                        }
                    }
            }
        }

        public void SetBowStringsWidth(float value)
        {
            if (!m_weaponTypeToParts.IsNullOrEmpty() && m_weaponTypeToParts.TryGetValue(WeaponType.Bow, out Transform[] parts))
            {
                foreach (Transform transform in parts)
                    if (transform && transform.parent && (transform.parent.name == "BowStringUpper" || transform.parent.name == "BowStringLower"))
                    {
                        transform.localScale = new Vector3(0.1f * value, transform.localScale.y, 0.1f * value);
                    }
            }
        }

        public PersonalizationEditorObjectBehaviour SpawnItem(string itemId)
        {
            if (itemId.IsNullOrEmpty())
                return null;

            return SpawnItem(PersonalizationManager.Instance.itemList.GetItem(itemId));
        }

        public PersonalizationEditorObjectBehaviour SpawnItem(PersonalizationItemInfo personalizationItemInfo)
        {
            if (personalizationItemInfo == null || (!PersonalizationEditorManager.IsInEditor() && !personalizationItemInfo.IsUnlocked(owner)) || personalizationItemInfo.RootObject == null || HasSpawnedItem(personalizationItemInfo))
                return null;

            RefreshWeaponVariantOfSpawnedSkin(personalizationItemInfo.Weapon);

            EnemyType enemyType = owner.CharacterType;
            if (owner.IsMindSpaceCharacter || enemyType == EnemyType.ZombieArcher1 || enemyType == EnemyType.FleetAnalysisBot1 || enemyType == EnemyType.FleetAnalysisBot2 || enemyType == EnemyType.FleetAnalysisBot3 || enemyType == EnemyType.FleetAnalysisBot4 || (personalizationItemInfo.Weapon == WeaponType.Bow && ModSpecialUtils.IsModEnabled("ee32ba1b-8c92-4f50-bdf4-400a14da829e")))
                return null;

            Transform transform = GetParentForItem(personalizationItemInfo);
            if (!transform)
                return null;

            PersonalizationEditorObjectBehaviour behaviour = personalizationItemInfo.RootObject.Deserialize(transform, new PersonalizationControllerInfo(this, personalizationItemInfo));
            if (!behaviour)
                return null;

            m_spawnedItems.Add(personalizationItemInfo, behaviour);

            if (personalizationItemInfo.Category == PersonalizationCategory.WeaponSkins)
            {
                if (personalizationItemInfo.Weapon == WeaponType.Bow)
                {
                    SetBowStringsWidth(Mathf.Clamp(personalizationItemInfo.BowStringsWidth, 0.1f, 1f));
                }

                refreshWeaponSkinAndParts();
            }

            return behaviour;
        }

        public void DestroyItem(PersonalizationItemInfo personalizationItemInfo, bool editCollection = true)
        {
            if (personalizationItemInfo == null || !HasSpawnedItem(personalizationItemInfo))
                return;

            if (personalizationItemInfo.Category == PersonalizationCategory.WeaponSkins)
                refreshWeaponSkinAndParts();

            PersonalizationEditorObjectBehaviour b = m_spawnedItems[personalizationItemInfo];
            if (b)
                Destroy(b.gameObject);

            if (editCollection)
                _ = m_spawnedItems.Remove(personalizationItemInfo);

            if (personalizationItemInfo.Category == PersonalizationCategory.WeaponSkins && personalizationItemInfo.Weapon == WeaponType.Bow)
            {
                SetBowStringsWidth(1f);
            }
        }

        public void DestroyAllItems()
        {
            Dictionary<PersonalizationItemInfo, PersonalizationEditorObjectBehaviour> dictionary = m_spawnedItems;
            if (dictionary == null || dictionary.Count == 0)
                return;

            foreach (KeyValuePair<PersonalizationItemInfo, PersonalizationEditorObjectBehaviour> kv in dictionary)
                DestroyItem(kv.Key, false);

            dictionary.Clear();
        }

        public bool HasSpawnedItem(PersonalizationItemInfo personalizationItemInfo)
        {
            return m_spawnedItems.ContainsKey(personalizationItemInfo);
        }

        public PersonalizationItemInfo GetItem(WeaponType weaponType)
        {
            foreach (KeyValuePair<PersonalizationItemInfo, PersonalizationEditorObjectBehaviour> keyValue in m_spawnedItems)
                if (keyValue.Key.Weapon == weaponType)
                    return keyValue.Key;

            return null;
        }

        public void RefreshWeaponRenderers()
        {
            m_weaponTypeToParts.Clear();
            CharacterModel characterModel = ownerModel;
            if (!characterModel || characterModel.WeaponModels.IsNullOrEmpty())
                return;

            foreach (WeaponModel weaponModel in characterModel.WeaponModels)
            {
                if (!weaponModel)
                    continue;

                WeaponType weaponType = weaponModel.WeaponType;
                if (PersonalizationManager.IsWeaponCustomizationSupported(weaponType))
                    if (m_weaponTypeToParts.ContainsKey(weaponType))
                        m_weaponTypeToParts[weaponType] = weaponModel.PartsToDrop;
                    else
                        m_weaponTypeToParts.Add(weaponModel.WeaponType, weaponModel.PartsToDrop);
            }
        }

        public void SpawnEquippedSkinsNextFrame()
        {
            m_spawnSkinsNextFrame = true;
        }

        public void SpawnEquippedSkins()
        {
            if (PersonalizationEditorManager.IsInEditor())
                return;

            DestroyAllItems();
            RefreshWeaponRenderers();

            if (GameModeManager.IsMultiplayer() && !owner.IsMainPlayer())
                return;

            string swordSkin = GetWeaponSkinDependingOnOwner(WeaponType.Sword);
            string bowSkin = GetWeaponSkinDependingOnOwner(WeaponType.Bow);
            string hammerSkin = GetWeaponSkinDependingOnOwner(WeaponType.Hammer);
            string spearSkin = GetWeaponSkinDependingOnOwner(WeaponType.Spear);
            string shieldSkin = GetWeaponSkinDependingOnOwner(WeaponType.Shield);

            _ = SpawnItem(swordSkin);
            _ = SpawnItem(bowSkin);
            _ = SpawnItem(hammerSkin);
            _ = SpawnItem(spearSkin);
            _ = SpawnItem(shieldSkin);
        }

        public void RespawnSkinsIfRequired()
        {
            Dictionary<PersonalizationItemInfo, PersonalizationEditorObjectBehaviour> d = m_spawnedItems;
            if (d.Count == 0)
            {
                SpawnEquippedSkins();
            }
            else
            {
                RefreshWeaponRenderers();

                List<PersonalizationItemInfo> skinsToRespawn = null;
                foreach (KeyValuePair<PersonalizationItemInfo, PersonalizationEditorObjectBehaviour> kv in d)
                {
                    if (ShouldRefreshSkinOfWeapon(kv.Key.Weapon))
                    {
                        if (skinsToRespawn == null)
                            skinsToRespawn = new List<PersonalizationItemInfo>();

                        skinsToRespawn.Add(kv.Key);
                    }
                }

                if (skinsToRespawn != null)
                    foreach (PersonalizationItemInfo info in skinsToRespawn)
                    {
                        DestroyItem(info);
                        _ = SpawnItem(GetWeaponSkinDependingOnOwner(info.Weapon));
                    }
            }
        }

        public void EquipItem(PersonalizationItemInfo itemToEquip)
        {
            PersonalizationItemInfo info = null;
            foreach (PersonalizationItemInfo key in m_spawnedItems.Keys)
                if (key.Weapon == itemToEquip.Weapon)
                {
                    info = key;
                    break;
                }

            PersonalizationManager.SetIsItemEquipped(itemToEquip, true);

            DestroyItem(info);
            _ = SpawnItem(itemToEquip);
        }

        public Transform GetParentForItem(PersonalizationItemInfo personalizationItemInfo)
        {
            if (personalizationItemInfo.Category == PersonalizationCategory.Pets)
            {
                return base.transform.transform;
            }
            else if (personalizationItemInfo.Category == PersonalizationCategory.Accessories)
            {
                _ = TransformUtils.FindChildRecursive(base.transform, personalizationItemInfo.BodyPartName);
            }
            else if (personalizationItemInfo.Category == PersonalizationCategory.WeaponSkins)
            {
                Transform transform = ownerModel.GetWeaponModel(personalizationItemInfo.Weapon)?.transform;
                if (!personalizationItemInfo.OverrideParent.IsNullOrEmpty())
                {
                    Transform transform2 = TransformUtils.FindChildRecursive(owner.transform, personalizationItemInfo.OverrideParent);
                    if (transform2)
                        transform = transform2;
                }
                return transform;
            }
            return null;
        }

        public string GetWeaponSkinDependingOnOwner(WeaponType weaponType)
        {
            if (!m_hasInitialized || PersonalizationEditorManager.IsInEditor())
                return string.Empty;

            if (m_isMainPlayer && UIPersonalizationItemBrowser.IsPreviewing)
                return GetWeaponSkin(weaponType);

            if ((m_isEnemy && AllowEnemiesUseSkins) || (m_isPlayer && !m_isMultiplayer))
                return GetWeaponSkin(weaponType);
            else if (m_isMultiplayer)
            {
                PersonalizationMultiplayerPlayerInfo multiplayerPlayerInfo = playerInfo;
                if (multiplayerPlayerInfo == null)
                    return string.Empty;

                switch (weaponType)
                {
                    case WeaponType.Sword:
                        return multiplayerPlayerInfo.SwordSkin;
                    case WeaponType.Bow:
                        return multiplayerPlayerInfo.BowSkin;
                    case WeaponType.Hammer:
                        return multiplayerPlayerInfo.HammerSkin;
                    case WeaponType.Spear:
                        return multiplayerPlayerInfo.SpearSkin;
                    case WeaponType.Shield:
                        return multiplayerPlayerInfo.ShieldSkin;
                    case ModWeaponsManager.SCYTHE_TYPE:
                        return multiplayerPlayerInfo.ScytheSkin;
                }
            }
            return string.Empty;
        }

        public static void DestroyWeaponSkin(WeaponType weaponType)
        {
            Character character = CharacterTracker.Instance.GetPlayer();
            if (character)
            {
                PersonalizationController personalizationController = character.GetComponent<PersonalizationController>();
                if (personalizationController)
                {
                    personalizationController.DestroyItem(personalizationController.GetItem(weaponType));
                }
            }
        }

        public static void SetWeaponSkin(WeaponType weaponType, string itemId)
        {
            switch (weaponType)
            {
                case WeaponType.Sword:
                    ModSettingsManager.SetStringValue(ModSettingsConstants.SWORD_SKIN, itemId);
                    break;
                case WeaponType.Bow:
                    ModSettingsManager.SetStringValue(ModSettingsConstants.BOW_SKIN, itemId);
                    break;
                case WeaponType.Hammer:
                    ModSettingsManager.SetStringValue(ModSettingsConstants.HAMMER_SKIN, itemId);
                    break;
                case WeaponType.Spear:
                    ModSettingsManager.SetStringValue(ModSettingsConstants.SPEAR_SKIN, itemId);
                    break;
                case WeaponType.Shield:
                    ModSettingsManager.SetStringValue(ModSettingsConstants.SHIELD_SKIN, itemId);
                    break;
                case ModWeaponsManager.SCYTHE_TYPE:
                    ModSettingsManager.SetStringValue(ModSettingsConstants.SCYTHE_SKIN, itemId);
                    break;
            }
        }

        public static string GetWeaponSkin(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Sword:
                    return SwordSkin;
                case WeaponType.Bow:
                    return BowSkin;
                case WeaponType.Hammer:
                    return HammerSkin;
                case WeaponType.Spear:
                    return SpearSkin;
                case WeaponType.Shield:
                    return ShieldSkin;
                case ModWeaponsManager.SCYTHE_TYPE:
                    return ScytheSkin;
            }
            return null;
        }
    }
}
