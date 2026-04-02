using System;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class SaveController : MonoBehaviour {

	
	#region FIelds

	private const int CurrentVersion = 1;
	
	private string saveFilePath;

	private GameObject playerObj;

	public static SaveController Instance;
	
	private const string EncryptionKey = "nellaFScuto_126!";
	
	#endregion

	#region Unity Functions
	
	private void Awake() {
		RegisterInstance(this);
		saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.dat");
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
	public SaveData GetSaveData()
	{
		if (!File.Exists(saveFilePath)) return null;
    
		try {
			string json = Decrypt(File.ReadAllText(saveFilePath));
			return JsonUtility.FromJson<SaveData>(json);
		} catch {
			return null;
		}
	}
	
	private string Encrypt(string plainText)
	{
		using var aes = Aes.Create();
		var       key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(16).Substring(0, 16));
		aes.Key = key;
		aes.GenerateIV();

		using var encryptor   = aes.CreateEncryptor();
		var       plainBytes  = Encoding.UTF8.GetBytes(plainText);
		var       cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

		// Prepend IV to the cipher so we can decrypt later
		var result = new byte[aes.IV.Length + cipherBytes.Length];
		aes.IV.CopyTo(result, 0);
		cipherBytes.CopyTo(result, aes.IV.Length);

		return Convert.ToBase64String(result);
	}

	private string Decrypt(string cipherText)
	{
		var fullBytes = Convert.FromBase64String(cipherText);
    
		using var aes = Aes.Create();
		var       key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(16).Substring(0, 16));
		aes.Key = key;

		// Extract the IV from the front of the data
		var iv     = new byte[16];
		var cipher = new byte[fullBytes.Length - 16];
		Array.Copy(fullBytes, 0,  iv,     0, 16);
		Array.Copy(fullBytes, 16, cipher, 0, cipher.Length);
		aes.IV = iv;

		using var decryptor  = aes.CreateDecryptor();
		var       plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
		return Encoding.UTF8.GetString(plainBytes);
	}

	public void SaveGame() {
		
		if (!AreRequiredInstancesReady())
		{
			Debug.LogError("Cannot save game one or more required scripts are missing!");
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
			File.WriteAllText(saveFilePath, Encrypt(JsonUtility.ToJson(saveData)));
			
		} catch (Exception e) {
			Debug.LogError($"Failed to save game: {e.Message}");
		}
		
	}
	

	public bool LoadGame() {
		if (!File.Exists(saveFilePath)) {
			Debug.Log("No save file found!");
			return false;
		}
		if (!AreRequiredInstancesReady())
		{
			Debug.LogError("Cannot load game one or more required scripts are missing!");
			return false;
		}

		try {
			string json = Decrypt(File.ReadAllText(saveFilePath));

			if (string.IsNullOrEmpty(json)) {
				Debug.LogError("Save file is empty!");
				return false;
			}

			SaveData saveData = JsonUtility.FromJson<SaveData>(json);

			if (saveData == null) {
				Debug.LogError("Failed to find save data!");
				return false;
			}

			if (saveData.version > CurrentVersion)
			{
				Debug.LogWarning("Save is from a newer version, cannot load."); //  We lowkey have no plans to update the save data format but this is here just in case
				return false;
			}

			if (saveData.version < CurrentVersion)
			{
				Debug.LogWarning("Old save version, cannot load."); // This too.
				return false;
			}
			
			
			PlayerData.Instance.Health                     = saveData.health;
			PlayerData.Instance.Battery                    = saveData.battery;
			PlayerData.Instance.IsLookingRight             = saveData.isLookingRight;
			CachePlayer().transform.position = saveData.checkpointPosition;
			return true;

		} catch (Exception e) {
			Debug.LogError($"Error loading save file: {e.Message}");
			return false;
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
