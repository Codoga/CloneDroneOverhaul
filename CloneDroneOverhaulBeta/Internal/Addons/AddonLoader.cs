﻿using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CDOverhaul
{
    public class AddonLoader : OverhaulDisposable
    {
        private static string s_LoadedContent = string.Empty;

        public void LoadAllContent(AddonManager manager)
        {
            unZipFiles(manager.GetZipFiles(), true, manager);
            foreach (string directory in manager.GetContentDirectories())
                LoadContent(directory, manager);
        }

        public void LoadContent(string directoryPath, AddonManager manager)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                OverhaulDebug.Warn("[AddonLoader] directoryPath is NULL or EMPTY!", EDebugType.Assets);
                return;
            }

            if (!Directory.Exists(directoryPath))
            {
                OverhaulDebug.Warn("[AddonLoader] Cannot find directory: " + directoryPath, EDebugType.Assets);
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

            string uniqueIdFilePath = directoryPath + "/UniqueID.txt";
            if (!File.Exists(uniqueIdFilePath))
            {
                OverhaulDebug.Warn(string.Format("[AddonLoader] File UniqueID.txt not found in {0} directory! Delete the directory!", directoryPath), EDebugType.Assets);
                return;
            }
            string uniqueIdFileContent = OverhaulCore.ReadText(uniqueIdFilePath);
            if (s_LoadedContent.Contains(uniqueIdFileContent))
            {
                OverhaulDebug.Warn(string.Format("[AddonLoader] {0} is already loaded!", uniqueIdFileContent), EDebugType.Assets);
                return;
            }

            string assetsToLoadPath = directoryPath + "/AssetsToLoad.txt";
            if (!File.Exists(assetsToLoadPath))
            {
                OverhaulDebug.Warn(string.Format("[AddonLoader] File AssetsToLoad.txt not found in {0} directory! Delete the directory!", directoryPath), EDebugType.Assets);
                return;
            }
            string assetsToLoadContent = OverhaulCore.ReadText(assetsToLoadPath);
            string[] assetFiles = assetsToLoadContent.Split(',');
            foreach (string assetFile in assetFiles)
            {
                string assetFilePath = "Content/" + directoryInfo.Name + "/" + assetFile;
                if (!File.Exists(OverhaulMod.Core.ModDirectory + assetFilePath))
                    continue;

                OverhaulDebug.Log("[AddonLoader] Loading asset bundle: " + assetFilePath, EDebugType.Assets);
                bool result = OverhaulAssetLoader.LoadAssetBundleIfNotLoaded(assetFilePath);
                OverhaulDebug.Log("[AddonLoader] Loaded asset bundle (" + assetFilePath + ") with result: " + result, EDebugType.Assets);
            }

            string scriptsPath = directoryPath + "/Scripts.txt";
            if (File.Exists(scriptsPath))
            {
                OverhaulDebug.Log(string.Format("[AddonLoader] File Scripts.txt found in {0} directory", directoryPath), EDebugType.Assets);

                string scriptsContent = OverhaulCore.ReadText(scriptsPath);
                string[] scripts = scriptsContent.Split(',');
                foreach (string scriptRef in scripts)
                {
                    string scriptReference = scriptRef.Replace(",", string.Empty);
                    if (string.IsNullOrEmpty(scriptReference))
                        continue;

                    OverhaulDebug.Log(string.Format("[AddonLoader] Instantiating script: {0}", scriptReference), EDebugType.Assets);

                    Assembly assembly = scriptReference.StartsWith("CDOverhaul") ? Assembly.GetExecutingAssembly() : null;
                    if (assembly == null)
                    {
                        OverhaulDebug.Warn(string.Format("[AddonLoader] Could not find assembly for {0}", scriptReference), EDebugType.Assets);
                        continue;
                    }

                    Type type = assembly.GetType(scriptReference);
                    if (type == null)
                    {
                        OverhaulDebug.Warn(string.Format("[AddonLoader] {0} script not found!", scriptReference), EDebugType.Assets);
                        continue;
                    }
                    else
                    {
                        if (type.BaseType != typeof(AddonBase))
                        {
                            OverhaulDebug.Warn(string.Format("[AddonLoader] Script {0} is not an AdditionalContentControllerBase!", scriptReference), EDebugType.Assets);
                            continue;
                        }
                        string loadedContentFolderPath = "Content/" + directoryInfo.Name + "/";
                        Component component = manager.gameObject.AddComponent(type);
                        AddonBase controllerBase = component as AddonBase;
                        controllerBase.folder = OverhaulMod.Core.ModDirectory + loadedContentFolderPath;
                        manager.LoadedAddons.Add(controllerBase);
                    }
                    OverhaulDebug.Log(string.Format("[AddonLoader] Instantiated script: {0}", scriptReference), EDebugType.Assets);
                }
            }
            s_LoadedContent += uniqueIdFileContent + ", ";
        }

        public static bool HasLoadedContent(string uniqueId) => s_LoadedContent.Contains(uniqueId);

        private static void unZipFiles(string[] files, bool deleteInitialFiles, AddonManager controller)
        {
            if (files.IsNullOrEmpty())
                return;

            int index = 0;
            do
            {
                string path = files[index];
                _ = OverhaulCore.UnZipFile(path, controller.GetContentDirectory() + path.Substring(path.LastIndexOf('/') + 1).Replace(".zip", string.Empty));
                if (deleteInitialFiles) File.Delete(path);
                index++;
            } while (index < files.Length);
        }
    }
}