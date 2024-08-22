using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMullen.Core
{
    /// <summary>
    /// An example IBootstrapComponent class that takes an arbitrary amount of time to ready up
    ///   in the bootstrap menu.
    /// </summary>
    public class TimeConsumer : MonoBehaviour, IBootstrapComponent
    {
        [SerializeField]
        private float timeToConsume = 5f;

        private float timeStartedAt;

        private void Awake() 
        {
            timeStartedAt = Time.time;
            Debug.Log("Set time started at as " + timeStartedAt);
        }

        public bool IsLoadingComplete() => Time.time >= (timeStartedAt + timeToConsume);
        public float LoadProgress() => Mathf.Clamp01((Time.time - timeStartedAt) / timeToConsume);

    }
}
