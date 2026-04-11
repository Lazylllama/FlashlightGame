using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Campfire : MonoBehaviour {
	#region Fields

	private bool           isInitialized, isResting;
	private SpriteRenderer spriteRenderer;
	private GameObject     player;
	private float          dist;

	private static DebugHandler Debug;

	[Range(0, 30)] [SerializeField] private float maxDist = 8f;

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("Campfire - " + Vector2.Normalize(transform.position));
	}

	private void Start() {
		player         = GameObject.FindGameObjectWithTag("Player");
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void OnEnable() => Initialize();

	private void OnDisable() {
		if (!isInitialized) return;
		InputHandler.Instance.onActionBtnTriggered.RemoveListener(HandleOnActionBtnTriggered);
	}

	private void FixedUpdate() {
		if (!player) Start();
		if (!isInitialized) Initialize();
	}

	#endregion

	#region Functions

	private void Initialize() {
		if (!InputHandler.Instance || isInitialized) return;

		InputHandler.Instance.onActionBtnTriggered.AddListener(HandleOnActionBtnTriggered);

		isInitialized = true;
	}

	private void HandleOnActionBtnTriggered(InputHandler.InputActions action) {
		if (action      != InputHandler.InputActions.Interact) return;
		if (maxDist / 2 < dist) return;

		Debug.Log("In-range and ready to rest");

		StartCoroutine(RestRoutine());
	}

	private IEnumerator RestRoutine() {
		if (isResting) yield break;
		isResting = true;

		// PlayerData.Instance.Relieved = true;
		//
		// yield return new WaitForSeconds(0.2f);
		//
		// PlayerData.Instance.Relieved = false;
		//
		// yield return ScreenFader.Instance.FadeIn(0.3f);
		// yield return new WaitForSeconds(0.2f);

		UIController.Instance.SaveGame();
		isResting = false;
	}

	#endregion
}