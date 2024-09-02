﻿using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class ModSettingsManager : Singleton<ModSettingsManager>
    {
        public const string SETTING_CHANGED_EVENT = "OverhaulSettingChanged";

        public static int ExtraResolutionLength;

        private List<ModSetting> m_settings;
        private Dictionary<string, ModSetting> m_nameToSetting;
        private Dictionary<string, ModSettingSubDescription> m_idToSubDescription;

        public override void Awake()
        {
            base.Awake();

            m_settings = new List<ModSetting>();
            m_nameToSetting = new Dictionary<string, ModSetting>();
            m_idToSubDescription = new Dictionary<string, ModSettingSubDescription>();

            loadSettings();

            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.SettingDescriptionBox))
                loadSubDescriptions();
        }

        private void loadSubDescriptions()
        {
            m_idToSubDescription.Clear();

            string fn = Path.Combine(ModCore.dataFolder, "settingSubDescriptions.txt");
            if (File.Exists(fn))
            {
                string content;
                try
                {
                    content = ModFileUtils.ReadText(fn);
                }
                catch
                {
                    return;
                }

                string[] idWithValues = content.Split(Environment.NewLine.ToCharArray());
                foreach (string entry in idWithValues)
                {
                    string[] split = entry.Split(' ');
                    if (split.Length == 2)
                    {
                        string settingId = split[0];
                        string sub = split[1];

                        if (!sub.IsNullOrEmpty())
                        {
                            string[] subSplit = sub.Split(',');
                            if (subSplit.Length == 2)
                            {
                                string typeText = subSplit[0];
                                string valueText = subSplit[1];

                                if (int.TryParse(typeText, out int type) && int.TryParse(valueText, out int value))
                                {
                                    m_idToSubDescription.Add(settingId, new ModSettingSubDescription(type, value));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void loadSettings()
        {
            foreach (System.Type type in ModCache.modAssembly.GetTypes())
            {
                foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    ModSetting modSetting = CreateSettingFromField(fieldInfo);
                    if (modSetting == null)
                        continue;

                    m_settings.Add(modSetting);
                    m_nameToSetting.Add(modSetting.name, modSetting);
                }
            }
        }

        public string GetSubDescription(string settingId)
        {
            if (m_idToSubDescription.ContainsKey(settingId))
            {
                ModSettingSubDescription modSettingSubDescription = m_idToSubDescription[settingId];
                if (modSettingSubDescription.Type == 0)
                {
                    string postfix;
                    switch (modSettingSubDescription.Value)
                    {
                        case 1:
                            postfix = "Low".AddColor(Color.green);
                            break;
                        case 2:
                            postfix = "Medium".AddColor(Color.yellow);
                            break;
                        case 3:
                            postfix = "High".AddColor(Color.yellow);
                            break;
                        case 4:
                            postfix = "Very high".AddColor(Color.red);
                            break;
                        default:
                            postfix = "N/A".AddColor(Color.white);
                            break;
                    }
                    return $"Performance impact: {postfix}";
                }
            }
            return null;
        }

        public bool HasSetting(string name)
        {
            return m_nameToSetting.ContainsKey(name);
        }

        public ModSetting GetSetting(string name)
        {
            return m_nameToSetting.TryGetValue(name, out ModSetting modSetting) ? modSetting : null;
        }

        public List<ModSetting> GetSettings()
        {
            return m_settings;
        }

        public List<ModSetting> GetSettings(ModSetting.Tag tag)
        {
            List<ModSetting> result = new List<ModSetting>();
            foreach (ModSetting modSetting in m_settings)
            {
                if (modSetting.tag == tag)
                    result.Add(modSetting);
            }
            return result;
        }

        public void ResetSettings()
        {
            foreach (ModSetting setting in m_settings)
                setting.SetValue(setting.defaultValue);

            ModSettingsDataManager.Instance.Save();
        }

        public void AddSettingValueChangedListener(Action<object> action, string settingId)
        {
            ModSetting setting = GetSetting(settingId);
            action.Invoke(setting.GetFieldValue());
            setting.valueChangedEvent += action;
        }

        public void RemoveSettingValueChangedListener(Action<object> action, string settingId)
        {
            ModSetting setting = GetSetting(settingId);
            setting.valueChangedEvent -= action;
        }

        public ModSetting CreateSettingFromField(FieldInfo field, bool setFieldValue = true)
        {
            if (field == null)
                return null;

            ModSettingAttribute modSettingAttribute = field.GetCustomAttribute<ModSettingAttribute>();
            if (modSettingAttribute == null || modSettingAttribute.Name.IsNullOrEmpty() || HasSetting(modSettingAttribute.Name))
                return null;

            ModSetting.ValueType valueType;
            if (field.FieldType == typeof(bool))
                valueType = ModSetting.ValueType.Bool;
            else if (field.FieldType == typeof(int) || field.FieldType.IsEnum)
                valueType = ModSetting.ValueType.Int;
            else if (field.FieldType == typeof(float))
                valueType = ModSetting.ValueType.Float;
            else if (field.FieldType == typeof(string))
                valueType = ModSetting.ValueType.String;
            else
                return null;

            ModSetting setting = new ModSetting
            {
                name = modSettingAttribute.Name,
                defaultValue = modSettingAttribute.DefaultValue,
                tag = modSettingAttribute.Tag,
                valueType = valueType,
                fieldInfo = field,
                requireRestarting = field.GetCustomAttribute<ModSettingRequireRestartAttribute>() != null
            };

            if (setFieldValue)
                field.SetValue(null, setting.GetValue());

            return setting;
        }

        public void SetSettingValueFromUI(string name, object value)
        {
            ModSetting modSetting = GetSetting(name);
            if (modSetting != null)
            {
                modSetting.SetValueFromUI(value);
            }
        }

        public static bool GetBoolValue(string name)
        {
            return (bool)Instance.GetSetting(name).GetFieldValue();
        }

        public static int GetIntValue(string name)
        {
            return (int)Instance.GetSetting(name).GetFieldValue();
        }

        public static float GetFloatValue(string name)
        {
            return (float)Instance.GetSetting(name).GetFieldValue();
        }

        public static string GetStringValue(string name)
        {
            return (string)Instance.GetSetting(name).GetFieldValue();
        }

        public static void ResetValue(string name, bool fromUi = false)
        {
            ModSetting setting = Instance.GetSetting(name);
            if (fromUi)
            {
                setting.SetValueFromUI(setting.defaultValue);
                return;
            }
            setting.SetValue(setting.defaultValue);
        }

        public static void SetBoolValue(string name, bool value, bool fromUi = false)
        {
            if (fromUi)
            {
                Instance.GetSetting(name).SetValueFromUI(value);
                return;
            }
            Instance.GetSetting(name).SetValue(value);
        }

        public static void SetIntValue(string name, int value, bool fromUi = false)
        {
            if (fromUi)
            {
                Instance.GetSetting(name).SetValueFromUI(value);
                return;
            }
            Instance.GetSetting(name).SetValue(value);
        }

        public static void SetFloatValue(string name, float value, bool fromUi = false)
        {
            if (fromUi)
            {
                Instance.GetSetting(name).SetValueFromUI(value);
                return;
            }
            Instance.GetSetting(name).SetValue(value);
        }

        public static void SetStringValue(string name, string value, bool fromUi = false)
        {
            if (fromUi)
            {
                Instance.GetSetting(name).SetValueFromUI(value);
                return;
            }
            Instance.GetSetting(name).SetValue(value);
        }
    }
}
