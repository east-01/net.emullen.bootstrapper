using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using EMullen.Core;

namespace EMullen.Bootstrapper 
{
    /// <summary>
    /// A bootstrap sequence dictates what the overarching bootstrap process will look like.
    /// It is a singleton and only one BootstrapSequence can be running at a time.
    /// The hierarchy has two layers:
    ///                   BootstrapSequence
    ///              /        /        \      \    
    ///       /             /            \          \
    /// <GameScenes>  Bootstrapper Bootstrapper GameScenes
    /// 
    /// 1. The first set of GameScenes are optional, if we do have them though they will be
    ///   cached to be loaded at the end of the sequence
    /// </summary>
    public class BootstrapSequenceManager {

        private readonly string SEQ_ERR = "Sequence error:";
        private readonly string SEQ_WRN = "Sequence warning:";
        private readonly string INIT_ERR = "Can't initialize bootstrap sequence:";

        public static BootstrapSequenceManager Instance { get; private set; }
        /// <summary>
        /// The currently running sequence used by the BootstrapSequenceManager.
        /// It needs to be seperate because there are causes where the active sequence can be
        ///   loaded by a bootstrapper, but not executed by said bootstrapper (starting scenee
        ///   isn't correct in sequence).
        /// </summary>
        public static BootstrapSequence? ActiveSequence { get; private set; }
        public static Queue<BootstrapSequence> SequenceHistory { get; private set; } = new();
        private static List<int> blacklistedBootstrapScenes = new();
        public static bool IsBlacklistedBootstrapScene(int sceneIndex) => blacklistedBootstrapScenes.Contains(sceneIndex);

        public Bootstrapper CurrentBootstrapper { get; private set; }

        public int SequencePosition { get; private set; } = 0;

        public bool LoadingTargetScenes { get; private set; }= false;
        private List<int> targetScenesStillLoading = new();
        
        /// <summary>
        /// Cleanup scenes from other bootstrap sequences to be cleared by a new one.
        /// </summary>
        public static List<int> cleanupScenes = new();

        public BootstrapSequenceManager(BootstrapSequence sequence, out bool initStatus) 
        {
            if(Instance != null) {
                Debug.LogError($"{INIT_ERR} there is already another sequence running.");
                initStatus = false;
                return;
            }

            if(ActiveSequence == null) {
                if(!sequence.IsSequenceValid(out string reason)) {
                    if(reason == BootstrapSequence.INVLD_SEQ_NO_NECESSARY_BOOTSTRAP)
                        Debug.LogWarning($"Skipped bootstrap sequence since all bootstrappers were marked as run only once.");
                    else
                        Debug.LogError($"{INIT_ERR} {reason}");
                    initStatus = false;
                    return;
                }

                BLog.Highlight($"Initiating new sequence");
                ActiveSequence = sequence;
                SequenceHistory.Enqueue(sequence);
            }

            // Load up the proper starting bootstrap scene if this isn't the correct one.
            if(SceneManager.GetActiveScene().buildIndex != ActiveSequence.Value.bootstrapScenes[0]) {
                initStatus = false;
                ActiveSequence = sequence;
                LoadScene(sequence.bootstrapScenes[0], LoadSceneMode.Single);
                Debug.Log("Loading first bootstrap scene");
                return;
            }

            // if(sequence.overrideTargetScenesWithOpenScenes) {
            //     sequence.targetScenes.Clear();
            //     List<int> targetScenes = new();
            //     // Store all scenes that are currently loaded so we can re-load them when the actual bootstrap scene gets loaded
            //     LoadedScenes.ForEach(ls => sequence.targetScenes.Add(ls.buildIndex));
            // }            
            
            SequencePosition = 0;

            Instance = this;

            Debug.Log("#####################################");
            Debug.Log("###### Beginning bootstrapping ######");
            Debug.Log("###### Bootstrappers:          ######");
            ActiveSequence.Value.bootstrapScenes.ForEach(bootstrapSceneIndex => Debug.Log("###### " + bootstrapSceneIndex + "      ######"));
            Debug.Log("###### Targets:                ######");
            ActiveSequence.Value.targetScenes.ForEach(targetSceneIndex => Debug.Log("###### " + targetSceneIndex + "      ######"));
            Debug.Log("#####################################");

            SceneManager.sceneLoaded += SceneManager_SceneLoaded;

            initStatus = true;
        }

