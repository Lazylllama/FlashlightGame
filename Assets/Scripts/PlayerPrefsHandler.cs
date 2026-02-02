using System;
using System.Linq;
using FlashlightGame;
using UnityEngine;

//! DOCUMENTATION: https://github.com/richardelms/FileBasedPlayerPrefs?tab=readme-ov-file#api

public class PlayerPrefsHandler : MonoBehaviour {
	#region Unity Functions

	private void Awake() {
		var config = new FBPPConfig() {
			SaveFileName     = "preferences.cfg",
			AutoSaveData     = false,
			ScrambleSaveData = false,
			SaveFilePath     = Application.persistentDataPath,
			OnLoadError      = () => DebugHandler.Log("Error loading FBPP data.", DebugLevel.Fatal)
		};

		FBPP.Start(config);

		// Load preferences
		Application.targetFrameRate = FBPP.GetInt("TargetFrameRate", 60);

		DebugHandler.DbgLevel = FBPP.GetString("DebugLevel", "Error") switch {
			"None"    => DebugLevel.None,
			"Fatal"   => DebugLevel.Fatal,
			"Error"   => DebugLevel.Error,
			"Warning" => DebugLevel.Warning,
			"Info"    => DebugLevel.Info,
			"Debug"   => DebugLevel.Debug,
			_         => DebugLevel.Error
		};

		DebugHandler.LogFilter = FBPP.GetString("LogFilter")
		                             .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
		                             .Select(s => s.Trim())
		                             .ToList();
	}

	private void Start() {
		DebugHandler.Log("PlayerPrefsHandler initialized.");
		CheckMissingKeysAndSave();
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
		if (!FBPP.HasKey("TargetFrameRate")) {
			FBPP.SetInt("TargetFrameRate", 60);
		}

		if (!FBPP.HasKey("DebugLevel")) {
			FBPP.SetString("DebugLevel", "Error");
		}

		if (!FBPP.HasKey("LogFilter")) {
			FBPP.SetString("LogFilter", "");
		}

		SavePreferences();
	}

	#endregion
}