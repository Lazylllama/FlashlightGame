using System.Collections.Generic;
using FlashlightGame;
using UnityEngine;

public class FlashlightPickUp : MonoBehaviour {
	#region Fields

	[Header("Settings")]
	[SerializeField] private float maxDist;

	//? Refs
	private GameObject player;

	//? States
	private bool  isInitialized;
	private float dist;

	#endregion

	#region Unity Functions

	private void Start() {
		player = GameObject.FindGameObjectWithTag("Player");

		if (!Preferences.Debug.SkipPickups) return;
		PickUpFlashlight();
	}

	private void FixedUpdate() {
		if (!isInitialized) Initialize();
		dist = Vector2.Distance(player.transform.position, transform.position);
	}

	#endregion

	#region Custom Functions

	private void HandleOnActionBtnTriggered(InputHandler.InputActions action) {
		if (action      != InputHandler.InputActions.Interact) return;
		if (maxDist / 2 < dist) return;

		PickUpFlashlight();
	}

	private void Initialize() {
		if (!InputHandler.Instance || isInitialized) return;

		InputHandler.Instance.onActionBtnTriggered.AddListener(HandleOnActionBtnTriggered);

		isInitialized = true;
	}

	private void PickUpFlashlight() {
		PlayerData.Instance.UnlockFlashlightMode(1);
		PlayerMovement.Instance.PickupFlashlight();
		TutorialHandler.Instance.ShowTutorial(0);
		ConversationHandler.Instance.StartConversation("FlashlightPickup");
		Destroy(gameObject);
	}

	#endregion
}