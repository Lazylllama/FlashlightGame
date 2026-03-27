using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CampfireCheckpoint : MonoBehaviour
{
	InputAction interactAction;
	[SerializeField] private GameObject saveMenu;
	void Start() {
		interactAction = InputSystem.actions.FindAction("Interact");
	}
	
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			PlayerData.Instance.Relieved = true;
			Debug.Log("Player Relieved");
			
			if (interactAction.WasPerformedThisFrame()) {
				bool isActive = saveMenu.activeSelf;
				saveMenu.SetActive(!isActive);
				SaveController.Instance.UpdateSlotTexts();
			}
		}
	}
}
