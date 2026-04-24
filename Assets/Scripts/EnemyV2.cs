using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyV2 : MonoBehaviour {
	[Header("Settings")]
	[SerializeField] private float playerDetectionRange = 15f;
	[SerializeField] private float chargeTime  = 1f;
	[SerializeField] private float chargeRange = 5f;
	[SerializeField] private float chargeSpeed = 2f;
	[SerializeField] private float speed       = 1f;

	[Header("Points")]
	[SerializeField] private bool goA;
	[SerializeField] private GameObject pointA, pointB, lastPlayerPoint;

	private Collider2D pointAColl, pointBColl, lastPlayerPointColl;


	[Header("PlayerDetection")]
	[SerializeField] private GameObject player;

	private bool playerInSight;

	//? Components
	private Collider2D coll;

	//? States
	private bool          directPath, goToLastPlayerPos;
	private List<Vector3> points               = new();
	private float         changeTargetCooldown = 0.5f, speedMult = 1f;
	private Coroutine     chargeRoutine;

	#region Unity Functions

	private void Start() {
		coll = GetComponent<Collider2D>();

		pointAColl          = pointA.GetComponent<Collider2D>();
		pointBColl          = pointB.GetComponent<Collider2D>();
		lastPlayerPointColl = lastPlayerPoint.GetComponent<Collider2D>();
	}

	private void Update() {
		changeTargetCooldown -= Time.deltaTime;
		points.Clear();
		directPath = false;
		CheckTargetChange();
		CheckPlayerSight();
		var target = goToLastPlayerPos ? lastPlayerPoint : goA ? pointA : pointB;
		PathFindTo(target);
	}

	#endregion

	private void CheckTargetChange() {
		if (((goA  && Vector3.Distance(pointA.transform.position, transform.position) < 0.1f) ||
		     (!goA && Vector3.Distance(pointB.transform.position, transform.position) < 0.1f)) &&
		    changeTargetCooldown <= 0 && playerInSight == false) {
			goA = !goA;
			if (goA) {
				pointAColl.enabled = true;
				pointBColl.enabled = false;
			} else {
				pointAColl.enabled = false;
				pointBColl.enabled = true;
			}

			changeTargetCooldown = 0.5f;
		}

		if (!playerInSight && goToLastPlayerPos &&
		    Vector3.Distance(lastPlayerPoint.transform.position, transform.position) < 0.5f) {
			goToLastPlayerPos           = false;
			lastPlayerPointColl.enabled = true;
		}
	}

	private void CheckPlayerSight() {
		if (Vector3.Distance(player.transform.position, transform.position) > playerDetectionRange) {
			playerInSight = false;
			return;
		}

		var hit = Physics2D.Linecast(transform.position, player.transform.position);
		DebugLine(transform.position, hit, "line", new Vector3(), player.transform.position);
		if (!hit.collider) return;
		playerInSight = hit.collider.gameObject.CompareTag("Player");
		if (playerInSight) {
			lastPlayerPoint.transform.position = player.transform.position;
			goToLastPlayerPos                  = true;
			lastPlayerPointColl.enabled        = true;
			if (chargeRoutine                                                            == null &&
			    Vector3.Distance(lastPlayerPoint.transform.position, transform.position) < chargeRange) {
				chargeRoutine = StartCoroutine(Charge(lastPlayerPoint.transform.position));
			}
		}
	}

	private void PathFindTo(GameObject target) {
		var foundPath = false;
		var startPos  = transform.position;
		while (!foundPath) {
			var hit = ColliderBoundsRayLineCast("line", coll, startPos, target.transform.position);

			if (hit.collider && hit.collider.gameObject == target) {
				foundPath = true;
				if (points.Count == 0) directPath = true;
				break;
			}

			var tempExclude   = new List<Vector3>();
			var tempCounter   = 0;
			var closestCorner = Vector3.zero;
			while (true) {
				closestCorner = GetClosestCorner(startPos, hit, tempExclude, targetPos: target.transform.position);
				//var tempHit = Physics2D.Linecast(startPos, closestCorner);
				//DebugLine(startPos, tempHit, "line", new Vector3(), closestCorner);
				var tempHit = ColliderBoundsRayLineCast("line", coll, startPos, closestCorner);
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

			//hit = Physics2D.Linecast(startPos, closestCorner);
			hit = ColliderBoundsRayLineCast("line", coll, startPos, closestCorner);
			DebugLine(startPos, hit, "line", new Vector3(), closestCorner);
			points.Add(closestCorner);
			startPos = closestCorner;
			if (points.Count == 20) {
				break;
			}
		}

		if (points.Count > 0 && directPath == false) {
			transform.position = Vector3.MoveTowards(transform.position, points[0], speed * Time.deltaTime * speedMult);
		} else if (directPath == true) {
			transform.position = Vector3.MoveTowards(transform.position, target.transform.position,
			                                         speed * Time.deltaTime * speedMult);
		}
	}

	private Vector3 GetClosestCorner(Vector3 origin, RaycastHit2D hit, List<Vector3> exclude, Vector3 checkPos = new(),
	                                 Vector3 targetPos = new()) {
		if (!hit.collider) return Vector3.zero;
		if (checkPos == new Vector3()) checkPos = origin;
		var corners                             = new List<Vector3>();
		var radius                              = coll.GetComponent<CircleCollider2D>().radius * 1.01f;
		for (int i = 0; i < 4; i++) {
			Vector3 temp;
			var     newGameObject = hit.collider.gameObject;
			switch (i) {
				case 0:
					temp = hit.collider.bounds.max + new Vector3(radius, radius, 0.0f);
					if (exclude.Contains(temp)) {
						continue;
					}

					corners.Add(temp);
					break;
				case 1:
					temp = hit.collider.bounds.min - new Vector3(radius, radius, 0.0f);
					if (exclude.Contains(temp)) {
						continue;
					}

					corners.Add(temp);
					break;
				case 2:
					temp = new Vector3(hit.collider.bounds.max.x + radius, hit.collider.bounds.min.y - radius,
					                   transform.position.z);
					if (exclude.Contains(temp)) {
						continue;
					}

					corners.Add(temp);
					break;
				case 3:
					temp = new Vector3(hit.collider.bounds.min.x - radius, hit.collider.bounds.max.y + radius,
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
			    corner != origin && Vector3.Distance(corner, transform.position) > 0.01f) {
				result = corner;
			}
		}

		return result;
	}

	private void DebugLine(Vector3 origin,      RaycastHit2D hit, string type = "Line", Vector3 direction = new(),
	                       Vector3 end = new(), GameObject   target = null) {
		if (hit.collider)
			Debug.DrawLine(origin, hit.point,
			               (hit.collider.gameObject.CompareTag("Player") ||
			                (target && hit.collider.gameObject == target))
				               ? Color.green
				               : Color.red);
		else if (type.ToLower() == "line") Debug.DrawLine(origin, end, Color.red);
		else Debug.DrawRay(origin, direction, Color.red);
	}

	private void OnDrawGizmos() {
		foreach (var point in points) {
			Gizmos.DrawSphere(point, 0.1f);
		}
	}

	private RaycastHit2D ColliderBoundsRayLineCast(string  type,        Collider2D coll, Vector3 start,
	                                               Vector3 end = new(), Vector3    dir = new()) {
		Vector2 direction = new();
		if (type.ToLower() == "raycast" || type.ToLower() == "ray") {
			direction = dir.normalized;
		} else if (type.ToLower() == "linecast" || type.ToLower() == "line") {
			var temp = Mathf.Atan((end.x - start.x) / (end.y - start.y));
			print(temp);
			direction = new Vector2(Mathf.Cos(temp) * -1, Mathf.Sin(temp));
			direction = direction.normalized;
		}

		switch (coll) {
			case CircleCollider2D:
				var circleColl  = coll.GetComponent<CircleCollider2D>();
				var radius      = circleColl.radius;
				var startPoints = new List<Vector3>();
				startPoints.Add(new Vector3(start.x + radius * direction.x * 0.99f,
				                            start.y + radius * direction.y * 0.99f, 0));
				startPoints.Add(new Vector3(start.x + radius * direction.x        * -1 * 0.99f,
				                            start.y + radius * (direction.y * -1) * 0.99f,
				                            0));

				if (type.ToLower() == "line" || type.ToLower() == "linecast") {
					var endPoints = new List<Vector3>();
					endPoints.Add(new Vector3(end.x + radius * direction.x * 0.99f,
					                          end.y + radius * direction.y * 0.99f, 0));
					endPoints.Add(new Vector3(end.x + radius * direction.x        * -1 * 0.99f,
					                          end.y + radius * (direction.y * -1) * 0.99f, 0));

					var hitList = new List<RaycastHit2D>();
					for (int i = 0; i < startPoints.Count; i++) {
						hitList.Add(Physics2D.Linecast(startPoints[i], endPoints[i]));
						DebugLine(startPoints[i], hitList[i], "line", new Vector3(), endPoints[i]);
					}

					RaycastHit2D finalHit = new();
					foreach (var hit in hitList) {
						if (!hit.collider) {
							continue;
						}

						if (hit.collider) {
							if (!finalHit.collider) {
								finalHit = hit;
							}

							if (Vector3.Distance(hit.point, start) < Vector3.Distance(finalHit.point, start)) {
								finalHit = hit;
							}
						} else {
							continue;
						}
					}

					return finalHit;
				}

				break;
			default:
				print("Only CircleCollider2D is supported at the moment");
				return new RaycastHit2D();
		}

		return new RaycastHit2D();
	}

	#region Coroutines

	private IEnumerator Charge(Vector3 targetPos) {
		speedMult = 0;
		yield return new WaitForSeconds(chargeTime);
		speedMult = chargeSpeed;
		while (true) {
			if (Vector3.Distance(transform.position, targetPos) < 0.1f) {
				chargeRoutine = null;
				yield break;
			}
			Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime * speedMult);
			yield return new WaitForEndOfFrame();
		}
	}

	#endregion
}