        ~BootstrapSequenceManager() 
        {
            SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
        }

        /// <summary>
        /// Called by bootstrappers when they wake up, checks if the bootstrapper is waking up at
        ///   the right time in the bootstrap sequence.
        /// The returned boolean indicates if the bootstrapper was successfully registered.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper that is being registered.</param>
        public bool RegisterBoostrapper(Bootstrapper bootstrapper) 
        {
            if(CurrentBootstrapper != null) {
                SequenceError("A bootstrapper tried to load even though the current one hasn't finished loading.");
                return false;
            }
            if(SequencePosition >= ActiveSequence.Value.bootstrapScenes.Count) {
                SequenceError("A bootstrapper registered even though we're at the end of the bootstrap sequence.");
                return false;
            }
            if(bootstrapper.gameObject.scene.buildIndex != ActiveSequence.Value.bootstrapScenes[SequencePosition]) {
                SequenceError("The bootstrapper that was loaded wasn't expected to be loaded. "
                + $"Expected bootstrap scene \"{BuildIndexToName(ActiveSequence.Value.bootstrapScenes[SequencePosition+1])}\" but \"{BuildIndexToName(bootstrapper.gameObject.scene.buildIndex)}\" loaded instead.");
                return false;
            }

            if(bootstrapper.OnlyBootstrapOnce)
                blacklistedBootstrapScenes.Add(bootstrapper.gameObject.scene.buildIndex);

            Debug.Log($"###### Registered bootstrapper for scene \"{bootstrapper.gameObject.scene.name}\" ######");

            CurrentBootstrapper = bootstrapper;

            return true;
        }

        /// <summary>
        /// Called by bootstrappers when they've finished loading all of their IBoostrapperComponents.
        /// </summary>
        /// <param name="bootstrapper"></param>
        public void BootstrapperCompleted(Bootstrapper bootstrapper) 
        {
            if(bootstrapper != CurrentBootstrapper) {
                SequenceError("A bootstrapper called complete even though it's not the current bootstrapper.");
                return;
            }

            Debug.Log($"###### Completed bootstrapping in scene \"{bootstrapper.gameObject.scene.name}\" ######");

            CurrentBootstrapper = null;

            MoveToNextBootstrapper();

            void MoveToNextBootstrapper() {
                if(SequencePosition < ActiveSequence.Value.bootstrapScenes.Count-1) {
                    Debug.Log($"###### Completed bootstrapping, moving on to sequence position {SequencePosition}");

                    SequencePosition++;
                    int nextSceneBuildIndex = ActiveSequence.Value.bootstrapScenes[SequencePosition];
                    if(IsBlacklistedBootstrapScene(nextSceneBuildIndex)) {
                        SequenceWarning($"The bootstrapper in scene \"{bootstrapper.gameObject.scene.name}\" is set to only bootstrap once. Skipping.");
                        MoveToNextBootstrapper();
                        return;
                    }

                    LoadScene(nextSceneBuildIndex, LoadSceneMode.Single);
                } else {
                    FinishBootstrapping();
                }

                // hidden return warning: There is a return statement in the top if statement, code here may not always run
            }
        }

