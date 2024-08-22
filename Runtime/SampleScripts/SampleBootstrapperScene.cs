using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EMullen.Core
{
    public class SampleBootstrapperScene : MonoBehaviour
    {
        
        [SerializeField]
        private TMP_Text sceneIndexText;

        private void Awake() 
        {
            if(sceneIndexText != null)
                sceneIndexText.text = SceneManager.GetActiveScene().buildIndex.ToString();
        }

        public void ReloadScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
}
