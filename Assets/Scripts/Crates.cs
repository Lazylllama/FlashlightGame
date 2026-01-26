using UnityEngine;

public class Crates : MonoBehaviour {
	#region Fields

	//* Refs                                                   
	[SerializeField] private new ParticleSystem particleSystem;

	//* Cache
	private Vector2 lastPosition;

	#endregion

	#region Unity Functions

	private void Start() {
		particleSystem = GetComponent<ParticleSystem>();
	}

	private void FixedUpdate() {
		if (lastPosition.Equals(particleSystem.transform.position)) return;
		lastPosition = transform.position;

		particleSystem.Emit(1);
	}

	#endregion
}