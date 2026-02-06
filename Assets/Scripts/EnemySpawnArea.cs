using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnArea : MonoBehaviour {
	#region Fields

	[Header("Settings")]
	[SerializeField] private List<GameObject> enemyTypes;
	[SerializeField] private int   maxEnemies;
	[SerializeField] private float spawnInterval;

	private float timeSinceLastSpawn;

	#endregion

	#region Unity Functions

	private void Start() {
		timeSinceLastSpawn = spawnInterval; //? Allow spawning immediately
	}

	private void Update() {
		timeSinceLastSpawn += Time.deltaTime;
		SpawnEnemy();
	}

	private void OnDrawGizmos() {
		Gizmos.color = new Color(1, 0, 0, 0.2f);
		Gizmos.DrawCube(transform.position, transform.localScale);
	}

	#endregion

	#region Functions

	private void SpawnEnemy() {
		if (MaxEnemiesReached() || timeSinceLastSpawn < spawnInterval) return;
		
		//? Random x position within spawn area
		//* Takes left half of X values and right half and then chooses something in between
		var randomX = UnityEngine.Random.Range(-transform.localScale.x / 2f, transform.localScale.x / 2f);
		
		//? Random enemy type
		var randomIndex = UnityEngine.Random.Range(0, enemyTypes.Count);
		
		//? Set spawn position and instantiate enemy
		var spawnPosition = new Vector2(transform.position.x + randomX, transform.position.y);
		var enemy = Instantiate(enemyTypes[randomIndex], spawnPosition, Quaternion.identity);
		
		//? Set as child of spawn area for organization
		enemy.transform.SetParent(enemy.transform, true);

		timeSinceLastSpawn = 0f;
	}

	//? Only checks for enemies owned by that spawner.
	// TODO(@lazylllama): Edit to include enemies inside spawn area maybe?
	private bool MaxEnemiesReached() {
		var currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
		return currentEnemies >= maxEnemies;
	}

	#endregion
}