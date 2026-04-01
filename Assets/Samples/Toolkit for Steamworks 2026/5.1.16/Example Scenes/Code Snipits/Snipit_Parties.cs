//Anything that works with Steamworks should only compile if Steamworks is available
#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEngine;

//You must "use" the Heathen namespace to access our tools
//In our examples we will also work with the API so we add its namespace as well
using Steamworks;
using Heathen.SteamworksIntegration.API;
//In some cases you also need the Steamworks namespace so you can work with its native enums and similar


//You must ALWAYS define your code in a properly formed namespace
//You can obviously name it whatever you want
namespace MyProperlyFormedNamespaceName
{
    [Obsolete("This script is for demonstration purposes only and should NEVER be used as is in any project.")]
    public class Snipit_Parties : MonoBehaviour
    {
#if UNITY_EDITOR
        public void CreateBeacon()
        {
            // Make sure Steam and the parties system are initialized.
            // Retrieve the list of available beacon locations.
            SteamPartyBeaconLocation_t[] availableLocations = Parties.Client.GetAvailableBeaconLocations();
            if (availableLocations == null || availableLocations.Length == 0)
            {
                Debug.LogWarning("No available beacon locations found.");
                return;
            }

            // Select a beacon location (using the first one for this example).
            SteamPartyBeaconLocation_t selectedLocation = availableLocations[0];

            // Set up the join string which provides connection details to joining players.
            string joinString = "JoinGameSessionToken123";

            // Set metadata (for example, game mode or map details).
            string metadata = "GameMode=Deathmatch;Map=Arena";

            // Set the number of open slots (e.g., if the host takes one slot and three open slots remain).
            uint openSlots = 3;

            // Create a beacon using your custom API.
            Parties.Client.CreateBeacon(openSlots, ref selectedLocation, joinString, metadata, OnCreateBeacon);
        }

        private void OnCreateBeacon(CreateBeaconCallback_t result, bool ioFailure)
        {
            if (ioFailure)
            {
                Debug.LogError("I/O Failure occurred while creating beacon.");
                return;
            }

            // Check if the creation was successful.
            if (result.m_eResult == EResult.k_EResultOK)
            {
                Debug.Log("Beacon created successfully. Beacon ID: " + result.m_ulBeaconID);
            }
            else
            {
                Debug.LogError("Failed to create beacon. Steam error: " + result.m_eResult);
            }
        }

        public void DisplayAvailableBeacons()
        {
            // Get the list of active beacon IDs.
            PartyBeaconID_t[] beaconIDs = Parties.Client.GetBeacons();
            if (beaconIDs == null || beaconIDs.Length == 0)
            {
                Debug.Log("No active beacons found.");
                return;
            }

            Debug.Log("Active beacons count: " + beaconIDs.Length);

            // Iterate over each beacon ID.
            foreach (PartyBeaconID_t beaconId in beaconIDs)
            {
                // Optionally, get details about the beacon.
                var beaconDetails = Parties.Client.GetBeaconDetails(beaconId);
                if (beaconDetails.HasValue)
                {
                    Debug.Log($"BeaconID: {beaconId} | Owner: {beaconDetails.Value.owner} | Metadata: {beaconDetails.Value.metadata}");
                }
                else
                {
                    Debug.Log($"BeaconID: {beaconId} - No details available.");
                }
            }
        }

        public void JoinParty()
        {
            //The beacon ID passed in on connect?
            PartyBeaconID_t beacon = new PartyBeaconID_t(123456789);
            Parties.Client.JoinParty(beacon, HandleJoinParty);
        }

        private void HandleJoinParty(JoinPartyCallback_t Callback, bool IOError)
        {
            // Callback.m_rgchConnectString the connection string to connect to
            // Callback.m_SteamIDBeaconOwner who your looking to connect to
            // Callback.m_eResult the EResult of the request to join ... should be OK
        }

        public void DestroyBeacon()
        {
            // Given a beacon such as
            PartyBeaconID_t beacon = new PartyBeaconID_t(123456789);

            // Then
            Parties.Client.DestroyBeacon(beacon);
        }
#endif
    }
}
#endif