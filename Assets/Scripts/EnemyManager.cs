using UnityEngine;

public class EnemyManager : MonoBehaviour {
	public static EnemyManager Instance { get; private set; }

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	public void RespawnAll() {
		var enemies = FindObjectsByType<EnemyController>(
		                                                 FindObjectsInactive.Include,
		                                                 FindObjectsSortMode.None
		                                                );

		foreach (var enemy in enemies) {
			//* Conflict or sum shit idk
			//enemy.ResetEnemy();
		}
	}
}