using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveController : MonoBehaviour
{
	#region  Fields
	
    private string saveFilePath;

    private GameObject playerObj;

    [SerializeField] private GameObject      loadingScreen;
    [SerializeField] private GameObject      loadMenu;
    private                  string          lastSave;
    [SerializeField] private TextMeshProUGUI lastSaveText;
    
    #endregion
    
    #region Unity Functions
    
    void Start()
    {
	    playerObj = GameObject.Find("Player");
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log(saveFilePath);
    }
    #endregion

    #region Functions

    

    
    public void SaveGame() {
	    SaveData saveData = new SaveData {
		    playerPosition          = playerObj.transform.position,
		    health                  = PlayerData.Instance.Health,
		    battery                 = PlayerData.Instance.Battery,
		    isLookingRight          = PlayerData.Instance.IsLookingRight,
		    flashLightModeUnlocked1 = PlayerData.Instance.FlashlightModesUnlocked[1],
		    flashLightModeUnlocked2 = PlayerData.Instance.FlashlightModesUnlocked[2],
		    flashLightModeUnlocked3 = PlayerData.Instance.FlashlightModesUnlocked[3],
		    timeCreated             = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
	    };
	    
	    File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveData));
	    if (PlayerData.Instance == null)
	    {
		    Debug.LogError("PlayerData not found");
		    return;
	    }
    }
    
    public void InitiateLoadMenu() {
	    if (!loadMenu.activeInHierarchy)
	    {
		    loadMenu.SetActive(true);
		    if (File.Exists(saveFilePath))
		    {
			    SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveFilePath));
			    lastSave = $"Are You Sure You Want To Load Game? Last Save: {saveData.timeCreated}";
			    lastSaveText.text = lastSave;
		    }
		    else
		    {
			    lastSaveText.text = "No save file found";
		    }
	    }
	    
    }

    public void LoadGame() {
	    if (File.Exists(saveFilePath)) {
		    loadingScreen.SetActive(true);
		    loadMenu.SetActive(false);
		    SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveFilePath));
		    playerObj.transform.position                   = saveData.playerPosition;
		    UIController.Instance.SwitchToGameCams();
		    PlayerController.Instance.StartFall();
		    PlayerData.Instance.Health                     = saveData.health;
		    PlayerData.Instance.Battery                    = saveData.battery;
		    PlayerData.Instance.IsLookingRight             = saveData.isLookingRight;
		    PlayerData.Instance.FlashlightModesUnlocked[1] = saveData.flashLightModeUnlocked1;
		    PlayerData.Instance.FlashlightModesUnlocked[2] = saveData.flashLightModeUnlocked2;
		    PlayerData.Instance.FlashlightModesUnlocked[3] = saveData.flashLightModeUnlocked3;
		    StartCoroutine(ExitLoadingScreen());
		    GameController.Instance.InActiveGame = true;
	    } else {
		    Debug.Log("No save file found");
	    }
    }

    public void DeleteSave()
    {
	    if (File.Exists(saveFilePath))
	    {
		    File.Delete(saveFilePath);
		    Debug.Log("Save file deleted");
	    }
	    else
	    {
		    Debug.Log("There is no save file to delete");
	    }
    }
    #endregion

    #region Coroutines
    IEnumerator ExitLoadingScreen() {
	    yield return new WaitForSeconds(2.6f);
	    loadingScreen.SetActive(false);
    }
    #endregion
}
