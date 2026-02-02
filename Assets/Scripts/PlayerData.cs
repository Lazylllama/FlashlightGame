using System;
using FlashlightGame;
using UnityEngine;


public class PlayerData : MonoBehaviour {
	#region Fields

	public static PlayerData Instance;

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
	public bool FlashlightEnabled { get; set; } = true;

	//* Options *//
	[SerializeField] private float drainInterval = 1f;

	//* States *//
	private float drainTimer;

	#endregion

	#region Unity Functions

	private void Awake() => RegisterInstance(this);

	private void FixedUpdate() {
		//? Timer
		drainTimer = Time.deltaTime;

		//? Drain Battery
		if (!FlashlightEnabled || drainInterval > drainTimer) return;
		Battery = Mathf.Clamp(Battery--, 0, 100);
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

			DebugHandler.Log("PlayerData initialized.");
		}
	}

	#endregion
}