using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;

namespace OverhaulMod.Utils
{
    public static class ModIntegrationUtils
    {
        private static bool s_hasLoaded;

        public static void Load()
        {
            if (s_hasLoaded)
                return;

            s_hasLoaded = true;
            ModdedMultiplayer.Load();
            SelectGarbageBotSkin.Load();
        }

        public static class SelectGarbageBotSkin
        {
            private static MethodInfo s_getGarbageBotSkinOptionsMethod;

            private static MethodInfo s_hasLocalPlayerStatsMethod;

            private static PropertyInfo s_selectedGarbageBotSkinIndexProperty;

            private static readonly object[] s_getGarbageBotSkinOptionsMethodArgs = { true };

            private static bool s_modMethodsPresent;

            public static int selectedGarbageBotSkinIndex
            {
                get
                {
                    if (s_selectedGarbageBotSkinIndexProperty == null)
                        return -1;

                    return (int)s_selectedGarbageBotSkinIndexProperty.GetValue(null);
                }
                set
                {
                    if (s_selectedGarbageBotSkinIndexProperty == null)
                        return;

                    s_selectedGarbageBotSkinIndexProperty.SetValue(null, value);
                }
            }

            public static void Load()
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name != "SelectGarbageBotSkins")
                        continue;

                    Type type = assembly.GetType("SelectGarbageBotSkins.ModCore");
                    if (type != null)
                    {
                        s_getGarbageBotSkinOptionsMethod = type.GetMethod("GetGarbageBotSkinOptions", BindingFlags.Static | BindingFlags.Public);
                        s_hasLocalPlayerStatsMethod = type.GetMethod("HasLocalPlayerStats", BindingFlags.Static | BindingFlags.Public);
                    }

                    Type type2 = assembly.GetType("SelectGarbageBotSkins.ModSettings");
                    if (type2 != null)
                    {
                        s_selectedGarbageBotSkinIndexProperty = type2.GetProperty("selectedGarbageBotSkinIndex", BindingFlags.Static | BindingFlags.Public);
                    }

