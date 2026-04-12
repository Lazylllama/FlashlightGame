using System.Collections;
using UnityEngine;

public class Lever : MonoBehaviour {
	#region Fields

	[Header("Settings")]
	[SerializeField] private string id;
	[SerializeField] private GameObject rotatingPart;

	public float maxDist       = 5f;
	public float rotationAngle = 45f;
	public float rotationSpeed = 2f;

	private GameObject player;
	private bool       isInitialized;
	private float      dist;
	private bool       inRot;

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

	private void HandleOnActionBtnTriggered(InputHandler.InputActions action) {
		if (action      != InputHandler.InputActions.Interact) return;
		if (maxDist / 2 < dist) return;

		if (!inRot) StartCoroutine(LeverRoutine());
	}

	private void Initialize() {
		if (!InputHandler.Instance || isInitialized) return;

		InputHandler.Instance.onActionBtnTriggered.AddListener(HandleOnActionBtnTriggered);

		isInitialized = true;
	}

	#endregion

	#region Coroutines

	private IEnumerator LeverRoutine() {
		inRot = true;
		var oldAngle = rotatingPart.transform.eulerAngles.z;
		yield return LeanTween.rotate(rotatingPart, new Vector3(0f, 0f, oldAngle + rotationAngle), rotationSpeed)
		                      .setEase(LeanTweenType.easeInOutSine).id;

		GameController.Instance.leverEvent.Invoke(id);
		yield return new WaitForSecondsRealtime(rotationSpeed);

		yield return LeanTween.rotate(rotatingPart, new Vector3(0f, 0f, oldAngle), rotationSpeed)
		                      .setEase(LeanTweenType.easeInOutSine).id;
		inRot = false;
	}

	#endregion
}