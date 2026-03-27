using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour {
	#region Fields

	[Header("Refs")]
	[SerializeField] private List<ParticleSystem> dustParticles;
	[SerializeField] private List<ParticleSystem> fallParticles;

	[Header("Settings")]
	[Range(-20f, 20.0f)] [SerializeField] private float afterMovement;
	[Range(0,    0.2f)] [SerializeField] private float dustPeriod;
	[Range(-5f,  5f)] [SerializeField]   private float dustLocalOffsetY;

	//* State *//
	private float counter;
	private int   groundCount;
	private bool  OnGround => groundCount > 0;
	private int   groundLayer;

	#endregion

	#region Unity Functions

	private void Awake() {
		groundLayer = LayerMask.NameToLayer("Ground");
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.layer != groundLayer) return;


		// ? Emit fall particles
		if (groundCount == 0) {
			foreach (var particle in fallParticles) {
				particle.Play();
			}

			counter = 0;
		}

		groundCount++;

		Debug.Log($"On Ground (count: {groundCount})");
	}

	private void OnTriggerExit2D(Collider2D collision) {
		if (collision.gameObject.layer != groundLayer) return;
		groundCount = Mathf.Max(0, groundCount - 1);


		Debug.Log($"On Ground (count: {groundCount})");
	}

	#endregion

	#region Functions

	/// <summary>
	/// Emit particles when the crate moves
	/// </summary>
	/// <param name="deltaX">Change in X position</param>
	public void CrateMovement(float deltaX) {
		if (Mathf.Abs(deltaX) < 0.01f) return;

		//? Update counter
		counter += Time.deltaTime;
		if (!OnGround || counter <= dustPeriod) return;

		//? Move dust particles to the correct side and emit
		foreach (var particle in dustParticles) {
			particle.transform.localPosition = new Vector3(-Mathf.Sign(deltaX) * afterMovement, dustLocalOffsetY, 0);

			particle.Emit(1);
			
		}

		//? Reset counter
		counter = 0;
	}

	#endregion
}