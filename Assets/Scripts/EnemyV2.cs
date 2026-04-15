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
		DebugLine(hit, "line", new Vector3(), player.transform.position);
		playerInSight = hit.collider.gameObject.CompareTag("Player");
	}

	private void PathFindTo(GameObject target) {
		var foundPath = false;
		var startPos = transform.position;
		while (!foundPath) {
			var hit = Physics2D.Linecast(startPos, target.transform.position);
			DebugLine(hit, "line", new Vector3(), target.transform.position);

			if (hit.collider.gameObject == target) {
				foundPath = true;
				if (points.Count == 0) directPath = true;
				break;
			}

			var closestCorner = GetClosestCorner(hit);
			print(closestCorner);
			points.Add(closestCorner);
			if (points.Count == 10) {
				break;
			}
		}
	}

	private Vector3 GetClosestCorner(RaycastHit2D hit) {
		var corners = new List<Vector3>();
		for (int i = 0; i < 4; i++) {
			Vector3 temp;
			switch (i) {
				case 0:
					temp = hit.collider.bounds.max + coll.bounds.max - transform.position;
					corners.Add(temp);
					break;
				case 1:
					temp = hit.collider.bounds.min - coll.bounds.min - transform.position;
					corners.Add(temp);
					break;
				case 2:
					temp = new Vector3(hit.collider.bounds.max.x + coll.bounds.max.x - transform.position.x, hit.collider.bounds.min.y - coll.bounds.max.y - transform.position.y, hit.collider.bounds.center.z);
					corners.Add(temp);
					break;
				case 3:
					temp = new Vector3(hit.collider.bounds.min.x - coll.bounds.max.x - transform.position.x, hit.collider.bounds.max.y + coll.bounds.max.y - transform.position.y, hit.collider.bounds.center.z);
					corners.Add(temp);
					break;
			}
		}

		Vector3 result = Vector3.zero;
		foreach (var corner in corners) {
			if (Vector3.Distance(corner, transform.position) < dist || result == Vector3.zero) {
				result = corner;
			}
		}
		return result;
	}

	private void DebugLine(RaycastHit2D hit, string type = "Line", Vector3 direction = new(), Vector3 end = new()) {
		if(hit.collider) Debug.DrawLine(transform.position, hit.point, hit.collider.gameObject.CompareTag("Player") ? Color.green : Color.red);
		else if(type.ToLower() == "line") Debug.DrawLine(transform.position, end, Color.red);
		else Debug.DrawRay(transform.position, direction, Color.red);
	}

	private void OnDrawGizmos() {
		foreach (var point in points) {
			Gizmos.DrawSphere(point, 0.1f);
		}
	}
}
