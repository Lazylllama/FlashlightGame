using UnityEngine;

public class PlayerData : MonoBehaviour {
	#region Fields

	public static PlayerData Instance;

	//* Player Stats
	public int Health  { get; private set; } = 100;
	public int Battery { get; set; }         = 100;

	//* Player Data
	private bool isLookingRight;
	public bool IsLookingRight {
		set => SetIsLookingRight(value);
		get => isLookingRight;
	}

	#endregion

	#region Unity Functions

	private void Awake() {
		//* Instance
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	#endregion

	#region Functions

	//! Public Functions
	/// <summary>
	/// Deal damage to the player. Caps out at 100.
	/// </summary>
	/// <param name="damage">1-100 Health Points</param>
	public void TakeDamage(int damage) {
		if (damage < 0) return;
		Health -= damage;
	}

	/// <summary>
	/// Restore health to the player. Caps ou at 100.
	/// </summary>
	/// <param name="amount">1-100 Health Points</param>
	public void RestoreHealth(int amount) {
		if (amount < 0) return;
		Health += amount;
		if (Health > 100) Health = 100;
	}
	
	//! Private Functions
	/// Set whether the player is looking right.
	private void SetIsLookingRight(bool value) {
		isLookingRight = value;
		PlayerController.Instance.UpdateDirection();
	}

	#endregion
}