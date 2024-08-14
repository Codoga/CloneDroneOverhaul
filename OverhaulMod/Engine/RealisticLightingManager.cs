using OverhaulMod.Content;
using OverhaulMod.Utils;
using System.Collections;
using System.IO;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class RealisticLightingManager : Singleton<RealisticLightingManager>
    {
        public const string OLD_LIGHTING_INFO_LIST_FILE = "lightningInfo.json";

        public const string LIGHTING_INFO_LIST_FILE = "lightingInfo.json";

        public const string OVERHAUL_ASSETS_SKYBOXES = "overhaulassets_skyboxes";

        public static readonly string LightSettingsObjectResourcePath = "Prefabs/LevelObjects/Settings/LevelLightSettings";

        public static readonly string LightSettingsOverrideObjectResourcePath = "Prefabs/LevelObjects/Lights/LightSettingsOverride";

        private Material[] m_skyboxes;

        private RealisticLightingInfoList m_lightingInfoList;

        private void Start()
        {
            DelegateScheduler.Instance.Schedule(loadSkyboxesAddon, 3f);
        }

        private void loadSkyboxesAddon()
        {
            if (ErrorManager.Instance.HasCrashed() || !ContentManager.Instance.HasContent(ContentManager.REALISTIC_SKYBOXES_CONTENT_FOLDER_NAME))
                return;

            ContentManager.Instance.SetContentIsLoading(this, true);
            ModResources.LoadBundleAsync(OVERHAUL_ASSETS_SKYBOXES, delegate (bool result)
            {
                if (!result)
                    return;

                _ = ModActionUtils.RunCoroutine(loadAllSkyboxesCoroutine());
            }, Path.Combine(ModCore.addonsFolder, ContentManager.REALISTIC_SKYBOXES_CONTENT_FOLDER_NAME));

            RealisticLightingInfoList realisticLightingInfoList = null;
            try
            {
                string path = Path.Combine(ModCore.addonsFolder, ContentManager.REALISTIC_SKYBOXES_CONTENT_FOLDER_NAME, LIGHTING_INFO_LIST_FILE);
                if (!File.Exists(path))
                {
                    string oldPath = Path.Combine(ModCore.addonsFolder, ContentManager.REALISTIC_SKYBOXES_CONTENT_FOLDER_NAME, OLD_LIGHTING_INFO_LIST_FILE);
                    if (File.Exists(oldPath))
                    {
                        ModIOUtils.WriteText(ModIOUtils.ReadText(oldPath).Replace("Lightning", "Lighting"), oldPath);
                        File.Move(oldPath, path);
                    }
                    else
                    {
                        realisticLightingInfoList = new RealisticLightingInfoList();
                    }
                }
                realisticLightingInfoList = ModJsonUtils.DeserializeStream<RealisticLightingInfoList>(path);
                realisticLightingInfoList.FixValues();
            }
            catch
            {
                realisticLightingInfoList = new RealisticLightingInfoList();
                realisticLightingInfoList.FixValues();
            }
            m_lightingInfoList = realisticLightingInfoList;
        }

        private IEnumerator loadAllSkyboxesCoroutine()
        {
            AssetBundle bundle = ModResources.AssetBundle("overhaulassets_skyboxes", Path.Combine(ModCore.addonsFolder, ContentManager.REALISTIC_SKYBOXES_CONTENT_FOLDER_NAME));
            if (ModAdvancedCache.TryGet("RealisticSkyboxes", out Material[] array))
                m_skyboxes = array;
            else
            {
                AssetBundleRequest r = bundle.LoadAllAssetsAsync<Material>();
                yield return r;
                if (!r.allAssets.IsNullOrEmpty())
                {
                    m_skyboxes = new Material[r.allAssets.Length];
                    int i = 0;
                    foreach (Object obj in r.allAssets)
                    {
                        m_skyboxes[i] = r.allAssets[i] as Material;
                        i++;
                    }
                }
                ModAdvancedCache.Add("RealisticSkyboxes", m_skyboxes);
            }

            LevelEditorLightManager.Instance.RefreshLightInScene();
            ContentManager.Instance.SetContentIsLoading(this, false);
            yield break;
        }

        public void SaveLightingInfo()
        {
            RealisticLightingInfoList realisticLightingInfoList = m_lightingInfoList;
            if (realisticLightingInfoList == null || realisticLightingInfoList.LightingInfos.IsNullOrEmpty())
                return;

            ModJsonUtils.WriteStream(Path.Combine(ModCore.addonsFolder, ContentManager.REALISTIC_SKYBOXES_CONTENT_FOLDER_NAME, LIGHTING_INFO_LIST_FILE), realisticLightingInfoList);
        }

        public void SaveCurrentLightingInfo(int skyboxIndex)
        {
            RealisticLightingInfoList realisticLightingInfoList = m_lightingInfoList;
            if (realisticLightingInfoList == null)
            {
                ModUIUtils.MessagePopupOK("Could not save current level lighting info", "info list is missing");
                return;
            }

            string prefabName = GetLevelPrefabName(null);
            if (prefabName.IsNullOrEmpty())
            {
                ModUIUtils.MessagePopupOK("Could not save current level lighting info", "prefab name is missing");
                return;
            }

            RealisticLightingInfo realisticLightingInfo = realisticLightingInfoList.GetLightingInfo(prefabName);
            if (realisticLightingInfo == null)
            {
                realisticLightingInfo = new RealisticLightingInfo();
                realisticLightingInfoList.LightingInfos.Add(realisticLightingInfo);
            }
            realisticLightingInfo.FixValues();

            realisticLightingInfo.Lighting.SetValuesUsingEnvironmentSettings();
            realisticLightingInfo.LevelPrefabName = prefabName;
            realisticLightingInfo.SkyboxIndex = skyboxIndex;
            SaveLightingInfo();
        }

        public string GetLevelPrefabName(LevelDescription levelDescription)
        {
            if (levelDescription == null)
            {
                try
                {
                    levelDescription = GameFlowManager.Instance.HasWonRound() ? LevelManager.Instance.GetLastSpawnedLevelDescription() : LevelManager.Instance.GetCurrentLevelDescription();
                }
                catch
                {
                    return null;
                }
            }

            if (levelDescription == null)
                return null;

            string name = levelDescription.PrefabName;
            return name.IsNullOrEmpty() ? null : name.Substring(name.LastIndexOf("/") + 1);
        }

        public void SetSkybox(int index)
        {
            if (index < 0 || m_skyboxes.IsNullOrEmpty())
                return;

            Material material = m_skyboxes[index];
            SkyBoxManager.Instance._currentSkybox = material;
            RenderSettings.skybox = material;
        }

        public RealisticLightingInfo GetCurrentRealisticLightingInfo()
        {
            string prefabName = GetLevelPrefabName(null);
            if (prefabName.IsNullOrEmpty())
                return null;

            RealisticLightingInfoList realisticLightingInfoList = m_lightingInfoList;
            if (realisticLightingInfoList == null || realisticLightingInfoList.LightingInfos.IsNullOrEmpty())
                return null;

            return realisticLightingInfoList.GetLightingInfo(prefabName);
        }

        public void PatchLevelLightSettings(LevelLightSettings lightSettings = null, bool refreshLight = false)
        {
            if (!lightSettings)
                lightSettings = LevelEditorLightManager.Instance.GetActiveLightSettings();

            RealisticLightSettings realisticLightSettings = lightSettings.GetComponent<RealisticLightSettings>();
            if (!realisticLightSettings)
            {
                realisticLightSettings = lightSettings.gameObject.AddComponent<RealisticLightSettings>();
            }

            RealisticLightingInfo realisticLightingInfo = GetCurrentRealisticLightingInfo();
            if (realisticLightingInfo != null)
            {
                if (realisticLightingInfo.Lighting != null)
                    realisticLightingInfo.Lighting.ApplyValues(lightSettings);

                realisticLightSettings.RealisticSkyBoxIndex = realisticLightingInfo.SkyboxIndex;

                if (refreshLight)
                    LevelEditorLightManager.Instance.RefreshLightInScene();
            }
            else
            {
                realisticLightSettings.RealisticSkyBoxIndex = -1;
            }
            //realisticLightSettings.HasSetSkyBoxIndex = true;
        }
    }
}
