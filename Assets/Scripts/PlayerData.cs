using System;
using System.Collections.Generic;
using FlashlightGame;
using UnityEngine;
using UnityEngine.Serialization;


public class PlayerData : MonoBehaviour {
	#region Fields

	public static  PlayerData   Instance;
	private static DebugHandler Debug;

	//* Player Stats *//
	public int  Health  { get; private set; } = 100;
	public int  Battery { get; private set; } = 100;
	public bool IsDead  => Health <= 0;


	//* Player Data *//
	private bool isLookingRight;
	public bool IsLookingRight {
		set => SetIsLookingRight(value);
		get => isLookingRight;
	}

	//* Player States *//
	public Dictionary<int, bool> FlashlightModesUnlocked { get; private set; } = new Dictionary<int, bool>() {
		{ 1, true }, // Mode 1 is always unlocked
		{ 2, false },
		{ 3, false }
	};
	public bool FlashlightEnabled { get; set; }         = true;
	public int  FlashlightMode    { get; private set; } = 1;

	//* Mood States *//
	//? Relieved   = player is at a checkpoint.
	//? Frightened = player is/was recently in danger.
	public bool Frightened { get; set; } = false;
	public bool Relieved   { get; set; } = false;

	//* Movement *//
	public float MovementMultiplier => Frightened ? 1.25f : 1f;
	public int InjuryLevel =>
		Health switch {
			> 75 => 0,
			> 50 => 1,
			> 25 => 2,
			> 0  => 3,
			_    => 4
		};

	//* Options *//
	[SerializeField] private float batteryDrainInterval = 1f;

	//* States *//
	private float drainTimer;

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("PlayerData");
	}

	private void Start() => RegisterInstance(this);

	private void FixedUpdate() {
		HandleBatteryDrain();
	}

	#endregion

	#region Functions

	//! Public Functions
	/// <summary>
	/// Update the player's battery by the specified difference. Clamps between 0 and 100.
	/// </summary>
	/// <param name="difference">-100 to +100 Health Points</param>
	private void UpdateHealth(int difference) {
		UIController.Instance.UpdateUI();
		Health = Mathf.Clamp(Health + difference, 0, 100);
	}

	/// <summary>
	/// Set the flashlight mode to a specific mode.
	/// </summary>
	/// <param name="mode">Int - 1,2 and 3</param>
	public void HandleFlashlightModeChange(int mode) {
		if (!FlashlightModesUnlocked.ContainsKey(mode)) {
			Debug.Log($"Invalid flashlight mode: {mode}. No such mode exists.", DebugLevel.Warning);
			return;
		}

		FlashlightMode = mode;
		Debug.LogKv("Flashlight Mode Changed", DebugLevel.Debug, new object[] {
			"New Mode", FlashlightMode
		});
	}

	/// <summary>
	/// Handle changing the flashlight mode by incrementing or decrementing.
	/// </summary>
	/// <param name="increment"></param>
	public void HandleFlashlightModeChange(bool increment) {
		var newMode = FlashlightMode + (increment ? 1 : -1);

		if (
			FlashlightModesUnlocked.ContainsKey(newMode) &&
			FlashlightModesUnlocked[newMode]
		) {
			FlashlightMode = newMode;
			Debug.LogKv("Flashlight Mode Changed", DebugLevel.Debug, new object[] {
				"New Mode", FlashlightMode
			});
		} else {
			Debug.Log($"Cannot change flashlight mode to {newMode}. Mode is locked or doesn't exist.",
			          DebugLevel.Warning);
		}
	}

	/// <summary>
	/// Unlock specified flashlight mode. Logs a warning if the mode is already unlocked or doesn't exist.
	/// </summary>
	/// <param name="mode">1-3</param>
	public void UnlockFlashlightMode(int mode) {
		if (FlashlightModesUnlocked.ContainsKey(mode) && !FlashlightModesUnlocked[mode]) {
			FlashlightModesUnlocked[mode] = true;
			Debug.LogKv("Flashlight Mode Unlocked", DebugLevel.Debug, new object[] {
				"Mode Unlocked", mode
			});
		} else if (!FlashlightModesUnlocked.ContainsKey(mode)) {
			Debug.Log($"Invalid flashlight mode: {mode}. No such mode exists.", DebugLevel.Warning);
		} else {
			Debug.Log($"Flashlight mode {mode} is already unlocked.", DebugLevel.Warning);
		}
	}

	//! Private Functions
	/// Set whether the player is looking right.
	private void SetIsLookingRight(bool value) {
		isLookingRight = value;
		var controller = PlayerController.Instance;
		var flashlightController = FlashlightController.Instance;
		if (controller != null) {
			controller.UpdateDirection();
		}
		if (flashlightController != null) {
			flashlightController.UpdateDirection();
		}
	}

	private void HandleBatteryDrain() {
		//? Timer
		drainTimer += Time.deltaTime;

		if (!FlashlightEnabled || batteryDrainInterval > drainTimer) return;

		Debug.Log("Draining Battery by 1", DebugLevel.Debug);

		Battery           = Mathf.Clamp(Battery--, 0, 100);
		FlashlightEnabled = Battery > 0;
		drainTimer        = 0f;
		
		UIController.Instance.UpdateUI();
	}

	/// Register the PlayerData instance.
	private static void RegisterInstance(PlayerData instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;

			Debug.Log("PlayerData initialized.");
		}
	}

	#endregion
}