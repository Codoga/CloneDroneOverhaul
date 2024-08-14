using HarmonyLib;
using Newtonsoft.Json;
using OverhaulMod.Engine;
using System;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OverhaulMod.Utils
{
    public static class ModCache
    {
        private static string[] s_commandLineArgs;
        public static string[] commandLineArgs
        {
            get
            {
                if (s_commandLineArgs == null)
                {
                    s_commandLineArgs = Environment.GetCommandLineArgs();
                }
                return s_commandLineArgs;
            }
        }

        private static Assembly s_modAssembly;
        public static Assembly modAssembly
        {
            get
            {
                if (s_modAssembly == null)
                {
                    s_modAssembly = Assembly.GetExecutingAssembly();
                }
                return s_modAssembly;
            }
        }

        private static AssemblyName s_modAssemblyName;
        public static AssemblyName modAssemblyName
        {
            get
            {
                if (s_modAssemblyName == null)
                {
                    s_modAssemblyName = modAssembly?.GetName();
                }
                return s_modAssemblyName;
            }
        }

        private static DataRepository s_dataRepository;
        public static DataRepository dataRepository
        {
            get
            {
                if (!s_dataRepository)
                {
                    s_dataRepository = DataRepository.Instance;
                }
                return s_dataRepository;
            }
        }

        private static JsonSerializerSettings s_jsonSerializerSettings;
        public static JsonSerializerSettings jsonSerializerSettings
        {
            get
            {
                if (s_jsonSerializerSettings == null)
                {
                    s_jsonSerializerSettings = dataRepository.GetSettings().Clone();
                }
                return s_jsonSerializerSettings;
            }
        }

        private static JsonSerializerSettings s_jsonSerializerSettingsFormatted;
        public static JsonSerializerSettings jsonSerializerSettingsFormatted
        {
            get
            {
                if (s_jsonSerializerSettingsFormatted == null)
                {
                    s_jsonSerializerSettingsFormatted = dataRepository.GetSettings().Clone();
                    s_jsonSerializerSettingsFormatted.Formatting = Formatting.Indented;
                }
                return s_jsonSerializerSettingsFormatted;
            }
        }

        private static Encoding s_utf8Encoding;
        public static Encoding utf8Encoding
        {
            get
            {
                if (s_utf8Encoding == null)
                {
                    s_utf8Encoding = Encoding.UTF8;
                }
                return s_utf8Encoding;
            }
        }

        private static GameUIRoot s_gameUIRoot;
        public static GameUIRoot gameUIRoot
        {
            get
            {
                if (!s_gameUIRoot)
                {
                    s_gameUIRoot = GameUIRoot.Instance;
                }
                return s_gameUIRoot;
            }
        }

        private static Camera s_gameUIRootCamera;
        public static Camera gameUIRootCamera
        {
            get
            {
                if (!s_gameUIRootCamera)
                {
                    s_gameUIRootCamera = gameUIRoot.GetComponent<Canvas>().worldCamera;
                }
                return s_gameUIRootCamera;
            }
        }

        private static TitleScreenUI s_titleScreenUI;
        public static TitleScreenUI titleScreenUI
        {
            get
            {
                if (!s_titleScreenUI)
                {
                    s_titleScreenUI = gameUIRoot?.TitleScreenUI;
                }
                return s_titleScreenUI;
            }
        }

        private static SettingsMenu s_settingsMenu;
        public static SettingsMenu settingsMenu
        {
            get
            {
                if (!s_settingsMenu)
                {
                    s_settingsMenu = gameUIRoot?.SettingsMenu;
                }
                return s_settingsMenu;
            }
        }

        private static AttackManager s_attackManager;
        public static AttackManager attackManager
        {
            get
            {
                if (!s_attackManager)
                {
                    s_attackManager = AttackManager.Instance;
                }
                return s_attackManager;
            }
        }

        private static AudioManager s_audioManager;
        public static AudioManager audioManager
        {
            get
            {
                if (!s_audioManager)
                {
                    s_audioManager = AudioManager.Instance;
                }
                return s_audioManager;
            }
        }

        private static AudioLibrary s_audioLibrary;
        public static AudioLibrary audioLibrary
        {
            get
            {
                if (!s_audioLibrary)
                {
                    s_audioLibrary = AudioLibrary.Instance;
                }
                return s_audioLibrary;
            }
        }

        private static GlobalFireParticleSystem s_globalFireParticleSystem;
        public static GlobalFireParticleSystem globalFireParticleSystem
        {
            get
            {
                if (!s_globalFireParticleSystem)
                {
                    s_globalFireParticleSystem = GlobalFireParticleSystem.Instance;
                }
                return s_globalFireParticleSystem;
            }
        }

        private static FadingVoxelManager s_fadingVoxelManager;
        public static FadingVoxelManager fadingVoxelManager
        {
            get
            {
                if (!s_fadingVoxelManager)
                {
                    s_fadingVoxelManager = FadingVoxelManager.Instance;
                }
                return s_fadingVoxelManager;
            }
        }

        private static GameObject s_titleScreenRootButtonsBG;
        public static GameObject titleScreenRootButtonsBG
        {
            get
            {
                if (!s_titleScreenRootButtonsBG)
                {
                    s_titleScreenRootButtonsBG = titleScreenUI?.RootButtonsContainerBG;
                }
                return s_titleScreenRootButtonsBG;
            }
        }

        private static PhotoManager s_photoManager;
        public static PhotoManager photoManager
        {
            get
            {
                if (!s_photoManager)
                {
                    s_photoManager = PhotoManager.Instance;
                }
                return s_photoManager;
            }
        }

        private static MethodInfo s_unityTimeFixedUnscaledDeltaTimePropertyGetter;
        public static MethodInfo unityTimeFixedUnscaledDeltaTimePropertyGetter
        {
            get
            {
                if (s_unityTimeFixedUnscaledDeltaTimePropertyGetter == null)
                {
                    s_unityTimeFixedUnscaledDeltaTimePropertyGetter = AccessTools.DeclaredPropertyGetter(typeof(Time), nameof(Time.fixedUnscaledDeltaTime));
                }
                return s_unityTimeFixedUnscaledDeltaTimePropertyGetter;
            }
        }

        private static MethodInfo s_unityTimeUnscaledDeltaTimePropertyGetter;
        public static MethodInfo unityTimeUnscaledDeltaTimePropertyGetter
        {
            get
            {
                if (s_unityTimeUnscaledDeltaTimePropertyGetter == null)
                {
                    s_unityTimeUnscaledDeltaTimePropertyGetter = AccessTools.DeclaredPropertyGetter(typeof(Time), nameof(Time.unscaledDeltaTime));
                }
                return s_unityTimeUnscaledDeltaTimePropertyGetter;
            }
        }

        private static MethodInfo s_unityInputGetMouseButtonMethod;
        public static MethodInfo unityInputGetMouseButtonMethod
        {
            get
            {
                if (s_unityInputGetMouseButtonMethod == null)
                {
                    s_unityInputGetMouseButtonMethod = AccessTools.DeclaredMethod(typeof(Input), nameof(Input.GetMouseButton));
                }
                return s_unityInputGetMouseButtonMethod;
            }
        }

        private static MethodInfo s_unityInputGetMouseButtonDownMethod;
        public static MethodInfo unityInputGetMouseButtonDownMethod
        {
            get
            {
                if (s_unityInputGetMouseButtonDownMethod == null)
                {
                    s_unityInputGetMouseButtonDownMethod = AccessTools.DeclaredMethod(typeof(Input), nameof(Input.GetMouseButtonDown));
                }
                return s_unityInputGetMouseButtonDownMethod;
            }
        }

        public static GameUIThemeData gameUIThemeData { get; set; }
    }
}
