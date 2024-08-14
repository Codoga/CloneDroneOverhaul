using OverhaulMod.Utils;
using System.Linq;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class ArenaRemodelManager : Singleton<ArenaRemodelManager>, IGameLoadListener
    {
        [ModSettingRequireRestart]
        [ModSetting(ModSettingsConstants.ENABLE_ARENA_REMODEL, true)]
        public static bool EnableRemodel;

        public readonly string[] IgnoredParts = new string[]
        {
            "ARENA_THRONE_END_PARTS-3"
        };

        private GameObject m_newArenaObject;

        private Transform m_worldRootTransform;

        private Transform m_arenaFinalTransform;
        private Transform m_arenaUpperInteriorTransform;

        private Transform m_liftTransform;
        private Transform m_liftWallTransform;

        private Transform m_arenaMainTransform;

        private Transform m_emperorSectionTransform;
        private Transform m_battleCruiserTransform;

        private Transform m_commentatorBoxTransform;
        private Transform m_commentatronTransform;
        private Transform m_analysisBotTransform;

        private Transform m_arenaCameraAnimatorTransform;
        private Transform m_commentatorTargetTransform;

        private Transform m_arenaGroundArrowsTransform;
        private Transform m_returnSignTransform;
        private Transform m_arenaGroundArrowsSpotlightTransform;

        private Transform m_tvsTransform;
        private Transform m_giantScreen2Transform;
        private Transform m_arenaTVEndlessLevelTransform;

        private Transform m_garbageShuteTransform;
        private Transform m_garbageDoorTransform;
        private Transform m_garbageDoor2Transform;

        private Transform m_overhaulGarbageDoorTransform;

        private Material m_arenaOverhaulMaterial;
        private Material m_arenaLightsMaterial;

        private Vector3 m_ogGiantScreen2Position;
        private Vector3 m_ogGiantScreen2EulerAngles;
        private Vector3 m_ogGiantScreen2LocalScale;

        private Vector3 m_ogReturnSignPosition;
        private Vector3 m_ogReturnSignEulerAngles;

        private readonly float m_timeToRefreshVanillaArenaParts;

        private void Start()
        {
            RefreshArenaLook();
        }

        public void OnGameLoaded()
        {
            RefreshArenaLook();
        }

        public void RefreshArenaLook()
        {
            if (!EnableRemodel || m_newArenaObject)
                return;

            m_worldRootTransform = WorldRoot.Instance.transform;

            m_arenaFinalTransform = m_worldRootTransform.FindChildRecursive("ArenaFinal");
            m_arenaUpperInteriorTransform = m_arenaFinalTransform.FindChildRecursive("Arena2019");

            m_liftTransform = m_worldRootTransform.FindChildRecursive("LiftContainer");
            m_liftWallTransform = m_liftTransform.FindChildRecursive("LiftWall (1)");

            m_arenaMainTransform = m_arenaFinalTransform.FindChildRecursive("Arena");
            m_emperorSectionTransform = m_arenaMainTransform.FindChildRecursive("EmperorSection");

            m_commentatorBoxTransform = m_arenaFinalTransform.FindChildRecursive("CommentatorBox");
            m_commentatronTransform = m_commentatorBoxTransform.FindChildRecursive("Commentatron");
            if (!m_commentatronTransform)
                m_commentatronTransform = m_commentatorBoxTransform.FindChildRecursive("Commentatron_Xmas(Clone)");

            m_analysisBotTransform = m_commentatorBoxTransform.FindChildRecursive("AnalysisBot");
            if (!m_analysisBotTransform)
                m_analysisBotTransform = m_commentatorBoxTransform.FindChildRecursive("AnalysisBot_Xmas(Clone)");

            m_arenaCameraAnimatorTransform = m_arenaFinalTransform.FindChildRecursive("ArenaCameraAnimator");
            if (!m_arenaCameraAnimatorTransform)
            {
                m_arenaCameraAnimatorTransform = m_worldRootTransform.FindChildRecursive("ArenaCameraAnimator");
            }

            m_commentatorTargetTransform = m_arenaCameraAnimatorTransform.FindChildRecursive("CommentatorTarget");

            m_tvsTransform = m_arenaFinalTransform.FindChildRecursive("ArenaSideTVs");
            m_giantScreen2Transform = m_tvsTransform.FindChildRecursive("GiantScreen (2)");
            m_ogGiantScreen2Position = m_giantScreen2Transform.localPosition;
            m_ogGiantScreen2EulerAngles = m_giantScreen2Transform.localEulerAngles;
            m_ogGiantScreen2LocalScale = m_giantScreen2Transform.localScale;
            m_arenaTVEndlessLevelTransform = m_tvsTransform.FindChildRecursive("ArenaTV_EndlessLevel");

            m_arenaGroundArrowsTransform = m_arenaFinalTransform.FindChildRecursive("ArenaGroundArrows");
            m_returnSignTransform = m_arenaGroundArrowsTransform.FindChildRecursive("ReturnSign");
            m_ogReturnSignPosition = m_returnSignTransform.localPosition;
            m_ogReturnSignEulerAngles = m_returnSignTransform.localEulerAngles;
            m_arenaGroundArrowsSpotlightTransform = m_arenaGroundArrowsTransform.FindChildRecursive("Spotlight");

            m_garbageShuteTransform = m_arenaFinalTransform.FindChildRecursive("GarbageShute");
            m_garbageDoorTransform = m_garbageShuteTransform.FindChildRecursive("GarbageDoor2019");
            m_garbageDoorTransform.GetComponent<Renderer>().enabled = false;
            m_garbageDoor2Transform = m_garbageShuteTransform.FindChildRecursive("GarbageDoor2019Static");
            m_garbageDoor2Transform.GetComponent<Renderer>().enabled = false;

            GameObject gameObject = Instantiate(ModResources.Prefab(AssetBundleConstants.MODELS, "ArenaOverhaul"), m_arenaFinalTransform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localEulerAngles = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;
            m_newArenaObject = gameObject;

            ModdedObject moddedObject = gameObject.GetComponent<ModdedObject>();
            m_overhaulGarbageDoorTransform = moddedObject.GetObject<Transform>(0);

            m_arenaOverhaulMaterial = gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            m_arenaOverhaulMaterial.shader = Shader.Find("Standard");
            m_arenaLightsMaterial = gameObject.transform.GetChild(1).GetComponentInChildren<MeshRenderer>().sharedMaterial;
            m_arenaLightsMaterial.shader = Shader.Find("Standard");

            GlobalEventManager.Instance.AddEventListener("ArenaSettingsRefreshed", onArenaSettingsUpdate);

            SetUpperInteriorActive(true);
            SetVanillaPartsActive(false);
            PatchVanillaParts(true);
            setUpBattleCruiser();
            onArenaSettingsUpdate();
        }

        public void PatchVanillaParts(bool overhaul)
        {
            if (!m_giantScreen2Transform || !m_returnSignTransform)
                return;

            if (!overhaul)
            {
                m_giantScreen2Transform.localPosition = m_ogGiantScreen2Position;
                m_giantScreen2Transform.localEulerAngles = m_ogGiantScreen2EulerAngles;
                m_giantScreen2Transform.localScale = m_ogGiantScreen2LocalScale;

                m_returnSignTransform.localPosition = m_ogReturnSignPosition;
                m_returnSignTransform.localEulerAngles = m_ogReturnSignEulerAngles;
                return;
            }
            m_giantScreen2Transform.localPosition = new Vector3(82, 75);
            m_giantScreen2Transform.localEulerAngles = new Vector3(345, 90);
            m_giantScreen2Transform.localScale = Vector3.one * 4.4f;

            m_returnSignTransform.localPosition = new Vector3(83, 47, 0);
            m_returnSignTransform.localEulerAngles = new Vector3(350, 90, 0);

            m_arenaTVEndlessLevelTransform.localPosition = new Vector3(-76.5f, 25.15f, 0);
            m_arenaTVEndlessLevelTransform.localEulerAngles = new Vector3(347.64f, 270f, 0f);

            m_arenaGroundArrowsSpotlightTransform.localPosition = new Vector3(60, 55, 0);
            m_arenaGroundArrowsSpotlightTransform.localEulerAngles = new Vector3(47.5f, 90, 90);
            m_arenaGroundArrowsSpotlightTransform.localScale = Vector3.one;

            m_commentatorTargetTransform.localPosition = new Vector3(-62.55f, 38f, 0f);
            m_commentatorTargetTransform.localEulerAngles = new Vector3(8f, 270f, 0f);

            if (m_analysisBotTransform)
                m_analysisBotTransform.localPosition = new Vector3(5.2f, 4.7f, 0.52f);
            if (m_commentatronTransform)
                m_commentatronTransform.localPosition = new Vector3(3.44f, 2, -3.71f);

            m_commentatorBoxTransform.localPosition = new Vector3(-83.8f, 26.4f, 1.95f);
            m_commentatorBoxTransform.localScale = Vector3.one * 1.3f;

            m_emperorSectionTransform.localPosition = new Vector3(-12.9f, 5.0925f, 1.663f);
        }

        public void SetVanillaPartsActive(bool value)
        {
            for (int i = 0; i < m_arenaUpperInteriorTransform.childCount; i++)
            {
                GameObject gameObject = m_arenaUpperInteriorTransform.GetChild(i).gameObject;
                if (!IgnoredParts.Contains(gameObject.name))
                {
                    gameObject.SetActive(value);
                }
                else
                {
                    if (gameObject.name == "ARENA_THRONE_END_PARTS-3")
                    {
                        gameObject.transform.localPosition = new Vector3(3.2f, 10f, 15.5f);
                    }
                }
            }
            m_liftWallTransform.gameObject.SetActive(value);
        }

        public void SetUpperInteriorActive(bool value)
        {
            if(m_arenaUpperInteriorTransform)
                m_arenaUpperInteriorTransform.gameObject.SetActive(value);
        }

        private void setUpBattleCruiser()
        {
            m_battleCruiserTransform = TransformUtils.FindChildRecursive(m_emperorSectionTransform, "Battlecruiser");
            if (!m_battleCruiserTransform)
                return;

            Transform transformBC = Instantiate(EnemyFactory.Instance.Enemies[56].EnemyPrefab.GetComponent<BattleCruiserController>().CharacterModelPrefab.transform);
            foreach (MonoBehaviour behaviour in transformBC.GetComponentsInChildren<MonoBehaviour>())
                Destroy(behaviour);

            TransformUtils.HideAllChildren(transformBC);
            transformBC.GetChild(0).gameObject.SetActive(true);
            transformBC.SetParent(m_battleCruiserTransform, false);
            transformBC.localPosition = new Vector3(0f, 0f, -0.65f);
            transformBC.localEulerAngles = Vector3.zero;
            transformBC.localScale = new Vector3(0.35f, 0.35f, 0.35f);

            m_battleCruiserTransform.GetComponent<MeshRenderer>().enabled = false;
        }

        private void onArenaSettingsUpdate()
        {
            LevelEditorArenaSettings activeSettings = ArenaCustomizationManager.Instance?.GetActiveSettings();
            if (activeSettings)
            {
                if (m_arenaOverhaulMaterial)
                    m_arenaOverhaulMaterial.SetColor("_EmissionColor", activeSettings.HighlightColor * activeSettings.HighlightEmission);
                if (m_arenaLightsMaterial)
                    m_arenaLightsMaterial.SetColor("_EmissionColor", activeSettings.LightsColor * activeSettings.LightsEmission);
            }
        }

        private void Update()
        {
            if (m_overhaulGarbageDoorTransform && m_garbageDoorTransform)
            {
                Vector3 position = m_garbageDoorTransform.position;
                position.x = -1.225f;
                position.y += 1.21f;
                position.z = 0f;
                m_overhaulGarbageDoorTransform.localPosition = position;
            }
        }
    }
}
