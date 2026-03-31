using System;
using System.Collections.Generic;
using System.Linq;
using FlashlightGame;
using NUnit.Framework;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour {
	#region Types

	public enum InputActions {
		ToggleFlashlight,
		ToggleModeLeft,
		ToggleModeRight,
		Flashlight1,
		Flashlight2,
		Mantle,
		CrankKeyboard,
		CrankLeft,
		CrankRight,
		NextSentence,
		Interact,
		Leap,
		FlashlightDirection
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

	public Lib.InputType             CurrentInputType { get; private set; } = Lib.InputType.KeyboardMouse;
	public UnityEvent<Lib.InputType> inputChange;
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
		if (inputAtlases.TryGetValue(CurrentInputType, out var atlas) && atlas != null)
			return atlas.GetInputSprite(action);

		return invalidFlairSprite;
	}
	
	public bool WasPressedThisFrame(InputActions action) => inputActionsList[action].WasPressedThisFrame();
	public Vector2 ReadValue(InputActions action) => inputActionsList[action].ReadValue<Vector2>();

	private void HandleInputChange(object obj, InputActionChange context) {
		if (context != InputActionChange.ActionPerformed) return;
		if (obj is not InputAction action) return;

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
			Debug.Log($"Action '{kvp.Key.ToString()}' was triggered", DebugLevel.Debug);
			switch (kvp.Key) {
				case InputActions.ToggleFlashlight:
					//* Let the player turn it on even though the flashlight is dead just as a feature, it will be disabled automatically right after.
					PlayerData.Instance.FlashlightEnabled = !PlayerData.Instance.FlashlightEnabled;
					break;
				case InputActions.ToggleModeLeft:
					PlayerData.Instance.HandleFlashlightModeChange(false);
					break;
				case InputActions.ToggleModeRight:
					PlayerData.Instance.HandleFlashlightModeChange(true);
					break;
				case InputActions.Flashlight1:
					PlayerData.Instance.HandleFlashlightModeChange(1);
					break;
				case InputActions.Flashlight2:
					PlayerData.Instance.HandleFlashlightModeChange(2);
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
				case InputActions.CrankLeft:
					PlayerData.Instance.Crank(false);
					break;
				case InputActions.CrankRight:
					PlayerData.Instance.Crank(true);
					break;
				case InputActions.Interact:
				case InputActions.Leap:
				case InputActions.FlashlightDirection:
					break;
				default:
					throw new ArgumentOutOfRangeException();
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