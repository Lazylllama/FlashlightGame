using System;
using System.Collections.Generic;
using System.Linq;
using FlashlightGame;
using UnityEngine;

//! DOCUMENTATION: https://github.com/richardelms/FileBasedPlayerPrefs?tab=readme-ov-file#api

public class PlayerPrefsHandler : MonoBehaviour {
	#region Fields

	private static DebugHandler Debug;
	private static Dictionary<string, object> configurationSchema = new Dictionary<string, object>() {
		{
			"TargetFrameRate", 60
		}, {
			"DebugLevel", "Warning"
		}, {
			"LogFilter", ""
		}
	};

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("PlayerPrefsHandler");

		var config = new FBPPConfig() {
			SaveFileName     = "preferences.cfg",
			AutoSaveData     = false,
			ScrambleSaveData = false,
			SaveFilePath     = Application.persistentDataPath,
			OnLoadError      = () => Debug.Log("Error loading FBPP data.", DebugLevel.Fatal)
		};

		FBPP.Start(config);

		LoadPreferences();
	}

	private void Start() {
		CheckMissingKeysAndSave();

		Debug.LogKv("DebugInformation:", DebugLevel.Info, new object[] {
			"isEditor", Application.isEditor,
			"isProduction", Application.version,
		});
	}

	#endregion

	#region Functions

	/// <summary>
	/// Save player preferences to file.
	/// </summary>
	public void SavePreferences() {
		FBPP.Save();
	}

	/// <summary>
	/// Reset player preferences by deleting all saved data then saving.
	/// </summary>
	public void ResetPreferences() {
		FBPP.DeleteAll();
		CheckMissingKeysAndSave();
	}

	private void CheckMissingKeysAndSave() {
		Debug.Log("Checking for missing keys");
		foreach (var kvp in configurationSchema) {
			if (FBPP.HasKey(kvp.Key)) return;

			Debug.LogKv($"Key '{kvp.Key}' is missing, attempting to add now.",
			            DebugLevel.Warning, new object[] {
				            "Key", kvp.Key,
				            "DefaultValue", kvp.Value
			            });

			switch (kvp.Value) {
				case int val:
					FBPP.SetInt(kvp.Key, val);
					break;
				case string val:
					FBPP.SetString(kvp.Key, val);
					break;
				case bool val:
					FBPP.SetBool(kvp.Key, val);
					break;
				case float val:
					FBPP.SetFloat(kvp.Key, val);
					break;
			}
		}

		Debug.Log("Finished checking for missing keys, saving.");
		SavePreferences();
	}

	private void LoadPreferences() {
		//? Framerate Soft-cap/Target
		Preferences.Game.TargetFrameRate = FBPP.GetInt("TargetFrameRate", 60);
		Application.targetFrameRate      = Preferences.Game.TargetFrameRate;

		//? DebugHandler Level & Filter
		Preferences.DebugHandler.DbgLevel = FBPP.GetString("DebugLevel", "Error") switch {
			"None"    => DebugLevel.None,
			"Fatal"   => DebugLevel.Fatal,
			"Error"   => DebugLevel.Error,
			"Warning" => DebugLevel.Warning,
			"Info"    => DebugLevel.Info,
			"Debug"   => DebugLevel.Debug,
			_         => DebugLevel.Error
		};

		Preferences.DebugHandler.LogFilter = FBPP.GetString("LogFilter")
		                                         .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
		                                         .Select(s => s.Trim())
		                                         .ToList();
	}

	#endregion
}