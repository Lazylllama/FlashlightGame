using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	public Transform pointA;
	public Transform pointB;
	
	[Header("Settings")]
	[SerializeField] private List<GameObject> enemyTypes;
	[SerializeField] private int   maxEnemies;
	[SerializeField] private float spawnInterval;

	private float timeSinceLastSpawn;
	
	private void Start()
	{
		timeSinceLastSpawn = spawnInterval;
		SpawnEnemy();
			
	}

	private void Update() {
		timeSinceLastSpawn += Time.deltaTime;
	}

	private void SpawnEnemy()
	{
		if (MaxEnemiesReached() || timeSinceLastSpawn < spawnInterval) return;
		
		//? Random x position within spawn area
		
		
		
		
		timeSinceLastSpawn = 0f;
	}
	
	private bool MaxEnemiesReached() {
		var currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
		return currentEnemies >= maxEnemies;
	}

	private void OnDrawGizmos()
	{
		if (pointA == null || pointB == null)
			return;

		
		var posA = pointA.position;
		var posB = pointB.position;

		//Calculate center
		var center = (posA + posB) / 2f;

		//Calculate size
		var size = new Vector3(
		                       Mathf.Abs(posA.x - posB.x),
		                       Mathf.Abs(posA.y - posB.y),
		                       Mathf.Abs(posA.z - posB.z)
		                      );

		// Set gizmo color
		Gizmos.color = Color.darkOliveGreen;

		// Draw filled box
		Gizmos.DrawCube(center, size);

		// Optional: draw wireframe for clarity
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(center, size);
	}
}
