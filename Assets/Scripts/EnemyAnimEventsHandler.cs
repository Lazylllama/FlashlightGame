using System;
using Unity.Cinemachine;
using UnityEngine;

public class EnemyAnimEventsHandler : MonoBehaviour {
	private EnemyController          enemyController;
	private CinemachineImpulseSource impulseSource;
	private Transform                playerTransform;

	private void Start() {
		impulseSource   = GetComponent<CinemachineImpulseSource>();
		enemyController = GetComponent<EnemyController>();
		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
	}

	public void CameraImpulse(float impulse) {
		impulseSource.GenerateImpulseWithForce(impulse);
	}

	public void BearAttack() {
		CameraImpulse(2f);
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bearAttack, transform.position);
		enemyController.TryDealDamageToPlayer();
	}

	public void BearFootstep() {
		if (Vector3.Distance(playerTransform.position, transform.position) > 25f) return;
		CameraImpulse(1f / Vector3.Distance(playerTransform.position, transform.position));
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bearFootstep, transform.position);
	}

	public void FlapSound() {
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.enemyFlap, transform.position);
	}
}