using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FlashlightGame;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
	//? Debug should be lowercase cause it's private, but I don't want that. Sooooooo...
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	private static DebugHandler Debug;

	public Transform pointA;
	public Transform pointB;

	[Header("Settings")]
	[SerializeField] private List<GameObject> enemyTypes;
	[SerializeField] private int   maxEnemies;
	[SerializeField] private float spawnInterval;

	private float timeSinceLastSpawn;
	private List<GameObject> spawnedEnemies = new List<GameObject>();
	
	private void Awake() {
		Debug = new DebugHandler("EnemySpawner ("                 +
		                         Math.Floor(transform.position.x) + ", " +
		                         Math.Floor(transform.position.y) + ")");
	}

	private void Update() {
		timeSinceLastSpawn += Time.deltaTime;
		SpawnEnemy();
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.CompareTag("Player")) {
			PlayerData.Instance.UpdateHealth(25);
		}
	}

	private void SpawnEnemy() {
		// ? Check if we can spawn an enemy (max limit and spawn interval)
		if (MaxEnemiesReached() || timeSinceLastSpawn < spawnInterval) return;

		//? Ensure we have at least one enemy type to spawn
		if (enemyTypes == null || enemyTypes.Count == 0) {
			Debug.Log("Missing enemy types! Please assign at least one enemy prefab to the spawner.", DebugLevel.Error);
			return;
		}

		;

		//? Random position within the spawn area defined by pointA and pointB
		if (pointA == null || pointB == null) {
			Debug.Log("Spawn points not set!", DebugLevel.Error);
			return;
		}

		var posA = pointA.position;
		var posB = pointB.position;

		//? Decide X position randomly between pointA and pointB
		var minX = Mathf.Min(posA.x, posB.x);
		var maxX = Mathf.Max(posA.x, posB.x);

		//? Y position is fixed at the spawner's Y position, Z is 0 for 2D
		var spawnPosition = new Vector3(UnityEngine.Random.Range(minX, maxX), transform.position.y, 0);

		//? Choose a random enemy prefab and instantiate it at the spawn position
		int enemyIndex  = UnityEngine.Random.Range(0, enemyTypes.Count);
		var enemyPrefab = enemyTypes[enemyIndex];

		if (enemyPrefab != null) {
			var enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
			spawnedEnemies.Add(enemy);
		}

		timeSinceLastSpawn = 0f;
	}

	private bool MaxEnemiesReached() {
		var currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
		return currentEnemies >= maxEnemies;
	}

	private void OnDrawGizmos() {
		if (pointA == null || pointB == null)
			return;

		var posA = pointA.position;
		var posB = pointB.position;

		//? Calculate center
		var center = (posA + posB) / 2f;

		//? Calculate size
		var size = new Vector3(
		                       Mathf.Abs(posA.x - posB.x),
		                       Mathf.Abs(posA.y - posB.y),
		                       1
		                      );

		//? Set gizmo color
		Gizmos.color = Color.yellow;

		// Draw filled box
		Gizmos.DrawCube(center, size);
	}
}