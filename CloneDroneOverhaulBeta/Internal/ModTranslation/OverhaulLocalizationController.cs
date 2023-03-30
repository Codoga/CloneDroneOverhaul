﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CDOverhaul.HUD;
using UnityEngine.UI;
using System.Collections;

namespace CDOverhaul.Localization
{
    public static class OverhaulLocalizationController
    {
        private static bool? m_Error;
        public static bool Error => !m_Error.HasValue || m_Error.Value || m_Data == null;

        public const string LocalizationFileName = "Localization";
        private static OverhaulLocalizationData m_Data;
        public static OverhaulLocalizationData Localization
        {
            get
            {
                if (!m_Error.HasValue || m_Error.Value)
                {
                    return null;
                }
                return m_Data;
            }
        }

        private static readonly List<Text> m_ListOfTexts = new List<Text>();

        public static void Initialize()
        {
            OverhaulCanvasController controller = OverhaulController.GetController<OverhaulCanvasController>();
            m_ListOfTexts.Clear();
            m_ListOfTexts.AddRange(controller.GetAllComponentsWithModdedObjectRecursive<Text>("LID_", controller.HUDModdedObject.transform));

            if (OverhaulSessionController.GetKey<bool>("LoadedTranslations"))
            {
                return;
            }
            OverhaulSessionController.SetKey("LoadedTranslations", true);

            loadData();
        }
        
        private static async void loadData()
        {
            string path = OverhaulMod.Core.ModDirectory + "Assets/" + LocalizationFileName + ".json";
            if (!File.Exists(path))
            {
                File.Create(path);
                m_Error = false;
                m_Data = new OverhaulLocalizationData();
                m_Data.RepairFields();
                m_Data.SavedInVersion = OverhaulVersion.ModVersion;
                SaveData();
                return;
            }
            StreamReader reader = File.OpenText(path);

            Task<string> task = reader.ReadToEndAsync();
            await task;
            if(task.IsCanceled || task.IsFaulted)
            {
                m_Error = true;
                return;
            }

            m_Error = false;
            m_Data = JsonConvert.DeserializeObject<OverhaulLocalizationData>(task.Result);
            if(m_Data != null)
            {
                m_Data.RepairFields();
            }

            TryLocalizeHUD();
        }

        public static void SaveData()
        {
            if(m_Data != null && m_Error.HasValue && !m_Error.Value)
            {
                m_Data.SavedInVersion = OverhaulVersion.ModVersion;
                File.WriteAllText(OverhaulMod.Core.ModDirectory + "Assets/" + OverhaulLocalizationController.LocalizationFileName + ".json",
                 Newtonsoft.Json.JsonConvert.SerializeObject(m_Data, Newtonsoft.Json.Formatting.None, DataRepository.CreateSettings()));
            }
        }

        public static void TryLocalizeHUD()
        {
            StaticCoroutineRunner.StartStaticCoroutine(localizeHUDCoroutine());
        }

        private static IEnumerator localizeHUDCoroutine()
        {
            int iteration = 0;
            foreach(Text text in m_ListOfTexts)
            {
                if(text != null)
                {
                    if(iteration % 10 == 0)
                    {
                        yield return null;
                    }

                    m_Data.GetTranslation(text.GetComponent<ModdedObject>(), true);
                }
                iteration++;
            }
            yield break;
        }
    }
}
