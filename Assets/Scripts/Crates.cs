using UnityEngine;

public class Crates : MonoBehaviour {
	#region Fields

	//* Refs *//
	private ParticleController particleController;

	//* State *//
	private Vector2 lastPosition;

	#endregion

	#region Unity Functions

	private void Start() {
		particleController = GetComponentInChildren<ParticleController>();
	}

	private void FixedUpdate() {
		Vector2 currentPosition = transform.position;
		var     delta           = currentPosition - lastPosition;

		//? No change = Ignore
		if (delta == Vector2.zero) return;
		
		particleController.CrateMovement(delta.x);

		lastPosition = currentPosition;
	}

	#endregion
}