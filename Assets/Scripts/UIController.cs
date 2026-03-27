using System.Collections;
using FlashlightGame;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	#region Fields

	//* Instance *//
	public static  UIController Instance;
	private static DebugHandler Debug;

	//* Refs *//
	[SerializeField] private Image healthFill;
	[SerializeField] private Image batteryFill;

	[SerializeField] private CinemachineCamera playerCinemachine;
	[SerializeField] private CinemachineCamera mainMenuCinemachine;
	[SerializeField] private CinemachineCamera storyboardBlackCinemachine;

	[SerializeField] private Camera mainCamera;
	[SerializeField] private Camera mainMenuOverlayCamera;
	[SerializeField] private Camera gameOverlayCamera;

	//* State *//
	public bool IsInMenu { get; private set; } = true;

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("UIController");
	}

	private void Start() => RegisterInstance(this);

	#endregion

	#region Functions

	private void SwitchToGameCams() {
		mainMenuOverlayCamera.enabled = false;
		StartCoroutine(FadeBetweenCams(
		                               mainMenuCinemachine,
		                               playerCinemachine,
		                               3f,
		                               mainMenuOverlayCamera,
		                               gameOverlayCamera));
		StartCoroutine(DelayFunction(3f, () => {
			                                 gameOverlayCamera.enabled            = true;
			                                 GameController.Instance.InActiveGame = true;
		                                 }));
	}

	private static void RegisterInstance(UIController instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;
		}
	}

	/// <summary>
	/// Update the UI with the current player data.
	/// </summary>
	public void UpdateUI() {
		Debug.LogKv("Updating UI", DebugLevel.Debug, new object[] {
			"Health", PlayerData.Instance.Health,
			"Battery", PlayerData.Instance.Battery
		});

		healthFill.fillAmount  = PlayerData.Instance.Health  / 100f;
		batteryFill.fillAmount = PlayerData.Instance.Battery / 100f;
	}

	public void InitiatePlayerFall() {
		Debug.Log("Initiating player fall sequence.", DebugLevel.Debug);
		StartCoroutine(PlayerFallSequence());
	}

	#endregion

	#region Coroutines

	private IEnumerator PlayerFallSequence() {
		PlayerController.Instance.StartFall();
		yield return new WaitForSecondsRealtime(1.5f);
		SwitchToGameCams();
		yield return null;
	}

	private IEnumerator FadeBetweenCams(
		CinemachineCamera fromCam,
		CinemachineCamera toCam,
		float             duration,
		Camera            fromOverlay,
		Camera            toOverlay
	) {
		fromCam.Priority    = 0;
		fromOverlay.enabled = false;
		toCam.Priority      = 10;

		storyboardBlackCinemachine.Priority = 11;

		yield return new WaitForSecondsRealtime(duration);

		toOverlay.enabled                   = true;
		storyboardBlackCinemachine.Priority = 0;
	}

	private IEnumerator FadeBetweenCams(
		CinemachineCamera fromCam,
		CinemachineCamera toCam,
		float             duration
	) {
		fromCam.Priority                    = 0;
		storyboardBlackCinemachine.Priority = 11;
		toCam.Priority                      = 10;
		yield return new WaitForSecondsRealtime(duration);
		storyboardBlackCinemachine.Priority = 0;
	}

	private IEnumerator DelayFunction(float delay, System.Action action) {
		yield return new WaitForSecondsRealtime(delay);
		action();
	}

	#endregion
}