using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FlashlightPickUp : MonoBehaviour {
	#region Fields

	[Header("Settings")]
	[SerializeField] private float maxDist;
	
	//? Refs
	private GameObject player;
	private SpriteRenderer spriteRenderer;
	
	//? States
	private bool  isInitialized;
	private float dist;
	
	#endregion
	
	#region Unity Functions

	private void Start() {
		player         = GameObject.FindGameObjectWithTag("Player");
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	private void FixedUpdate() {
		if(!isInitialized) Initialize();
		
		dist = Vector2.Distance(player.transform.position, transform.position);
	}
	
	#endregion
	
	#region Custom Functions
	
	private void HandleOnActionBtnTriggered(InputHandler.InputActions action) {
		if (action      != InputHandler.InputActions.Interact) return;
		if (maxDist / 2 < dist) return;

		Debug.Log("In-range and ready to rest");

		PickUpFlashlight();
	}
	
	private void Initialize() {
		if (!InputHandler.Instance || isInitialized) return;

		InputHandler.Instance.onActionBtnTriggered.AddListener(HandleOnActionBtnTriggered);

		isInitialized = true;
	}

	private void PickUpFlashlight() {
		PlayerData.Instance.UnlockFlashlightMode(1);
		gameObject.SetActive(false);
	}
	
	#endregion
}