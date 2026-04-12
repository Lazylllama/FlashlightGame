using System;
using UnityEngine;

public class lateUpdatePositionConstraint : MonoBehaviour {
	[SerializeField] private Transform target;
	private void LateUpdate() {
		transform.position = target.position;
	}
}
