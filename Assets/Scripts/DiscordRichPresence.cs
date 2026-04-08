using System;
using Discord;
using FlashlightGame;
using UnityEngine;

public class DiscordRichPresence : MonoBehaviour {
	#region Fields

	private static DebugHandler Debug;

	private Discord.Discord discord;

	private bool isInitialized;

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("DiscordRichPresence");
	}

	private void Awake() {
		Debug = new DebugHandler("DiscordRichPresence");
	}

	private void Start() {
		if (isInitialized) return;

		try {
			discord = new Discord.Discord(1488935054846722280, (ulong)CreateFlags.NoRequireDiscord);
		} catch (Exception e) {
			Debug.LogWarning($"Failed to initialize Discord Rich Presence. {e.Message}");
		} finally {
			if (discord != null) {
				isInitialized = true;
				Debug.Log($"Recognized running Discord Client", DebugLevel.Info);
			}
		}

		UpdateActivity();
	}

	private void Update() {
		if (!isInitialized) return;
		discord?.RunCallbacks();
	}

	private void OnDisable() {
		if (!isInitialized) return;
		discord.Dispose();
	}

	#endregion

	#region Functions

	private void UpdateActivity() {
		if (!isInitialized) return;
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