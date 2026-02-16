using System;
using FlashlightGame;
using UnityEngine;


public class PlayerData : MonoBehaviour {
	#region Fields

	public static  PlayerData   Instance;
	private static DebugHandler Debug;

	//* Player Stats *//
	public int Health  { get; private set; } = 100;
	public int Battery { get; private set; } = 100;

	//* Player Data *//
	private bool isLookingRight;
	public bool IsLookingRight {
		set => SetIsLookingRight(value);
		get => isLookingRight;
	}

	//* Player States *//
	public bool FlashlightEnabled { get; set; }         = true;
	public int  FlashlightMode    { get; private set; } = 1;

	//* Options *//
	[SerializeField] private float drainInterval = 1f;

	//* States *//
	private float drainTimer;

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("PlayerData");
	}

	private void Start() => RegisterInstance(this);

	private void FixedUpdate() {
		//? Timer
		drainTimer += Time.deltaTime;

		if (!FlashlightEnabled || drainInterval > drainTimer) return;

		Debug.Log("Draining Battery by 1", DebugLevel.Debug);

		Battery           = Mathf.Clamp(Battery--, 0, 100);
		FlashlightEnabled = Battery > 0;
		drainTimer        = 0f;

		UIController.Instance.UpdateUI();
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
		FlashlightMode = increment ? FlashlightMode + 1 : FlashlightMode - 1;

		if (FlashlightMode < 1) FlashlightMode = 3;
		if (FlashlightMode > 3) FlashlightMode = 1;

		Debug.LogKv("Flashlight Mode Changed", DebugLevel.Debug, new object[] {
			"New Mode", FlashlightMode
		});
	}

	//! Private Functions
	/// Set whether the player is looking right.
	private void SetIsLookingRight(bool value) {
		isLookingRight = value;
		var controller = PlayerController.Instance;
		if (controller != null) {
			controller.UpdateDirection();
		}
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