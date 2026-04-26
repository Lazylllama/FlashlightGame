using System;
using Unity.Cinemachine;
using UnityEngine;

public class EnemyAnimEventsHandler : MonoBehaviour {
	//[SerializeField] private 
	
	private CinemachineImpulseSource impulseSource;

	private void Start() {
		impulseSource = GetComponent<CinemachineImpulseSource>();
	}

	public void CameraImpulse(float impulse) {
		impulseSource.GenerateImpulseWithForce(impulse);
	}

	//public void 
	
	public void FlapSound() {
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.enemyFlap, transform.position);
	}
}