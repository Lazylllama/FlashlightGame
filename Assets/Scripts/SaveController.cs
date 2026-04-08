using System;
using System.IO;
using UnityEngine;
using FlashlightGame;

[System.Serializable]
public class SaveData {
	public int version;

	public int     health;
	public int     battery;
	public bool    isLookingRight;
	public Vector2 checkpointPosition;
	public long    lastSavedTicks;
}

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

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("SaveController");
	}

	private void Awake() {
		RegisterInstance(this);

		Debug ??= new DebugHandler("SaveController");

		//! DO NOT EDIT PATH - STEAM CLOUD SYNC RELIES ON THIS
		//TODO: Use cloud saves properly dumbahh
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
			#if UNITY_EDITOR

			try {
				Debug.LogWarning("Trying to read plaintext save file in unity editor");
				var json = File.ReadAllText(saveFilePath);
				return JsonUtility.FromJson<SaveData>(json);
			} catch (Exception e) {
				Debug.Log($"Yeah nah bro, either you dont have a savefile or you touchy touchy in the wrong places");
				return null;
			}

			#else
			return null;
			#endif
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
			#if UNITY_EDITOR
			Debug.Log("Saving game without encryption when running in editor.");
			File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveData));
			#else
			File.WriteAllText(saveFilePath, Lib.AES.Encrypt(JsonUtility.ToJson(saveData)));
			#endif

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
			var data = GetSaveData();

			if (data == null) {
				Debug.LogError("Save file is empty!");
				return false;
			}

			switch (data.version) {
				case > CurrentVersion:
					Debug.LogWarning("Save is from a newer version, cannot load."); //  We lowkey have no plans to update the save data format but this is here just in case
					return false;
				case < CurrentVersion:
					Debug.LogWarning("Old save version, cannot load."); // This too.
					return false;
			}


			PlayerData.Instance.Health         = data.health;
			PlayerData.Instance.Battery        = data.battery;
			PlayerData.Instance.IsLookingRight = data.isLookingRight;
			CachePlayer().transform.position   = data.checkpointPosition;
			UIController.Instance.UpdateUI();
			return true;
		} catch (Exception e) {
			Debug.LogError($"Error loading save file: {e.Message}");
			return false;
		}
	}

	// Used by SaveControllerUI to check if save file exists and display last save time yeee
	public string GetSaveFilePath() {
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