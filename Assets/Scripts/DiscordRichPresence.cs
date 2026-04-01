using System;
using Discord;
using UnityEngine;

public class DiscordRichPresence : MonoBehaviour {
	#region Fields

	private static DebugHandler Debug;

	private Discord.Discord discord;

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("DiscordRichPresence");
	}

	private void Start() {
		discord = new Discord.Discord(1488935054846722280, (ulong)Discord.CreateFlags.NoRequireDiscord);
		UpdateActivity();
	}

	private void Update() {
		discord.RunCallbacks();
	}

	private void OnDisable() => discord.Dispose();

	#endregion

	#region Functions

	private void UpdateActivity() {
		var activityManager = discord.GetActivityManager();
		var activity = new Discord.Activity {
			State   = "Scouting",
			Details = "Exploring the world",
		};

		activityManager.UpdateActivity(activity, HandleUpdateCallback);
	}

	private static void HandleUpdateCallback(Result result) {
		if (result == Discord.Result.Ok) {
			Debug.Log("Activity updated successfully.");
		} else {
			Debug.LogError($"Failed to update activity: {result}");
		}
	}

	#endregion
}