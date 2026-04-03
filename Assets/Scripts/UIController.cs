using System;
using System.Collections;
using FlashlightGame;
using TMPro;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	#region Fields

	//* Hash *//
	private static readonly int IsSavingGame = Animator.StringToHash("isSavingGame");

	//* Instance *//
	public static  UIController Instance;
	private static DebugHandler Debug;

	//* Refs *//
	[Header("UI Elements")]
	[SerializeField] private Image healthFill;
	[SerializeField] private Image batteryFill;

	[SerializeField] private GameObject      loadMenu;
	[SerializeField] private TextMeshProUGUI loadMenuLastSavedDate;

	[Header("Cinemachine")]
	[SerializeField] private CinemachineCamera playerCinemachine;
	[SerializeField] private CinemachineCamera mainMenuCinemachine;
	[SerializeField] private CinemachineCamera storyboardBlackCinemachine;

	[SerializeField] private Camera mainCamera;
	[SerializeField] private Camera mainMenuOverlayCamera;
	[SerializeField] private Camera gameOverlayCamera;

	[Header("Focus Elements")]
	[SerializeField] private GameObject startButtonMainMenu;
	//[SerializeField] private Button returnButtonEscMenu;

	[Header("UI Animators")]
	[SerializeField] private Animator savingGameUIAnimator;


	private GameObject mainMenuUI;

	//* State *//
	private bool isInitialized;
	public  bool IsInMenu     { get; private set; } = true;
	private bool InActiveGame => GameController.Instance.InActiveGame;

	//* PlayerPrefs *//
	//private static bool SkipIntroFade => FBPP.GetBool("SkipIntroFade");
	private static bool SkipIntroFade => true; //? Temp hardcoded cause it lowk looks better...

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug      = new DebugHandler("UIController");
		mainMenuUI = GameObject.FindGameObjectWithTag("MainMenuUI");

		if (!mainMenuUI)
			Debug.LogException(new
				                   Exception("MainMenuUI not found in scene. Please ensure there is a GameObject with the tag 'MainMenuUI'."));

		EventSystem.current.firstSelectedGameObject = startButtonMainMenu;
	}

	private void Start()    => RegisterInstance(this);
	private void OnEnable() => Initialize();

	private void OnDisable() {
		if (!isInitialized || !InputHandler.Instance) return;
		InputHandler.Instance.inputChange.RemoveListener(OnInputChanged);
	}

	private void FixedUpdate() {
		if (!isInitialized) Initialize();
	}

	#endregion

	#region Functions

	private void Initialize() {
		if (isInitialized || !InputHandler.Instance || !PlayerData.Instance || !SaveController.Instance) return;

		InputHandler.Instance.inputChange.AddListener(OnInputChanged);
		UpdateUI();
		UpdateSaveDataMenu();

		isInitialized = true;
	}

	private void OnInputChanged(Lib.InputType newType) {
		if (GameController.Instance.InActiveGame || newType == Lib.InputType.KeyboardMouse) {
			Debug.Log("Input type changed to " + newType, DebugLevel.Debug);
		} else {
			Debug.Log("Input type changed to " + newType + ", resetting selected UI element.", DebugLevel.Debug,
			          EventSystem.current.currentSelectedGameObject);
			EventSystem.current.SetSelectedGameObject(startButtonMainMenu);
		}
	}

	private void ShowSavingUI() {
		if (!savingGameUIAnimator) {
			Debug.LogError("Saving Game UI Animator is not assigned in the inspector.");
			return;
		}

		savingGameUIAnimator.SetBool(IsSavingGame, true);

		StartCoroutine(Lib.DelayFunction(3f, () => { savingGameUIAnimator.SetBool(IsSavingGame, false); }));
	}

	public void SwitchToGameCams() {
		var duration = SkipIntroFade ? 0f : 3f;
		mainMenuOverlayCamera.enabled = false;
		mainMenuUI.SetActive(false);

		StartCoroutine(FadeBetweenCams(
		                               mainMenuCinemachine,
		                               playerCinemachine,
		                               duration,
		                               mainMenuOverlayCamera,
		                               gameOverlayCamera));

		StartCoroutine(Lib.DelayFunction(Math.Clamp(duration - 1f, 0f, float.MaxValue),
		                                 () => {
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


	/// <summary>
	/// Initiate the game start sequence, which includes fading from the main menu to the game cameras and enabling the game overlay camera after a delay.
	/// </summary>
	public void InitiateGameStart() {
		Debug.Log("Initiating player fall sequence.", DebugLevel.Debug);
		StartCoroutine(GameStartSequence());
	}

	/// <summary>
	/// Fade the screen to black over a specified duration, optionally pausing the game during the fadeout.
	/// </summary>
	/// <param name="duration">seconds (realtime, non timescale based)</param>
	/// <param name="pauseGame">defaults to false</param>
	public void FadeToBlack(float duration, bool pauseGame = false) {
		StartCoroutine(BlackScreenFadeout(duration, pauseGame));
	}

	/// <summary>
	/// Exit the game 
	/// </summary>
	public void ExitGame() {
		Debug.Log("ExitGame has been called, bye.", DebugLevel.Fatal);

		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif

		Application.Quit();
	}

	/// <summary>
	/// Save the game, handles UI and Backend.
	/// </summary>
	public void SaveGame() {
		ShowSavingUI();

		// TODO: Callback?
		SaveController.Instance.SaveGameBackend();
	}

	/// <summary>
	/// Set the Load/Save Menu to active or inactive.
	/// </summary>
	/// <param name="newState">New open state of the UI, default true.</param>
	public void SetLoadSaveMenu(bool newState = true) {
		UpdateSaveDataMenu();
		loadMenu.SetActive(newState);
	}

	/// <summary>
	/// Delete the current save game.
	/// </summary>
	public void DeleteSaveGame() {
		SaveController.Instance.DeleteSave();
		UpdateSaveDataMenu();
	}

	/// <summary>
	/// Load a save game and switch to the game cameras.
	/// </summary>
	public void LoadSaveGame() {
		var loaded = SaveController.Instance.LoadGame();
		if (loaded) SwitchToGameCams();
	}

	/// <summary>
	/// Update the Save Data Menu with the last saved date.
	/// </summary>
	private void UpdateSaveDataMenu() {
		Debug.Log("Updating Save Data Menu");

		var saveData = SaveController.Instance.GetSaveData();

		if (saveData == null) {
			loadMenuLastSavedDate.text = $"You have no save.";
		} else {
			var date = new DateTime(saveData.lastSavedTicks);
			loadMenuLastSavedDate.text = $"Last Saved: {date:yyyy-MM-dd HH:mm:ss}";
		}
	}

	#endregion

	#region Coroutines

	private IEnumerator GameStartSequence() {
		yield return new WaitForSecondsRealtime(1f);
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

	private IEnumerator BlackScreenFadeout(float duration, bool pauseGame = false) {
		storyboardBlackCinemachine.Priority = 11;
		if (pauseGame) GameController.Instance.InActiveGame = false;
		yield return new WaitForSecondsRealtime(duration);
		if (pauseGame) GameController.Instance.InActiveGame = true;
		storyboardBlackCinemachine.Priority = 0;
	}

	#endregion
}