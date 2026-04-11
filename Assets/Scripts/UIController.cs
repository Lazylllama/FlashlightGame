using System;
using System.Collections;
using FlashlightGame;
using TMPro;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;
using UnityEngine.SceneManagement;

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

	[SerializeField] private GameObject      settingsMenu;
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

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("UIController");
	}

	private void Awake() {
		Debug      ??= new DebugHandler("UIController");
		mainMenuUI =   GameObject.FindGameObjectWithTag("MainMenuUI");

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
		UpdateLoadMenu();

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

		StartCoroutine(Lib.DelayFunction(3f, () => {
			                                     AudioManager.Instance.PlayOneShot(FMODEvents.Instance.savedGame,
				                                     mainCamera.transform.position);
			                                     savingGameUIAnimator.SetBool(IsSavingGame, false);
		                                     }));
	}

	private void SwitchToGameCams() {
		var duration = Preferences.Game.SkipIntroFade ? 0f : 3f;
		print($"Switching to game cameras with a fade duration of {duration} seconds.");

		StartCoroutine(FadeBetweenCams(
		                               mainMenuCinemachine,
		                               playerCinemachine,
		                               duration,
		                               mainMenuOverlayCamera,
		                               gameOverlayCamera));

		StartCoroutine(Lib.DelayFunction(Math.Clamp(duration - 1f, 0f, float.MaxValue),
		                                 () => {
			                                 mainMenuOverlayCamera.enabled = false;
			                                 mainMenuUI.SetActive(false);
			                                 gameOverlayCamera.enabled            = true;
			                                 GameController.Instance.InActiveGame = true;
		                                 }));

		StartCoroutine(Lib.DelayFunction(duration + 3f, () => {
			                                                ConversationHandler.Instance
			                                                                   .StartConversation("BigBossMan");
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
			"Health", PlayerData.Instance.Health.ToString(),
			"Battery", PlayerData.Instance.Battery.ToString()
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
	/// Set the Load Menu to active or inactive.
	/// </summary>
	/// <param name="newState">New open state of the UI, default true.</param>
	public void SetLoadMenu(bool newState = true) {
		UpdateLoadMenu();
		loadMenu.SetActive(newState);

		if (newState) return;
		EventSystem.current.SetSelectedGameObject(startButtonMainMenu);
	}

	/// <summary>
	/// Set the settings
	/// </summary>
	/// <param name="newState"></param>
	public void SetSettingsMenu(bool newState = true) {
		settingsMenu.SetActive(newState);

		if (newState) return;
		EventSystem.current.SetSelectedGameObject(startButtonMainMenu);
		PlayerPrefsHandler.Instance.SavePreferences();
	}


	/// <summary>
	/// Delete the current save game.
	/// </summary>
	public void DeleteSaveGame() {
		SaveController.Instance.DeleteSave();
		UpdateLoadMenu();
	}

	/// <summary>
	/// Load a save game and switch to the game cameras.
	/// </summary>
	public void LoadSaveGame() {
		var loaded = SaveController.Instance.LoadGame();
		if (loaded) StartCoroutine(GameStartSequence());
	}

	/// <summary>
	/// Reloads the current scene.
	/// </summary>
	public void ReloadScene() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	/// <summary>
	/// Update the Load Menu with the last saved date.
	/// </summary>
	private void UpdateLoadMenu() {
		Debug.Log("Updating Load Menu");

		var saveData = SaveController.Instance.GetSaveData();

		if (saveData == null) {
			loadMenuLastSavedDate.text = $"You have no save.";
		} else {
			var date = new DateTime(saveData.lastSavedTicks);
			loadMenuLastSavedDate.text = $"Last Saved: {date:yyyy-MM-dd HH:mm:ss}";
		}
	}

	/// <summary>
	/// Play a button press sound.
	/// </summary>
	public void ButtonPressSound() {
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonPress, mainCamera.transform.position);
	}

	#endregion

	#region Coroutines

	private IEnumerator GameStartSequence() {
		yield return new WaitForSecondsRealtime(1f);
		AudioManager.Instance.GameStarted();
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
		storyboardBlackCinemachine.Priority = 11;
		
		yield return new WaitForSecondsRealtime(duration);

		fromCam.Priority    = 0;
		fromOverlay.enabled = false;
		toCam.Priority      = 10;

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