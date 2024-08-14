using OverhaulMod.Content.Personalization;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementPersonalizationEditorPropertiesPanel : OverhaulUIBehaviour
    {
        public static readonly List<Dropdown.OptionData> s_weapons = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData("Sword"),
            new Dropdown.OptionData("Bow"),
            new Dropdown.OptionData("Hammer"),
            new Dropdown.OptionData("Spear"),
        };

        public static readonly List<Dropdown.OptionData> s_variants = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData("Normal"),
            new Dropdown.OptionData("Fire"),
            new Dropdown.OptionData("Normal (Multiplayer)"),
            new Dropdown.OptionData("Fire (Multiplayer)"),
        };

        [UIElement("VolumeColorsConfigPanel", typeof(UIElementPersonalizationEditorVolumeColorsSettings), false)]
        private readonly UIElementPersonalizationEditorVolumeColorsSettings m_volumeColorsSettings;

        [UIElementAction(nameof(OnPositionChanged))]
        [UIElement("PositionPanel", typeof(UIElementVector3Field))]
        private readonly UIElementVector3Field m_positionField;

        [UIElementAction(nameof(OnRotationChanged))]
        [UIElement("RotationPanel", typeof(UIElementVector3Field))]
        private readonly UIElementVector3Field m_rotationField;

        [UIElementAction(nameof(OnScaleChanged))]
        [UIElement("ScalePanel", typeof(UIElementVector3Field))]
        private readonly UIElementVector3Field m_scaleField;

        [UIElement("ForceVolumeSettingsPresetDropdown", false)]
        private readonly ModdedObject m_forceVolumeSettingsPresetDropdown;

        [UIElement("VolumeSettingsPresetDisplay", false)]
        private readonly ModdedObject m_volumeSettingsPresetDisplay;

        [UIElement("EnableIfPresetDropdown", false)]
        private readonly ModdedObject m_enableIfPresetDropdown;

        [UIElement("VolumeExtraSettings", false)]
        private readonly ModdedObject m_volumeExtraSettings;

        [UIElement("ColorPickButton", false)]
        private readonly ModdedObject m_colorPickButton;

        [UIElement("BoolFieldDisplay", false)]
        private readonly ModdedObject m_togglePrefab;

        [UIElement("IntFloatSlider", false)]
        private readonly ModdedObject m_intFloatSliderPrefab;

        [UIElement("CvmModelPresetDisplay", false)]
        private readonly ModdedObject m_cvmModelPresetDisplay;

        [UIElement("AddVolumeSettingsPresetButton", false)]
        private readonly Button m_addVolumeSettingsPresetButton;

        [UIElement("Content")]
        private readonly Transform m_container;

        [UIElement("NothingToEditOverlay", true)]
        private readonly GameObject m_nothingToEditOverlay;

        [UIElementCallback(true)]
        [UIElementAction(nameof(OnObjectNameChanged))]
        [UIElement("ObjectNameField")]
        private readonly InputField m_objectNameField;

        private UIElementMouseEventsComponent m_mousePositionChecker;

        private int m_objectId;

        private PersonalizationEditorObjectBehaviour m_object;

        private VolumePropertiesController m_volumePropertiesController;

        private VisibilityPropertiesController m_visibilityPropertiesController;

        private FireParticlesPropertiesController m_fireParticlesPropertiesController;

        private CvmModelPropertiesController m_cvmModelPropertiesController;

        private bool m_disableCallbacks;

        private bool m_prevObjectState;

        protected override void OnInitialized()
        {
            m_objectId = -1;
            m_volumePropertiesController = new VolumePropertiesController();
            m_visibilityPropertiesController = new VisibilityPropertiesController();
            m_fireParticlesPropertiesController = new FireParticlesPropertiesController();
            m_cvmModelPropertiesController = new CvmModelPropertiesController();

            m_mousePositionChecker = base.gameObject.AddComponent<UIElementMouseEventsComponent>();
            m_volumeColorsSettings.onColorChanged = OnVolumeColorReplacementsChanged;
        }

        private void LateUpdate()
        {
            bool newObjectState = m_object;
            if (newObjectState != m_prevObjectState)
            {
                if (!newObjectState)
                {
                    EditObject(null);
                }
                m_prevObjectState = newObjectState;
            }
        }

        public void Clear()
        {
            TransformUtils.DestroyAllChildren(m_container);
        }

        public void Refresh()
        {
            EditObject(m_object);
        }

        public int GetEditingObjectUniqueIndex()
        {
            if (m_object)
                return m_object.UniqueIndex;

            return -1;
        }

        public void EditObjectAgain()
        {
            if (m_objectId == -1)
                return;

            EditObject(PersonalizationEditorObjectManager.Instance.GetInstantiatedObject(m_objectId));
        }

        public void EditObject(PersonalizationEditorObjectBehaviour objectBehaviour)
        {
            bool isNotNull = objectBehaviour;

            ModUIConstants.HideGenericColorPicker();
            m_volumeColorsSettings.Hide();
            m_nothingToEditOverlay.SetActive(!isNotNull);
            if (!isNotNull)
            {
                m_objectId = -1;
                m_object = null;
                m_objectNameField.text = string.Empty;
                GlobalEventManager.Instance.Dispatch(PersonalizationEditorObjectManager.OBJECT_SELECTION_CHANGED_EVENT);
                return;
            }
            m_objectId = objectBehaviour.UniqueIndex;
            m_object = objectBehaviour;
            GlobalEventManager.Instance.Dispatch(PersonalizationEditorObjectManager.OBJECT_SELECTION_CHANGED_EVENT);

            Clear();

            m_disableCallbacks = true;
            m_objectNameField.text = objectBehaviour.Name;
            m_positionField.vector = objectBehaviour.transform.localPosition;
            m_rotationField.vector = objectBehaviour.transform.localEulerAngles;
            m_scaleField.vector = objectBehaviour.transform.localScale;

            if (objectBehaviour.GetComponent<PersonalizationEditorObjectVisibilityController>())
            {
                m_visibilityPropertiesController.PopulateFields(this, m_container, objectBehaviour);
            }

            if (objectBehaviour.GetComponent<PersonalizationEditorObjectVolume>())
            {
                m_volumePropertiesController.PopulateFields(this, m_container, objectBehaviour);
            }

            if (objectBehaviour.GetComponent<PersonalizationEditorObjectCVMModel>())
            {
                m_cvmModelPropertiesController.PopulateFields(this, m_container, objectBehaviour);
            }

            if (objectBehaviour.GetComponent<PersonalizationEditorObjectFireParticles>())
            {
                m_fireParticlesPropertiesController.PopulateFields(this, m_container, objectBehaviour);
            }

            /*
            foreach (PersonalizationEditorObjectPropertyAttribute attribute in objectBehaviour.GetProperties())
            {
                FieldDisplay fieldDisplay = null;
                if (attribute.propertyInfo.PropertyType == typeof(string))
                {
                    if (attribute.IsFileLocation)
                    {
                        fieldDisplay = Instantiate(m_fileLocationFieldDisplay, m_container).gameObject.AddComponent<FileLocationField>();
                    }
                }

                if (fieldDisplay)
                {
                    fieldDisplay.gameObject.SetActive(true);
                    fieldDisplay.InitializeElement();
                    list.Add(fieldDisplay);

                    PersonalizationEditorObjectComponentBase cb = (PersonalizationEditorObjectComponentBase)objectBehaviour.GetComponent(attribute.propertyInfo.DeclaringType);
                    fieldDisplay.Set(attribute, cb, attribute.propertyInfo.GetValue(cb));
                }
            }*/

            m_disableCallbacks = false;
        }

        public void OnVolumeColorReplacementsChanged(string str)
        {
            /*
            PersonalizationEditorObjectVolume volume = m_volume;
            if (volume)
                volume.colorReplacements = str;*/
        }

        public void OnPositionChanged(Vector3 value)
        {
            if (m_disableCallbacks)
                return;

            PersonalizationEditorObjectBehaviour objectBehaviour = m_object;
            if (!objectBehaviour)
                return;

            objectBehaviour.transform.localPosition = value;
        }

        public void OnRotationChanged(Vector3 value)
        {
            if (m_disableCallbacks)
                return;

            PersonalizationEditorObjectBehaviour objectBehaviour = m_object;
            if (!objectBehaviour)
                return;

            objectBehaviour.transform.localEulerAngles = value;
        }

        public void OnScaleChanged(Vector3 value)
        {
            if (m_disableCallbacks)
                return;

            PersonalizationEditorObjectBehaviour objectBehaviour = m_object;
            if (!objectBehaviour)
                return;

            objectBehaviour.transform.localScale = value;
            objectBehaviour.SerializedScale = value;
        }

        public void OnObjectNameChanged(string str)
        {
            if (!m_disableCallbacks && m_object)
            {
                m_object.Name = str;
                PersonalizationEditorManager.Instance.SerializeRoot();
                UIPersonalizationEditor.instance.Inspector.RefreshHierarchyPanel();
            }
        }

        public class ObjectPropertiesController
        {
            public virtual void PopulateFields(UIElementPersonalizationEditorPropertiesPanel propertiesPanel, Transform container, PersonalizationEditorObjectBehaviour objectBehaviour)
            {

            }
        }

        public class CvmModelPropertiesController : ObjectPropertiesController
        {
            public override void PopulateFields(UIElementPersonalizationEditorPropertiesPanel propertiesPanel, Transform container, PersonalizationEditorObjectBehaviour objectBehaviour)
            {
                void populateFieldsAction()
                {
                    propertiesPanel.Refresh();
                }

                PersonalizationEditorObjectCVMModel model = objectBehaviour.GetComponent<PersonalizationEditorObjectCVMModel>();

                ModdedObject volumeExtraSettings = Instantiate(propertiesPanel.m_volumeExtraSettings, container);
                volumeExtraSettings.gameObject.SetActive(true);
                Toggle hideIfNoPresetToggle = volumeExtraSettings.GetObject<Toggle>(0);
                hideIfNoPresetToggle.isOn = model.hideIfNoPreset;
                hideIfNoPresetToggle.onValueChanged.AddListener(delegate (bool value)
                {
                    model.hideIfNoPreset = value;
                    GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                });

                Dictionary<WeaponVariant, CVMModelPreset> presets = model.presets;
                if (presets != null && presets.Count != 0)
                {
                    foreach (KeyValuePair<WeaponVariant, CVMModelPreset> keyValue in presets)
                    {
                        CVMModelPreset preset = keyValue.Value;

                        ModdedObject display = Instantiate(propertiesPanel.m_cvmModelPresetDisplay, container);
                        display.gameObject.SetActive(true);

                        // voxel model file
                        ModdedObject voxelModelFileField = display.GetObject<ModdedObject>(1);
                        InputField voxelModelFileFieldText = voxelModelFileField.GetObject<InputField>(0);
                        voxelModelFileFieldText.text = preset.CvmFilePath;
                        voxelModelFileField.GetObject<Button>(1).onClick.AddListener(delegate
                        {
                            ModUIUtils.FileExplorer(UIPersonalizationEditor.instance.transform, true, delegate (string filePath)
                            {
                                if (filePath.IsNullOrEmpty())
                                {
                                    voxelModelFileFieldText.text = string.Empty;
                                    preset.CvmFilePath = string.Empty;
                                }
                                else
                                {
                                    string directoryName = ModIOUtils.GetDirectoryName(PersonalizationEditorManager.Instance.currentEditingItemInfo.FolderPath);
                                    string fileName = Path.GetFileName(filePath);
                                    string path = Path.Combine(directoryName, "files", fileName);

                                    if (preset.CvmFilePath == path)
                                        return;

                                    voxelModelFileFieldText.text = path;
                                    preset.CvmFilePath = path;

                                    UIPersonalizationEditor.instance.Utilities.SetPresetPreview(keyValue.Key);
                                }

                                GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                            }, PersonalizationItemInfo.GetImportedFilesFolder(PersonalizationEditorManager.Instance.currentEditingItemInfo), "*.cvm");
                        });

                        // conditions dropdown
                        bool allowCallback = true;
                        WeaponVariant prevCondition = keyValue.Key;
                        Dropdown conditionsDropdown = display.GetObject<Dropdown>(0);
                        conditionsDropdown.options = PersonalizationEditorManager.Instance.GetConditionOptionsDependingOnEditingWeapon();

                        int conditionDropdownValueToSet = -1;
                        for (int i = 0; i < conditionsDropdown.options.Count; i++)
                        {
                            if (conditionsDropdown.options[i] is DropdownWeaponVariantOptionData showConditionOptionData && showConditionOptionData.Value == prevCondition)
                            {
                                conditionDropdownValueToSet = i;
                            }
                        }

                        if (conditionDropdownValueToSet == -1)
                        {
                            conditionsDropdown.options.Add(new DropdownWeaponVariantOptionData(prevCondition));
                            conditionsDropdown.RefreshShownValue();

                            conditionDropdownValueToSet = conditionsDropdown.options.Count - 1;
                        }

                        conditionsDropdown.value = conditionDropdownValueToSet;
                        conditionsDropdown.onValueChanged.AddListener(delegate (int value)
                        {
                            if (!allowCallback)
                                return;

                            WeaponVariant condition = (conditionsDropdown.options[value] as DropdownWeaponVariantOptionData).Value;
                            if (presets.ContainsKey(condition))
                            {
                                allowCallback = false;
                                conditionsDropdown.value = ((int)prevCondition) - 1;
                                allowCallback = true;

                                ModUIUtils.MessagePopupOK("Cannot change preset usage condition", "Another preset is already using this condition");
                            }
                            else
                            {
                                _ = presets.Remove(prevCondition);
                                presets.Add(condition, preset);

                                prevCondition = condition;
                                GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                            }
                        });
                        conditionsDropdown.interactable = model.GetUnusedShowCondition() != WeaponVariant.None;

                        // active frame
                        void refreshActiveFrameAction()
                        {
                            display.GetObject<GameObject>(2).SetActive(prevCondition == PersonalizationEditorManager.Instance.previewPresetKey);
                        }
                        refreshActiveFrameAction();

                        EventController singleEventController = display.gameObject.AddComponent<EventController>();
                        singleEventController.AddEventListener(PersonalizationEditorManager.PRESET_PREVIEW_CHANGED_EVENT, refreshActiveFrameAction);
                        singleEventController.AddEventListener(PersonalizationEditorManager.OBJECT_EDITED_EVENT, refreshActiveFrameAction);

                        // delete
                        display.GetObject<Button>(3).onClick.AddListener(delegate
                        {
                            _ = presets.Remove(prevCondition);
                            populateFieldsAction();
                        });

                        Dropdown weaponDropdown = display.GetObject<Dropdown>(4);
                        weaponDropdown.options = s_weapons;
                        switch (preset.Weapon)
                        {
                            case WeaponType.Sword:
                                weaponDropdown.value = 0;
                                break;
                            case WeaponType.Bow:
                                weaponDropdown.value = 1;
                                break;
                            case WeaponType.Hammer:
                                weaponDropdown.value = 2;
                                break;
                            case WeaponType.Spear:
                                weaponDropdown.value = 3;
                                break;
                        }
                        weaponDropdown.onValueChanged.AddListener(delegate (int value)
                        {
                            switch (value)
                            {
                                case 0:
                                    preset.Weapon = WeaponType.Sword;
                                    break;
                                case 1:
                                    preset.Weapon = WeaponType.Bow;
                                    break;
                                case 2:
                                    preset.Weapon = WeaponType.Hammer;
                                    break;
                                case 3:
                                    preset.Weapon = WeaponType.Spear;
                                    break;
                            }
                            GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                        });

                        Dropdown variantDropdown = display.GetObject<Dropdown>(5);
                        variantDropdown.options = s_variants;
                        switch (preset.Variant)
                        {
                            case WeaponVariant.Normal:
                                variantDropdown.value = 0;
                                break;
                            case WeaponVariant.OnFire:
                                variantDropdown.value = 1;
                                break;
                            case WeaponVariant.NormalMultiplayer:
                                variantDropdown.value = 2;
                                break;
                            case WeaponVariant.OnFireMultiplayer:
                                variantDropdown.value = 3;
                                break;
                        }
                        variantDropdown.onValueChanged.AddListener(delegate (int value)
                        {
                            switch (value)
                            {
                                case 0:
                                    preset.Variant = WeaponVariant.Normal;
                                    break;
                                case 1:
                                    preset.Variant = WeaponVariant.OnFire;
                                    break;
                                case 2:
                                    preset.Variant = WeaponVariant.NormalMultiplayer;
                                    break;
                                case 3:
                                    preset.Variant = WeaponVariant.OnFireMultiplayer;
                                    break;
                            }
                            GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                        });
                    }
                }

                if (model.GetUnusedShowCondition() != WeaponVariant.None)
                {
                    Button newPresetButton = Instantiate(propertiesPanel.m_addVolumeSettingsPresetButton, container);
                    newPresetButton.gameObject.SetActive(true);
                    newPresetButton.onClick.AddListener(delegate
                    {
                        model.presets.Add(model.GetUnusedShowCondition(), new CVMModelPreset(true));
                        populateFieldsAction();
                    });
                }
            }
        }

        public class FireParticlesPropertiesController : ObjectPropertiesController
        {
            public override void PopulateFields(UIElementPersonalizationEditorPropertiesPanel propertiesPanel, Transform container, PersonalizationEditorObjectBehaviour objectBehaviour)
            {
                PersonalizationEditorObjectFireParticles fireParticles = objectBehaviour.GetComponent<PersonalizationEditorObjectFireParticles>();

                ModdedObject colorPickButton = Instantiate(propertiesPanel.m_colorPickButton, container);
                colorPickButton.gameObject.SetActive(true);
                colorPickButton.GetObject<Text>(2).text = "Fire color";
                UIElementColorPickerButton colorPickerButtonComponent = colorPickButton.gameObject.AddComponent<UIElementColorPickerButton>();
                colorPickerButtonComponent.InitializeElement();
                colorPickerButtonComponent.colorPickerParent = UIPersonalizationEditor.instance.transform;
                colorPickerButtonComponent.useAlpha = true;
                colorPickerButtonComponent.color = fireParticles.color;
                colorPickerButtonComponent.onValueChanged.AddListener(delegate (Color color)
                {
                    fireParticles.color = color;
                    fireParticles.RefreshColor();
                });

                ModdedObject applyFavoriteColorToggleModdedObject = Instantiate(propertiesPanel.m_togglePrefab, container);
                applyFavoriteColorToggleModdedObject.gameObject.SetActive(true);
                Toggle applyFavoriteColorToggle = applyFavoriteColorToggleModdedObject.GetComponent<Toggle>();
                applyFavoriteColorToggle.isOn = fireParticles.applyFavoriteColor;
                applyFavoriteColorToggle.onValueChanged.AddListener(delegate (bool value)
                {
                    fireParticles.applyFavoriteColor = value;
                    fireParticles.RefreshColor();
                });
                applyFavoriteColorToggleModdedObject.GetObject<Text>(0).text = "Apply favorite color";

                ModdedObject hueOffsetSliderModdedObject = Instantiate(propertiesPanel.m_intFloatSliderPrefab, container);
                hueOffsetSliderModdedObject.gameObject.SetActive(true);
                hueOffsetSliderModdedObject.GetObject<Text>(1).text = "Fav. color hue offset";
                Slider hueOffsetSlider = hueOffsetSliderModdedObject.GetObject<Slider>(0);
                hueOffsetSlider.minValue = -0.1f;
                hueOffsetSlider.maxValue = 0.1f;
                hueOffsetSlider.value = fireParticles.favoriteColorHueOffset;
                hueOffsetSlider.onValueChanged.AddListener(delegate (float value)
                {
                    float ho = Mathf.Round(value * 100f) / 100f;
                    ModUIUtils.Tooltip($"{ho}");
                    fireParticles.favoriteColorHueOffset = ho;
                    fireParticles.RefreshColor();
                });

                ModdedObject brightnessSliderModdedObject = Instantiate(propertiesPanel.m_intFloatSliderPrefab, container);
                brightnessSliderModdedObject.gameObject.SetActive(true);
                brightnessSliderModdedObject.GetObject<Text>(1).text = "Fav. color brightness";
                Slider brightnessSlider = brightnessSliderModdedObject.GetObject<Slider>(0);
                brightnessSlider.minValue = 0f;
                brightnessSlider.maxValue = 100f;
                brightnessSlider.value = Mathf.Round(fireParticles.favoriteColorBrightness * 100f);
                brightnessSlider.onValueChanged.AddListener(delegate (float value)
                {
                    ModUIUtils.Tooltip($"{value}%");
                    fireParticles.favoriteColorBrightness = value / 100f;
                    fireParticles.RefreshColor();
                });

                ModdedObject saturationSliderModdedObject = Instantiate(propertiesPanel.m_intFloatSliderPrefab, container);
                saturationSliderModdedObject.gameObject.SetActive(true);
                saturationSliderModdedObject.GetObject<Text>(1).text = "Fav. color saturation";
                Slider saturationSlider = saturationSliderModdedObject.GetObject<Slider>(0);
                saturationSlider.minValue = 0f;
                saturationSlider.maxValue = 100f;
                saturationSlider.value = Mathf.Round(fireParticles.favoriteColorSaturation * 100f);
                saturationSlider.onValueChanged.AddListener(delegate (float value)
                {
                    ModUIUtils.Tooltip($"{value}%");
                    fireParticles.favoriteColorSaturation = value / 100f;
                    fireParticles.RefreshColor();
                });

                ModdedObject enableSmokeToggleModdedObject = Instantiate(propertiesPanel.m_togglePrefab, container);
                enableSmokeToggleModdedObject.gameObject.SetActive(true);
                Toggle enableSmokeToggle = enableSmokeToggleModdedObject.GetComponent<Toggle>();
                enableSmokeToggle.isOn = fireParticles.enableSmoke;
                enableSmokeToggle.onValueChanged.AddListener(delegate (bool value)
                {
                    fireParticles.enableSmoke = value;
                    fireParticles.RefreshColor();
                });
                enableSmokeToggleModdedObject.GetObject<Text>(0).text = "Enable smoke";
            }
        }

        public class VisibilityPropertiesController : ObjectPropertiesController
        {
            public override void PopulateFields(UIElementPersonalizationEditorPropertiesPanel propertiesPanel, Transform container, PersonalizationEditorObjectBehaviour objectBehaviour)
            {
                ModdedObject enableIfPresetDropdown = Instantiate(propertiesPanel.m_enableIfPresetDropdown, container);
                enableIfPresetDropdown.gameObject.SetActive(true);

                PersonalizationEditorObjectVisibilityController visibilityController = objectBehaviour.GetComponent<PersonalizationEditorObjectVisibilityController>();

                Dropdown dropdown = enableIfPresetDropdown.GetObject<Dropdown>(0);
                dropdown.options = PersonalizationEditorManager.Instance.GetConditionOptionsDependingOnEditingWeapon(true);

                int conditionDropdownValueToSet = -1;
                for (int i = 0; i < dropdown.options.Count; i++)
                {
                    if (dropdown.options[i] is DropdownWeaponVariantOptionData showConditionOptionData && showConditionOptionData.Value == visibilityController.enableIfWeaponVariant)
                    {
                        conditionDropdownValueToSet = i;
                    }
                }

                if (conditionDropdownValueToSet == -1)
                {
                    dropdown.options.Add(new DropdownWeaponVariantOptionData(visibilityController.enableIfWeaponVariant));
                    dropdown.RefreshShownValue();
                    conditionDropdownValueToSet = dropdown.options.Count - 1;
                }

                dropdown.value = conditionDropdownValueToSet;
                dropdown.onValueChanged.AddListener(delegate (int value)
                {
                    WeaponVariant weaponVariant = (dropdown.options[value] as DropdownWeaponVariantOptionData).Value;
                    visibilityController.enableIfWeaponVariant = weaponVariant;
                    visibilityController.RefreshVisibility();
                });
            }
        }

        public class VolumePropertiesController : ObjectPropertiesController
        {
            public override void PopulateFields(UIElementPersonalizationEditorPropertiesPanel propertiesPanel, Transform container, PersonalizationEditorObjectBehaviour objectBehaviour)
            {
                void populateFieldsAction()
                {
                    propertiesPanel.Refresh();
                }

                PersonalizationEditorObjectVolume volume = objectBehaviour.GetComponent<PersonalizationEditorObjectVolume>();

                ModdedObject volumeExtraSettings = Instantiate(propertiesPanel.m_volumeExtraSettings, container);
                volumeExtraSettings.gameObject.SetActive(true);
                Toggle hideIfNoPresetToggle = volumeExtraSettings.GetObject<Toggle>(0);
                hideIfNoPresetToggle.isOn = volume.hideIfNoPreset;
                hideIfNoPresetToggle.onValueChanged.AddListener(delegate (bool value)
                {
                    volume.hideIfNoPreset = value;
                    GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                });

                Dictionary<WeaponVariant, VolumeSettingsPreset> volumePresets = volume.volumeSettingPresets;
                if (volumePresets != null && volumePresets.Count != 0)
                {
                    foreach (KeyValuePair<WeaponVariant, VolumeSettingsPreset> preset in volumePresets)
                    {
                        VolumeSettingsPreset settingsPreset = preset.Value;

                        ModdedObject display = Instantiate(propertiesPanel.m_volumeSettingsPresetDisplay, container);
                        display.gameObject.SetActive(true);

                        // voxel model file
                        ModdedObject voxelModelFileField = display.GetObject<ModdedObject>(1);
                        InputField voxelModelFileFieldText = voxelModelFileField.GetObject<InputField>(0);
                        voxelModelFileFieldText.text = settingsPreset.VoxFilePath;
                        voxelModelFileField.GetObject<Button>(1).onClick.AddListener(delegate
                        {
                            ModUIUtils.FileExplorer(UIPersonalizationEditor.instance.transform, true, delegate (string filePath)
                            {
                                if (filePath.IsNullOrEmpty())
                                {
                                    voxelModelFileFieldText.text = string.Empty;
                                    settingsPreset.VoxFilePath = string.Empty;
                                }
                                else
                                {
                                    string directoryName = ModIOUtils.GetDirectoryName(PersonalizationEditorManager.Instance.currentEditingItemInfo.FolderPath);
                                    string fileName = Path.GetFileName(filePath);
                                    string path = Path.Combine(directoryName, "files", fileName);

                                    if (settingsPreset.VoxFilePath == path)
                                        return;
                                    else
                                    {
                                        settingsPreset.ColorReplacements = null; // reset color replacements for other palette
                                        Dictionary<string, FavoriteColorSettings> d = settingsPreset.ReplaceWithFavoriteColors;
                                        if (d == null)
                                            settingsPreset.ReplaceWithFavoriteColors = new Dictionary<string, FavoriteColorSettings>();
                                        else
                                            d.Clear();
                                    }

                                    voxelModelFileFieldText.text = path;
                                    settingsPreset.VoxFilePath = path;

                                    UIPersonalizationEditor.instance.Utilities.SetPresetPreview(preset.Key);
                                }

                                GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                            }, PersonalizationItemInfo.GetImportedFilesFolder(PersonalizationEditorManager.Instance.currentEditingItemInfo), "*.vox");
                        });

                        // center pivot
                        Toggle centerPivotToggle = display.GetObject<Toggle>(2);
                        centerPivotToggle.isOn = settingsPreset.CenterPivot;
                        centerPivotToggle.onValueChanged.AddListener(delegate (bool value)
                        {
                            settingsPreset.CenterPivot = value;
                            GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                        });

                        // conditions dropdown
                        bool allowCallback = true;
                        WeaponVariant prevCondition = preset.Key;
                        Dropdown conditionsDropdown = display.GetObject<Dropdown>(0);
                        conditionsDropdown.options = PersonalizationEditorManager.Instance.GetConditionOptionsDependingOnEditingWeapon();

                        int conditionDropdownValueToSet = -1;
                        for (int i = 0; i < conditionsDropdown.options.Count; i++)
                        {
                            if (conditionsDropdown.options[i] is DropdownWeaponVariantOptionData showConditionOptionData && showConditionOptionData.Value == prevCondition)
                            {
                                conditionDropdownValueToSet = i;
                            }
                        }

                        if (conditionDropdownValueToSet == -1)
                        {
                            conditionsDropdown.options.Add(new DropdownWeaponVariantOptionData(prevCondition));
                            conditionsDropdown.RefreshShownValue();

                            conditionDropdownValueToSet = conditionsDropdown.options.Count - 1;
                        }

                        conditionsDropdown.value = conditionDropdownValueToSet;
                        conditionsDropdown.onValueChanged.AddListener(delegate (int value)
                        {
                            if (!allowCallback)
                                return;

                            WeaponVariant condition = (conditionsDropdown.options[value] as DropdownWeaponVariantOptionData).Value;
                            if (volumePresets.ContainsKey(condition))
                            {
                                allowCallback = false;
                                conditionsDropdown.value = ((int)prevCondition) - 1;
                                allowCallback = true;

                                ModUIUtils.MessagePopupOK("Cannot change preset usage condition", "Another preset is already using this condition");
                            }
                            else
                            {
                                _ = volumePresets.Remove(prevCondition);
                                volumePresets.Add(condition, settingsPreset);

                                prevCondition = condition;
                                GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                            }
                        });
                        conditionsDropdown.interactable = volume.GetUnusedShowCondition() != WeaponVariant.None;

                        // active frame
                        void refreshActiveFrameAction()
                        {
                            display.GetObject<GameObject>(4).SetActive(prevCondition == PersonalizationEditorManager.Instance.previewPresetKey);
                        }
                        refreshActiveFrameAction();

                        EventController singleEventController = display.gameObject.AddComponent<EventController>();
                        singleEventController.AddEventListener(PersonalizationEditorManager.PRESET_PREVIEW_CHANGED_EVENT, refreshActiveFrameAction);
                        singleEventController.AddEventListener(PersonalizationEditorManager.OBJECT_EDITED_EVENT, refreshActiveFrameAction);

                        // colors
                        void onColorChangedAction(string value)
                        {
                            settingsPreset.ColorReplacements = value;
                            GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                        }

                        display.GetObject<Button>(3).onClick.AddListener(delegate
                        {
                            UIElementPersonalizationEditorVolumeColorsSettings volumeColorsSettings = propertiesPanel.m_volumeColorsSettings;
                            volumeColorsSettings.Show();
                            volumeColorsSettings.Populate(settingsPreset);
                            volumeColorsSettings.onColorChanged = onColorChangedAction;
                        });

                        // delete
                        display.GetObject<Button>(5).onClick.AddListener(delegate
                        {
                            _ = volumePresets.Remove(prevCondition);
                            populateFieldsAction();
                        });

                        // voxel size
                        float prevValue = settingsPreset.VoxelSize;
                        InputField voxelSizeFiled = display.GetObject<InputField>(6);
                        voxelSizeFiled.text = settingsPreset.VoxelSize.ToString(CultureInfo.InvariantCulture);
                        voxelSizeFiled.onEndEdit.AddListener(delegate (string value)
                        {
                            if (!allowCallback)
                                return;

                            float newValue = ModParseUtils.TryParseToFloat(value, prevValue);
                            if (newValue <= 0f)
                            {
                                newValue = 0.01f;

                                allowCallback = false;
                                voxelSizeFiled.text = newValue.ToString(CultureInfo.InvariantCulture);
                                allowCallback = true;
                            }

                            settingsPreset.VoxelSize = newValue;
                            prevValue = newValue;
                            GlobalEventManager.Instance.Dispatch(PersonalizationEditorManager.OBJECT_EDITED_EVENT);
                        });
                    }
                }

                if (volume.GetUnusedShowCondition() != WeaponVariant.None)
                {
                    Button newPresetButton = Instantiate(propertiesPanel.m_addVolumeSettingsPresetButton, container);
                    newPresetButton.gameObject.SetActive(true);
                    newPresetButton.onClick.AddListener(delegate
                    {
                        volume.volumeSettingPresets.Add(volume.GetUnusedShowCondition(), new VolumeSettingsPreset()
                        {
                            CenterPivot = true,
                            VoxelSize = 0.1f,
                            ReplaceWithFavoriteColors = new Dictionary<string, FavoriteColorSettings>()
                        });
                        populateFieldsAction();
                    });
                }
            }
        }

        public class FieldDisplay : OverhaulUIBehaviour
        {
            protected PersonalizationEditorObjectPropertyAttribute m_attribute;

            protected PersonalizationEditorObjectComponentBase m_componentBaseObject;

            public virtual void Set(PersonalizationEditorObjectPropertyAttribute attribute, PersonalizationEditorObjectComponentBase componentBaseObject, object value)
            {
                m_attribute = attribute;
                m_componentBaseObject = componentBaseObject;
            }
        }

        public class FileLocationField : FieldDisplay
        {
            [UIElement("InputField")]
            private readonly InputField m_field;

            [UIElementAction(nameof(OnEditButtonClicked))]
            [UIElement("EditButton")]
            private readonly Button m_editButton;

            public override void Set(PersonalizationEditorObjectPropertyAttribute attribute, PersonalizationEditorObjectComponentBase componentBaseObject, object value)
            {
                base.Set(attribute, componentBaseObject, value);
                m_field.text = value?.ToString();
            }

            public void OnEditButtonClicked()
            {
                ModUIUtils.FileExplorer(UIPersonalizationEditor.instance.transform, true, delegate (string filePath)
                {
                    string directoryName = ModIOUtils.GetDirectoryName(PersonalizationEditorManager.Instance.currentEditingItemInfo.FolderPath);
                    string fileName = Path.GetFileName(filePath);
                    string path = $"{directoryName}/files/{fileName}";

                    if ((string)m_attribute.propertyInfo.GetValue(m_componentBaseObject) == path)
                        return;

                    m_field.text = path;
                    m_attribute.propertyInfo.SetValue(m_componentBaseObject, path);
                }, PersonalizationItemInfo.GetImportedFilesFolder(PersonalizationEditorManager.Instance.currentEditingItemInfo), m_attribute.FileLocationSearchPattern);
            }
        }
    }
}
