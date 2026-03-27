using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveController : MonoBehaviour {
	
	public static SaveController Instance;
	
	#region Fields

    private GameObject playerObj;

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject loadMenu;
    
    [SerializeField] private TextMeshProUGUI saveSlot1Text;
    [SerializeField] private TextMeshProUGUI saveSlot2Text;
    [SerializeField] private TextMeshProUGUI saveSlot3Text;

    [SerializeField] private TextMeshProUGUI loadSlot1Text;
    [SerializeField] private TextMeshProUGUI loadSlot2Text;
    [SerializeField] private TextMeshProUGUI loadSlot3Text;
    

    #endregion

    #region Unity Functions

    private void Awake() => RegisterInstance(this);

    void Start()
    {
        playerObj = GameObject.Find("Player");
    }

    #endregion

    private string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }
    
    private string GetLastSaveTime(int slot)
    {
	    string path = GetSavePath(slot);

	    if (File.Exists(path))
	    {
		    SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
		    return saveData.timeCreated;
	    }
	    else
	    {
		    return "Empty Slot";
	    }
    }

    

    #region Functions
    
    public void InitiateSaveMenu()
	{
	    bool isActive = loadMenu.activeSelf;
	    loadMenu.SetActive(!isActive);
	    UpdateSlotTexts();
	}

    public void SaveGame(int slot)
    {
        if (PlayerData.Instance == null)
        {
            Debug.LogError("PlayerData not found");
            return;
        }

        SaveData saveData = new SaveData
        {
            playerPosition          = playerObj.transform.position,
            health                  = PlayerData.Instance.Health,
            battery                 = PlayerData.Instance.Battery,
            isLookingRight          = PlayerData.Instance.IsLookingRight,
            flashLightModeUnlocked1 = PlayerData.Instance.FlashlightModesUnlocked[1],
            flashLightModeUnlocked2 = PlayerData.Instance.FlashlightModesUnlocked[2],
            flashLightModeUnlocked3 = PlayerData.Instance.FlashlightModesUnlocked[3],
            timeCreated             = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        File.WriteAllText(GetSavePath(slot), JsonUtility.ToJson(saveData, true));
        UpdateSlotTexts();
    }

    public void InitiateLoadMenu()
    {
	    bool isActive = loadMenu.activeSelf;
	    loadMenu.SetActive(!isActive);
	    UpdateSlotTexts();
    }

    public void LoadGame(int slot)
    {
        string path = GetSavePath(slot);

        if (File.Exists(path))
        {
            loadingScreen.SetActive(true);
            loadMenu.SetActive(false);

            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));

            playerObj.transform.position = saveData.playerPosition;
            UIController.Instance.SwitchToGameCams();
            PlayerController.Instance.StartFall();
            PlayerData.Instance.Health = saveData.health;
            PlayerData.Instance.Battery = saveData.battery;
            PlayerData.Instance.IsLookingRight = saveData.isLookingRight;
            PlayerData.Instance.FlashlightModesUnlocked[1] = saveData.flashLightModeUnlocked1;
            PlayerData.Instance.FlashlightModesUnlocked[2] = saveData.flashLightModeUnlocked2;
            PlayerData.Instance.FlashlightModesUnlocked[3] = saveData.flashLightModeUnlocked3;

            StartCoroutine(ExitLoadingScreen());

            GameController.Instance.InActiveGame = true;
        }
        else
        {
            Debug.Log("No save file found bradar");
        }
    }
    
    private static void RegisterInstance(SaveController instance) {
	    if (Instance && Instance != instance) {
		    Destroy(instance.gameObject);
	    } else {
		    Instance = instance;
	    }
    }

    public void UpdateSlotTexts()
    {
	    string slot1 = GetLastSaveTime(1);
	    string slot2 = GetLastSaveTime(2);
	    string slot3 = GetLastSaveTime(3);

	    saveSlot1Text.text = slot1;
	    saveSlot2Text.text = slot2;
	    saveSlot3Text.text = slot3;
	    
	    loadSlot1Text.text = slot1;
	    loadSlot2Text.text = slot2;
	    loadSlot3Text.text = slot3;
    }

    public void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Save file {slot} deleted");
        }
        else
        {
            Debug.Log("There is no save file to delete");
        }

        UpdateSlotTexts();
    }

    #endregion

    #region Coroutines

    IEnumerator ExitLoadingScreen()
    {
        yield return new WaitForSeconds(2.6f);
        loadingScreen.SetActive(false);
    }

    #endregion
}
