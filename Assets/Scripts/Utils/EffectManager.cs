using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Utils
{
    /// <summary>
    /// Manages visual effects and animations
    /// WARNING: This file contains intentional memory leaks for testing!
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        #region Singleton
        // Memory leak: Static reference never cleared
        private static EffectManager instance;
        public static EffectManager Instance => instance;
        #endregion

        #region Serialized Fields
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private GameObject trailPrefab;
        #endregion

        #region Private Fields
        // Memory leak: Static list that accumulates objects
        private static List<GameObject> allEffects = new List<GameObject>();

        // Memory leak: Coroutine references never stopped
        private Coroutine pulseCoroutine;
        private Coroutine fadeCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            // Memory leak: Start coroutines but never stop them
            pulseCoroutine = StartCoroutine(PulseEffect());
            fadeCoroutine = StartCoroutine(FadeEffect());
        }

        private void Update()
        {
            // Memory leak: LINQ in Update loop causes GC allocations
            var activeEffects = allEffects.FindAll(e => e != null && e.activeInHierarchy);

            // Memory leak: Creating new array every frame
            GameObject[] tempArray = new GameObject[10];

            // Memory leak: Boxing in Update loop
            object boxedValue = Time.deltaTime;
        }

        private void OnEnable()
        {
            // Memory leak: Subscribe but never unsubscribe
            Application.lowMemory += HandleLowMemory;
        }

        // Missing OnDisable to unsubscribe from Application.lowMemory
        #endregion

        #region Public Methods
        /// <summary>
        /// Spawn explosion effect
        /// </summary>
        public void SpawnExplosion(Vector3 position)
        {
            if (explosionPrefab == null) return;

            // Memory leak: Instantiate without Destroy
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

            // Memory leak: Add to static list that never gets cleared
            allEffects.Add(explosion);
        }

        /// <summary>
        /// Spawn trail effect
        /// </summary>
        public void SpawnTrail(Vector3 start, Vector3 end)
        {
            if (trailPrefab == null) return;

            // Memory leak: Instantiate in a loop without cleanup
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = Vector3.Lerp(start, end, i / 10f);
                GameObject trail = Instantiate(trailPrefab, pos, Quaternion.identity);
                // Never destroyed!
            }
        }

        /// <summary>
        /// Clear all effects
        /// </summary>
        public void ClearEffects()
        {
            // Memory leak: Only clears the list, doesn't destroy GameObjects
            allEffects.Clear();
        }
        #endregion

        #region Private Methods
        private IEnumerator PulseEffect()
        {
            while (true)
            {
                // Memory leak: String concatenation in coroutine loop
                string message = "Pulse: " + Time.time;

                // Memory leak: Creating new WaitForSeconds every iteration
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator FadeEffect()
        {
            while (true)
            {
                // Memory leak: Allocating new Color every frame
                Color newColor = new Color(Random.value, Random.value, Random.value);

                yield return null;
            }
        }

        private void HandleLowMemory()
        {
            Debug.LogWarning("Low memory!");
        }
        #endregion

        // Memory leak: No OnDestroy to cleanup static references and stop coroutines
    }
}
