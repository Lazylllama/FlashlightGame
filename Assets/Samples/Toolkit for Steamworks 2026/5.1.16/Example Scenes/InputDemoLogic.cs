#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using Steamworks;
using System.Text;
using UnityEngine;

namespace Heathen.DEMO
{
    public class InputDemoLogic : MonoBehaviour
    {
        [Header("UI References")]
        public TMPro.TextMeshProUGUI label;

        private void Start()
        {
            if (SteamTools.Interface.IsReady)
                ActivateShipControls();
            else
                SteamTools.Interface.OnReady += ActivateShipControls;
        }

        public void UpdateDisplay(InputControllerStateData state)
        {
            StringBuilder sb = new();
            foreach(var inputState in state.inputs)
            {
                sb.Append($"{inputState.name}: {inputState}");
            }
            label.text = sb.ToString();
        }

        public void ActivateMenuControls()
        {
            if (SteamInputManager.Controllers != null && SteamInputManager.Controllers.Count > 0)
                SteamTools.Interface.GetSet("menu_controls").Activate(SteamInputManager.Controllers[0]);
        }

        public void ActivateShipControls()
        {
            if (SteamInputManager.Controllers != null && SteamInputManager.Controllers.Count > 0)
                SteamTools.Interface.GetSet("ship_controls").Activate(SteamInputManager.Controllers[0]);
        }

        public void OpenKnowledgeBaseUserData()
        {
            Application.OpenURL("https://kb.heathen.group/steam/input");
        }
    }
}
#endif