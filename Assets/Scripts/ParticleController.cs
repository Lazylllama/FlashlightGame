using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour {
	#region Fields

	[Header("Refs")]
	[SerializeField] private Rigidbody2D crateRb;
	[SerializeField] private List<ParticleSystem> dustParticles;
	[SerializeField] private List<ParticleSystem> fallParticles;

	[Header("Settings")]
	[Range(0, 20)] [SerializeField] float afterMovement;
	[Range(0, 0.2f)] [SerializeField] float dustPeriod;


	//* State *//
	private float counter;
	private bool  onGround;

	#endregion

	#region Unity Functions

	private void OnTriggerEnter2D(Collider2D collision) {
		if (!collision.CompareTag("Ground")) return;

		// ? Emit fall particles
		foreach (ParticleSystem particle in fallParticles) {
			particle.Play();
		}

		onGround = true;
	}

	private void OnTriggerExit2D(Collider2D collision) {
		if (collision.CompareTag("Ground")) {
			onGround = false;
		}
	}

	#endregion

	#region Functions

	/// <summary>
	/// Emit particles when crate moves
	/// </summary>
	/// <param name="deltaX">Change in X position</param>
	public void CrateMovement(float deltaX) {
		if (Mathf.Abs(deltaX) < 0.01f) return;

		//? Update counter
		counter += Time.deltaTime;

		if (!onGround || !(counter > dustPeriod)) return;

		//? Move dust particles to correct side and emit
		foreach (ParticleSystem particle in dustParticles) {
			particle.transform.localPosition = new Vector3(-Mathf.Sign(deltaX) * afterMovement, -2.4f, 0);
			
			particle.Emit(1);
		}

		//? Reset counter
		counter = 0;
	}

	#endregion
}