using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveControllerUI : MonoBehaviour
{
	#region Fields
	
	public  TextMeshProUGUI savedGameText;
	private Coroutine       messageRoutine;
	public  GameObject      loadMenu;
	public  TextMeshProUGUI lastTimeSavedText;
	
	public static SaveControllerUI Instance;
	
	#endregion

	#region Unity Functions
	
	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
		} else {
			Instance = this;
		}
	}
	
	#endregion

	#region Functions
	
	public void OnLoadButtonPressed()
	{
		bool loaded = SaveController.Instance.LoadGame();
		if (loaded) {
			UIController.Instance.SwitchToGameCams();
		}
	}
	
	public void OnDeleteButtonPressed()
	{
		SaveController.Instance.DeleteSave();
		UpdateUI();
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

		var saveData = SaveController.Instance.GetSaveData();

		DateTime date = new DateTime(saveData.lastSavedTicks);
		lastTimeSavedText.text = $"Last Saved: {date:yyyy-MM-dd HH:mm:ss}";
	}
	
	public void ShowMessage() {
		if (messageRoutine != null) {
			StopCoroutine(messageRoutine);
		}
		messageRoutine = StartCoroutine(ShowMessageRoutine());
		
	}
	#endregion

	#region Coroutines
	
	IEnumerator ShowMessageRoutine()
	{
		StartCoroutine(FadeInAndOut());
		yield return new WaitForSeconds(2f);
	}
	
	IEnumerator FadeInAndOut()
	{
		Color color = savedGameText.color;
		
		while (color.a < 1)
		{
			color.a    += Time.deltaTime;
			savedGameText.color =  color;
			yield return null;
		}
		
		while (color.a > 0)
		{
			color.a    -= Time.deltaTime;
			savedGameText.color =  color;
			yield return null;
		}
	}
	#endregion
}
