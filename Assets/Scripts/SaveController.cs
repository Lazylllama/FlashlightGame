using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveController : MonoBehaviour {

	private string saveFilePath;

	public GameObject playerObj;

	public static SaveController Instance;
	

	private void Awake() => RegisterInstance(this);
		

	void Start()
	{
		playerObj= GameObject.FindWithTag("Player");
		saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
		Debug.Log(saveFilePath);
	}

	public void SaveGame() {
		if (PlayerData.Instance == null)
		{
			Debug.LogError("PlayerData instance is null!");
			return;
		}
		
		if (!NullCheck())
		{
			Debug.LogError("Cannot load game one or more required scripts are missing!");
			return;
		}
		
		SaveData saveData = new SaveData {
			checkpointPosition      = RespawnManager.Instance.respawnPoint,
			health                  = PlayerData.Instance.Health,
			battery                 = PlayerData.Instance.Battery,
			isLookingRight          = PlayerData.Instance.IsLookingRight,
			timeCreatedTicks        = DateTime.Now.Ticks
		};
		try {
			File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveData));
			SaveControllerUI.Instance.ShowMessage();
		} catch (Exception e) {
			Debug.LogError($"Failed to save game: {e.Message}");
		}
		
	}

	public void LoadGame() {
		if (!File.Exists(saveFilePath)) {
			Debug.Log("No save file found!");
			return;
		}
		if (!NullCheck())
		{
			Debug.LogError("Cannot load game one or more required scripts are missing!");
			return;
		}
		playerObj= GameObject.FindWithTag("Player");

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
			
			
			
			playerObj.transform.position                   = saveData.checkpointPosition;
			PlayerData.Instance.Health                     = saveData.health;
			PlayerData.Instance.Battery                    = saveData.battery;
			PlayerData.Instance.IsLookingRight             = saveData.isLookingRight;
			UIController.Instance.SwitchToGameCams();
			PlayerController.Instance.StartFall();
			GameController.Instance.InActiveGame = true;

		} catch (Exception e) {
			Debug.LogError($"Error loading save file: {e.Message}");
		}

	}

	public string GetSaveFilePath()
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

	private bool NullCheck()
	{
		var allGood = true;

		if (playerObj == null)
		{
			Debug.LogWarning("Player object not assigned!");
			allGood = false;
		}

		if (PlayerData.Instance == null)
		{
			Debug.LogWarning("PlayerData instance not found!");
			allGood = false;
		}

		if (UIController.Instance == null)
		{
			Debug.LogWarning("UIController instance not found!");
			allGood = false;
		}

		if (PlayerController.Instance == null)
		{
			Debug.LogWarning("PlayerController instance not found!");
			allGood = false;
		}

		if (GameController.Instance == null)
		{
			Debug.LogWarning("GameController instance not found!");
			allGood = false;
		}

		return allGood;
	}
	
}
