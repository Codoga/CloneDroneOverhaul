using OverhaulMod.Combat;
using OverhaulMod.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

namespace OverhaulMod.Engine
{
    public class ModLocalizationManager : Singleton<ModLocalizationManager>, IGameLoadListener
    {
        public const string FILE_NAME = "localization.json";
        public const string LANGUAGE_OPTIONS_CACHE_KEY = "LangOptions_";

        private ModLocalizationInfo m_localizationInfo;

        private void Start()
        {
            LoadInfo();
        }

        public void OnGameLoaded()
        {
            LoadInfo();
        }

        public void LoadInfo()
        {
            ModLocalizationInfo info = m_localizationInfo;
            if (info != null)
                return;

            try
            {
                info = ModJsonUtils.DeserializeStream<ModLocalizationInfo>(Path.Combine(ModCore.dataFolder, FILE_NAME));
                info.FixValues();
            }
            catch
            {
                info = new ModLocalizationInfo();
                info.FixValues();
            }

            m_localizationInfo = info;
            string currentLangId;
            try
            {
                currentLangId = SettingsManager.Instance.GetCurrentLanguageID();
            }
            catch
            {
                currentLangId = "en";
            }

            PopulateTranslationDictionary(ref LocalizationManager.Instance._translatedStringsDictionary, currentLangId);
        }

        public void SaveInfo()
        {
            ModLocalizationInfo info = m_localizationInfo;
            if (info != null)
            {
                ModJsonUtils.WriteStream(Path.Combine(ModCore.dataFolder, FILE_NAME), info);
            }
        }

        public void AddTranslation(string key)
        {
            ModLocalizationInfo info = m_localizationInfo;
            if (info != null)
            {
                info.AddTranslation(key);
            }
        }

        public void ChangeTranslation(string oldName, string newName)
        {
            ModLocalizationInfo info = m_localizationInfo;
            if (info != null)
            {
                info.ChangeTranslation(oldName, newName);
            }
        }

        public void DeleteTranslation(string key)
        {
            ModLocalizationInfo info = m_localizationInfo;
            if (info != null)
            {
                info.DeleteTranslation(key);
            }
        }

        public string GetTranslation(string langId, string key)
        {
            ModLocalizationInfo info = m_localizationInfo;
            if (info != null)
            {
                Dictionary<string, string> d = info.GetDictionary(langId);
                if (d != null && d.ContainsKey(key))
                {
                    return d[key];
                }
            }
            return null;
        }

        public void SetTranslation(string langId, string key, string value)
        {
            ModLocalizationInfo info = m_localizationInfo;
            if (info != null)
            {
                Dictionary<string, string> d = info.GetDictionary(langId);
                if (d != null && d.ContainsKey(key))
                {
                    d[key] = value;
                }
            }
        }

        public List<Dropdown.OptionData> GetLanguageOptions(bool returnLangCode)
        {
            string postfix = returnLangCode ? "codes" : LocalizationManager.Instance.GetCurrentLanguageCode();
            return ModAdvancedCache.GetOrCreate($"{LANGUAGE_OPTIONS_CACHE_KEY}{postfix}", () =>
            {
                List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
                foreach (LanguageDefinition lang in LocalizationManager.Instance.SupportedLanguages)
                {
                    string displayName = returnLangCode ? lang.LanguageCode : LocalizationManager.Instance.GetTranslatedString(lang.LanguageNameLocID);
                    list.Add(new Dropdown.OptionData(displayName, lang.FlagImage));
                }
                return list;
            });
        }

        public bool CanLanguageBeTranslated(string langId)
        {
            switch (langId)
            {
                case "en":
                    return true;
                case "ru":
                    return true;
            }
            return false;
        }

        public Dictionary<string, string> GetTranslationDictionary(string langId)
        {
            ModLocalizationInfo modLocalizationInfo = m_localizationInfo;
            return modLocalizationInfo != null ? modLocalizationInfo.GetDictionary(langId) : null;
        }

        public void PopulateTranslationDictionary(ref Dictionary<string, string> keyValuePairs, string langId)
        {
            ModUpgradesManager.Instance.DeleteLocalizationKeysOfUpgrades(keyValuePairs); // fixes a bug which corrupts the name of upgrades after switching the language 
            ModLocalizationInfo modLocalizationInfo = m_localizationInfo;
            if (modLocalizationInfo != null)
            {
                Dictionary<string, string> modTranslations = langId == "ru" ? modLocalizationInfo.GetDictionary("ru") : modLocalizationInfo.GetDictionary("en");
                if (modTranslations != null && modTranslations.Count != 0)
                    foreach (KeyValuePair<string, string> translationKeyValue in modTranslations)
                    {
                        if (!keyValuePairs.ContainsKey(translationKeyValue.Key))
                            keyValuePairs.Add(translationKeyValue.Key, !translationKeyValue.Value.IsNullOrEmpty() ? translationKeyValue.Value : translationKeyValue.Key);
                    }
            }
        }
    }
}
