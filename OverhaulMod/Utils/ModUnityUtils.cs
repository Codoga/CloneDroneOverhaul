﻿using UnityEngine;

namespace OverhaulMod.Utils
{
    internal static class ModUnityUtils
    {
        public static Transform CloneLevelEnemySpawner(string name)
        {
            GameObject toInstantiate = Resources.Load<GameObject>("Prefabs/LevelObjects/EnemySpawns/Bow1");
            toInstantiate.SetActive(false);
            GameObject gameObject = UnityEngine.Object.Instantiate(toInstantiate);
            gameObject.name = name;
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            toInstantiate.SetActive(true);
            return gameObject.transform;
        }

        public static Transform CloneAndSetLevelEnemySpawnerUp(string name, EnemyType enemyType)
        {
            GameObject toInstantiate = Resources.Load<GameObject>("Prefabs/LevelObjects/EnemySpawns/Bow1");
            toInstantiate.SetActive(false);
            GameObject gameObject = UnityEngine.Object.Instantiate(toInstantiate);
            gameObject.name = name;
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            toInstantiate.SetActive(true);

            LevelEnemySpawner levelEnemySpawner = gameObject.GetComponent<LevelEnemySpawner>();
            if (levelEnemySpawner)
                levelEnemySpawner.EnemyPrefab = EnemyFactory.Instance.GetEnemyPrefab(enemyType).transform;
            return gameObject.transform;
        }

        public static void SetEmissionEnabled(this ParticleSystem particleSystem, bool enabled)
        {
            if (!particleSystem)
                return;

            ParticleSystem.EmissionModule em = particleSystem.emission;
            em.enabled = enabled;
        }

        public static void DisableRendererAndCollider(GameObject gameObject)
        {
            if (!gameObject)
                return;

            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer)
                renderer.enabled = false;

            Collider collider = gameObject.GetComponent<Collider>();
            if (collider)
                collider.enabled = false;
        }

        public static Vector3 LerpVector3(Vector3 a, Vector3 b, float deltaTime)
        {
            Vector3 vector = a;
            vector.x = Mathf.Lerp(a.x, b.x, deltaTime);
            vector.y = Mathf.Lerp(a.y, b.y, deltaTime);
            vector.z = Mathf.Lerp(a.z, b.z, deltaTime);
            return vector;
        }

        public static Color LerpRGB(Color a, Color b, float deltaTime)
        {
            Color color = a;
            color.r = Mathf.Lerp(a.r, b.r, deltaTime);
            color.g = Mathf.Lerp(a.g, b.g, deltaTime);
            color.b = Mathf.Lerp(a.b, b.b, deltaTime);
            return color;
        }

        public static Color LerpRGBA(Color a, Color b, float deltaTime)
        {
            Color color = a;
            color.r = Mathf.Lerp(a.r, b.r, deltaTime);
            color.g = Mathf.Lerp(a.g, b.g, deltaTime);
            color.b = Mathf.Lerp(a.b, b.b, deltaTime);
            color.a = Mathf.Lerp(a.a, b.a, deltaTime);
            return color;
        }
    }
}
