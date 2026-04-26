using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class CameraTrigger : MonoBehaviour {
	public CustomInspectorObjects customInspectorObjects;

	private Collider2D OwnCollider;

	private void Awake() {
		OwnCollider = GetComponent<Collider2D>();
	}
	
	private void OnTriggerExit2D(Collider2D collision) {
		if (!collision.CompareTag("Player")) return;

		Vector2 exitDirection = (collision.transform.position - OwnCollider.bounds.center).normalized;

		if (customInspectorObjects.cameraOnLeft != null && customInspectorObjects.cameraOnRight != null) {
			CameraController.Instance.SwapCamera(customInspectorObjects.cameraOnLeft,
			                                     customInspectorObjects.cameraOnRight, exitDirection);
		}
	}
}

[System.Serializable]
public class CustomInspectorObjects {
	public CinemachineCamera cameraOnLeft;
	public CinemachineCamera cameraOnRight;
}