//Anything that works with Steamworks should only compile if Steamworks is available
#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEngine;

//You must "use" the Heathen namespace to access our tools
using Heathen.SteamworksIntegration;
//In our examples we will also work with the API so we add its namespace as well
using Heathen.SteamworksIntegration.API;
using Steamworks;
//In some cases you also need the Steamworks namespace so you can work with its native enums and similar


//You must ALWAYS define your code in a properly formed namespace
//You can obviously name it whatever you want
namespace MyProperlyFormedNamespaceName
{


    [Obsolete("This script is for demonstration purposes only and should NEVER be used as is in any project.")]
    public class Snipit_AchievementData : MonoBehaviour
    {
#if UNITY_EDITOR
        AchievementData myAch;

        private void Start()
        {
            // Manually identify an achievement
            myAch = "Your Achievement API Name";

            // If you have generated your wrapper then you can access the achievements there as well
            // SteamTools.Game.Achievements.YourAchivementName
        }

        public void Example_ReadAchievement()
        {
            //Is this achievement achieved?
            var isAchieved = myAch.IsAchieved;

            //When was this unlocked
            var dateTime = myAch.UnlockTime;

            //What is the icon for this achievement
            //This is an asynchronous call so its parameter is a delegate
            //that will be ran when the call is completed
            myAch.GetIcon(texture2D =>
            {
                //texture2D is a UnityEngine.Texture2D that can be used as needed
                //This will be the icon the user sees e.g. if the achievement is unlocked
                //this will be the unlocked version
                //if the achievement is locked it will be the locked version
                //If you unlock the achievement or if its status changes you can get the icon again
                //to get the new icon version
            });
        }

        public void Example_ReadAchievementForUser(UserData user)
        {
            //Before we can read other user's data we have to ask Steam to cash it
            var requestedData = user.RequestInformation();
            //if the resulting bool is true, that means the request has been sent and we should wait for 
            //Person State Change to let us know when its ready
            // ... This is not a functional event listener ... just highlighting the event you would use
            Friends.Client.OnPersonaStateChange.AddListener(null /*Some Function Name*/);
            //The parameters on this event tell us who the change is about so we can know if we now have this users data


            //You can read the achievements of other uses once you have there data
            var achState = myAch.GetAchievementAndUnlockTime(user);
            //The return is a tuple that defines the unlocked
            var isAchieved = achState.unlocked;
            //and unlock time
            var dateTime = achState.unlockTime;
        }

        public void Example_SetAchievement()
        {
            myAch.IsAchieved = true;
            // or
            myAch.Unlock();
        }
#endif
    }

    [Obsolete("This script is for demonstration purposes only and should NEVER be used as is in any project.")]
    public class Snipit_StatData : MonoBehaviour
    {
#if UNITY_EDITOR
        public void GetStat()
        {
            // Identify which stat
            StatData statData = "API_NameHere"; // Use the API name you created in the Steamworks Developer Portal

            // Get the value as a float
            float FloatValue = statData.FloatValue();

            // Get the value as an int
            int IntValue = statData.IntValue();

            // Steam Game Servers can read stats for users
            CSteamID userId = new(); // This is just a standing for the ID of the user you want to read for

            // First the server must request the user's stats, this can only be done after authentication
            StatsAndAchievements.Server.RequestUserStats(userId, HandleStatsReceived);

            // When the HandleStatsReceived has completed you can then read the stat values 
            // Get the value as a float
            StatsAndAchievements.Server.GetUserStat(userId, "API_NameHere", out float floatValue);

            // Get the value as an int
            StatsAndAchievements.Server.GetUserStat(userId, "API_NameHere", out int intValue);
        }

        private void HandleStatsReceived(GSStatsReceived_t results, bool arg2)
        {
            // results.m_eResult The EResult of the request
            // results.m_steamIDUser Which user you got the results for
        }

        public void SetStat()
        {
            // Identify which stat
            StatData statData = "API_NameHere"; // Use the API name you created in the Steamworks Developer Portal

            // Set has an int and a float overload
            statData.Set(42);
            // or
            statData.Set(42.0f);

            // Servers that have authenticated a user can request
            // that user's stats and when they have those stats they can update them
            CSteamID userId = new(); // This is just a standing for the ID of the user you want to read for
            StatsAndAchievements.Server.RequestUserStats(userId, HandleStatsReceived);

            StatsAndAchievements.Server.SetUserStat(userId, "API_NameHere", 42);
            // or
            StatsAndAchievements.Server.SetUserStat(userId, "API_NameHere", 42.0f);
        }

        public void UpdateAVRATE()
        {
            // Identify which stat
            StatData statData = "API_NameHere"; // Use the API name you created in the Steamworks Developer Portal

            // The Set overload that takes a value and a length is how you update average stats
            float value = 42f;
            double length = 14;
            statData.Set(value, length);
        }
#endif
    }
}
#endif