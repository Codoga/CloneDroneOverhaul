﻿using OverhaulMod.Content.LevelEditor;
using OverhaulMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Visuals.Environment
{
    public class WeatherManager : Singleton<WeatherManager>, IGameLoadListener
    {
        private float m_timeToUpdate;

        public List<WeatherInfo> WeatherInfos;

        private Dictionary<string, ParticleSystem> m_nameToVFX;

        public static Vector3 vectorOffset
        {
            get
            {
                return Vector3.up * 180f;
            }
        }

        public Transform weatherVFXHolder
        {
            get;
            private set;
        }

        public LevelEditorWeatherSettingsOverride weatherOverrideObject
        {
            get;
            set;
        }

        private void Start()
        {
            if (!ModFeatures.IsEnabled(ModFeatures.FeatureType.WeatherSystem))
            {
                base.enabled = false;
                return;
            }

            createWeatherInfos();
            createHolderIfNull();
            populateHolder();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            m_timeToUpdate -= deltaTime;
            if (m_timeToUpdate < 0f)
            {
                m_timeToUpdate = 2f;
                Transform transform = weatherVFXHolder;
                if (transform)
                {
                    Camera target = Camera.main;
                    transform.position = target
                        ? Physics.Raycast(target.transform.position, Vector3.up, out RaycastHit hitInfo, 1000f, PhysicsManager.GetEnvironmentLayerMask(), QueryTriggerInteraction.UseGlobal)
                            ? hitInfo.transform.position + vectorOffset
                            : target.transform.position + vectorOffset
                        : vectorOffset;
                }
            }
        }

        public void RefreshWeather()
        {
            refreshWeatherBasedOnLevel();
        }

        private void createWeatherInfos()
        {
            if (WeatherInfos != null)
                return;

            WeatherInfos = new List<WeatherInfo>
            {
                new WeatherInfo()
                {
                    Name = "Clear",
                    IsDefault = true,
                },
                new WeatherInfo()
                {
                    Name = "Snowy",
                    EmissionRate = 75f,
                    BundleAndAsset = new KeyValuePair<string, string>(AssetBundleConstants.VFX, "WeatherVFX_Snow"),
                },
                new WeatherInfo()
                {
                    Name = "Rainy",
                    EmissionRate = 125f,
                    BundleAndAsset = new KeyValuePair<string, string>(AssetBundleConstants.VFX, "WeatherVFX_Rain"),
                },
            };
        }

        private void createHolderIfNull()
        {
            if (weatherVFXHolder)
                return;

            GameObject newHolder = new GameObject("OverhaulWeather");
            newHolder.transform.position = vectorOffset;
            DontDestroyOnLoad(newHolder);
            weatherVFXHolder = newHolder.transform;
        }

        private void populateHolder()
        {
            Transform holderTransform = weatherVFXHolder;
            if (!holderTransform)
                return;

            if (m_nameToVFX == null)
                m_nameToVFX = new Dictionary<string, ParticleSystem>();
            else
                m_nameToVFX.Clear();

            if (holderTransform.childCount != 0)
                TransformUtils.DestroyAllChildren(holderTransform);

            foreach (WeatherInfo info in WeatherInfos)
            {
                if (info.IsDefault)
                    continue;

                Transform instantiatedObjectTransform = Instantiate(info.GetVFXPrefab(), holderTransform).transform;
                instantiatedObjectTransform.localPosition = Vector3.zero;
                instantiatedObjectTransform.localEulerAngles = Vector3.zero;
                instantiatedObjectTransform.localScale = Vector3.one;
                instantiatedObjectTransform.gameObject.SetActive(true);
                ParticleSystem particleSystem = instantiatedObjectTransform.GetComponent<ParticleSystem>();
                particleSystem.SetEmissionEnabled(false);
                m_nameToVFX.Add(info.Name, particleSystem);
            }
        }

        private void refreshWeatherBasedOnLevel()
        {
            DeactivateAllParticles();

            LevelEditorWeatherSettingsOverride levelEditorWeatherSettingsOverride = weatherOverrideObject;

            int max = 250;
            float rate = 75f;
            float intensity = 1f;
            string weatherValue = string.Empty;
            bool isVanillaLevel = !levelEditorWeatherSettingsOverride && (!LevelManager.Instance || !LevelManager.Instance.IsCurrentLevelHidingTheArena() || GameModeManager.IsBattleRoyale());
            if (isVanillaLevel)
            {
                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, 11, 25);
                DateTime end = new DateTime(now.Year, 1, 25);
                bool shouldBeSnowy = end < start ? (now >= start || now <= end) : (now <= end && now >= start);
                if (shouldBeSnowy)
                {
                    weatherValue = "Snowy";
                }
            }

            if (levelEditorWeatherSettingsOverride)
            {
                weatherValue = levelEditorWeatherSettingsOverride.WeatherType;
                intensity = levelEditorWeatherSettingsOverride.Intensity;
                max = levelEditorWeatherSettingsOverride.MaxParticles;
            }

            WeatherInfo weatherInfo = GetWeatherInfo(weatherValue);
            if (weatherInfo != null)
            {
                rate = weatherInfo.EmissionRate * intensity;
            }

            ActivateParticles(weatherValue, rate, max);
        }

        public WeatherInfo GetWeatherInfo(string name)
        {
            if (WeatherInfos.IsNullOrEmpty())
                return null;

            foreach (WeatherInfo info in WeatherInfos)
                if (info != null && info.Name == name)
                    return info;

            return null;
        }

        public void DeactivateAllParticles()
        {
            if (m_nameToVFX == null || m_nameToVFX.Count == 0)
                return;

            foreach (ParticleSystem value in m_nameToVFX.Values)
            {
                if (value)
                    value.SetEmissionEnabled(false);
            }
        }

        public void ActivateParticles(string name, float emissionRate, int maxParticles = 250)
        {
            if (m_nameToVFX != null && m_nameToVFX.ContainsKey(name))
            {
                ParticleSystem ps = m_nameToVFX[name];
                if (ps)
                {
                    ParticleSystem.EmissionModule emission = ps.emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(emissionRate);
                    emission.enabled = true;
                    ParticleSystem.MainModule main = ps.main;
                    main.maxParticles = Mathf.Clamp(maxParticles, 1, 2000);
                }
            }
        }

        public void OnGameLoaded()
        {
            DeactivateAllParticles();

            GlobalEventManager.Instance.AddEventListener(GlobalEvents.LevelSpawned, onLevelSpawned);
            GlobalEventManager.Instance.AddEventListener(GlobalEvents.LevelEditorLevelOpened, onLevelSpawned);
        }

        private void onLevelSpawned()
        {
            _ = ModActionUtils.RunCoroutine(waitThenRefreshWeather());
        }

        private IEnumerator waitThenRefreshWeather()
        {
            yield return new WaitForSeconds(3f);
            refreshWeatherBasedOnLevel();
            yield break;
        }
    }
}
