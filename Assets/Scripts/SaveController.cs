using System;
using System.IO;
using UnityEngine;
using FlashlightGame;

public class SaveController : MonoBehaviour {
	#region Fields

	public static  SaveController Instance;
	private static DebugHandler   Debug;

	//* Consts *//
	private const int CurrentVersion = 1;

	//* Settings *//
	private string saveFilePath;

	//* Refs *//
	private GameObject playerObj;

	#endregion

	#region Unity Functions

	private void Awake() {
		RegisterInstance(this);

		Debug = new DebugHandler("SaveController");

		//! DO NOT EDIT PATH - STEAM CLOUD SYNC RELIES ON THIS
		saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.dat");
	}

	#endregion

	#region Functions

	public static void RegisterPlayer(GameObject player) {
		Instance.playerObj = player;
	}

	private GameObject CachePlayer() {
		if (!playerObj) playerObj = GameObject.FindGameObjectWithTag("Player");
		return playerObj;
	}

	public SaveData GetSaveData() {
		if (!File.Exists(saveFilePath)) return null;

		try {
			var json = Lib.AES.Decrypt(File.ReadAllText(saveFilePath));
			return JsonUtility.FromJson<SaveData>(json);
		} catch {
			return null;
		}
	}

	/// <summary>
	/// You probably don't want to call this. Try UIController.Instance.SaveGame() instead...
	/// </summary>
	public void SaveGameBackend() {
		if (!AreRequiredInstancesReady()) {
			Debug.LogError("Cannot save game one or more required scripts are missing!");
			return;
		}

		PlayerData.Instance.CheckpointPosition = playerObj.transform.position;

		var saveData = new SaveData {
			version = CurrentVersion,

			checkpointPosition = PlayerData.Instance.CheckpointPosition,
			health             = PlayerData.Instance.Health,
			battery            = PlayerData.Instance.Battery,
			isLookingRight     = PlayerData.Instance.IsLookingRight,
			lastSavedTicks     = DateTime.Now.Ticks // For SaveControllerUI to display last save time yes
		};

		try {
			File.WriteAllText(saveFilePath, Lib.AES.Encrypt(JsonUtility.ToJson(saveData)));

			Debug.Log("Game saved!");
		} catch (Exception e) {
			Debug.LogError($"Failed to save game: {e.Message}");
		}
	}


	public bool LoadGame() {
		if (!File.Exists(saveFilePath)) {
			Debug.Log("No save file found!", DebugLevel.Info);
			return false;
		}

		if (!AreRequiredInstancesReady()) {
			Debug.LogError("Cannot load game one or more required scripts are missing!");
			return false;
		}

		try {
			var json = Lib.AES.Decrypt(File.ReadAllText(saveFilePath));

			if (string.IsNullOrEmpty(json)) {
				Debug.LogError("Save file is empty!");
				return false;
			}

			var saveData = JsonUtility.FromJson<SaveData>(json);

			if (saveData == null) {
				Debug.LogError("Failed to find save data!");
				return false;
			}

			if (saveData.version > CurrentVersion) {
				Debug.LogWarning("Save is from a newer version, cannot load."); //  We lowkey have no plans to update the save data format but this is here just in case
				return false;
			}

			if (saveData.version < CurrentVersion) {
				Debug.LogWarning("Old save version, cannot load."); // This too.
				return false;
			}


			PlayerData.Instance.Health         = saveData.health;
			PlayerData.Instance.Battery        = saveData.battery;
			PlayerData.Instance.IsLookingRight = saveData.isLookingRight;
			CachePlayer().transform.position   = saveData.checkpointPosition;
			return true;
		} catch (Exception e) {
			Debug.LogError($"Error loading save file: {e.Message}");
			return false;
		}
	}


	public string
		GetSaveFilePath() // Used by SaveControllerUI to check if save file exists and display last save time yeee
	{
		return saveFilePath;
	}


	private static void RegisterInstance(SaveController instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;
			DontDestroyOnLoad(instance.gameObject);
		}
	}

	public void DeleteSave() {
		if (File.Exists(saveFilePath)) {
			File.Delete(saveFilePath);
			Debug.Log("Save file deleted.");
		} else {
			Debug.Log("No save file to delete.");
		}
	}

	private bool AreRequiredInstancesReady() {
		var allGood = true;

		if (!CachePlayer()) {
			Debug.LogWarning("Player object not assigned!");
			allGood = false;
		}

		if (!PlayerData.Instance) {
			Debug.LogWarning("PlayerData instance not found!");
			allGood = false;
		}

		return allGood;
	}

	#endregion
}