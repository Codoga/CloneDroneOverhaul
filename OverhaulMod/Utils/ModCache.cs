﻿using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OverhaulMod.Utils
{
    public static class ModCache
    {
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
                    s_modAssemblyName = modAssembly.GetName();
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
                    s_jsonSerializerSettings = (JsonSerializerSettings)dataRepository.GetSettings().MemberwiseClone();
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
                    s_jsonSerializerSettingsFormatted = (JsonSerializerSettings)dataRepository.GetSettings().MemberwiseClone();
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

        private static TitleScreenUI s_titleScreenUI;
        public static TitleScreenUI titleScreenUI
        {
            get
            {
                if (!s_titleScreenUI)
                {
                    s_titleScreenUI = gameUIRoot.TitleScreenUI;
                }
                return s_titleScreenUI;
            }
        }

        private static GameObject s_titleScreenRootButtonsBG;
        public static GameObject titleScreenRootButtonsBG
        {
            get
            {
                if (!s_titleScreenRootButtonsBG)
                {
                    s_titleScreenRootButtonsBG = titleScreenUI.RootButtonsContainerBG;
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

        public static GameUIThemeData gameUIThemeData { get; set; }
    }
}