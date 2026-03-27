using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FlashlightPickUp : MonoBehaviour {
	private bool playerInRange;

	[Header("Refs")]
	[SerializeField] private GameObject prompt;


	private void Update() {
		if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame) {
			Destroy(gameObject);
			Debug.Log("EKey.wasPressedThisFrame");
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
}