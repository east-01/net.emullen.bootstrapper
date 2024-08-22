using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace EMullen.Bootstrapper 
{
    /// <summary>
    /// The Bootstrapper uses a scene to bootstrap GameObjects before other things load. This
    ///   special scene is called a bootstrapping scene.
    /// A Bootstrapper component should always exist in every game scene, with non-bootstrapping
    ///   scenes pointing to the bootstrapping scenes that they rely on.
    /// </summary>
    [Serializable]
    public class Bootstrapper : MonoBehaviour
    {

        [SerializeField]
        private bool isBootstrapScene;
        public bool IsBootstrapScene => isBootstrapScene;
        /// <summary>
        /// Is a promise that this scene will only be bootstrapped one time, meaning if this
        ///   bootstrapper appears in another sequence it will be skipped.
        /// </summary>
        [SerializeField]
        private bool onlyBootstrapOnce;
        public bool OnlyBootstrapOnce => onlyBootstrapOnce;

        [SerializeField]
        private BootstrapSequence sequence;
        public BootstrapSequence BootstrapSequence => sequence;

        /* Bootstrapper scene fields */
        /// <summary>
        /// Set true to run CacheIBootstrapComponents() every Update() call.
        /// </summary>
        [SerializeField]
        private bool cacheIBCEveryUpdate;
        [SerializeField]
        private List<GameObject> blacklistedGameObjects;

        public int IBCsStillLoading {get; private set; }
        public bool BootstrapComplete => IBCsStillLoading == 0;
        public float LoadProgress { get; private set; }

        /// <summary>
        /// A list of GameObjects that were issued warnings in the console for not having an 
        ///   IBootstrapperComponent
        /// </summary>
        private List<GameObject> issuedWarnings = new();
        /// <summary>
        /// A list of IBootstrapComponents that will be used in calculating the LoadProgress. 
        /// IBootstrapComponents will be added to this list in Update().
        /// </summary>
        private List<IBootstrapComponent> cachedBootstrapComponents;

        private void Awake() 
        {
            BootstrapSequenceManager bsm = BootstrapSequenceManager.Instance;
            if(bsm == null) {
                bsm = new BootstrapSequenceManager(sequence, out bool bmsInitStatus);
                if(!bmsInitStatus) {
                    Destroy(gameObject);
                    return;
                }
            }

            if(bsm != null && bsm.LoadingTargetScenes) {
                Destroy(gameObject);
                return;
            }

            // Bootstrapper scene, register it properly
            if(!bsm.RegisterBoostrapper(this))
                return;

        }

        private void Update() 
        {
            if(cachedBootstrapComponents == null) {
                cachedBootstrapComponents = new();
                CacheBootstrapComponents();
                UpdateComponentLoadProgress();
            }

            if(BootstrapComplete)
                return;

            if(cacheIBCEveryUpdate)
                CacheBootstrapComponents();

            UpdateComponentLoadProgress();

            if(BootstrapComplete) {
                BootstrapSequenceManager.Instance.BootstrapperCompleted(this);
            }
        }

        private void CacheBootstrapComponents() 
        {
            foreach(GameObject go in SceneManager.GetActiveScene().GetRootGameObjects()) {
                if(blacklistedGameObjects.Contains(go))
                    continue;

                IBootstrapComponent[] ibcs = go.GetComponents<IBootstrapComponent>();
                bool shouldIssueWarning = ibcs == null || ibcs.Length == 0;
                if(shouldIssueWarning) {
                    if(!issuedWarnings.Contains(go)) {
                        Debug.LogWarning($"Bootstrap scene has GameObject named \"{go.name}\" without a IBootstrapComponent on it, this isn't recommended. We can't really know when we're done bootstrapping.");
                        issuedWarnings.Add(go);
                    }
                    continue;
                }
                ibcs.ToList().ForEach(ibc => {
                    if(ibc is not MonoBehaviour)
                        return;
                    if(!cachedBootstrapComponents.Contains(ibc)) {
                        cachedBootstrapComponents.Add(ibc);
                        Debug.Log($"Cached bootstrapper component type \"{ibc.GetType()}\" on GameObject \"{(ibc as MonoBehaviour).name}\"");
                    }
                });
            }
        }

        private void UpdateComponentLoadProgress() 
        {
            IBCsStillLoading = 0;
            float totalLoadProgress = 0;
            foreach(IBootstrapComponent ibc in cachedBootstrapComponents) {
                totalLoadProgress += ibc.LoadProgress();
                if(!ibc.IsLoadingComplete())
                    IBCsStillLoading++;
            }

            LoadProgress = totalLoadProgress / cachedBootstrapComponents.Count;
        }

        public static List<Bootstrapper> FindBootstrappersInScene(Scene scene) 
        {
            if(!scene.IsValid())
                return null;
            List<Bootstrapper> bootstrappers = new();
            foreach(GameObject rootGO in scene.GetRootGameObjects()) {
                foreach(Bootstrapper bootstrapper in rootGO.GetComponentsInChildren<Bootstrapper>()) {
                    bootstrappers.Add(bootstrapper);
                }
            }
            return bootstrappers;
        }

    }

    public interface IBootstrapComponent 
    {
        public abstract bool IsLoadingComplete();
        public virtual float LoadProgress() => IsLoadingComplete() ? 1 : 0;
        // TODO Add behaviour for if something fails to bootstrap
    }
}