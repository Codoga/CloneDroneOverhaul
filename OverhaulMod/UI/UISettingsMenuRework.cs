using AmplifyOcclusion;
using InternalModBot;
using ModBotWebsiteAPI;
using OverhaulMod.Engine;
using OverhaulMod.Patches.Behaviours;
using OverhaulMod.UI.Elements;
using OverhaulMod.Utils;
using OverhaulMod.Visuals;
using OverhaulMod.Visuals.Environment;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UISettingsMenuRework : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_closeButton;

        [UIElementAction(nameof(OnLegacyUIButtonClicked))]
        [UIElement("OldUIButton")]
        public Button LegacyUIButton;

        [UIElement("Content")]
        public Transform PageContentsTransform;

        [UIElement("Header1Prefab", false)]
        public ModdedObject Header1Prefab;
        [UIElement("Header2Prefab", false)]
        public ModdedObject Header2Prefab;
        [UIElement("Header3Prefab", false)]
        public ModdedObject Header3Prefab;
        [UIElement("Header4Prefab", false)]
        public ModdedObject Header4Prefab;

        [UIElement("DropdownPrefab", false)]
        public Dropdown DropdownPrefab;
        [UIElement("DropdownImagePrefab", false)]
        public Dropdown DropdownWithImagePrefab;
        [UIElement("DropdownImage169Prefab", false)]
        public Dropdown DropdownWithImage169Prefab;

        [UIElement("SliderPrefab", false)]
        public Slider SliderPrefab;

        [UIElement("TogglePrefab", false)]
        public ModdedObject TogglePrefab;
        [UIElement("ToggleWithOptionsPrefab", false)]
        public ModdedObject ToggleWithOptionsPrefab;

        [UIElement("ButtonPrefab", false)]
        public ModdedObject ButtonPrefab;

        [UIElement("KeyBindPrefab", false)]
        public ModdedObject KeyBindPrefab;

        [UIElement("GridContainer", false)]
        public GameObject GridContainer;

        [UIElement("LanguageButtonPrefab", false)]
        public ModdedObject LanguageButton;

        [TabManager(typeof(UIElementSettingsMenuCategoryTab), nameof(m_tabPrefab), nameof(m_tabContainer), nameof(OnTabCreated), nameof(OnTabSelected), new string[] { "Home", "Gameplay", "Interface", "Graphics", "Sounds", "Controls", "Multiplayer", "Languages", "Advanced" })]
        private readonly TabManager m_tabs;
        [UIElement("TabPrefab", false)]
        private readonly ModdedObject m_tabPrefab;
        [UIElement("TabsContainer")]
        private readonly Transform m_tabContainer;

        [UIElement("PanelNew")]
        private readonly RectTransform m_panelTransform;

        [UIElement("Shading")]
        private readonly GameObject m_shadingObject;

        [UIElement("BG")]
        private readonly GameObject m_normalBgObject;

        [UIElement("BGSetup")]
        private readonly GameObject m_setupBgObject;

        private bool m_hasSelectedTab;

        private bool m_hasMultiplayerCustomizationChanges;

        private string m_selectedTabId;

        public override bool hideTitleScreen => true;

        public bool disallowUsingKey
        {
            get;
            private set;
        }

        protected override void OnInitialized()
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
                settingsMenu.populateSettings();
        }

        public override void Show()
        {
            base.Show();

            SubtitleTextFieldPatchBehaviour subtitleTextFieldPatchBehaviour = GamePatchBehaviour.GetBehaviour<SubtitleTextFieldPatchBehaviour>();
            if (subtitleTextFieldPatchBehaviour)
            {
                subtitleTextFieldPatchBehaviour.SetSiblingIndex(base.transform);
            }

            UISubtitleTextFieldRework subtitleTextFieldRework = ModUIManager.Instance.Get<UISubtitleTextFieldRework>(AssetBundleConstants.UI, ModUIConstants.UI_SUBTITLE_TEXT_FIELD_REWORK);
            if (subtitleTextFieldRework)
            {
                subtitleTextFieldRework.SetSiblingIndex(true);
            }

            UIPressActionKeyDescription pressActionKeyDescription = ModUIManager.Instance.Get<UIPressActionKeyDescription>(AssetBundleConstants.UI, ModUIConstants.UI_PRESS_ACTION_KEY_DESCRIPTION);
            if (!pressActionKeyDescription)
                pressActionKeyDescription = ModUIConstants.ShowPressActionKeyDescription();
            pressActionKeyDescription.SetSiblingIndex(true);

            if (!m_selectedTabId.IsNullOrEmpty())
                PopulatePage(m_selectedTabId);
        }

        public override void Hide()
        {
            base.Hide();

            SubtitleTextFieldPatchBehaviour subtitleTextFieldPatchBehaviour = GamePatchBehaviour.GetBehaviour<SubtitleTextFieldPatchBehaviour>();
            if (subtitleTextFieldPatchBehaviour)
            {
                subtitleTextFieldPatchBehaviour.ResetSiblingIndex();
            }

            UISubtitleTextFieldRework subtitleTextFieldRework = ModUIManager.Instance.Get<UISubtitleTextFieldRework>(AssetBundleConstants.UI, ModUIConstants.UI_SUBTITLE_TEXT_FIELD_REWORK);
            if (subtitleTextFieldRework)
            {
                subtitleTextFieldRework.SetSiblingIndex(false);
            }

            UIPressActionKeyDescription pressActionKeyDescription = ModUIManager.Instance.Get<UIPressActionKeyDescription>(AssetBundleConstants.UI, ModUIConstants.UI_PRESS_ACTION_KEY_DESCRIPTION);
            if (pressActionKeyDescription)
            {
                pressActionKeyDescription.SetSiblingIndex(false);
            }

            ModSettingsDataManager.Instance.Save();
            if (m_hasMultiplayerCustomizationChanges && BoltNetwork.IsRunning && !BoltNetwork.IsServer && !BoltNetwork.IsSinglePlayer)
            {
                MultiplayerMatchManager.Instance.SendClientCharacterCustomizationEvent();
                m_hasMultiplayerCustomizationChanges = false;
            }
        }

        public void ShowRegularElements()
        {
            disallowUsingKey = false;
            m_panelTransform.anchorMax = new Vector2(1f, 1f);
            m_panelTransform.anchorMin = new Vector2(0f, 0f);
            m_panelTransform.sizeDelta = new Vector2(0f, 0f);
            m_shadingObject.SetActive(true);
            m_normalBgObject.SetActive(true);
            m_setupBgObject.SetActive(false);

            if (!m_hasSelectedTab)
            {
                m_tabs.SelectTab("Home");
                m_hasSelectedTab = true;
            }
        }

        public void ShowSetupElements()
        {
            disallowUsingKey = true;
            m_panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
            m_panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
            m_panelTransform.sizeDelta = new Vector2(325f, 450f);
            m_shadingObject.SetActive(false);
            m_normalBgObject.SetActive(false);
            m_setupBgObject.SetActive(true);
            PopulatePage("setup");
        }

        public void OnTabSelected(UIElementTab elementTab)
        {
            PopulatePage(elementTab.tabId);
        }

        public void OnTabCreated(UIElementTab elementTab)
        {
            UIElementSettingsMenuCategoryTab elementTabWithText = elementTab as UIElementSettingsMenuCategoryTab;
            elementTabWithText.LocalizationID = $"settings_tab_{elementTab.tabId.ToLower()}";
        }

        public void ClearPageContents()
        {
            if (PageContentsTransform && PageContentsTransform.childCount > 0)
                TransformUtils.DestroyAllChildren(PageContentsTransform);
        }

        public void PopulatePageIfSelected(string id)
        {
            if (m_selectedTabId != id)
                return;

            PopulatePage(id);
        }

        public void PopulatePage(string id)
        {
            m_selectedTabId = id;
            ClearPageContents();

            UIElementTab oldTab = m_tabs.prevSelectedTab;
            UIElementTab newTab = m_tabs.selectedTab;
            if (oldTab)
            {
                RectTransform rt = oldTab.transform as RectTransform;
                Vector2 vector = rt.sizeDelta;
                vector.y = 25f;
                rt.sizeDelta = vector;
            }
            if (newTab)
            {
                RectTransform rt = newTab.transform as RectTransform;
                Vector2 vector = rt.sizeDelta;
                vector.y = 30f;
                rt.sizeDelta = vector;
            }

            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (!settingsMenu)
                return;

            switch (id)
            {
                case "setup":
                    populateSetupPage(settingsMenu);
                    break;
                case "Home":
                    populateHomePage(settingsMenu);
                    break;
                case "Gameplay":
                    populateGameplayPage(settingsMenu);
                    break;
                case "Interface":
                    populateInterfacePage(settingsMenu);
                    break;
                case "Graphics":
                    populateGraphicsPage(settingsMenu);
                    break;
                case "Sounds":
                    populateSoundsPage(settingsMenu);
                    break;
                case "Controls":
                    populateControlsPage(settingsMenu);
                    break;
                case "Multiplayer":
                    populateMultiplayerPage(settingsMenu);
                    break;
                case "Mod-Bot":
                    populateModBotPage(settingsMenu);
                    break;
                case "Advanced":
                    populateAdvancedPage(settingsMenu);
                    break;
                case "UI Patches":
                    populateUIPatchesPage(settingsMenu);
                    break;
                case "Languages":
                    populateLanguagesPage(settingsMenu);
                    break;
                default:
                    populateDefaultPage(settingsMenu);
                    break;
            }
        }

        private void populateSetupPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);

            _ = pageBuilder.Header1("Graphics");
            _ = pageBuilder.Header3("Post effects");
            _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_SSAO), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_SSAO, value, true);
            }, "Ambient occlusion", delegate
            {
                populateSSAOSettingsPage(m_selectedTabId);
            });

            bool moreEffectsEnabled = ModFeatures.IsEnabled(ModFeatures.FeatureType.MoreImageEffects);
            if (moreEffectsEnabled)
            {
                _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CHROMATIC_ABERRATION), delegate (bool value)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CHROMATIC_ABERRATION, value, true);
                }, "Chromatic aberration", delegate
                {
                    populateCASettingsPage(m_selectedTabId);
                });
            }

            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_DITHERING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_DITHERING, value, true);
            }, "Dithering");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_VIGNETTE), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_VIGNETTE, value, true);
            }, "Vignette");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.TWEAK_BLOOM), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.TWEAK_BLOOM, value, true);
            }, "Adjust bloom settings");

            _ = pageBuilder.Header3("Camera effects");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CAMERA_ROLLING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CAMERA_ROLLING, value, true);
            }, "Camera rolling");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING, value, true);
            }, "Camera bobbing");

            _ = pageBuilder.Header3("Particle effects");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_NEW_PARTICLES), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_NEW_PARTICLES, value, true);
            }, "Enable particles");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_GARBAGE_PARTICLES), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_GARBAGE_PARTICLES, value, true);
            }, "Enable sparks");

            _ = pageBuilder.Header3("Voxel engine");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_VOXEL_FIRE_FADING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_VOXEL_FIRE_FADING, value, true);
            }, "Better fire spreading");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.CHANGE_HIT_COLORS), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.CHANGE_HIT_COLORS, value, true);
            }, "Better damage colors");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_VOXEL_BURNING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_VOXEL_BURNING, value, true);
            }, "Always burn voxels");

            _ = pageBuilder.Header1("Gameplay");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_FIRST_PERSON_MODE), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_FIRST_PERSON_MODE, value, true);
            }, "First person mode");
            _ = pageBuilder.KeyBind("Toggle camera mode", (KeyCode)ModSettingsManager.GetIntValue(ModSettingsConstants.CAMERA_MODE_TOGGLE_KEYBIND), KeyCode.Y, delegate (KeyCode value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.CAMERA_MODE_TOGGLE_KEYBIND, (int)value, true);
            });

            _ = pageBuilder.Header1("Game interface");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.SHOW_VERSION_LABEL), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.SHOW_VERSION_LABEL, value, true);
            }, "Show Overhaul mod version");
            _ = pageBuilder.Button("Configure Overhaul mod UIs", delegate
            {
                _ = ModUIConstants.ShowOverhaulUIManagementPanel(base.transform);
            });

            _ = pageBuilder.Button("Done", delegate
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.SHOW_MOD_SETUP_SCREEN_ON_START, false);
                UITitleScreenRework titleScreenCustomizationPanel = ModUIManager.Instance?.Get<UITitleScreenRework>(AssetBundleConstants.UI, ModUIConstants.UI_TITLE_SCREEN);
                if (titleScreenCustomizationPanel)
                {
                    titleScreenCustomizationPanel.enableRework = ModUIManager.ShowTitleScreenRework;
                }

                ModUIUtils.ShowNewUpdateMessageOrChangelog(1f, true, false);

                Hide();
            });
        }

        private void populateHomePage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("Game interface");
            //_ = pageBuilder.Header3("Language");
            //_ = pageBuilder.Dropdown(ModLocalizationManager.Instance.GetLanguageOptions(false), getCurrentLanguageIndex(), OnLanguageDropdownChanged);
            _ = pageBuilder.Toggle(!settingsMenu.HideGameUIToggle.isOn, OnHideGameUIToggleChanged, "Show game UI");
            _ = pageBuilder.Toggle(settingsMenu.SubtitlesToggle.isOn, OnSubtitlesToggleChanged, "Show subtitles");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.SHOW_SPEAKER_NAME), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.SHOW_SPEAKER_NAME, value, true);
                SpeechAudioManager.Instance.PlaySequence("CloneDroneIntro", false);
            }, "Display who's speaking");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.SHOW_VERSION_LABEL), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.SHOW_VERSION_LABEL, value, true);
            }, "Show Overhaul mod version");
            _ = pageBuilder.Button("Configure Overhaul mod UIs", delegate
            {
                _ = ModUIConstants.ShowOverhaulUIManagementPanel(base.transform);
            });
            /*_ = pageBuilder.Button("Configure UI enhancements", delegate
            {
                ClearPageContents();
                populateUIPatchesPage(settingsMenu);
            });*/

            _ = pageBuilder.KeyBind("Camera mode", (KeyCode)ModSettingsManager.GetIntValue(ModSettingsConstants.CAMERA_MODE_TOGGLE_KEYBIND), KeyCode.Y, delegate (KeyCode value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.CAMERA_MODE_TOGGLE_KEYBIND, (int)value, true);
            });
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_FIRST_PERSON_MODE), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_FIRST_PERSON_MODE, value, true);
            }, "First person mode");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING, value, true);
            }, "Camera bobbing");

            _ = pageBuilder.Header1("Multiplayer settings");
            _ = pageBuilder.Header3("Preferred region");
            _ = pageBuilder.Dropdown(settingsMenu.RegionDropdown.options, settingsMenu.RegionDropdown.value, OnRegionChanged);
            _ = pageBuilder.Header3("Player");
            _ = pageBuilder.DropdownWithImage(settingsMenu.MultiplayerCharacterModelDropdown.options, settingsMenu.MultiplayerCharacterModelDropdown.value, OnCharacterModelChanged);
            _ = pageBuilder.DropdownWithImage(settingsMenu.MultiplayerFavoriteColorDropdown.options, settingsMenu.MultiplayerFavoriteColorDropdown.value, OnMultiplayerFavoriteColorDropdownChanged);
            _ = pageBuilder.Toggle(settingsMenu.UseSkinInSinglePlayer.isOn, OnUseSkinInSinglePlayerToggleChanged, "Use skin in singleplayer");
            _ = pageBuilder.Button("Select emotes", delegate
            {
                GameUIRoot.Instance.EmoteSettingsUI.Show();
                ModUIManager.Instance.InvokeActionInsteadOfHidingCustomUI(delegate
                {
                    GameUIRoot.Instance.EmoteSettingsUI.Hide();
                });
            });
        }

        private void populateInterfacePage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("Game interface");
            _ = pageBuilder.Toggle(!settingsMenu.HideGameUIToggle.isOn, OnHideGameUIToggleChanged, "Show game UI");
            _ = pageBuilder.Toggle(settingsMenu.SubtitlesToggle.isOn, OnSubtitlesToggleChanged, "Show subtitles");
            _ = pageBuilder.Button("Configure Overhaul mod UIs", delegate
            {
                _ = ModUIConstants.ShowOverhaulUIManagementPanel(base.transform);
            });
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.SHOW_VERSION_LABEL), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.SHOW_VERSION_LABEL, value, true);
            }, "Show Overhaul mod version");

            _ = pageBuilder.Header1("Energy bar enhancements");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENERGY_UI_REWORK), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENERGY_UI_REWORK, value, true);
            }, "Energy bar redesign");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_IF_FULL), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_IF_FULL, value, true);
            }, "Fade out energy bar if full");
            _ = pageBuilder.Header3("Fade out intensity");
            _ = pageBuilder.Slider(0.1f, 1f, false, ModSettingsManager.GetFloatValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_INTENSITY), delegate (float value)
            {
                ModSettingsManager.SetFloatValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_INTENSITY, value, true);
            }, true);
            _ = pageBuilder.Button("Reset energy bar settings", delegate
            {
                ModSettingsManager.ResetValue(ModSettingsConstants.ENERGY_UI_REWORK, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_IF_FULL, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_INTENSITY, true);

                ClearPageContents();
                populateUIPatchesPage(settingsMenu);
            });

            _ = pageBuilder.Header1("Labels");
            _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_PRESS_BUTTON_TRIGGER_DESCRIPTION_REWORK), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_PRESS_BUTTON_TRIGGER_DESCRIPTION_REWORK, value, true);
                if (value)
                {
                    PressActionKeyObjectManager.Instance.ShowThenHideDescription(ModCore.LoremIpsumText, 2f);
                }
            }, "Use key trigger description rework", delegate
            {
                populateUKTDReworkSettingsPage(m_selectedTabId);
            });
            _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_SUBTITLE_TEXT_FIELD_REWORK), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_SUBTITLE_TEXT_FIELD_REWORK, value, true);
                SpeechAudioManager.Instance.PlaySequence("CloneDroneIntro", false);
            }, "Commentator subtitles rework", delegate
            {
                populateSubtitlesReworkSettingsPage(m_selectedTabId);
            });
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.SHOW_SPEAKER_NAME), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.SHOW_SPEAKER_NAME, value, true);
                SpeechAudioManager.Instance.PlaySequence("CloneDroneIntro", false);
            }, "Display who's speaking");
        }

        private void populateGraphicsPage(SettingsMenu settingsMenu)
        {
            settingsMenu.refreshResolutionOptions();

            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("Graphics");
            _ = pageBuilder.Header3("Window");
            _ = pageBuilder.Dropdown(settingsMenu.ScreenResolutionDropDown.options, settingsMenu.ScreenResolutionDropDown.value, OnScreenResolutionChanged);
            _ = pageBuilder.Toggle(settingsMenu.FullScreenToggle.isOn, OnFullScreenChanged, "Fullscreen");
            _ = pageBuilder.Toggle(settingsMenu.VsyncOnToggle.isOn, OnVSyncChanged, "V-Sync");

            if (ModSpecialUtils.SupportsTitleBarOverhaul())
            {
                _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_TITLE_BAR_OVERHAUL), delegate (bool value)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_TITLE_BAR_OVERHAUL, value, true);
                }, "Enable title bar changes");
            }

            _ = pageBuilder.Header3("Render");
            _ = pageBuilder.Dropdown(settingsMenu.QualityDropDown.options, settingsMenu.QualityDropDown.value, OnQualityDropdownChanged);
            _ = pageBuilder.Dropdown(settingsMenu.AntiAliasingDropdown.options, settingsMenu.AntiAliasingDropdown.value, OnAntiAliasingDropdownChanged);

            bool moreEffectsEnabled = ModFeatures.IsEnabled(ModFeatures.FeatureType.MoreImageEffects);
            _ = pageBuilder.Header3("Post effects");
            _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_SSAO), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_SSAO, value, true);
            }, "Ambient occlusion", delegate
            {
                populateSSAOSettingsPage(m_selectedTabId);
            });

            if (moreEffectsEnabled)
            {
                _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CHROMATIC_ABERRATION), delegate (bool value)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CHROMATIC_ABERRATION, value, true);
                }, "Chromatic aberration", delegate
                {
                    populateCASettingsPage(m_selectedTabId);
                });
            }

            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_DITHERING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_DITHERING, value, true);
            }, "Dithering");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_VIGNETTE), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_VIGNETTE, value, true);
            }, "Vignette");
            _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_BLOOM), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_BLOOM, value, true);
            }, "Bloom", delegate
            {
                populateBloomSettingsPage(m_selectedTabId);
            });

            if (moreEffectsEnabled)
            {
                _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_DOF), delegate (bool value)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_DOF, value, true);
                }, "Depth of field (DoF)", delegate
                {
                    populateCASettingsPage(m_selectedTabId);
                });
                _ = pageBuilder.ToggleWithOptions(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_SUN_SHAFTS), delegate (bool value)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_SUN_SHAFTS, value, true);
                }, "Sun shafts", delegate
                {
                    populateSSAOSettingsPage(m_selectedTabId);
                });

                _ = pageBuilder.Header3("Color blindness mode");
                _ = pageBuilder.Dropdown(PostEffectsManager.ColorBlindnessOptions, ModSettingsManager.GetIntValue(ModSettingsConstants.COLOR_BLINDNESS_MODE), delegate (int value)
                {
                    ModSettingsManager.SetIntValue(ModSettingsConstants.COLOR_BLINDNESS_MODE, value, true);
                });
                _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.COLOR_BLINDNESS_AFFECT_UI), delegate (bool value)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.COLOR_BLINDNESS_AFFECT_UI, value, true);
                }, "Affect UI");
            }
            else
            {
                _ = pageBuilder.Header4("New post effects coming in update 4.3!");
            }

            /*_ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.TWEAK_BLOOM), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.TWEAK_BLOOM, value, true);
            }, "Adjust bloom settings");
            _ = pageBuilder.Header4("Disable this setting to revert the vanilla bloom");*/

            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.WeaponBag))
            {
                _ = pageBuilder.Header1("Robots");
                _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_WEAPON_BAG), delegate (bool value)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_WEAPON_BAG, value, true);
                }, "Show equipped weapons");
            }

            _ = pageBuilder.Header1("Camera");
            _ = pageBuilder.Header3("Field of view");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_FOV_OVERRIDE), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_FOV_OVERRIDE, value, true);
                PopulatePage("Graphics");
            }, "Enable FOV override");
            Text fovOverrideHeader4 = pageBuilder.Header4("fov_override");
            Vector2 fovOverrideHeader4SizeDelta = (fovOverrideHeader4.transform.parent as RectTransform).sizeDelta;
            fovOverrideHeader4SizeDelta.y += 15f;
            (fovOverrideHeader4.transform.parent as RectTransform).sizeDelta = fovOverrideHeader4SizeDelta;

            if (CameraFOVController.EnableFOVOverride)
            {
                _ = pageBuilder.Slider(-10f, 40f, true, ModSettingsManager.GetFloatValue(ModSettingsConstants.CAMERA_FOV_OFFSET), delegate (float value)
                {
                    ModSettingsManager.SetFloatValue(ModSettingsConstants.CAMERA_FOV_OFFSET, value, true);
                }, true, (float val) =>
                {
                    float roundedValue = Mathf.Round(val * 10f) / 10f;
                    return $"{(val > 0f ? "+" : string.Empty)}{roundedValue} ({60f + roundedValue})";
                });
            }

            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CAMERA_ROLLING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CAMERA_ROLLING, value, true);
            }, "Camera rolling");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING, value, true);
            }, "Camera bobbing");
            _ = pageBuilder.Button("Reset camera settings", delegate
            {
                ModSettingsManager.ResetValue(ModSettingsConstants.CAMERA_FOV_OFFSET, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.ENABLE_CAMERA_ROLLING, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING, true);
                PopulatePage("Graphics");
            });


            _ = pageBuilder.Header1("Environment");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_LIGHTING_TRANSITION), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_LIGHTING_TRANSITION, value, true);
            }, "Lighting transitions");
            _ = pageBuilder.Header4("The lighting changes smoothly as level switches");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_ARENA_REMODEL), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_ARENA_REMODEL, value, true);
            }, "Arena remodel");
            _ = pageBuilder.Header4("Made by @water2977");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_FLOATING_DUST), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_FLOATING_DUST, value, true);
            }, "Floating dust");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_WEATHER), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_WEATHER, value, true);
            }, "Enable weather");
            _ = pageBuilder.Header3("Force weather type");
            _ = pageBuilder.Dropdown(WeatherManager.Instance.GetTranslatedWeatherOptions(), ModSettingsManager.GetIntValue(ModSettingsConstants.FORCE_WEATHER_TYPE), delegate (int value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.FORCE_WEATHER_TYPE, value, true);
            });

            _ = pageBuilder.Header1("Voxel engine");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.CHANGE_HIT_COLORS), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.CHANGE_HIT_COLORS, value, true);
            }, "Better damage colors");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_VOXEL_FIRE_FADING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_VOXEL_FIRE_FADING, value, true);
            }, "Better fire spreading");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_VOXEL_BURNING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_VOXEL_BURNING, value, true);
            }, "Always burn voxels");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_NEW_PARTICLES), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_NEW_PARTICLES, value, true);
            }, "Enable particles");

            _ = pageBuilder.Header1("Garbage");
            _ = pageBuilder.Dropdown(settingsMenu.GarbageSettingsDropdown.options, settingsMenu.GarbageSettingsDropdown.value, OnGarbageSettingsChanged);
            _ = pageBuilder.Toggle(settingsMenu.PlayerPushesGarbageToggle.isOn, OnPlayerPushesGarbageToggleChanged, "Collisions");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_GARBAGE_PARTICLES), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_GARBAGE_PARTICLES, value, true);
            }, "Enable sparks");
        }

        private void populateGameplayPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("Gameplay settings");
            _ = pageBuilder.Header3("Difficulty");
            _ = pageBuilder.Dropdown(settingsMenu.StoryModeDifficultyDropDown.options, settingsMenu.StoryModeDifficultyDropDown.value, OnStoryDifficultyIndexChanged);
            _ = pageBuilder.Header4("Change what enemies spawn");

            _ = pageBuilder.KeyBind("Camera mode", (KeyCode)ModSettingsManager.GetIntValue(ModSettingsConstants.CAMERA_MODE_TOGGLE_KEYBIND), KeyCode.Y, delegate (KeyCode value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.CAMERA_MODE_TOGGLE_KEYBIND, (int)value, true);
            });
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_FIRST_PERSON_MODE), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_FIRST_PERSON_MODE, value, true);
            }, "First person mode");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CAMERA_BOBBING, value, true);
            }, "Camera bobbing");

            _ = pageBuilder.Header3("Endless levels");
            _ = pageBuilder.Dropdown(settingsMenu.WorkshopLevelPolicyDropdown.options, settingsMenu.WorkshopLevelPolicyDropdown.value, OnWorkshopEndlessLevelPolicyIndexChanged);
            Button button = pageBuilder.Button("Get more levels", delegate
            {
                Hide();
                if (!ModFeatures.IsEnabled(ModFeatures.FeatureType.WorkshopBrowserRework))
                    ModCache.titleScreenUI.OnWorkshopBrowserButtonClicked();
                else
                    _ = ModUIConstants.ShowWorkshopBrowserRework();
            });
            button.interactable = GameModeManager.IsOnTitleScreen();

            _ = pageBuilder.Header1("Twitch");
            _ = pageBuilder.Button("Enemy spawn settings", delegate
            {
                settingsMenu.OnTwitchEnemyLimitButtonClicked();
                ModUIManager.Instance.InvokeActionInsteadOfHidingCustomUI(delegate
                {
                    GameUIRoot.Instance.TwitchEnemySettingsMenu.Hide();
                });
            });
            _ = pageBuilder.Toggle(settingsMenu.MuteEmotesToggle.isOn, OnMuteEmotesToggleChanged, "Mute twitch emotes");
            _ = pageBuilder.Toggle(settingsMenu.DevIsLiveEnabledToggle.isOn, OnDevIsLiveToggleChanged, "Dev stream notifications");
        }

        private void populateSoundsPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("Volume");
            _ = pageBuilder.Header3("Global");
            _ = pageBuilder.Slider(0f, 1f, false, settingsMenu.SoundVolume.value, OnGlobalVolumeChanged);
            _ = pageBuilder.Header3("Music");
            _ = pageBuilder.Slider(0f, 1f, false, settingsMenu.MusicVolume.value, OnMusicVolumeChanged);
            _ = pageBuilder.Header3("Commentator");
            _ = pageBuilder.Slider(0f, 1f, false, settingsMenu.CommentatorsVolume.value, OnCommentatorVolumeChanged);

            _ = pageBuilder.Header1("Filters");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_REVERB_FILTER), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_REVERB_FILTER, value, true);
            }, "Reverb");
            _ = pageBuilder.Header3("Reverb intensity");
            _ = pageBuilder.Slider(0.1f, 1.5f, false, ModSettingsManager.GetFloatValue(ModSettingsConstants.REVERB_FILTER_INTENSITY), delegate (float value)
            {
                ModSettingsManager.SetFloatValue(ModSettingsConstants.REVERB_FILTER_INTENSITY, value, true);
            });
        }

        private void populateMultiplayerPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("Multiplayer settings");
            _ = pageBuilder.Header3("Preferred region");
            _ = pageBuilder.Dropdown(settingsMenu.RegionDropdown.options, settingsMenu.RegionDropdown.value, OnRegionChanged);
            _ = pageBuilder.Button("Manage muted players", delegate
            {
                GameUIRoot.Instance.BlockListSettingsUI.Show();
                ModUIManager.Instance.InvokeActionInsteadOfHidingCustomUI(delegate
                {
                    GameUIRoot.Instance.BlockListSettingsUI.Hide();
                });
            });

            _ = pageBuilder.Header1("Player");
            _ = pageBuilder.Header3("Skin");
            _ = pageBuilder.DropdownWithImage(settingsMenu.MultiplayerCharacterModelDropdown.options, settingsMenu.MultiplayerCharacterModelDropdown.value, OnCharacterModelChanged);
            _ = pageBuilder.DropdownWithImage(settingsMenu.MultiplayerFavoriteColorDropdown.options, settingsMenu.MultiplayerFavoriteColorDropdown.value, OnMultiplayerFavoriteColorDropdownChanged);
            _ = pageBuilder.Toggle(settingsMenu.UseSkinInSinglePlayer.isOn, OnUseSkinInSinglePlayerToggleChanged, "Use skin in singleplayer");

            _ = pageBuilder.Header3("Personalization");
            _ = pageBuilder.Button("Select emotes", delegate
            {
                GameUIRoot.Instance.EmoteSettingsUI.Show();
            });
        }

        private void populateControlsPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("Controls settings");
            _ = pageBuilder.Button("Edit controls", delegate
            {
                GameUIRoot.Instance.ControlMapper.Open();
                ModUIManager.Instance.InvokeActionInsteadOfHidingCustomUI(delegate
                {
                    _ = GameUIRoot.Instance.ControlMapper.Close(true);
                });
            });

            _ = pageBuilder.Header3("Mouse");
            _ = pageBuilder.Slider(settingsMenu.MouseSensitivitySlider.minValue, settingsMenu.MouseSensitivitySlider.maxValue, false, settingsMenu.MouseSensitivitySlider.value, OnMouseSensitivityChanged);
            _ = pageBuilder.Toggle(settingsMenu.InvertMouseToggle.isOn, OnInvertMouseToggleChanged, "Invert mouse");
            _ = pageBuilder.Header3("Controller");
            _ = pageBuilder.Slider(settingsMenu.ControllerSensitivitySlider.minValue, settingsMenu.ControllerSensitivitySlider.maxValue, false, settingsMenu.ControllerSensitivitySlider.value, OnControllerSensitivityChanged);
            _ = pageBuilder.Toggle(settingsMenu.InvertControllerToggle.isOn, OnInvertControllerToggleChanged, "Invert controller");
            _ = pageBuilder.Toggle(settingsMenu.EqualLookRatioToggle.isOn, OnEqualLookRatioToggleChanged, "1:1 look ratio");
        }

        private void populateModBotPage(SettingsMenu settingsMenu)
        {
        }

        private void populateDefaultPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("This page is not implemented yet.");
            _ = pageBuilder.Header2("Try using original menu");
            _ = pageBuilder.Button("Open original settings menu", OnLegacyUIButtonClicked);
        }

        private void populateAdvancedPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Header1("Mod-Bot");
            _ = pageBuilder.Header3("Controls");
            _ = pageBuilder.KeyBind("Open console", ModBotInputManager.GetKeyCode(ModBotInputType.OpenConsole), KeyCode.F1, delegate (KeyCode value)
            {
                ModBotInputManager.InputOptions[0].Key = value;
            });
            _ = pageBuilder.KeyBind("Toggle FPS label", ModBotInputManager.GetKeyCode(ModBotInputType.ToggleFPSLabel), KeyCode.F3, delegate (KeyCode value)
            {
                ModBotInputManager.InputOptions[1].Key = value;
            });

            _ = pageBuilder.Header3("Website integration");
            if (API.HasSession)
            {
                _ = pageBuilder.Button("Edit tags", delegate
                {
                    Application.OpenURL("https://modbot.org/tagBrowsing.html");
                });

                Button button = null;
                button = pageBuilder.Button("Sign out", delegate
                {
                    button.interactable = false;
                    API.SignOut(onSignedOut);
                });
            }
            else
            {
                _ = pageBuilder.Button("Sign in", delegate
                {
                    GameUIRoot.Instance.SettingsMenu.Hide();
                    ModBotUIRoot.Instance.ModBotSignInUI.OpenSignInForm();
                });
            }

            _ = pageBuilder.Header1("Transitions");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.OVERHAUL_SCENE_TRANSITIONS), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.OVERHAUL_SCENE_TRANSITIONS, value, true);
            }, "Better scene transitions");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.OVERHAUL_NON_SCENE_TRANSITIONS), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.OVERHAUL_NON_SCENE_TRANSITIONS, value, true);
            }, "Better in-game transitions");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.TRANSITION_ON_STARTUP), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.TRANSITION_ON_STARTUP, value, true);
            }, "Transition on startup");
            _ = pageBuilder.Header4("Doborog logo will be smoothly faded out on game start");

            _ = pageBuilder.Header1("Rich presence");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_RPC), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_RPC, value, true);
            }, "Enable rich presence");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.RPC_DETAILS), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.RPC_DETAILS, value, true);
            }, "Display details");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.RPC_DISPLAY_LEVEL_FILE_NAME), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.RPC_DISPLAY_LEVEL_FILE_NAME, value, true);
            }, "Display editing level name");

            _ = pageBuilder.Header1("Multiplayer settings");
            _ = pageBuilder.Toggle(settingsMenu.RelayToggle.isOn, OnRelayToggleChanged, "Relay connection");
            _ = pageBuilder.Header4("Improves ping on some machines, but can also make it worse");

            _ = pageBuilder.Header1("Misc.");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ADVANCED_PHOTO_MODE), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ADVANCED_PHOTO_MODE, value, true);
            }, "Advanced photo mode");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.REQUIRE_RMB_HOLD_WHEN_UI_IS_HIDDEN), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.REQUIRE_RMB_HOLD_WHEN_UI_IS_HIDDEN, value, true);
            }, "Require RMB holding");
            Text rmbHoldHeader4 = pageBuilder.Header4("Require holding right mouse button when controls are hidden");
            Vector2 rmbHoldHeader4SizeDelta = (rmbHoldHeader4.transform.parent as RectTransform).sizeDelta;
            rmbHoldHeader4SizeDelta.y += 15f;
            (rmbHoldHeader4.transform.parent as RectTransform).sizeDelta = rmbHoldHeader4SizeDelta;

            if (!ModFeatures.IsEnabled(ModFeatures.FeatureType.AdvancedSettings))
            {
                return;
            }

            _ = pageBuilder.Header1("Reset settings");
            _ = pageBuilder.Button("Reset Overhaul settings", delegate
            {
                ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("settings_reset_settings_header"), LocalizationManager.Instance.GetTranslatedString("settings_reset_settings_description"), 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "Ok", "Yes", "No", null, delegate
                {
                    ModSettingsManager.Instance.ResetSettings();
                    _ = ModUIConstants.ShowRestartRequiredScreen(true);
                });
            });
        }

        private void populateUIPatchesPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Button("Go back", delegate
            {
                ClearPageContents();
                populateHomePage(settingsMenu);
            });

            _ = pageBuilder.Header1("Energy bar enhancements");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENERGY_UI_REWORK), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENERGY_UI_REWORK, value, true);
            }, "Energy bar redesign");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_IF_FULL), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_IF_FULL, value, true);
            }, "Fade out energy bar if full");
            _ = pageBuilder.Header3("Fade out intensity");
            _ = pageBuilder.Slider(0.1f, 1f, false, ModSettingsManager.GetFloatValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_INTENSITY), delegate (float value)
            {
                ModSettingsManager.SetFloatValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_INTENSITY, value, true);
            }, true);
            _ = pageBuilder.Button("Reset energy bar settings", delegate
            {
                ModSettingsManager.ResetValue(ModSettingsConstants.ENERGY_UI_REWORK, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_IF_FULL, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.ENERGY_UI_FADE_OUT_INTENSITY, true);

                ClearPageContents();
                populateUIPatchesPage(settingsMenu);
            });
        }

        private void populateLanguagesPage(SettingsMenu settingsMenu)
        {
            PageBuilder pageBuilder = new PageBuilder(this);

            Text header = pageBuilder.Header1("Change language");
            header.alignment = TextAnchor.LowerCenter;

            RectTransform container = pageBuilder.GridContainer(new Vector2(410f, 1f), new Vector2(200f, 50f), new Vector2(5f, 5f));
            pageBuilder.LanguageButton("en", container);
            pageBuilder.LanguageButton("fr", container);
            pageBuilder.LanguageButton("it", container);
            pageBuilder.LanguageButton("de", container);
            pageBuilder.LanguageButton("es-ES", container);
            pageBuilder.LanguageButton("es-419", container);
            pageBuilder.LanguageButton("zh-CN", container);
            pageBuilder.LanguageButton("zh-TW", container);
            pageBuilder.LanguageButton("ru", container);
            pageBuilder.LanguageButton("pt-BR", container);
            pageBuilder.LanguageButton("ja", container);
            pageBuilder.LanguageButton("ko", container);
        }

        private void populateSubtitlesReworkSettingsPage(string initialPage)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Button("Go back", delegate
            {
                ClearPageContents();
                PopulatePage(initialPage);
            });

            _ = pageBuilder.Header1("Commentator subtitles rework");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_SUBTITLE_TEXT_FIELD_REWORK), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_SUBTITLE_TEXT_FIELD_REWORK, value, true);
                SpeechAudioManager.Instance.PlaySequence("CloneDroneIntro", false);
            }, "Enable");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_UPPER_POSITION), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_UPPER_POSITION, value, true);
                SpeechAudioManager.Instance.PlaySequence("CloneDroneIntro", false);
            }, "Be on top");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_BG), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_BG, value, true);
                SpeechAudioManager.Instance.PlaySequence("CloneDroneIntro", false);
            }, "Enable background");

            _ = pageBuilder.Header3("Font");
            _ = pageBuilder.Dropdown(UISubtitleTextFieldRework.FontOptions, ModSettingsManager.GetIntValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_FONT), delegate (int value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_FONT, value, true);
                SpeechAudioManager.Instance.PlaySequence("CloneDroneIntro", false);
            });
            _ = pageBuilder.Header4("Some languages might not be supported by certain fonts");
            _ = pageBuilder.Header3("Font size");
            _ = pageBuilder.Slider(8f, 15f, true, ModSettingsManager.GetIntValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_FONT_SIZE), delegate (float value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_FONT_SIZE, Mathf.RoundToInt(value), true);
                SpeechAudioManager.Instance.PlaySequence("CloneDroneIntro", false);
            }, true, (float val) =>
            {
                return $"{Mathf.RoundToInt(val)}";
            });

            _ = pageBuilder.Button("Reset settings", delegate
            {
                ModSettingsManager.ResetValue(ModSettingsConstants.ENABLE_SUBTITLE_TEXT_FIELD_REWORK, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_UPPER_POSITION, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_BG, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_FONT, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.SUBTITLE_TEXT_FIELD_FONT_SIZE, true);

                ClearPageContents();
                populateSubtitlesReworkSettingsPage(initialPage);
            });
        }

        private void populateUKTDReworkSettingsPage(string initialPage)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Button("Go back", delegate
            {
                ClearPageContents();
                PopulatePage(initialPage);
            });

            _ = pageBuilder.Header1("Use key trigger description rework");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_PRESS_BUTTON_TRIGGER_DESCRIPTION_REWORK), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_PRESS_BUTTON_TRIGGER_DESCRIPTION_REWORK, value, true);
                if (value)
                {
                    PressActionKeyObjectManager.Instance.ShowThenHideDescription(ModCore.LoremIpsumText, 2f);
                }
            }, "Enable");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.PAK_DESCRIPTION_BG), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.PAK_DESCRIPTION_BG, value, true);
                PressActionKeyObjectManager.Instance.ShowThenHideDescription(ModCore.LoremIpsumText, 2f);
            }, "Enable background");

            _ = pageBuilder.Header3("Font");
            _ = pageBuilder.Dropdown(UISubtitleTextFieldRework.FontOptions, ModSettingsManager.GetIntValue(ModSettingsConstants.PAK_DESCRIPTION_FONT), delegate (int value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.PAK_DESCRIPTION_FONT, value, true);
                PressActionKeyObjectManager.Instance.ShowThenHideDescription(ModCore.LoremIpsumText, 2f);
            });
            _ = pageBuilder.Header4("Some languages might not be supported by certain fonts");
            _ = pageBuilder.Header3("Font size");
            _ = pageBuilder.Slider(8f, 13f, true, ModSettingsManager.GetIntValue(ModSettingsConstants.PAK_DESCRIPTION_FONT_SIZE), delegate (float value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.PAK_DESCRIPTION_FONT_SIZE, Mathf.RoundToInt(value), true);
                PressActionKeyObjectManager.Instance.ShowThenHideDescription(ModCore.LoremIpsumText, 2f);
            }, true, (float val) =>
            {
                return $"{Mathf.RoundToInt(val)}";
            });

            _ = pageBuilder.Button("Reset settings", delegate
            {
                ModSettingsManager.ResetValue(ModSettingsConstants.ENABLE_PRESS_BUTTON_TRIGGER_DESCRIPTION_REWORK, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.PAK_DESCRIPTION_BG, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.PAK_DESCRIPTION_FONT, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.PAK_DESCRIPTION_FONT_SIZE, true);

                ClearPageContents();
                populateUKTDReworkSettingsPage(initialPage);
            });
        }

        private void populateSSAOSettingsPage(string initialPage)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Button("Go back", delegate
            {
                ClearPageContents();
                PopulatePage(initialPage);
            });

            _ = pageBuilder.Header1("Ambient occlusion settings");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_SSAO), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_SSAO, value, true);
            }, "Enable");

            _ = pageBuilder.Header3("Intensity");
            _ = pageBuilder.Slider(0.4f, 1.2f, false, ModSettingsManager.GetFloatValue(ModSettingsConstants.SSAO_INTENSITY), delegate (float value)
            {
                ModSettingsManager.SetFloatValue(ModSettingsConstants.SSAO_INTENSITY, value, true);
            });

            _ = pageBuilder.Header3("Sample count");
            _ = pageBuilder.Slider(0f, 3f, true, ModSettingsManager.GetIntValue(ModSettingsConstants.SSAO_SAMPLE_COUNT), delegate (float value)
            {
                ModSettingsManager.SetIntValue(ModSettingsConstants.SSAO_SAMPLE_COUNT, Mathf.RoundToInt(value), true);
            }, true, (float val) =>
            {
                SampleCountLevel sampleCountLevel = (SampleCountLevel)Mathf.RoundToInt(val);
                switch (sampleCountLevel)
                {
                    case SampleCountLevel.Low:
                        return "Low";
                    case SampleCountLevel.Medium:
                        return "Medium";
                    case SampleCountLevel.High:
                        return "High";
                    case SampleCountLevel.VeryHigh:
                        return "Very high";
                }
                return sampleCountLevel.ToString();
            });

            _ = pageBuilder.Button("Reset settings", delegate
            {
                ModSettingsManager.ResetValue(ModSettingsConstants.ENABLE_SSAO, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.SSAO_INTENSITY, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.SSAO_SAMPLE_COUNT, true);

                ClearPageContents();
                populateSSAOSettingsPage(initialPage);
            });
        }

        private void populateCASettingsPage(string initialPage)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Button("Go back", delegate
            {
                ClearPageContents();
                PopulatePage(initialPage);
            });

            _ = pageBuilder.Header1("Chromatic aberration settings");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_CHROMATIC_ABERRATION), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_CHROMATIC_ABERRATION, value, true);
            }, "Enable");

            _ = pageBuilder.Header3("Intensity");
            _ = pageBuilder.Slider(0.1f, 1f, false, ModSettingsManager.GetFloatValue(ModSettingsConstants.CHROMATIC_ABERRATION_INTENSITY), delegate (float value)
            {
                ModSettingsManager.SetFloatValue(ModSettingsConstants.CHROMATIC_ABERRATION_INTENSITY, value, true);
            });

            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.CHROMATIC_ABERRATION_ON_SCREEN_EDGES), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.CHROMATIC_ABERRATION_ON_SCREEN_EDGES, value, true);
            }, "On screen edges");

            _ = pageBuilder.Button("Reset settings", delegate
            {
                ModSettingsManager.ResetValue(ModSettingsConstants.ENABLE_CHROMATIC_ABERRATION, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.CHROMATIC_ABERRATION_INTENSITY, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.CHROMATIC_ABERRATION_ON_SCREEN_EDGES, true);

                ClearPageContents();
                populateCASettingsPage(initialPage);
            });
        }

        private void populateBloomSettingsPage(string initialPage)
        {
            PageBuilder pageBuilder = new PageBuilder(this);
            _ = pageBuilder.Button("Go back", delegate
            {
                ClearPageContents();
                PopulatePage(initialPage);
            });

            _ = pageBuilder.Header1("Bloom settings");
            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.ENABLE_BLOOM), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.ENABLE_BLOOM, value, true);
            }, "Enable");

            _ = pageBuilder.Toggle(ModSettingsManager.GetBoolValue(ModSettingsConstants.TWEAK_BLOOM), delegate (bool value)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.TWEAK_BLOOM, value, true);
            }, "Adjust bloom settings");

            _ = pageBuilder.Button("Reset settings", delegate
            {
                ModSettingsManager.ResetValue(ModSettingsConstants.ENABLE_BLOOM, true);
                ModSettingsManager.ResetValue(ModSettingsConstants.TWEAK_BLOOM, true);

                ClearPageContents();
                populateBloomSettingsPage(initialPage);
            });
        }

        private int getCurrentLanguageIndex()
        {
            SettingsManager settingsManager = SettingsManager.Instance;
            if (settingsManager && settingsManager._data != null)
            {
                string langId = settingsManager.GetCurrentLanguageID();

                int index = 0;
                foreach (LanguageDefinition lang in LocalizationManager.Instance.SupportedLanguages)
                {
                    if (lang.LanguageCode == langId)
                        return index;

                    index++;
                }
            }
            return -1;
        }

        public void OnLegacyUIButtonClicked()
        {
            TitleScreenUI titleScreenUI = ModCache.titleScreenUI;
            if (titleScreenUI && GameModeManager.IsOnTitleScreen())
            {
                Hide();
                titleScreenUI.OnOptionsButtonClicked();
                return;
            }

            SettingsMenu settingsMenu = ModCache.gameUIRoot.SettingsMenu;
            if (settingsMenu)
            {
                Hide();

                ModActionUtils.DoInFrame(delegate
                {
                    settingsMenu.Show();
                });
            }
        }

        public void OnQualityDropdownChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.QualityDropDown.value = value;
            }
        }

        public void OnAntiAliasingDropdownChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.AntiAliasingDropdown.value = value;
            }
        }

        public void OnLanguageDropdownChanged(int value)
        {
            LocalizationManager localizationManager = LocalizationManager.Instance;
            if (localizationManager)
            {
                localizationManager.SetCurrentLanguage(localizationManager.SupportedLanguages[value].LanguageCode);

                m_tabs.ReinstantiatePreconfiguredTabs();
                m_tabs.SelectTab("Home");
            }
        }

        public void OnScreenResolutionChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.ScreenResolutionDropDown.value = value;
            }
        }

        public void OnFullScreenChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.FullScreenToggle.isOn = value;
            }
        }

        public void OnVSyncChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.VsyncOnToggle.isOn = value;
            }
        }

        public void OnRegionChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.RegionDropdown.value = value;
            }
        }

        public void OnCharacterModelChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.MultiplayerCharacterModelDropdown.value = value;
            }
            m_hasMultiplayerCustomizationChanges = true;
        }

        public void OnMultiplayerFavoriteColorDropdownChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.MultiplayerFavoriteColorDropdown.value = value;
            }
            m_hasMultiplayerCustomizationChanges = true;
        }

        public void OnStoryDifficultyIndexChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.StoryModeDifficultyDropDown.value = value;
            }
        }

        public void OnWorkshopEndlessLevelPolicyIndexChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.WorkshopLevelPolicyDropdown.value = value;
            }
        }

        public void OnDevIsLiveToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.DevIsLiveEnabledToggle.isOn = value;
            }
        }

        public void OnMuteEmotesToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.MuteEmotesToggle.isOn = value;
            }
        }

        public void OnHideGameUIToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.HideGameUIToggle.isOn = !value;
            }
        }

        public void OnGarbageSettingsChanged(int value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.GarbageSettingsDropdown.value = value;
            }
        }

        public void OnPlayerPushesGarbageToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.PlayerPushesGarbageToggle.isOn = value;
            }
        }

        public void OnSubtitlesToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.SubtitlesToggle.isOn = value;
            }
        }

        public void OnGlobalVolumeChanged(float value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.SoundVolume.value = value;
            }
        }

        public void OnMusicVolumeChanged(float value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.MusicVolume.value = value;
            }
        }

        public void OnCommentatorVolumeChanged(float value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.CommentatorsVolume.value = value;
            }
        }

        public void OnMouseSensitivityChanged(float value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.MouseSensitivitySlider.value = value;
            }
        }

        public void OnInvertMouseToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.InvertMouseToggle.isOn = value;
            }
        }

        public void OnControllerSensitivityChanged(float value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.ControllerSensitivitySlider.value = value;
            }
        }

        public void OnInvertControllerToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.InvertControllerToggle.isOn = value;
            }
        }

        public void OnEqualLookRatioToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.EqualLookRatioToggle.isOn = value;
            }
        }

        public void OnUseSkinInSinglePlayerToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.UseSkinInSinglePlayer.isOn = value;
            }
        }

        public void OnRelayToggleChanged(bool value)
        {
            SettingsMenu settingsMenu = ModCache.settingsMenu;
            if (settingsMenu)
            {
                settingsMenu.RelayToggle.isOn = value;
            }
        }

        private static void onSignedOut(JsonObject jsonObject)
        {
            ModBotUIRoot.Instance.ModBotSignInUI.SetSession("");
            VersionLabelManager.Instance.SetLine(2, "Not signed in");

            UISettingsMenuRework settingsMenuRework = ModUIManager.Instance.Get<UISettingsMenuRework>(AssetBundleConstants.UI, ModUIConstants.UI_SETTINGS_MENU);
            if (settingsMenuRework)
                settingsMenuRework.PopulatePage("Mod-Bot");
        }

        public class PageBuilder
        {
            public UISettingsMenuRework SettingsMenu;

            public PageBuilder(UISettingsMenuRework settingsMenu)
            {
                SettingsMenu = settingsMenu;
            }

            private void addLocalizedTextField(Text text, string localizationId)
            {
                if (localizationId == null)
                    localizationId = string.Empty;

                if (!localizationId.IsNullOrEmpty())
                {
                    LocalizedTextField localizedTextField = text.gameObject.AddComponent<LocalizedTextField>();
                    localizedTextField.LocalizationID = localizationId;
                }
            }

            private Text instantiateHeader(string text, string localizationId, ModdedObject prefab, Transform parentOverride = null)
            {
                ModdedObject moddedObject = Instantiate(prefab, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                moddedObject.gameObject.SetActive(true);
                Text textComponent = moddedObject.GetObject<Text>(0);
                textComponent.text = text;
                addLocalizedTextField(textComponent, localizationId);
                return textComponent;
            }

            private Dropdown instantiateDropdown(List<Dropdown.OptionData> list, int value, UnityAction<int> callback, Dropdown prefab, Transform parentOverride = null)
            {
                if (callback == null)
                    callback = delegate { ModUIUtils.MessagePopupNotImplemented(); };

                if (list == null)
                    list = new List<Dropdown.OptionData>();

                Dropdown dropdown = Instantiate(prefab, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                dropdown.gameObject.SetActive(true);
                dropdown.options = list;
                dropdown.value = value;
                if (callback != null)
                    dropdown.onValueChanged.AddListener(callback);
                return dropdown;
            }

            public Text Header1(string text, bool localize = true, Transform parentOverride = null)
            {
                return instantiateHeader(text, localize ? $"settings_header_{text.ToLower().Replace(' ', '_')}" : null, SettingsMenu.Header1Prefab, parentOverride);
            }

            public Text Header2(string text, bool localize = true, Transform parentOverride = null)
            {
                return instantiateHeader(text, localize ? $"settings_subheader_{text.ToLower().Replace(' ', '_')}" : null, SettingsMenu.Header2Prefab, parentOverride);
            }

            public Text Header3(string text, bool localize = true, Transform parentOverride = null)
            {
                return instantiateHeader(text, localize ? $"settings_subheader_{text.ToLower().Replace(' ', '_')}" : null, SettingsMenu.Header3Prefab, parentOverride);
            }

            public Text Header4(string text, bool localize = true, Transform parentOverride = null)
            {
                return instantiateHeader(text, localize ? $"settings_tooltip_{text.ToLower().Replace(' ', '_')}" : null, SettingsMenu.Header4Prefab, parentOverride);
            }

            public Dropdown Dropdown(List<Dropdown.OptionData> list, int value, UnityAction<int> callback, Transform parentOverride = null)
            {
                return instantiateDropdown(list, value, callback, SettingsMenu.DropdownPrefab, parentOverride);
            }

            public Dropdown DropdownWithImage(List<Dropdown.OptionData> list, int value, UnityAction<int> callback, Transform parentOverride = null)
            {
                return instantiateDropdown(list, value, callback, SettingsMenu.DropdownWithImagePrefab, parentOverride);
            }

            public Dropdown DropdownWithImage169(List<Dropdown.OptionData> list, int value, UnityAction<int> callback, Transform parentOverride = null)
            {
                return instantiateDropdown(list, value, callback, SettingsMenu.DropdownWithImage169Prefab, parentOverride);
            }

            public Slider Slider(float min, float max, bool wholeNumbers, float value, UnityAction<float> callback, bool noBetterSlider = false, Func<float, string> fillTextFunc = null, Transform parentOverride = null)
            {
                Slider slider = Instantiate(SettingsMenu.SliderPrefab, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                slider.gameObject.SetActive(true);
                slider.minValue = min;
                slider.maxValue = max;
                slider.wholeNumbers = wholeNumbers;
                slider.value = value;
                if (callback != null)
                    slider.onValueChanged.AddListener(callback);

                if (!noBetterSlider)
                    _ = slider.gameObject.AddComponent<BetterSliderCallback>();

                if (fillTextFunc == null)
                    fillTextFunc = GetFillText;

                ModdedObject moddedObject = slider.GetComponent<ModdedObject>();

                SliderFillText sliderFillText = slider.gameObject.AddComponent<SliderFillText>();
                sliderFillText.SliderComponent = slider;
                sliderFillText.Label = moddedObject.GetObject<Text>(0);
                sliderFillText.Function = fillTextFunc;

                return slider;
            }

            public Toggle Toggle(bool isOn, UnityAction<bool> callback, string text, bool localize = true, Transform parentOverride = null)
            {
                if (callback == null)
                    callback = delegate { ModUIUtils.MessagePopupNotImplemented(); };

                ModdedObject moddedObject = Instantiate(SettingsMenu.TogglePrefab, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                moddedObject.gameObject.SetActive(true);
                Text textComponent = moddedObject.GetObject<Text>(1);
                textComponent.text = text;
                if (localize)
                    addLocalizedTextField(textComponent, $"settings_checkbox_{text.ToLower().Replace(' ', '_')}");
                Toggle toggle = moddedObject.GetObject<Toggle>(0);
                toggle.isOn = isOn;
                if (callback != null)
                    toggle.onValueChanged.AddListener(callback);
                return toggle;
            }

            public Toggle ToggleWithOptions(bool isOn, UnityAction<bool> callback, string text, Action populatePageAction, bool localize = true, Transform parentOverride = null)
            {
                if (callback == null)
                    callback = delegate { ModUIUtils.MessagePopupNotImplemented(); };

                ModdedObject moddedObject = Instantiate(SettingsMenu.ToggleWithOptionsPrefab, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                moddedObject.gameObject.SetActive(true);
                Text textComponent = moddedObject.GetObject<Text>(2);
                textComponent.text = text;
                if (localize)
                    addLocalizedTextField(textComponent, $"settings_checkbox_{text.ToLower().Replace(' ', '_')}");
                Toggle toggle = moddedObject.GetObject<Toggle>(0);
                toggle.isOn = isOn;
                if (callback != null)
                    toggle.onValueChanged.AddListener(callback);

                Button button = moddedObject.GetObject<Button>(1);
                button.onClick.AddListener(delegate
                {
                    if (populatePageAction != null)
                    {
                        SettingsMenu.ClearPageContents();
                        populatePageAction();
                    }
                });

                return toggle;
            }

            public Button Button(string text, Action onClicked, bool localize = true, Transform parentOverride = null)
            {
                if (onClicked == null)
                    onClicked = ModUIUtils.MessagePopupNotImplemented;

                ModdedObject moddedObject = Instantiate(SettingsMenu.ButtonPrefab, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                moddedObject.gameObject.SetActive(true);
                moddedObject.GetObject<Text>(0).text = localize ? LocalizationManager.Instance.GetTranslatedString($"settings_button_{text.ToLower().Replace(' ', '_')}") : text;
                Button button = moddedObject.GetComponent<Button>();
                button.onClick.AddListener(new UnityAction(onClicked));
                return button;
            }

            public UIElementKeyBindSetter KeyBind(string name, KeyCode keyCode, KeyCode defaultKey, Action<KeyCode> onChanged, Transform parentOverride = null)
            {
                ModdedObject moddedObject = Instantiate(SettingsMenu.KeyBindPrefab, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                moddedObject.gameObject.SetActive(true);

                UIElementKeyBindSetter elementKeyBind = moddedObject.gameObject.AddComponent<UIElementKeyBindSetter>();
                elementKeyBind.InitializeElement();
                elementKeyBind.key = keyCode;
                elementKeyBind.defaultKey = defaultKey;
                elementKeyBind.SetDescription(LocalizationManager.Instance.GetTranslatedString($"settings_subheader_{name.ToLower().Replace(' ', '_')}"));
                elementKeyBind.onValueChanged.AddListener(new UnityAction<KeyCode>(onChanged));

                return elementKeyBind;
            }

            public RectTransform GridContainer(Vector2 size, Vector2 cellSize, Vector2 spacing, Transform parentOverride = null)
            {
                GameObject gridContainerObject = Instantiate(SettingsMenu.GridContainer, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                gridContainerObject.SetActive(true);

                RectTransform rectTransform = gridContainerObject.GetComponent<RectTransform>();
                Vector2 sizeDelta = rectTransform.sizeDelta;
                sizeDelta.y = size.y;
                rectTransform.sizeDelta = sizeDelta;

                LayoutElement layoutElement = gridContainerObject.GetComponent<LayoutElement>();
                layoutElement.minWidth = size.x;

                GridLayoutGroup gridLayoutGroup = gridContainerObject.GetComponent<GridLayoutGroup>();
                gridLayoutGroup.cellSize = cellSize;
                gridLayoutGroup.spacing = spacing;

                return rectTransform;
            }

            public void LanguageButton(string langCode, Transform parentOverride = null)
            {
                ModdedObject moddedObject = Instantiate(SettingsMenu.LanguageButton, parentOverride ? parentOverride : SettingsMenu.PageContentsTransform);
                moddedObject.gameObject.SetActive(true);

                Text header = moddedObject.GetObject<Text>(0);
                Text subHeader = moddedObject.GetObject<Text>(1);

                string headerText;
                string subHeaderText;

                switch (langCode)
                {
                    case "en":
                        headerText = "English";
                        subHeaderText = "(English) [en]";
                        break;
                    case "fr":
                        headerText = "Français - la France";
                        subHeaderText = "(French - France) [fr]";
                        break;
                    case "it":
                        headerText = "Italiano";
                        subHeaderText = "(Italian) [it]";
                        break;
                    case "de":
                        headerText = "Deutsch";
                        subHeaderText = "(German) [de]";
                        break;
                    case "es-ES":
                        headerText = "Español - España";
                        subHeaderText = "(Spanish - Spain) [es-ES]";
                        break;
                    case "es-419":
                        headerText = "Español - Latinoamérica";
                        subHeaderText = "(Spanish - Latin America) [es-419]";
                        break;
                    case "zh-CN":
                        headerText = "简体中文";
                        subHeaderText = "(Simplified Chinese) [zh-CN]";
                        break;
                    case "zh-TW":
                        headerText = "繁體中文";
                        subHeaderText = "(Traditional Chinese) [zh-TW]";
                        break;
                    case "ru":
                        headerText = "Pусский";
                        subHeaderText = "(Russian) [ru]";
                        break;
                    case "pt-BR":
                        headerText = "Português do Brasil";
                        subHeaderText = "(Brazilian Portuguese) [pt-BR]";
                        break;
                    case "ja":
                        headerText = "日本語";
                        subHeaderText = "(Japanese) [ja]";
                        break;
                    case "ko":
                        headerText = "한국어";
                        subHeaderText = "(Korean) [ko]";
                        break;
                    default:
                        headerText = langCode;
                        subHeaderText = langCode;
                        break;
                }

                header.text = headerText;
                subHeader.text = subHeaderText;

                UnityEngine.UI.Button button = moddedObject.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    LocalizationManager.Instance.SetCurrentLanguage(langCode);
                    SettingsMenu.m_tabs.ReinstantiatePreconfiguredTabs();
                    SettingsMenu.m_tabs.SelectTab("Languages");
                });
            }

            public void Dispose()
            {
                SettingsMenu = null;
            }

            public static string GetFillText(float value)
            {
                return $"{Mathf.Round(value * 100f)}%";
            }
        }
    }
}
