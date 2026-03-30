using System;
using System.Collections;
using System.Collections.Generic;
using FlashlightGame;
using UnityEngine;


public class PlayerData : MonoBehaviour {
	#region Fields

	public static  PlayerData   Instance;
	private static DebugHandler Debug;

	//* Player Stats *//
	public int  Health  { get; set; }         = 100;
	public int  Battery { get; set; } = 25;
	public bool IsDead  => Health <= 0;

	//* Player Data *//
	private bool isWalkingRight;
	public bool IsWalkingRight {
		set => isWalkingRight = value;
		get => isWalkingRight;
	}

	private bool isLookingRight;
	public bool IsLookingRight {
		set => SetIsLookingRight(value);
		get => isLookingRight;
	}

	private bool lowBattery;

	//* Player States *//
	public Dictionary<int, bool> FlashlightModesUnlocked { get; private set; } = new Dictionary<int, bool>() {
		{ 1, true }, // TODO: Implement flashlight pickup
		{ 2, true }, // TODO: Implement flashlight level up (in-lore)
	};

	public bool IsTalking         { get; set; }
	public bool FlashlightEnabled { get; set; }
	public int  FlashlightMode    { get; private set; } = 1;
	public bool IsInvulnerable    { get; private set; } = false;

	//* Mood States *//
	//? Relieved = player is at a checkpoint.
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
	[SerializeField] private float invulnerabiltyTime   = 1f;

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
	/// Crank that flashlight.
	/// </summary>
	public void Crank() {
		if (Battery >= 100) Debug.Log("Battery is full, cannot crank flashlight.");
		Battery += 1;
		Battery =  Mathf.Clamp(Battery, 0, 100);
		UIController.Instance.UpdateUI();
		if (Battery <= 20 || !lowBattery) return;
		lowBattery = false;
		FlashlightController.Instance.LowBatteryWarning(false);
	}
	
	/// <summary>
	/// Update the player's battery by the specified difference. Clamps between 0 and 100.
	/// </summary>
	/// <param name="difference">-100 to +100 Health Points</param>
	public void UpdateHealth(int difference) {
		Health += difference;
		Health =  Mathf.Clamp(Health, 0, 100);
		
		UIController.Instance.UpdateUI();

		if (IsDead) OnDeath(); 
		else StartCoroutine(MakeInvulnerable());
	}

	private void OnDeath() {
		Debug.Log("Player died, respawning...");
		RespawnManager.Instance.Respawn(PlayerMovement.Instance.gameObject);
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
		if (!GameController.Instance || !GameController.Instance.InActiveGame) return;

		isLookingRight = value;

		var controller           = PlayerController.Instance;
		var flashlightController = FlashlightController.Instance;
		if (controller) {
			controller.UpdateDirection();
		}

		if (flashlightController) {
			flashlightController.UpdateDirection();
		}
	}

	private void HandleBatteryDrain() {
		//* Return before timer to avoid abusing timer by turning flashlight on off repeatedly...
		if (!FlashlightEnabled) return;

		//? Timer
		drainTimer += Time.deltaTime;

		if (batteryDrainInterval > drainTimer) return;

		Battery           -= 1;
		Battery           =  Mathf.Clamp(Battery, 0, 100);
		FlashlightEnabled =  Battery > 0;
		drainTimer        =  0f;
		
		UIController.Instance.UpdateUI();
		
		if (Battery > 20 || lowBattery) return;
		lowBattery = true;
		FlashlightController.Instance.LowBatteryWarning(true);
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

	#region Coroutines

	private IEnumerator MakeInvulnerable() {
		IsInvulnerable = true;
		yield return new WaitForSeconds(invulnerabiltyTime);
		IsInvulnerable = false;
	}

	#endregion
}