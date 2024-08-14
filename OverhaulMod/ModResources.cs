using OverhaulMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OverhaulMod
{
    public class ModResources : Singleton<ModResources>
    {
        internal const string ASSET_BUNDLES_FOLDER = "assets/";

        private static readonly Dictionary<string, AssetBundleInfo> s_bundles = new Dictionary<string, AssetBundleInfo>();

        private void OnDestroy()
        {
            foreach (AssetBundleInfo bundle in s_bundles.Values)
                bundle.Unload();

            s_bundles.Clear();
        }

        private AssetBundleInfo getOrCreateAssetBundleInfo(string fileLocation, string name, bool load)
        {
            string bundleName = name.IsNullOrEmpty() ? GetBundleName(fileLocation) : name;

            AssetBundleInfo assetBundleInfo;
            if (!s_bundles.ContainsKey(bundleName))
            {
                assetBundleInfo = new AssetBundleInfo(fileLocation);
                if (load)
                    assetBundleInfo.Load();

                s_bundles.Add(bundleName, assetBundleInfo);
            }
            else
            {
                assetBundleInfo = s_bundles[bundleName];
            }
            return assetBundleInfo;
        }

        public T LoadAsset<T>(string bundle, string asset, string pathPrefix = null) where T : UnityEngine.Object
        {
            return getOrCreateAssetBundleInfo(GetBundlePath(bundle, pathPrefix), bundle, true).GetAsset<T>(asset);
        }

        public void LoadAssetAsync<T>(string bundle, string asset, Action<T> callback, string pathPrefix = null) where T : UnityEngine.Object
        {
            getOrCreateAssetBundleInfo(GetBundlePath(bundle, pathPrefix), bundle, true).GetAssetAsync(asset, callback);
        }

        public void LoadAssetBundle(string bundle, string pathPrefix = null)
        {
            _ = getOrCreateAssetBundleInfo(GetBundlePath(bundle, pathPrefix), bundle, true);
        }

        public void LoadAssetBundleAsync(string bundle, Action<bool> callback, string pathPrefix = null)
        {
            AssetBundleInfo bundleInfo = getOrCreateAssetBundleInfo(GetBundlePath(bundle, pathPrefix), bundle, false);
            bundleInfo.LoadAsync(callback);
        }

        public static T Load<T>(string bundle, string asset, string pathPrefix = null) where T : UnityEngine.Object
        {
            return Instance.LoadAsset<T>(bundle, asset, pathPrefix);
        }

        public static void LoadAsync<T>(string bundle, string asset, Action<T> callback, string pathPrefix = null) where T : UnityEngine.Object
        {
            Instance.LoadAssetAsync(bundle, asset, callback, pathPrefix);
        }

        public static void LoadBundle(string bundle, string pathPrefix = null)
        {
            Instance.LoadAssetBundle(bundle, pathPrefix);
        }

        public static void LoadBundleAsync(string bundle, Action<bool> callback, string pathPrefix = null)
        {
            Instance.LoadAssetBundleAsync(bundle, callback, pathPrefix);
        }

        public static string GetBundlePath(string bundle, string pathPrefix = null)
        {
            return pathPrefix == null ? Path.Combine(ModCore.folder, ASSET_BUNDLES_FOLDER, bundle) : Path.Combine(pathPrefix, bundle);
        }

        public static GameObject Prefab(string bundle, string asset, string pathPrefix = null)
        {
            return Load<GameObject>(bundle, asset, pathPrefix);
        }

        public static TextAsset TextAsset(string bundle, string asset, string pathPrefix = null)
        {
            return Load<TextAsset>(bundle, asset, pathPrefix);
        }

        public static Font Font(string bundle, string asset, string pathPrefix = null)
        {
            return Load<Font>(bundle, asset, pathPrefix);
        }

        public static AudioClip AudioClip(string bundle, string asset, string pathPrefix = null)
        {
            return Load<AudioClip>(bundle, asset, pathPrefix);
        }

        public static Texture2D Texture2D(string bundle, string asset, string pathPrefix = null)
        {
            return Load<Texture2D>(bundle, asset, pathPrefix);
        }

        public static Sprite Sprite(string bundle, string asset, string pathPrefix = null)
        {
            return Load<Sprite>(bundle, asset, pathPrefix);
        }

        public static Shader Shader(string bundle, string asset, string pathPrefix = null)
        {
            return Load<Shader>(bundle, asset, pathPrefix);
        }

        public static AssetBundle AssetBundle(string bundle, string pathPrefix = null)
        {
            return Instance.getOrCreateAssetBundleInfo(GetBundlePath(bundle, pathPrefix), bundle, true).GetBundle();
        }

        public static string GetBundleName(string fileLocation)
        {
            return Path.GetFileNameWithoutExtension(fileLocation);
        }

        public enum AssetLoadingState
        {
            NotLoaded,
            Loading,
            Loaded
        }

        public class AssetBundleInfo
        {
            public readonly string FileLocation;

            public AssetLoadingState LoadingState;

            private float m_loadProgress;


            private AssetBundle m_bundle;

            private readonly Dictionary<string, UnityEngine.Object> m_cachedAssets;


            private readonly Dictionary<string, float> m_assetsBeingLoaded;

            public AssetBundleInfo(string fileLocation)
            {
                FileLocation = fileLocation;
                LoadingState = AssetLoadingState.NotLoaded;
                m_bundle = null;
                m_cachedAssets = new Dictionary<string, UnityEngine.Object>();
                m_assetsBeingLoaded = new Dictionary<string, float>();
                m_loadProgress = 0f;
            }

            public AssetBundle GetBundle()
            {
                return m_bundle;
            }

            public float GetBundleLoadProgress()
            {
                if (LoadingState == AssetLoadingState.NotLoaded)
                    return 0f;

                return m_loadProgress;
            }

            public float GetAssetLoadProgress(string name)
            {
                if (m_cachedAssets.ContainsKey(name))
                    return 1f;

                return m_assetsBeingLoaded.ContainsKey(name) ? m_assetsBeingLoaded[name] : 0f;
            }

            public T GetAsset<T>(string name) where T : UnityEngine.Object
            {
                if (LoadingState != AssetLoadingState.Loaded)
                    return null;

                if (m_cachedAssets.TryGetValue(name, out UnityEngine.Object obj))
                    return (T)obj;

                obj = m_bundle.LoadAsset<T>(name);
                if (obj)
                {
                    m_cachedAssets.Add(name, obj);
                }
                return (T)obj;
            }

            public void GetAssetAsync<T>(string name, Action<T> callback) where T : UnityEngine.Object
            {
                if (LoadingState != AssetLoadingState.Loaded)
                {
                    callback?.Invoke(null);
                    return;
                }

                if (m_assetsBeingLoaded.ContainsKey(name))
                {
                    _ = waitForAssetLoadCoroutine(name, callback).Run(true);
                    return;
                }

                if (m_cachedAssets.TryGetValue(name, out UnityEngine.Object obj))
                {
                    callback?.Invoke((T)obj);
                    return;
                }

                _ = getAssetAsync(name, callback).Run(true);
            }

            private IEnumerator getAssetAsync<T>(string name, Action<T> callback) where T : UnityEngine.Object
            {
                m_assetsBeingLoaded.Add(name, 0f);

                AssetBundleRequest assetRequest = m_bundle.LoadAssetAsync<T>(name);
                while (!assetRequest.isDone)
                {
                    m_assetsBeingLoaded[name] = assetRequest.progress;
                    yield return null;
                }

                m_cachedAssets.Add(name, assetRequest.asset);

                _ = m_assetsBeingLoaded.Remove(name);
                callback?.Invoke((T)assetRequest.asset);
                yield break;
            }

            private IEnumerator waitForAssetLoadCoroutine<T>(string name, Action<T> callback) where T : UnityEngine.Object
            {
                while (m_assetsBeingLoaded.ContainsKey(name))
                    yield return null;

                if (m_cachedAssets.TryGetValue(name, out UnityEngine.Object obj))
                {
                    callback?.Invoke((T)obj);
                }
                else
                {
                    callback?.Invoke(null);
                }
                yield break;
            }

            public void Load()
            {
                if (LoadingState != AssetLoadingState.NotLoaded)
                    return;

                if (!File.Exists(FileLocation))
                    return;

                LoadingState = AssetLoadingState.Loading;
                m_bundle = UnityEngine.AssetBundle.LoadFromFile(FileLocation);
                m_loadProgress = 1f;
                LoadingState = AssetLoadingState.Loaded;
            }

            public void LoadAsync(Action<bool> callback)
            {
                switch (LoadingState)
                {
                    case AssetLoadingState.NotLoaded:
                        if (!File.Exists(FileLocation))
                        {
                            callback?.Invoke(false);
                            return;
                        }

                        _ = loadAsyncCoroutine(callback).Run(true);
                        return;
                    case AssetLoadingState.Loading:
                        _ = waitForLoadCoroutine(callback).Run(true);
                        return;
                    case AssetLoadingState.Loaded:
                        callback?.Invoke(true);
                        return;
                }
            }

            private IEnumerator loadAsyncCoroutine(Action<bool> callback)
            {
                LoadingState = AssetLoadingState.Loading;
                AssetBundleCreateRequest createRequest = UnityEngine.AssetBundle.LoadFromFileAsync(FileLocation);
                while (!createRequest.isDone)
                {
                    m_loadProgress = createRequest.progress;
                    yield return null;
                }

                if (!createRequest.assetBundle)
                {
                    m_loadProgress = 0f;
                    LoadingState = AssetLoadingState.NotLoaded;
                    callback?.Invoke(false);
                    yield break;
                }

                m_bundle = createRequest.assetBundle;
                m_loadProgress = 1f;

                LoadingState = AssetLoadingState.Loaded;
                callback?.Invoke(true);
                yield break;
            }

            private IEnumerator waitForLoadCoroutine(Action<bool> callback)
            {
                while (LoadingState == AssetLoadingState.Loading)
                    yield return null;

                callback?.Invoke(LoadingState == AssetLoadingState.Loaded);
                yield break;
            }

            public void Unload()
            {
                if (LoadingState != AssetLoadingState.Loaded)
                    return;

                m_assetsBeingLoaded.Clear();
                m_cachedAssets.Clear();
                m_bundle.Unload(false);
                m_bundle = null;

                LoadingState = AssetLoadingState.NotLoaded;
                m_loadProgress = 0f;
            }
        }
    }
}
