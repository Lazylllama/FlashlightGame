using System.Collections;
using UnityEngine;

public class Campfire : MonoBehaviour {
	#region Fields

	private bool       isInitialized, isResting;
	private GameObject player;
	private float      dist;

	[Range(0, 30)] [SerializeField] private float maxDist = 8f;

	#endregion

	#region Unity Functions

	private void Start() {
		player = GameObject.FindGameObjectWithTag("Player");
	}

	private void OnEnable() => Initialize();

	private void OnDisable() {
		if (!isInitialized) return;
		InputHandler.Instance.onActionBtnTriggered.RemoveListener(HandleOnActionBtnTriggered);
	}

	private void FixedUpdate() {
		if (!player) Start();
		if (!isInitialized) Initialize();
		dist = Vector3.Distance(player.transform.position, transform.position);
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