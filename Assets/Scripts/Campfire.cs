using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Campfire : MonoBehaviour {
	private bool isResting;
	

	[Header("Refs")]
	[SerializeField] private GameObject prompt;

	private bool playerInRange;

	private void Update() {
		if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame && !isResting) {
			StartCoroutine(RestRoutine());
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
		Debug.Log("resting at campfire");

		yield return new WaitForSeconds(0.2f);
		PlayerData.Instance.Relieved = false;
		Debug.Log("PlayerData.NotRelieved");

		yield return ScreenFader.Instance.FadeIn(0.3f);


		isResting = false;
	}
}