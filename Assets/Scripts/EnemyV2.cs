using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyV2 : MonoBehaviour {
	[Header("Points")]
	[SerializeField] private bool goA;
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
		var target = playerInSight ? player : goA ? pointA : pointB;
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
		var startPos  = transform.position;
		while (!foundPath) {
			var hit = Physics2D.Linecast(startPos, target.transform.position);
			DebugLine(startPos, hit, "line", new Vector3(), target.transform.position);

			if (hit.collider.gameObject == target) {
				foundPath = true;
				if (points.Count == 0) directPath = true;
				break;
			}

			var tempExclude   = new List<Vector3>();
			var tempCounter   = 0;
			var closestCorner = Vector3.zero;
			while (true) {
				closestCorner = GetClosestCorner(startPos, hit, tempExclude, targetPos: target.transform.position);
				var tempHit = Physics2D.Linecast(startPos, closestCorner);
				DebugLine(startPos, tempHit, "line", new Vector3(), closestCorner);
				if (!tempHit.collider) break;
				foreach (var exclude in tempExclude) {
				}

				tempExclude.Add(closestCorner);
				tempCounter++;
				if (tempCounter == 4) {
					break;
				}
			}

			if (closestCorner == Vector3.zero) {
				break;
			}

			hit = Physics2D.Linecast(startPos, closestCorner);
			DebugLine(startPos, hit, "line", new Vector3(), closestCorner);
			points.Add(closestCorner);
			startPos = closestCorner;
			if (points.Count == 20) {
				break;
			}
		}
	}

	private Vector3 GetClosestCorner(Vector3 origin, RaycastHit2D hit, List<Vector3> exclude, Vector3 checkPos = new(),
	                                 Vector3 targetPos = new()) {
		if (checkPos == new Vector3()) checkPos = origin;
		var corners                             = new List<Vector3>();
		for (int i = 0; i < 4; i++) {
			Vector3    temp;
			GameObject newGameObject = hit.collider.gameObject;
			switch (i) {
				case 0:
					temp = hit.collider.bounds.max + new Vector3(0.5f, 0.5f, 0.0f);
					if (exclude.Contains(temp)) {
						continue;
					}

					corners.Add(temp);
					break;
				case 1:
					temp = hit.collider.bounds.min - new Vector3(0.5f, 0.5f, 0.0f);
					if (exclude.Contains(temp)) {
						continue;
					}

					corners.Add(temp);
					break;
				case 2:
					temp = new Vector3(hit.collider.bounds.max.x + 0.5f, hit.collider.bounds.min.y - 0.5f,
					                   transform.position.z);
					if (exclude.Contains(temp)) {
						continue;
					}

					corners.Add(temp);
					break;
				case 3:
					temp = new Vector3(hit.collider.bounds.min.x - 0.5f, hit.collider.bounds.max.y + 0.5f,
					                   transform.position.z);
					if (exclude.Contains(temp)) {
						continue;
					}

					corners.Add(temp);
					break;
			}
		}

		Vector3 result = Vector3.zero;
		;
		foreach (var corner in corners) {
			if ((result == Vector3.zero || Vector3.Distance(corner, targetPos) < Vector3.Distance(result, targetPos)) &&
			    corner != origin) {
				result = corner;
			}
		}

		return result;
	}

	private void DebugLine(Vector3 origin, RaycastHit2D hit, string type = "Line", Vector3 direction = new(),
	                       Vector3 end = new()) {
		if (hit.collider)
			Debug.DrawLine(origin, hit.point, hit.collider.gameObject.CompareTag("Player") ? Color.green : Color.red);
		else if (type.ToLower() == "line") Debug.DrawLine(origin, end, Color.red);
		else Debug.DrawRay(origin, direction, Color.red);
	}

	private void OnDrawGizmos() {
		foreach (var point in points) {
			Gizmos.DrawSphere(point, 0.1f);
		}

		foreach (var sigma in ColliderBoundsRayLineCast("line", coll, Vector3.zero, Vector3.right + Vector3.up)) {
			Gizmos.DrawSphere(sigma, 0.1f);
		}
	}

	private List<Vector3> ColliderBoundsRayLineCast(string  type,        Collider2D coll, Vector3 start,
	                                                Vector3 end = new(), Vector3    dir = new()) {
		Vector2 direction = new();
		if (type.ToLower() == "raycast" || type.ToLower() == "ray") {
			direction = dir.normalized;
		} else if (type.ToLower() == "linecast" || type.ToLower() == "line") {
			var temp = Mathf.Atan((end.x - start.x) / (end.y - start.y));
			print(temp);
			direction = new Vector2(Mathf.Sin(temp), Mathf.Cos(temp)) * (start.y <= end.y ? 1 : -1);
			direction = direction.normalized;
		}

		switch (coll) {
			case CircleCollider2D:
				var circleColl = coll.GetComponent<CircleCollider2D>();
				var radius     = circleColl.radius;
				var bounds     = new List<Vector3>();
				bounds.Add(new Vector3(start.x + radius * direction.x * -1, start.y + radius * direction.y,        0));
				bounds.Add(new Vector3(start.x + radius * direction.x,      start.y + radius * (direction.y * -1), 0));
				foreach (var bound in bounds) {
					print(bound + " " + direction);
				}

				return bounds;
			default:
				print("WE DON'T SUPPORT THIS STUPID FUCKING COLLIDER YET HAHAHA!");
				return new List<Vector3>();
		}
	}
}