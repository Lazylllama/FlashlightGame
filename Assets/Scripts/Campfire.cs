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
		InputHandler.Instance.onInteract.RemoveListener(HandleInteract);
	}

	private void FixedUpdate() {
		if (!player) Start();

		if (!isInitialized) Initialize();

		dist = Vector2.Distance(player.transform.position, transform.position);

		if (dist < maxDist) spriteRenderer.color = new Color(1f, 1f, 1f, Math.Clamp(1f - (dist / maxDist), 0f, 1f));
	}

	#endregion

	#region Functions

	private void Initialize() {
		if (!InputHandler.Instance || isInitialized) return;

		InputHandler.Instance.onInteract.AddListener(HandleInteract);

		isInitialized = true;
	}

	private void HandleInteract() {
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

		SaveController.Instance.SaveGame();
		SaveControllerUI.Instance.ShowMessage();
		AudioManager.Instance.PlaySfx(AudioManager.AudioName.SavedGame, 1f); //? audio/sfx/game/ui/confirm 10 or 11?

		isResting = false;
	}

	#endregion
}