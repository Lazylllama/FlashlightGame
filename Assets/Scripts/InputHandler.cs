using System;
using System.Collections.Generic;
using System.Linq;
using FlashlightGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour {
	#region Types

	private enum InputActions {
		ToggleFlashlight,
		ToggleModeLeft,
		ToggleModeRight,
		Flashlight1,
		Flashlight2,
		Flashlight3
	}

	#endregion

	#region Fields

	private static DebugHandler Debug;

	private Dictionary<InputActions, InputAction> inputActionsList = new();

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("InputHandler");
	}

	private void Start() => FindActions();

	private void Update() => CheckForTriggeredActions();

	#endregion

	#region Functions

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
			Debug.Log($"Action '{kvp.Key.ToString()}' was triggered", DebugLevel.Debug,
			          new object[] { });
			switch (kvp.Key) {
				case InputActions.ToggleFlashlight:
					if (PlayerData.Instance.Battery < 0) {
						Debug.LogKv("Battery Empty", DebugLevel.Debug, new object[] {
							"Battery Level", PlayerData.Instance.Battery
						});
						break;
					}

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
				case InputActions.Flashlight3:
					PlayerData.Instance.HandleFlashlightModeChange(3);
					break;
			}
		}
	}

	#endregion
}