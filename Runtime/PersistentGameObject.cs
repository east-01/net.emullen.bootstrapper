using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMullen.Bootstrapper
{
    public class PersistentGameObject : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
