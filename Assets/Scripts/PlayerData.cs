using System;
using System.Collections;
using System.Collections.Generic;
using FlashlightGame;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerData : MonoBehaviour {
	#region Fields

	public static  PlayerData   Instance;
	private static DebugHandler Debug;
	//* Respawn *//
	[SerializeField] private List<EnemyController> enemyControllers;

	//* Player Stats *//
	public int  Health  { get; set; } = 100;
	public int  Battery { get; set; } = 25;
	public bool IsDead  => Health <= 0;

	//* Player Data *//
	public bool IsWalkingRight { set; get; } = true;

	private bool isLookingRight = true;
	public bool IsLookingRight {
		set => SetIsLookingRight(value);
		get => isLookingRight;
	}

	public Vector2 CheckpointPosition { get; set; }

	private bool lowBattery;

	//* Player States *//
	public Dictionary<int, bool> FlashlightModesUnlocked { get; private set; } = new() {
		{ 1, true }, // TODO: Implement flashlight pickup
		{ 2, true }, // TODO: Implement flashlight level up (in-lore)
	};

	public bool  IsTalking         { get; set; }
	public bool  FlashlightEnabled { get; set; }
	public int   FlashlightMode    { get; private set; } = 1;
	public bool  IsInvulnerable    { get; private set; } = false;
	public float CrankSpeed        { get; set; }         = 0f;

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
	[SerializeField] private float invulnerabilityTime  = 1f;

	//* States *//
	private float drainTimer;
	private bool  lastCrankWasRight; // Controller Specific

	// Crank speed measurement
	[SerializeField] private float maxCrankFrequency = 6f; // how fast to crank to get max crank speed
	[SerializeField] private float crankDecayRate    = 1f; // how fast CrankSpeed goes back to 0
	private                  float lastCrankTime     = -1f;

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("PlayerData");
	}
	
	private void Awake() {
		Debug ??= new DebugHandler("PlayerData");
	}

	private void Start() => RegisterInstance(this);

	private void FixedUpdate() {
		HandleBatteryDrain();

		// Decay crank speed towards 0 when not actively cranking
		CrankSpeed = Mathf.MoveTowards(CrankSpeed, 0f, crankDecayRate * Time.deltaTime);

		//TODO: Optimize by only setting this when flashlight state changes and sum like that for crank speed? Or just not
		AudioManager.Instance.SetFlashlightState(FlashlightEnabled);
		AudioManager.Instance.SetCrankSpeedParameter(CrankSpeed);
	}

	#endregion

	#region Functions

	//! Public Functions
	/// <summary>
	/// Crank that flashlight.
	/// </summary>
	public void Crank() {
		if (Battery >= 100) {
			Debug.Log("Battery is full, cannot crank flashlight.");
			return;
		}

		UpdateCrankSpeedOnCrank();
		CrankLogic();
	}

	public void Crank(bool rightButton) {
		if (Battery >= 100) {
			Debug.Log("Battery is full, cannot crank flashlight.");
			return;
		}

		if (rightButton && lastCrankWasRight) {
			Debug.Log("Pressed right crank but last crank was also right, ignoring.");
			return;
		}

		lastCrankWasRight = rightButton;

		UpdateCrankSpeedOnCrank();
		CrankLogic();
	}

	/// When crank: update CrankSpeed (0-1) based on time since last crank!!
	private void UpdateCrankSpeedOnCrank() {
		var now = Time.time;
		if (lastCrankTime > 0f) {
			var delta      = now - lastCrankTime;
			var freq       = 1f / Mathf.Max(delta, 0.0001f);
			var normalized = Mathf.Clamp01(freq / maxCrankFrequency);
			CrankSpeed = normalized;
		} else {
			// First crank: set a small baseline crank speed
			CrankSpeed = 0.1f;
		}

		lastCrankTime = now;
	}

	private void CrankLogic() {
		Battery += Random.Range(1, 3);
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

	public void UpdateHealth(int difference, Vector3 hitSource) {
		UpdateHealth(difference);

		PlayerController.Instance.HandleKnockback(hitSource, 1f);
	}

	private void OnDeath() {
		RespawnEnemies();
		RespawnPlayer(); //!To be called when the player presses respawn!
	}

	private void RespawnEnemies() {
		for (int i = 0; i < enemyControllers.Count; i++) {
			enemyControllers[i].Respawn();
		}
	}

	private void RespawnPlayer() {
		var saveData = SaveController.Instance.GetSaveData();

		Health             = 100;
		Battery            = 100;
		transform.position = saveData.checkpointPosition;

		UIController.Instance.UpdateUI();
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

		if (PlayerController.Instance) PlayerController.Instance.UpdateDirection();
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
		yield return new WaitForSeconds(invulnerabilityTime);
		IsInvulnerable = false;
	}

	#endregion
}