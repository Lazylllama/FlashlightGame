using UnityEngine;

namespace Heathen.UX
{
    public class ScreenshotExampleScript : MonoBehaviour
    {
        public Vector2Int rectStart = new(400,400);
        public Vector2Int rectEnd = new(592,508);
        public GameObject imageContainer;
        public UnityEngine.UI.RawImage screenShotImage;
        public UnityEngine.UI.RawImage screenShotImage2;
        public string kbUrl;
        public string discordUrl;
        public string assetUrl;

        // Start is called before the first frame update
        void Start()
        {
            API.Screenshot.Capture(this, (texture, failure) =>
            {
                if (!failure)
                {
                    screenShotImage.texture = texture;
                }

                API.Screenshot.Capture(this, rectStart, rectEnd, (texture, failure) =>
                {
                    if (!failure)
                    {
                        screenShotImage2.texture = texture;
                        imageContainer.SetActive(true);
                    }
                });
            });

            
        }

        private void OnDestroy()
        {
            //You need to clean up screenshot textures when your down with them
            if (screenShotImage.texture != null)
                Destroy(screenShotImage.texture);
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