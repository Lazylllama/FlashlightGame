using System;
using System.Collections.Generic;
using System.Linq;
using FlashlightGame;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour {
	#region Types

	//! DO NOT CHANGE ORDER, DO NOT REMOVE!
	//? ONLY ADD TO END OR YOU ARE THE ONE REMAPPING ALL THE INPUTS AFTERWARD!
	public enum InputActions {
		ToggleFlashlight,    // Button North / Y / Triangle / F (keyboard)
		ToggleModeLeft,      // LEFT TOP BUTTON / LB / L1
		ToggleModeRight,     // RIGHT TOP BUTTON / RB / R1
		Flashlight1,         // 1
		Flashlight2,         // 2
		Mantle,              // Left Stick Up / W (keyboard)
		CrankKeyboard,       // Space
		CrankLeft,           // LEFT TRIGGER / LT / L2
		CrankRight,          // RIGHT TRIGGER / RT / R2
		NextSentence,        // Button South / A / Cross / Enter (keyboard)
		Interact,            // Button South / A / Cross / E (keyboard)
		Leap,                // Button West / X / Square / Left Shift (keyboard) [Prelimiary, maybe change soon]
		FlashlightDirection, // Right Stick 2D Vector
		Move,                // Left Stick 2D Vector / WASD 2D Vector (keyboard)
	}

	#endregion

	#region Fields

	[Header("Flairs")]
	[SerializeField] private Sprite invalidFlairSprite;

	[Header("Input Atlases")]
	[SerializeField] private InputSpriteAtlas keyboardAtlas;
	[SerializeField] private InputSpriteAtlas xboxAtlas;
	[SerializeField] private InputSpriteAtlas playstationAtlas;
	[SerializeField] private InputSpriteAtlas steamDeckAtlas;

	// TODO(@lazylllama): Add to player prefs
	public static float MoveInputDeadZone = 0.05f;
	public static float LookInputDeadZone = 0.05f;

	public Lib.InputType             CurrentInputType { get; private set; } = Lib.InputType.KeyboardMouse;
	public UnityEvent<Lib.InputType> inputChange;
	public UnityEvent                onInteract;
	public string CurrentInputTypeDisplayName =>
		Lib.InputTypeDisplayName.GetValueOrDefault(CurrentInputType, "Unknown");

	private static DebugHandler Debug;
	public static  InputHandler Instance;

	private readonly Dictionary<InputActions, InputAction>       inputActionsList = new();
	private          Dictionary<Lib.InputType, InputSpriteAtlas> inputAtlases     = new();

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("InputHandler");

		if (inputChange == null) {
			inputChange = new UnityEvent<Lib.InputType>();
			Debug.Log("Initialized inputChange UnityEvent.");
		}

		if (onInteract == null) {
			onInteract = new UnityEvent();
			Debug.Log("Initialized onInteract UnityEvent.");
		}

		inputAtlases = new Dictionary<Lib.InputType, InputSpriteAtlas> {
			{ Lib.InputType.KeyboardMouse, keyboardAtlas },
			{ Lib.InputType.Xbox, xboxAtlas },
			{ Lib.InputType.PlayStation, playstationAtlas },
			{ Lib.InputType.SteamDeck, steamDeckAtlas }
		};

		foreach (var atlas in inputAtlases.Values.Where(atlas => atlas)) atlas.MapSprites();
	}

	private void Start() {
		RegisterInstance(this);
		FindActions();
	}

	private void OnEnable()  => InputSystem.onActionChange += HandleInputChange;
	private void OnDisable() => InputSystem.onActionChange -= HandleInputChange;
	private void Update()    => CheckForTriggeredActions();

	#endregion

	#region Functions

	public Sprite GetSprite(InputActions action) {
		if (inputAtlases.TryGetValue(CurrentInputType, out var atlas) && atlas)
			return atlas.GetInputSprite(action) ?? invalidFlairSprite;

		return invalidFlairSprite;
	}

	public Sprite GetInputLogoSprite() {
		if (inputAtlases.TryGetValue(CurrentInputType, out var atlas) && atlas)
			return atlas.GetInputLogoSprite() ?? invalidFlairSprite;

		return invalidFlairSprite;
	}

	public bool    WasPressedThisFrame(InputActions action) => inputActionsList[action].WasPressedThisFrame();
	public Vector2 ReadValue(InputActions           action) => inputActionsList[action].ReadValue<Vector2>();

	private void HandleInputChange(object obj, InputActionChange context) {
		if (context != InputActionChange.ActionPerformed) return;
		if (obj is not InputAction action) return;
		
		var activeControl = action.activeControl.ToString();
		
		if (activeControl.Contains("UI/Point") || activeControl.Contains("Mouse/position")) return;
		
		var control = action.activeControl;
		if (control == null) return;

		var newInputType        = Lib.Input.RevealDevice(control.device);
		var newInputDisplayName = Lib.InputTypeDisplayName.GetValueOrDefault(newInputType, "Unknown");

		if (CurrentInputType == newInputType) return;

		CurrentInputType = newInputType;
		inputChange.Invoke(CurrentInputType);

		Debug.Log($"Input type changed to {newInputDisplayName}", DebugLevel.Info);
	}

	private void FindActions() {
		foreach (InputActions action in Enum.GetValues(typeof(InputActions))) {
			var inputAction = InputSystem.actions.FindAction(action.ToString());
			if (inputAction != null) {
				Debug.Log($"Registered Action: {action.ToString()}", DebugLevel.Debug);
				inputActionsList[action] = inputAction;
			} else {
				Debug.LogKv($"Input action '{action}' not found!", DebugLevel.Error, new object[] {
					"Action", action
				});
			}
		}
	}

	private void CheckForTriggeredActions() {
		foreach (var kvp in inputActionsList.Where(kvp => kvp.Value.WasPressedThisFrame())) {
			//? Use the UI controls
			if (!GameController.Instance.InActiveGame) return;

			Debug.Log($"Action '{kvp.Key.ToString()}' was triggered", DebugLevel.Debug);

			switch (kvp.Key) {
				case InputActions.ToggleFlashlight:
					//* Let the player turn it on even though the flashlight is dead just as a feature, it will be disabled automatically right after.
					PlayerData.Instance.FlashlightEnabled = !PlayerData.Instance.FlashlightEnabled;
					break;
				case InputActions.ToggleModeLeft or InputActions.ToggleModeRight:
					PlayerData.Instance.HandleFlashlightModeChange(kvp.Key == InputActions.ToggleModeRight);
					break;
				case InputActions.Flashlight1 or InputActions.Flashlight2:
					PlayerData.Instance.HandleFlashlightModeChange(kvp.Key == InputActions.Flashlight2 ? 2 : 1);
					break;
				case InputActions.NextSentence:
					if (PlayerData.Instance.IsTalking) ConversationHandler.Instance.SkipButtonPressed();
					break;
				case InputActions.Mantle:
					PlayerMovement.Instance.Mantle();
					break;
				case InputActions.CrankKeyboard:
					PlayerData.Instance.Crank();
					break;
				case InputActions.CrankLeft or InputActions.CrankRight:
					PlayerData.Instance.Crank(kvp.Key == InputActions.CrankRight);
					break;
				case InputActions.Interact:
					onInteract.Invoke();
					break;
				case InputActions.Leap:
				case InputActions.FlashlightDirection:
				case InputActions.Move:
					break;
				default:
					throw new ArgumentOutOfRangeException(kvp.Key.ToString());
			}
		}
	}

	private static void RegisterInstance(InputHandler instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;

			Debug.Log("PlayerData initialized.");
		}
	}

	#endregion
}