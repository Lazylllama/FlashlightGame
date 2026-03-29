using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Campfire : MonoBehaviour {
	private bool isResting;

	[Header("Refs")]
	[SerializeField] private GameObject prompt;

	private bool playerInRange;
	
	public static Campfire Instance;

	public bool inMenu;

	private void Awake() => RegisterInstance(this);

	private void Update() {
		if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame && !isResting) {
			StartCoroutine(RestRoutine());
			SaveController.Instance.SaveGame();
		}
	}
	
	private void OnTriggerEnter2D(Collider2D collision) {
		if (!collision.CompareTag("Player")) return;
		playerInRange = true;

		prompt.SetActive(true);
	}

	private void OnTriggerExit2D(Collider2D collision) {
		if (!collision.CompareTag("Player")) return;
		playerInRange = false;
		prompt.SetActive(false);
	}

	private static void RegisterInstance(Campfire instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;
		}
	}

	private IEnumerator RestRoutine() {
		if (isResting) yield break;
		isResting = true;


		while (ScreenFader.Instance == null) {
			yield return null;
		}

		yield return ScreenFader.Instance.FadeOut(0.6f);
		RespawnManager.Instance.SetRespawnPoint(transform.position);
		PlayerData.Instance.Relieved = true;
		Debug.Log("PlayerData.Relieved");
		SaveController.Instance.SaveGame();
		Debug.Log("resting at campfire and saved game");
		yield return new WaitForSeconds(0.2f);
		PlayerData.Instance.Relieved = false;
		Debug.Log("PlayerData.NotRelieved");

		yield return ScreenFader.Instance.FadeIn(0.3f);


		isResting = false;
	}
}