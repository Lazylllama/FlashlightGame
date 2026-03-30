using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveControllerUI : MonoBehaviour
{
	public  TextMeshProUGUI savedGameText;
	private Coroutine       messageRoutine;
	public  GameObject      loadMenu;
	public  TextMeshProUGUI lastTimeSavedText;
	
	public static SaveControllerUI Instance;
	
	
	
	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
		} else {
			Instance = this;
		}
	}
	
	public void OnLoadButtonPressed()
	{
		SaveController.Instance.LoadGame();
		UIController.Instance.SwitchToGameCams();
	}

	
	public void InitiateLoadMenu() {
		bool isActive = loadMenu.activeSelf;
		loadMenu.SetActive(!isActive);
		UpdateUI();
	}

	private void UpdateUI() {
		if (SaveController.Instance == null || !File.Exists(SaveController.Instance.GetSaveFilePath())) {
			lastTimeSavedText.text = "No save data found.";
			return;
		}

		var saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SaveController.Instance.GetSaveFilePath()));

		DateTime date = new DateTime(saveData.timeCreatedTicks);
		lastTimeSavedText.text = $"Last Saved: {date:yyyy-MM-dd HH:mm:ss}";
	}
	
	public void ShowMessage() {
		if (messageRoutine != null) {
			StopCoroutine(messageRoutine);
		}
		messageRoutine = StartCoroutine(ShowMessageRoutine());
		
	}
	
	IEnumerator ShowMessageRoutine()
	{
		StartCoroutine(FadeInAndOut());
		yield return new WaitForSeconds(2f);
	}
	
	IEnumerator FadeInAndOut()
	{
		Color color = savedGameText.color;

		// Fade in
		while (color.a < 1)
		{
			color.a    += Time.deltaTime;
			savedGameText.color =  color;
			yield return null;
		}

		// Fade out
		while (color.a > 0)
		{
			color.a    -= Time.deltaTime;
			savedGameText.color =  color;
			yield return null;
		}
	}
	
}
