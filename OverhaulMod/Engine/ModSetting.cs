using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OverhaulMod.Engine
{
    public class ModSetting
    {
        public string name
        {
            get;
            set;
        }

        public object defaultValue
        {
            get;
            set;
        }

        public Tag tag
        {
            get;
            set;
        }

        public ValueType valueType
        {
            get;
            set;
        }

        public FieldInfo fieldInfo
        {
            get;
            set;
        }

        public bool requireRestarting
        {
            get;
            set;
        }

        private List<Action<object>> m_valueChangedListeners;
        public event Action<object> valueChangedEvent
        {
            add
            {
                if (m_valueChangedListeners == null)
                    m_valueChangedListeners = new List<Action<object>>();

                m_valueChangedListeners.Add(value);
            }
            remove
            {
                if (m_valueChangedListeners == null)
                    m_valueChangedListeners = new List<Action<object>>();

                _ = m_valueChangedListeners.Remove(value);
            }
        }

        public bool ShouldNotNotifyPlayerAboutRestart;

        public string GetPlayerPrefKey()
        {
            return "OverhaulMod." + name;
        }

        public object GetValue()
        {
            ModSettingsDataManager modSettingsDataManager = ModSettingsDataManager.Instance;

            string key = GetPlayerPrefKey();
            object result;
            switch (valueType)
            {
                case ValueType.Bool:
                    result = modSettingsDataManager.GetInt(key, (bool)defaultValue ? 1 : 0) == 1;
                    break;
                case ValueType.Int:
                    result = modSettingsDataManager.GetInt(key, (int)defaultValue);
                    break;
                case ValueType.Float:
                    result = modSettingsDataManager.GetFloat(key, (float)defaultValue);
                    break;
                case ValueType.String:
                    result = modSettingsDataManager.GetString(key, (string)defaultValue);
                    break;
                default:
                    result = default;
                    break;
            }
            return result;
        }

        public object GetFieldValue()
        {
            FieldInfo fieldInfo = this.fieldInfo;
            return fieldInfo == null ? null : fieldInfo.GetValue(null);
        }

        public void SetValue(object value)
        {
            ModSettingsDataManager modSettingsDataManager = ModSettingsDataManager.Instance;

            FieldInfo fieldInfo = this.fieldInfo;
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(null, value);
            }

            string key = GetPlayerPrefKey();
            switch (valueType)
            {
                case ValueType.Bool:
                    modSettingsDataManager.SetInt(key, (bool)value ? 1 : 0);
                    break;
                case ValueType.Int:
                    modSettingsDataManager.SetInt(key, (int)value);
                    break;
                case ValueType.Float:
                    modSettingsDataManager.SetFloat(key, (float)value);
                    break;
                case ValueType.String:
                    modSettingsDataManager.SetString(key, (string)value);
                    break;
            }

            if (!m_valueChangedListeners.IsNullOrEmpty())
            {
                foreach (Action<object> a in m_valueChangedListeners)
                {
                    try
                    {
                        a?.Invoke(value);
                    }
                    catch { }
                }
            }

            GlobalEventManager.Instance.Dispatch(ModSettingsManager.SETTING_CHANGED_EVENT);
        }

        public void SetValueFromUI(object value)
        {
            SetValue(value);
            if (requireRestarting && !ShouldNotNotifyPlayerAboutRestart)
            {
                _ = ModUIConstants.ShowRestartRequiredScreen(true);
                ShouldNotNotifyPlayerAboutRestart = true;
            }
        }

        public void SetBoolValue(bool value)
        {
            if (valueType == ValueType.Bool)
                SetValue(value);
        }

        public void SetIntValue(int value)
        {
            if (valueType == ValueType.Int)
                SetValue(value);
        }

        public void SetFloatValue(float value)
        {
            if (valueType == ValueType.Float)
                SetValue(value);
        }

        public void SetStringValue(int value)
        {
            if (valueType == ValueType.String)
                SetValue(value);
        }

        public enum Tag
        {
            None,
            UISetting
        }

        public enum ValueType
        {
            None,
            Bool,
            Int,
            Float,
            String
        }
    }
}
