using System;
using System.Collections.Generic;
using System.Linq;
using FlashlightGame;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour {
	#region Types

	private enum InputActions {
		ToggleFlashlight,
		ToggleModeLeft,
		ToggleModeRight,
		NextSentence,
		Flashlight1,
		Flashlight2,
		Mantle,
		Crank
	}

	#endregion

	#region Fields

	public UnityEvent<Lib.InputType> inputChange;

	private static DebugHandler Debug;

	private readonly Dictionary<InputActions, InputAction> inputActionsList = new();

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("InputHandler");

		if (inputChange == null) {
			inputChange = new UnityEvent<Lib.InputType>();
			Debug.Log("Initialized inputChange UnityEvent.");
		}
	}

	private void Start() {
		FindActions();

		inputChange.AddListener(OnInputChange);
	}

	private void Update() => CheckForTriggeredActions();

	#endregion

	#region Functions

	private void OnInputChange(Lib.InputType inputType) {
		Debug.Log("Input lowk changed to " + inputType, DebugLevel.Info);
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
				case InputActions.Crank:
					PlayerData.Instance.Crank();
					break;
				default:
					Debug.LogException(new Exception("Unhandled InputAction: " + kvp.Key));
					break;
			}
		}
	}

	#endregion
}