using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveController : MonoBehaviour {

	#region FIelds

	private const int CurrentVersion = 1;
	
	private string saveFilePath;

	private GameObject playerObj;

	public static SaveController Instance;
	
	#endregion

	#region Unity Functions
	
	private void Awake() {
		RegisterInstance(this);
		saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
	}
		

	void Start() {
		CachePlayer();
		Debug.Log(saveFilePath);
		
	}
	
	#endregion

	#region Functions
	
	private GameObject CachePlayer()
	{
		if (playerObj == null)
			playerObj = GameObject.FindGameObjectWithTag("Player");
		return playerObj;
	}

	public void SaveGame() {
		
		if (!AreRequiredInstancesReady())
		{
			Debug.LogError("Cannot load game one or more required scripts are missing!");
			return;
		}

		PlayerData.Instance.CheckpointPosition = playerObj.transform.position;
		
		SaveData saveData = new SaveData {
			version = CurrentVersion,
			
			checkpointPosition = PlayerData.Instance.CheckpointPosition,
			health             = PlayerData.Instance.Health,
			battery            = PlayerData.Instance.Battery,
			isLookingRight     = PlayerData.Instance.IsLookingRight,
			timeCreatedTicks   = DateTime.Now.Ticks // For SaveControllerUI to display last save time yes
		};
		try {
			File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveData));
			
		} catch (Exception e) {
			Debug.LogError($"Failed to save game: {e.Message}");
		}
		
	}
	

	public void LoadGame() {
		if (!File.Exists(saveFilePath)) {
			Debug.Log("No save file found!");
			return;
		}
		if (!AreRequiredInstancesReady())
		{
			Debug.LogError("Cannot load game one or more required scripts are missing!");
			return;
		}

		try {
			string json = File.ReadAllText(saveFilePath);

			if (string.IsNullOrEmpty(json)) {
				Debug.LogError("Save file is empty!");
				return;
			}

			SaveData saveData = JsonUtility.FromJson<SaveData>(json);

			if (saveData == null) {
				Debug.LogError("Failed to find save data!");
				return;
			}

			if (saveData.version > CurrentVersion)
			{
				Debug.LogWarning("Save is from a newer version, cannot load."); //  We lowkey have no plans to update the save data format but this is here just in case
				return;
			}

			if (saveData.version < CurrentVersion)
			{
				Debug.LogWarning("Old save version, cannot load."); // This too.
				return;
			}
			
			
			PlayerData.Instance.Health                     = saveData.health;
			PlayerData.Instance.Battery                    = saveData.battery;
			PlayerData.Instance.IsLookingRight             = saveData.isLookingRight;
			CachePlayer().transform.position = saveData.checkpointPosition;

		} catch (Exception e) {
			Debug.LogError($"Error loading save file: {e.Message}");
		}

	}
	

	public string GetSaveFilePath() // Used by SaveControllerUI to check if save file exists and display last save time yeee
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

	public void DeleteSave()
	{
		if (File.Exists(saveFilePath))
		{
			File.Delete(saveFilePath);
			Debug.Log("Save file deleted.");
		}
		else
		{
			Debug.Log("No save file to delete.");
		}
	}

	private bool AreRequiredInstancesReady()
	{
		var allGood = true;

		if (CachePlayer() == null)
		{
			Debug.LogWarning("Player object not assigned!");
			allGood = false;
		}

		if (PlayerData.Instance == null)
		{
			Debug.LogWarning("PlayerData instance not found!");
			allGood = false;
		}
		
		return allGood;
	}
	#endregion
}