                    s_modMethodsPresent = s_getGarbageBotSkinOptionsMethod != null && s_hasLocalPlayerStatsMethod != null && s_selectedGarbageBotSkinIndexProperty != null;
                }
            }

            public static List<Dropdown.OptionData> GetGarbageBotSkinOptions()
            {
                if (s_getGarbageBotSkinOptionsMethod == null)
                    return null;

                return (List<Dropdown.OptionData>)s_getGarbageBotSkinOptionsMethod.Invoke(null, s_getGarbageBotSkinOptionsMethodArgs);
            }

            public static bool HasLocalPlayerStats()
            {
                if (s_hasLocalPlayerStatsMethod == null)
                    return false;

                return (bool)s_hasLocalPlayerStatsMethod.Invoke(null, null);
            }

            public static bool IsModAvailable()
            {
                return s_modMethodsPresent && ModSpecialUtils.IsModEnabled("battle-royale-garbage-bot-selection");
            }
        }

        public static class ModdedMultiplayer
        {
            private static PropertyInfo s_isInModdedMultiplayerProperty;

            private static PropertyInfo s_gameModeInfoCurrentProperty;

            private static PropertyInfo s_gameModeInfoDisplayNameProperty;

            private static PropertyInfo s_gameModeInfoIdProperty;

            private static PropertyInfo s_apiLobbyManagerProperty;

            private static PropertyInfo s_lobbyManagerLobbyMembersProperty;

            private static PropertyInfo s_lobbyManagerLobbyIdProperty;

            private static PropertyInfo s_arrayLengthProperty;

            private static MethodInfo s_apiOnInviteMethod;

            public static void Load()
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name != "CloneDroneModdedMultiplayerAPI")
                        continue;

                    Type type = assembly.GetType("CloneDroneModdedMultiplayerAPI.MultiplayerAPI");
                    if (type != null)
                    {
                        s_isInModdedMultiplayerProperty = type.GetProperty("isInModdedMultiplayer", BindingFlags.Static | BindingFlags.Public);
                        s_apiLobbyManagerProperty = type.GetProperty("lobbyManager", BindingFlags.Static | BindingFlags.Public);
                        s_apiOnInviteMethod = type.GetMethod("OnInvite", BindingFlags.Static | BindingFlags.Public);
                    }

                    Type type2 = assembly.GetType("CloneDroneModdedMultiplayerAPI.Gameplay.GameModeInfo");
                    if (type2 != null)
                    {
                        s_gameModeInfoCurrentProperty = type2.GetProperty("current", BindingFlags.Static | BindingFlags.Public);
                        s_gameModeInfoDisplayNameProperty = type2.GetProperty("displayName", BindingFlags.Instance | BindingFlags.Public);
                        s_gameModeInfoIdProperty = type2.GetProperty("id", BindingFlags.Instance | BindingFlags.Public);
                    }

                    Type type3 = assembly.GetType("CloneDroneModdedMultiplayerAPI.LobbyManager");
                    if (type3 != null)
                    {
                        s_lobbyManagerLobbyMembersProperty = type3.GetProperty("lobbyMembers", BindingFlags.Instance | BindingFlags.Public);
                        s_lobbyManagerLobbyIdProperty = type3.GetProperty("lobbyId", BindingFlags.Instance | BindingFlags.Public);
                    }

                    Type type4 = Type.GetType("System.Array");
                    if (type4 != null)
                    {
                        s_arrayLengthProperty = type4.GetProperty("Length", BindingFlags.Instance | BindingFlags.Public);
                    }
                    break;
                }
            }

            public static bool IsInModdedMultiplayer()
            {
                PropertyInfo p = s_isInModdedMultiplayerProperty;
                if (p == null)
                    return false;

                return (bool)p.GetValue(null);
            }

            public static string GetCurrentGameModeInfoDisplayName()
            {
                PropertyInfo gameModeInfoCurrentProperty = s_gameModeInfoCurrentProperty;
                if (gameModeInfoCurrentProperty == null)
                    return null;

                PropertyInfo gameModeInfoDisplayNameProperty = s_gameModeInfoDisplayNameProperty;
                if (gameModeInfoDisplayNameProperty == null)
                    return null;

                object value = gameModeInfoCurrentProperty.GetValue(null);
                if (value == null)
                    return null;

                return (string)gameModeInfoDisplayNameProperty.GetValue(value);
            }

            public static string GetCurrentGameModeInfoID()
            {
                PropertyInfo gameModeInfoCurrentProperty = s_gameModeInfoCurrentProperty;
                if (gameModeInfoCurrentProperty == null)
                    return null;

                PropertyInfo gameModeInfoIdProperty = s_gameModeInfoIdProperty;
                if (gameModeInfoIdProperty == null)
                    return null;

                object value = gameModeInfoCurrentProperty.GetValue(null);
                if (value == null)
                    return null;

                return (string)gameModeInfoIdProperty.GetValue(value);
            }

            public static int GetCurrentPlayerCount()
            {
                PropertyInfo apiLobbyManagerProperty = s_apiLobbyManagerProperty;
                if (apiLobbyManagerProperty == null)
                    return -1;

                PropertyInfo lobbyManagerLobbyMembersProperty = s_lobbyManagerLobbyMembersProperty;
                if (lobbyManagerLobbyMembersProperty == null)
                    return -1;

                PropertyInfo arrayLengthProperty = s_arrayLengthProperty;
                if (arrayLengthProperty == null)
                    return -1;

                object lobbyManagerObject = apiLobbyManagerProperty.GetValue(null);
                if (lobbyManagerObject == null)
                    return -1;

                object lobbyMembersArrayObject = lobbyManagerLobbyMembersProperty.GetValue(lobbyManagerObject);
                if (lobbyMembersArrayObject == null)
                    return -1;

                return (int)arrayLengthProperty.GetValue(lobbyMembersArrayObject);
            }

            public static int GetMaxPlayerCount()
            {
                PropertyInfo apiLobbyManagerProperty = s_apiLobbyManagerProperty;
                if (apiLobbyManagerProperty == null)
                    return -1;

                PropertyInfo lobbyManagerLobbyIdProperty = s_lobbyManagerLobbyIdProperty;
                if (lobbyManagerLobbyIdProperty == null)
                    return -1;

                object lobbyManagerObject = apiLobbyManagerProperty.GetValue(null);
                if (lobbyManagerObject == null)
                    return -1;

                object lobbyIdObject = lobbyManagerLobbyIdProperty.GetValue(lobbyManagerObject);
                if (lobbyIdObject == null)
                    return -1;

                return Steamworks.SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyIdObject);
            }

            public static string GetLobbyID()
            {
                PropertyInfo apiLobbyManagerProperty = s_apiLobbyManagerProperty;
                if (apiLobbyManagerProperty == null)
                    return null;

                PropertyInfo lobbyManagerLobbyIdProperty = s_lobbyManagerLobbyIdProperty;
                if (lobbyManagerLobbyIdProperty == null)
                    return null;

                object lobbyManagerObject = apiLobbyManagerProperty.GetValue(null);
                if (lobbyManagerObject == null)
                    return null;

                object lobbyIdObject = lobbyManagerLobbyIdProperty.GetValue(lobbyManagerObject);
                if (lobbyIdObject == null)
                    return null;

                return ((CSteamID)lobbyIdObject).ToString();
            }

            public static void OnInvite(string gameModeId, string lobbyCode)
            {
                MethodInfo method = s_apiOnInviteMethod;
                if (method == null)
                    return;

                _ = method.Invoke(null, new object[]
                {
                    gameModeId,
                    lobbyCode
                });
            }
        }
    }
}
