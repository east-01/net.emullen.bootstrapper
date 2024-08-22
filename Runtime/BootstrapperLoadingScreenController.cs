using UnityEngine;
using TMPro;

namespace EMullen.Bootstrapper 
{
    public class BootstrapperLoadingScreenController : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text loadingText;

        private void Update() 
        {
            if(BootstrapSequenceManager.Instance == null || BootstrapSequenceManager.Instance.CurrentBootstrapper == null)
                return;

            string text = "Loading: " + Mathf.RoundToInt(BootstrapSequenceManager.Instance.CurrentBootstrapper.LoadProgress*100) + "%";
            loadingText.text = text;
        }
    }
}