using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ParticleController : MonoBehaviour {
	[SerializeField] ParticleSystem dustParticles;

	[Range(0, 20)] [SerializeField] int afterMovement;

	[FormerlySerializedAs("force")] [Range(0, 0.2f)] [SerializeField]
	float dustPeriod;

	[SerializeField] private Rigidbody2D crateRb;

	private float counter;

	private void Update() {
		counter += Time.deltaTime;

		if (!(Mathf.Abs(crateRb.linearVelocityX) > afterMovement)) return;
		if (counter > dustPeriod)
		{
			dustParticles.Play();
			counter = 0;
		}
	}
}