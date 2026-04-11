using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
	public CustomInspectorObjects customInspectorObjects;

	private Collider2D collider;

	private void Awake() {
		collider = GetComponent<Collider2D>();
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		print(collision.name);
	}

	private void OnTriggerExit2D(Collider2D collision) {
		print(collision.name);
		if (!collision.CompareTag("Player")) return;
		print("Collided with " + collision.name);
		Vector2 exitDirection = (collision.transform.position - collider.bounds.center).normalized;
		if (customInspectorObjects.cameraOnLeft != null && customInspectorObjects.cameraOnRight != null) {
			print("Sent mathod call");
			CameraController.Instance.SwapCamera(customInspectorObjects.cameraOnLeft, customInspectorObjects.cameraOnRight, exitDirection);
		}
	}
}

[System.Serializable]
public class CustomInspectorObjects {

	public CinemachineCamera cameraOnLeft;
	public CinemachineCamera cameraOnRight;
	

}