        public void FinishBootstrapping(bool aborted = false) {
            Debug.Log("#####################################");
            Debug.Log("###### Finishing bootstrapping ######");
            Debug.Log("#####################################");

            targetScenesStillLoading.Clear();

            // If the bootstrap sequence was aborted we need to make sure it's static instances
            //   are cleared. This is also done after all of the target scenes are loaded.
            if(aborted || ActiveSequence.Value.targetScenes.Count == 0) {
                Instance = null;
                ActiveSequence = null;
                UnloadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            LoadingTargetScenes = true;

            if(ActiveSequence.Value.targetScenes.Count > 1) {
                UnloadScene(SceneManager.GetActiveScene().buildIndex);
            }

            foreach(int targetSceneIndex in ActiveSequence.Value.targetScenes) {
                targetScenesStillLoading.Add(targetSceneIndex);
                LoadScene(targetSceneIndex, ActiveSequence.Value.targetScenes.Count > 1 ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }
        }

        /// <summary>
        /// Used by bootstrappers to exit the sequence prematurely.
        /// An example of this is networked applications- a networking bootstrapper will join the
        ///   lobby and then the server will handle scene loading, this means the sequence must be
        ///   aborted.
        /// </summary>
        public void AbortSequence(string reason = null) 
        {
            if(reason != null)
                Debug.LogWarning("Aborted bootstrap sequence for reason: " + reason);

            FinishBootstrapping(true);
        }

        private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode loadSceneMode) 
        {
            if(targetScenesStillLoading.Contains(scene.buildIndex)) {
                targetScenesStillLoading.Remove(scene.buildIndex);

                if(targetScenesStillLoading.Count == 0) {
                    Instance = null;
                    ActiveSequence = null;
                }
            }
        }
        
        public void SequenceError(string message) 
        {
            Debug.LogError($"{SEQ_ERR} {message}");
            Application.Quit();
        }

        public void SequenceWarning(string message) 
        {
            Debug.LogWarning($"{SEQ_WRN} {message}");
        }

        private void LoadScene(int buildIndex, LoadSceneMode lsm) 
        {
            SceneManager.LoadScene(BuildIndexToName(buildIndex), lsm);
        }

        private void UnloadScene(int buildindex) 
        {
            if(LoadedScenes.Count > 1)
                SceneManager.UnloadSceneAsync(buildindex);
            else
                cleanupScenes.Add(buildindex);
        }

        public static string BuildIndexToName(int buildIndex) {
            if(buildIndex < 0 || buildIndex >= BuildProcessor.Scenes.Length) {
                Debug.LogError($"BuildIndexToName Error: Build index \"{buildIndex}\" out of bounds.");
                return null;
            }
            
            return BuildProcessor.Scenes[buildIndex].path;
        }

        public static List<Scene> LoadedScenes { get { 
            List<Scene> loadedScenes = new();
            BuildSettingsScene[] s = BuildProcessor.Scenes;
            s.ToList().ForEach(s => {
                Scene scene = SceneManager.GetSceneByPath(s.path);
                if(scene.isLoaded)
                    loadedScenes.Add(scene);
            });
            return loadedScenes;
        } }

    }

    [Serializable]
    public struct BootstrapSequence {
        public List<int> bootstrapScenes;
        public bool overrideTargetScenesWithOpenScenes;
        public List<int> targetScenes;
        public int targetSceneToSetActive;

        public const string INVLD_SEQ_NO_BOOTSTRAP = "there are no bootstrap scenes.";
        public const string INVLD_SEQ_NO_TARGET = "there are no target scenes.";
        public const string INVLD_SEQ_NO_NECESSARY_BOOTSTRAP = "all bootstrap scenes are marked as only bootstrap once, no scenes to run.";

        /// <summary>
        /// Check if a sequence is valid. Checks that there are at least 1 bootstrap and target
        ///   scene, and that each scene's bootstrapper is valid for it's scene type.
        /// </summary>
        /// <param name="reason">The reason why the sequence is not valid. Empty if the sequence 
        ///   is valid</param>
        /// <returns>Is the sequence valid?</returns>
        public bool IsSequenceValid(out string reason) {
            if(BootstrapSequenceManager.SequenceHistory.Count > 0 && BootstrapSequenceManager.SequenceHistory.Peek().Equals(this)) {
                reason = "Previous sequence matches.";
                return false;
            }
            if(bootstrapScenes == null || bootstrapScenes.Count == 0) {
                reason = INVLD_SEQ_NO_BOOTSTRAP;
                return false;
            }
            // if(targetScenes == null || targetScenes.Count == 0) {
            //     reason = INVLD_SEQ_NO_TARGET;
            //     return false;
            // }
            bool willAnyBootstrapsRun = false;
            foreach(int bootstrapSceneBuildIndex in bootstrapScenes) {
                if(!BootstrapSequenceManager.IsBlacklistedBootstrapScene(bootstrapSceneBuildIndex)) {
                    willAnyBootstrapsRun = true;
                    break;
                }
            }
            if(!willAnyBootstrapsRun) {
                reason = INVLD_SEQ_NO_NECESSARY_BOOTSTRAP;
                return false;
            }

            reason = "";
            return true;
        }

        public override bool Equals(object obj)
        {
            if(obj is BootstrapSequence toCompare)
                return bootstrapScenes.SequenceEqual(toCompare.bootstrapScenes) && targetScenes.SequenceEqual(toCompare.targetScenes);
            return false;
        }

        public override readonly int GetHashCode()
        {
            unchecked {
                int hash = 17;
                hash = hash * 23 + (bootstrapScenes?.GetHashCode() ?? 0);
                hash = hash * 23 + (targetScenes?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}