using System;
using FlashlightGame;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	#region Fields

	//* Instance *//
	public static            UIController Instance;
	private static           DebugHandler Debug;
	
	[SerializeField] private Image     healthFill;
	[SerializeField] private Image     batteryFill;

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("UIController");
	}

	private void Start() => RegisterInstance(this);

	#endregion

	#region Functions

	private static void RegisterInstance(UIController instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;
		}
	}

	/// <summary>
	/// Update the UI with the current player data.
	/// </summary>
	public void UpdateUI() {
		Debug.LogKv("Updating UI", DebugLevel.Debug, new object[] {
			"Health", PlayerData.Instance.Health,
			"Battery", PlayerData.Instance.Battery
		});

		healthFill.fillAmount = PlayerData.Instance.Health / 100f;
		batteryFill.fillAmount = PlayerData.Instance.Battery / 100f;
	}

	#endregion
}