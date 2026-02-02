using System;
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
		Battery--;
		if (Battery < 0) Battery = 0;
		UIController.Instance.UpdateUI();
	}

	#endregion

	#region Functions

	//! Public Functions
	/// <summary>
	/// Deal damage to the player. Caps out at 100.
	/// </summary>
	/// <param name="damage">1-100 Health Points</param>
	public void TakeDamage(int damage) {
		Health = Mathf.Clamp(Health - damage, 0, 100);

	}

	/// <summary>
	/// Restore health to the player. Caps out at 100.
	/// </summary>
	/// <param name="amount">1-100 Health Points</param>
	public void RestoreHealth(int amount) {
		Health = Mathf.Clamp(Health + amount, 0, 100);
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

			DebugHandler.Instance.Log("PlayerData initialized.");
		}
	}

	#endregion
}