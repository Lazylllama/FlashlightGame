using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lever : MonoBehaviour {
	[Header("Settings")]
	[SerializeField] private string id;

	public bool  isOn          = false;
	public float rotationAngle = 45f;
	public float rotationSpeed = 360f;
	public float onDuration    = 2f;

	private bool playerInRange = false;

	private InputAction toggleLeverAction;

	private float startAngle;
	private float targetAngle;
	private float leverOnTime = 0f;

	private void Start() {
		startAngle  = transform.eulerAngles.z;
		targetAngle = startAngle;
	}

	private void Update() {
		if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame) {
			TurnOnLever();
		}

		var currentAngle = transform.eulerAngles.z;
		var newAngle     = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
		transform.rotation = Quaternion.Euler(0f, 0f, newAngle);

		if (isOn && Time.time - leverOnTime >= onDuration) {
			TurnOffLever();
		}
	}

	private void TurnOnLever() {
		isOn        = true;
		targetAngle = startAngle - rotationAngle;
		leverOnTime = Time.time;
		Debug.Log("Lever turned ON");
		GameController.Instance.leverEvent.Invoke(id);
	}

	private void TurnOffLever() {
		isOn        = false;
		targetAngle = startAngle;
		Debug.Log("Lever turned OFF");
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.CompareTag("Player")) {
			playerInRange = true;
			Debug.Log("Player in range");
		}
	}

	private void OnTriggerExit2D(Collider2D collision) {
		if (collision.CompareTag("Player")) {
			playerInRange = false;
			Debug.Log("Player out of range");
		}
	}
}