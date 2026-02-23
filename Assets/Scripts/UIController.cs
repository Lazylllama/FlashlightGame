using System;
using System.Collections;
using FlashlightGame;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	#region Fields

	//* Instance *//
	public static  UIController Instance;
	private static DebugHandler Debug;

	//* Refs *//
	[SerializeField] private Image healthFill;
	[SerializeField] private Image batteryFill;

	[SerializeField] private CinemachineCamera playerCamera;
	[SerializeField] private CinemachineCamera menuCamera;

	//* State *//
	public bool IsInMenu { get; private set; } = true;

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

		healthFill.fillAmount  = PlayerData.Instance.Health  / 100f;
		batteryFill.fillAmount = PlayerData.Instance.Battery / 100f;
	}

	public void InitiatePlayerFall() {
		Debug.Log("Initiating player fall sequence.", DebugLevel.Debug);
		StartCoroutine(PlayerFallSequence());
	}

	#endregion

	#region Coroutines

	private IEnumerator PlayerFallSequence() {
		PlayerController.Instance.StartFall();
		yield return null;
	}	

	#endregion
}