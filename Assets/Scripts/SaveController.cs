using System;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveFilePath;

    private GameObject playerObj;
    
    void Start()
    {
	    playerObj = GameObject.Find("Player");
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log(saveFilePath);
    }

    public void SaveGame() {
	    SaveData saveData = new SaveData {
		    playerPosition = playerObj.transform.position,
		    health = PlayerData.Instance.Health,
		    battery = PlayerData.Instance.Battery,
		    isLookingRight = PlayerData.Instance.IsLookingRight
	    };
	    File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveData));
	    if (PlayerData.Instance == null)
	    {
		    Debug.LogError("PlayerData instance is null!");
		    return;
	    }
    }

    public void LoadGame() {
	    if (File.Exists(saveFilePath)) {
		    SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveFilePath));
		    GameObject.Find("Player").transform.position = saveData.playerPosition;
	    } else {
		    Debug.Log("No save file found!");
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
}
