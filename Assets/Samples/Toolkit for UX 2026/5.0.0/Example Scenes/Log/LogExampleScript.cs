using UnityEngine;

namespace Heathen.UX
{

    public class LogExampleScript : MonoBehaviour
    {
        public UnityEngine.UI.InputField logText;
        public string kbUrl;
        public string discordUrl;
        public string assetUrl;

        // Start is called before the first frame update
        void Start()
        {
            API.Log.Enabled = true;
            logText.text = API.Log.Text;
        }

        public void ToText()
        {
            logText.text = API.Log.Text;
        }

        public void ToJson()
        {
            logText.text = API.Log.Json;
        }

        public void OpenKb()
        {
            UnityEngine.Application.OpenURL(kbUrl);
        }

        public void OpenDiscord()
        {
            UnityEngine.Application.OpenURL(discordUrl);
        }

        public void OpenAsset()
        {
            UnityEngine.Application.OpenURL(assetUrl);
        }
    }
}