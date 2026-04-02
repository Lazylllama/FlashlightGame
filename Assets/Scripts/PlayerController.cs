using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	#region Fields

	//* Instance
	public static PlayerController Instance;

	//* Refs
	private SpriteRenderer playerSprite;
	private Rigidbody2D    playerRb;

	#endregion

	#region Unity Functions

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
		
		SaveController.RegisterPlayer(gameObject);
	}

	private void Start() {
		playerSprite = GetComponentInChildren<SpriteRenderer>();
		playerRb     = GetComponent<Rigidbody2D>();

		UpdateDirection();
	}

	#endregion

	#region Functions

	/// <summary>
	/// Update the player's sprite direction based on the PlayerData's IsLookingRight property.
	/// </summary>
	public void UpdateDirection() {
		if (!PlayerData.Instance) return;
		var rotationY = PlayerData.Instance.IsLookingRight ? 0 : 180;
		playerSprite.transform.rotation = new Quaternion(0, rotationY, 0, 0);
	}
	
	/// <summary>
	/// Respawn player at last checkpoint
	/// </summary>
	public void RespawnPlayer(bool tpToCheckpoint = true) {
		if (!PlayerData.Instance) return;
		if (tpToCheckpoint) transform.position = PlayerData.Instance.CheckpointPosition;
		PlayerData.Instance.UpdateHealth(+100);
	}

	#endregion
}