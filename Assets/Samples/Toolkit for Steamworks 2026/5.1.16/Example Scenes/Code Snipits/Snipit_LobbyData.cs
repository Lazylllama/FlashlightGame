//Anything that works with Steamworks should only compile if Steamworks is available
#if true || !DISABLESTEAMWORKS  && STEAMWORKSNET
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
    public class Snipit_LobbyData : MonoBehaviour
    {
        //This will make it so this code ONLY compiles in Unity Editor ... we do this to try and make sure you actually read and
        //DO NOT try to use this in a build as is ... it IS NOT meant to be copy and pasted its meant to be read like an article
        //A teach tool, not a working production ready but of code.
#if UNITY_EDITOR
        private void Start()
        {
            // You can check if the user is in a session lobby and get that lobby
            // with the following logic
            if (LobbyData.SessionLobby(out var sessionLobby))
            {
                Debug.Log($"The user is in a Session Lobby named {sessionLobby.Name}");
            }

            // You can similarly get the "Group" lobby if any
            if (LobbyData.GroupLobby(out var groupLobby))
            {
                Debug.Log($"The user is in a Group Lobby named {groupLobby.Name}");
            }

            // Don't understand what a session or group lobby is?
            // its just a lobby that you flagged as being "group" or "session"

            LobbyData someLobby = 123456789; // obviously not a real lobby ID
        }

        public void SearchForLobbies()
        {
            // First define your search arguments
            SearchArguments args = new();
            args.distance = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
            args.stringFilters.Add(new() { key = "SomeKey", value = "SomeValue", comparison = ELobbyComparison.k_ELobbyComparisonEqual });

            // Next use them ... in this case we use expression 
            LobbyData.Request(args, 1, (Lobbies, IOError) =>
            {
                // Lobbies is an array of lobbies that where found
                // IOError is true if there was some error
            });
            // Or you can use a named function
            LobbyData.Request(args, 1, HandleResults);
        }

        private void HandleResults(LobbyData[] Lobbies, bool IOError)
        {
            // Lobbies is an array of lobbies that where found
            // IOError is true if there was some error
        }

        public void JoinLobby(LobbyData Lobby)
        {
            // You can simply call Join an provide the callback as expression
            Lobby.Join((Result, IOError) =>
            {
                // Result.Lobby is the lobby joined if any
                // Result.Response the response message if any
                // Result.Locked is this lobby locked?
            });
            // Or feed it a named function
            Lobby.Join(HandleJoined);
        }

        private void HandleJoined(LobbyEnter Result, bool IOError)
        {
            // Result.Lobby is the lobby joined if any
            // Result.Response the response message if any
            // Result.Locked is this lobby locked?
        }

        public void LeaveLobby(LobbyData Lobby)
        {
            Lobby.Leave();
        }

        public void InviteToLobby(UserData User, LobbyData Lobby)
        {
            // From the user
            User.InviteToLobby(Lobby);
            // Or, from the Lobby
            Lobby.InviteUserToLobby(User);
        }

        public void MetadataUse(LobbyData Lobby)
        {
            // Lets set a simple field to a simple value
            Lobby["a simple field"] = "a simple value";

            // Okay that was easy, and now lets set that on our member
            LobbyMemberData Me = Lobby.Me;
            Me["a simple field"] = "a simple value";

            //Lets read the owner's metadata
            LobbyMemberData Owner = Lobby.Owner;
            Debug.Log($"Owner's field = {Owner["a simple field"]}");
        }

        public void LobbyGameServer(LobbyData Lobby)
        {
            // Setting the game server to the Lobby's owner
            Lobby.SetGameServer();

            // Lets set the game server to a specific ID
            CSteamID fakeServerID = new();
            Lobby.SetGameServer(fakeServerID);

            // And now lets set a server by IP:Port
            Lobby.SetGameServer("0.0.0.0", 7777);

            // And finally maybe we really try hard and use all of it
            Lobby.SetGameServer("0.0.0.0", 7777, fakeServerID);

            // You can read the current game server
            if (Lobby.HasServer)
            {
                LobbyGameServer server = Lobby.GameServer;

                CSteamID serverId = server.id;
                string ipAddress = server.IpAddress;
                ushort port = server.port;
            }

            // How to know when this was done?
            SteamTools.Events.OnLobbyGameServer += HandleGameServerSet;
        }

        private void HandleGameServerSet(LobbyData lobby, CSteamID serverId, string ip, ushort port)
        {
        }

        public void LobbyUserJoinOrLeave()
        {
            SteamTools.Events.OnLobbyChatUpdate += HandleChatUpdate;
        }

        private void HandleChatUpdate(LobbyData lobby, UserData user, EChatMemberStateChange state)
        {
            UserData who = user;
            LobbyData whatLobby = lobby;


            if(state == EChatMemberStateChange.k_EChatMemberStateChangeLeft)    
            {
                // The user left
            }
            else if (state== EChatMemberStateChange.k_EChatMemberStateChangeEntered)
            {
                // The user joined
            }
            else if (state == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
            {
                // The user lost connection
            }
        }

        public void LoopingLobbyMembers(LobbyData Lobby)
        {
            foreach(LobbyMemberData Member in Lobby.Members)
            {
                string MemberName = Member.user.Name;

                Member.user.LoadAvatar(AvatarTexture =>
                {
                    // AvatarTexture is now a Texture2D use it well
                });

                string ReadMetadataValues = Member["SomeKey"];
                
                if(Member.IsReady)
                {
                    // The member is ready!
                }
                else
                {
                    // The member is NOT ready!
                }
            }
        }
#endif
    }
}
#endif