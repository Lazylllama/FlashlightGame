using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyV2 : MonoBehaviour {
	[Header("Points")]
	[SerializeField] private bool      goA;
	[SerializeField] private GameObject pointA;
	[SerializeField] private GameObject pointB;

	private Vector3 startPointA;
	private Vector3 startPointB;
	

	[Header("PlayerDetection")]
	[SerializeField] private GameObject player;
	[SerializeField] private float dist;
	private                  bool  playerInSight;
	
	//? Components
	private Collider2D coll;
	
	//? States
	private bool          directPath;
	private List<Vector3> points = new();

	private void Start() {
		coll = GetComponent<Collider2D>();
		
		startPointA = pointA.transform.position;
		startPointB = pointB.transform.position;
	}

	private void Update() {
		points.Clear();
		directPath = false;
		CheckPlayerSight();
		var target = playerInSight? player : goA ? pointA : pointB;
		PathFindTo(target);
	}

	private void CheckPlayerSight() {
		if (Vector3.Distance(player.transform.position, transform.position) > dist) return;
		
		var hit = Physics2D.Linecast(transform.position, player.transform.position);
		DebugLine(transform.position, hit, "line", new Vector3(), player.transform.position);
		playerInSight = hit.collider.gameObject.CompareTag("Player");
	}

	private void PathFindTo(GameObject target) {
		var foundPath = false;
		var startPos = transform.position;
		while (!foundPath) {
			var hit = Physics2D.Linecast(startPos, target.transform.position);
			DebugLine(startPos, hit, "line", new Vector3(), target.transform.position);

			if (hit.collider.gameObject == target) {
				foundPath = true;
				if (points.Count == 0) directPath = true;
				break;
			}
			
			var tempExclude = points;
			var tempCounter = 0;
			var closestCorner = Vector3.zero;
			while (true) {
				closestCorner                  = GetClosestCorner(startPos, hit, tempExclude);
				var tempHit                        = Physics2D.Linecast(startPos, closestCorner);
				if (!tempHit.collider) break;
				tempExclude.Add(closestCorner);
				tempCounter++;
				if (tempCounter == 4) { 
					Debug.LogError("No valid corners found, breaking loop to prevent infinite loop.");
					break;
				}
			}

			if (closestCorner == Vector3.zero) {
				print("No valid corners found, breaking loop.");
				break;
			}
			hit = Physics2D.Linecast(startPos, closestCorner);
			DebugLine(startPos, hit, "line", new Vector3(), closestCorner);
			points.Add(closestCorner);
			startPos = closestCorner;
			if (points.Count == 10) {
				break;
			}
		}
	}

	private Vector3 GetClosestCorner(Vector3 origin, RaycastHit2D hit, List<Vector3> exclude, Vector3 checkPos = new()) {
		if (checkPos == new Vector3()) checkPos = origin; 
		var corners = new List<Vector3>();
		for (int i = 0; i < 4; i++) {
			Vector3 temp;
			GameObject newGameObject = hit.collider.gameObject;
			switch (i) {
				case 0:
					temp = hit.collider.bounds.max + new Vector3(1.0f, 1.0f, 0.0f);
					if (exclude.Contains(temp)) continue;
					corners.Add(temp);
					break;
				case 1:
					temp = hit.collider.bounds.min - new Vector3(1.0f, 1.0f, 0.0f);
					if (exclude.Contains(temp)) continue;
					corners.Add(temp);
					break;
				case 2:
					temp = new Vector3(hit.collider.bounds.max.x + 1.0f, hit.collider.bounds.min.y - 1.0f, transform.position.z);
					if (exclude.Contains(temp)) continue;
					corners.Add(temp);
					break;
				case 3:
					temp = new Vector3(hit.collider.bounds.min.x - 1.0f, hit.collider.bounds.max.y + 1.0f, transform.position.z);
					if (exclude.Contains(temp)) continue;
					corners.Add(temp);
					break;
			}
		}

		Vector3 result = Vector3.zero;
		;
		foreach (var corner in corners) {
			if (Vector3.Distance(corner, checkPos) < Vector3.Distance(result, checkPos) || result == Vector3.zero) {
				result = corner;
			}
		}
		return result;
	}

	private void DebugLine(Vector3 origin, RaycastHit2D hit, string type = "Line", Vector3 direction = new(), Vector3 end = new()) {
		if(hit.collider) Debug.DrawLine(origin, hit.point, hit.collider.gameObject.CompareTag("Player") ? Color.green : Color.red);
		else if(type.ToLower() == "line") Debug.DrawLine(origin, end, Color.red);
		else Debug.DrawRay(origin, direction, Color.red);
	}

	private void OnDrawGizmos() {
		foreach (var point in points) {
			Gizmos.DrawSphere(point, 0.1f);
		}
	}
}
