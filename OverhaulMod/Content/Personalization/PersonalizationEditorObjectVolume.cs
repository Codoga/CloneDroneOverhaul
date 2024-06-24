﻿using OverhaulMod.Engine;
using OverhaulMod.UI;
using OverhaulMod.Utils;
using PicaVoxel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationEditorObjectVolume : PersonalizationEditorObjectComponentBase
    {
        private bool m_hasAddedEventListeners;

        private static readonly Dictionary<WeaponVariant, VolumeSettingsPreset> s_emptySettingsDictionary = new Dictionary<WeaponVariant, VolumeSettingsPreset>();

        private PersonalizationEditorObjectVisibilityController m_visibilityController;
        public PersonalizationEditorObjectVisibilityController visibilityController
        {
            get
            {
                if (!m_visibilityController)
                {
                    m_visibilityController = base.GetComponent<PersonalizationEditorObjectVisibilityController>();
                }
                return m_visibilityController;
            }
        }

        private Volume m_volume;
        public Volume volume
        {
            get
            {
                if (!m_volume)
                {
                    m_volume = base.GetComponent<Volume>();
                }
                return m_volume;
            }
        }

        [PersonalizationEditorObjectProperty]
        public Dictionary<WeaponVariant, VolumeSettingsPreset> volumeSettingPresets
        {
            get
            {
                PersonalizationEditorObjectBehaviour ob = objectBehaviour;
                return ob.GetPropertyValue(nameof(volumeSettingPresets), s_emptySettingsDictionary);
            }
            set
            {
                PersonalizationEditorObjectBehaviour ob = objectBehaviour;
                ob.SetPropertyValue(nameof(volumeSettingPresets), value);
            }
        }

        private bool m_hasStarted, m_isDestroyed;

        private bool m_aboutToRefreshVolume;

        private void Start()
        {
            m_hasStarted = true;

            if (volumeSettingPresets == null)
                volumeSettingPresets = new Dictionary<WeaponVariant, VolumeSettingsPreset>();
            else
            {
                foreach (VolumeSettingsPreset value in volumeSettingPresets.Values)
                    if (value.ReplaceWithFavoriteColors == null)
                        value.ReplaceWithFavoriteColors = new Dictionary<string, FavoriteColorSettings>();
            }

            EventController singleEventController = base.gameObject.AddComponent<EventController>();
            singleEventController.AddEventListener(PersonalizationEditorManager.PRESET_PREVIEW_CHANGED_EVENT, RefreshVolume);

            RefreshVolume();
            if (PersonalizationEditorManager.IsInEditor())
            {
                GlobalEventManager.Instance.AddEventListener(PersonalizationEditorManager.OBJECT_EDITED_EVENT, RefreshVolume);
                m_hasAddedEventListeners = true;
            }
        }

        private void OnDestroy()
        {
            m_isDestroyed = true;
            if (m_hasAddedEventListeners)
            {
                m_hasAddedEventListeners = false;
                GlobalEventManager.Instance.RemoveEventListener(PersonalizationEditorManager.OBJECT_EDITED_EVENT, RefreshVolume);
            }
        }

        public WeaponVariant GetUnusedShowCondition()
        {
            WeaponType weaponType = objectBehaviour.ControllerInfo.ItemInfo.Weapon;
            if (!volumeSettingPresets.ContainsKey(WeaponVariant.Normal))
                return WeaponVariant.Normal;
            else if (!volumeSettingPresets.ContainsKey(WeaponVariant.OnFire) && weaponType != WeaponType.Bow)
                return WeaponVariant.OnFire;
            else if (!volumeSettingPresets.ContainsKey(WeaponVariant.NormalMultiplayer) && weaponType == WeaponType.Sword)
                return WeaponVariant.NormalMultiplayer;
            else if (!volumeSettingPresets.ContainsKey(WeaponVariant.OnFireMultiplayer) && weaponType == WeaponType.Sword)
                return WeaponVariant.OnFireMultiplayer;

            return WeaponVariant.None;
        }

        public WeaponVariant GetCurrentShowCondition()
        {
            if (PersonalizationEditorManager.IsInEditor())
            {
                return PersonalizationEditorManager.Instance.previewPresetKey;
            }
            visibilityController.GetWeaponVariant(out WeaponVariant showConditions);
            return showConditions;
        }

        public List<Dropdown.OptionData> GetUnusedConditionOptions()
        {
            List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
            if (!volumeSettingPresets.ContainsKey(WeaponVariant.Normal))
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.Normal));
            if (!volumeSettingPresets.ContainsKey(WeaponVariant.NormalMultiplayer))
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.NormalMultiplayer));
            if (!volumeSettingPresets.ContainsKey(WeaponVariant.OnFire))
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.OnFire));
            if (!volumeSettingPresets.ContainsKey(WeaponVariant.OnFireMultiplayer))
                list.Add(new DropdownWeaponVariantOptionData(WeaponVariant.OnFireMultiplayer));
            return list;
        }

        public VolumeSettingsPreset GetCurrentPreset()
        {
            WeaponVariant condition = GetCurrentShowCondition();
            Dictionary<WeaponVariant, VolumeSettingsPreset> d = volumeSettingPresets;
            if (d == null || d.Count == 0)
                return null;

            if (!d.ContainsKey(condition))
            {
                if (condition == WeaponVariant.OnFireMultiplayer && d.ContainsKey(WeaponVariant.OnFire))
                    return d[WeaponVariant.OnFire];
                else if (condition == WeaponVariant.NormalMultiplayer && d.ContainsKey(WeaponVariant.Normal))
                    return d[WeaponVariant.Normal];

                foreach (VolumeSettingsPreset value in d.Values)
                    return value;
            }
            return d[condition];
        }

        public void RefreshVolume()
        {
            if (m_aboutToRefreshVolume || m_isDestroyed)
                return;

            m_aboutToRefreshVolume = true;
            refreshVolumeCoroutine().Run();
        }

        private IEnumerator refreshVolumeCoroutine() // this fixes weird crash
        {
            while(!m_isDestroyed && (!objectBehaviour || objectBehaviour.ControllerInfo == null))
                yield return null;

            if (m_isDestroyed)
                yield break;

            Debug.Log($"refreshed volume {objectBehaviour.ControllerInfo.ItemInfo.Weapon}");
            m_aboutToRefreshVolume = false;
            refreshVolume();
            yield break;
        }

        private void refreshVolume()
        {
            if (!m_hasStarted)
                return;

            VolumeSettingsPreset preset = GetCurrentPreset();

            Volume volumeComponent = volume;
            if (volumeComponent)
            {
                Vector3 vector = base.transform.localScale;
                base.transform.localScale = Vector3.one;

                if (volumeComponent.NumFrames > 0)
                {
                    System.Collections.Generic.List<Frame> list = volumeComponent.Frames;
                    for (int i = 0; i < volumeComponent.NumFrames; i++)
                        Destroy(list[i].gameObject);

                    list.Clear();
                }

                if (preset == null)
                {
                    volumeComponent.GenerateBasic(FillMode.None);
                    return;
                }

                string voxFilePath = preset.VoxFilePath;
                if (voxFilePath == null)
                    voxFilePath = string.Empty;

                string path = Path.Combine(objectBehaviour.ControllerInfo.ItemInfo.RootFolderPath, voxFilePath);
                if (ModBuildInfo.debug)
                    Debug.Log($"{voxFilePath} (Exists? {File.Exists(path)})");

                if (!File.Exists(path))
                {
                    volumeComponent.GenerateBasic(FillMode.None);
                }
                else
                {
                    Color favoriteColor = objectBehaviour.ControllerInfo.Reference.owner.GetCharacterModel().GetFavouriteColor();

                    volumeComponent.AddFrame(0);
                    if (PersonalizationEditorManager.IsInEditor() || !PersonalizationCacheManager.Instance.TryGet(path, out byte[] array))
                    {
                        MagicaVoxelImporter.ImportModel(base.gameObject, path, "Import", preset.VoxelSize, preset.CenterPivot);
                    }
                    else
                    {
                        if (ModBuildInfo.debug)
                            Debug.Log("Loaded the model from memory");

                        MagicaVoxelImporter.ImportModelFromMemory(base.gameObject, array, "Import", preset.VoxelSize, preset.CenterPivot);
                    }

                    string cr = preset.ColorReplacements;

                    if (cr.IsNullOrEmpty())
                    {
                        List<Color32> colors = new List<Color32>();
                        Frame frame = volumeComponent.GetCurrentFrame();
                        int i = 0;
                        do
                        {
                            Voxel voxel = frame.Voxels[i];
                            Color32 color32 = voxel.Color;
                            if (voxel.Active && !colors.Contains(color32))
                            {
                                colors.Add(color32);
                            }
                            i++;
                        } while (i < frame.Voxels.Length);

                        List<ColorPairFloat> colorPairs = new List<ColorPairFloat>();
                        foreach (Color32 color in colors)
                            colorPairs.Add(new ColorPairFloat(color, color));

                        preset.ColorReplacements = PersonalizationEditorManager.Instance.GetStringFromColorPairs(colorPairs);
                    }
                    else
                    {
                        List<ColorPairFloat> list = PersonalizationEditorManager.Instance.GetColorPairsFromString(cr);
                        if (!list.IsNullOrEmpty())
                        {
                            foreach (ColorPairFloat cp in list)
                            {
                                Color colorB;
                                if (preset.ReplaceWithFavoriteColors != null && preset.ReplaceWithFavoriteColors.TryGetValue(ColorUtility.ToHtmlStringRGBA(cp.ColorA), out FavoriteColorSettings favoriteColorSettings))
                                {
                                    HSBColor hsbcolor = new HSBColor(favoriteColor);
                                    hsbcolor.s *= favoriteColorSettings.SaturationMultiplier;
                                    hsbcolor.b *= favoriteColorSettings.BrightnessMultiplier;
                                    colorB = hsbcolor.ToColor();
                                    colorB.a = Mathf.Clamp01(1f - favoriteColorSettings.GlowPercent);
                                }
                                else
                                {
                                    colorB = cp.ColorB;
                                }

                                ReplaceVoxelColor.ReplaceColors(volumeComponent, cp.ColorA, colorB, false);
                            }
                        }
                    }
                }
                base.transform.localScale = vector;
            }
        }
    }
}
