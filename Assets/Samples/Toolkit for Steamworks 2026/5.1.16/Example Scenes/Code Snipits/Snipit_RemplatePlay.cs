//Anything that works with Steamworks should only compile if Steamworks is available
#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEngine;

//You must "use" the Heathen namespace to access our tools
using Heathen.SteamworksIntegration;
//In our examples we will also work with the API so we add its namespace as well
using Steamworks;
using Heathen.SteamworksIntegration.API;
//In some cases you also need the Steamworks namespace so you can work with its native enums and similar


//You must ALWAYS define your code in a properly formed namespace
//You can obviously name it whatever you want
namespace MyProperlyFormedNamespaceName
{
    [Obsolete("This script is for demonstration purposes only and should NEVER be used as is in any project.")]
    public class Snipit_RemplatePlay : MonoBehaviour
    {
#if UNITY_EDITOR
        // The session of a connected player
        public RemotePlaySessionID_t session;

        public void InvitePlayer(UserData User)
        {
            // Be ready to listen when they accept
            RemotePlay.Client.OnSessionConnected.AddListener(HandleSessionConnected);

            // To invite a player
            if (RemotePlay.Client.SendInvite(User))
                Debug.Log("Done");
            else
                Debug.Log("Send failed");
        }

        private void HandleSessionConnected(SteamRemotePlaySessionConnected_t Callback)
        {
            // You should store this somewhere as you will use it later.
            session = Callback.m_unSessionID;

            // Get the UserData for the connected user
            UserData user = RemotePlay.Client.GetSessionUser(session);

            // Get the name of the device the session is playing on
            string clientName = RemotePlay.Client.GetSessionClientName(session);

            // Get the form factor the session is playing in
            ESteamDeviceFormFactor formFactor = RemotePlay.Client.GetSessionClientFormFactor(session);

            //Get the resolution the session is playing at
            Vector2Int resolution = RemotePlay.Client.GetSessionClientResolution(session);
        }
#endif
    }
}
#endif