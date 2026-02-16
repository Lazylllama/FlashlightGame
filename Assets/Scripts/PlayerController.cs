using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	#region Fields

	//* Instance
	public static PlayerController Instance;

	//* Refs
	[SerializeField] private SpriteRenderer playerSprite;

	#endregion

	#region Unity Functions

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	#endregion

	#region Functions

	/// <summary>
	/// Update the player's sprite direction based on the PlayerData's IsLookingRight property.
	/// </summary>
	public void UpdateDirection() {
		var rotationY = PlayerData.Instance.IsLookingRight ? 0 : 180;

		playerSprite.transform.rotation = new Quaternion(0, rotationY, 0, 0);
	}

	#endregion
}