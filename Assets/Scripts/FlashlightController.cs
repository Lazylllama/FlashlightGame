using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


public class FlashlightController : MonoBehaviour {
	#region Fields
	// Settings
	[SerializeField] private int   rayAmount       = 100;
	[SerializeField] private float flashlightWidth = 45;
	[SerializeField] private float beamWidth       = 10;
	[SerializeField] private float range           = 10;

	// Refs
	[SerializeField] private Transform lightOutput;
	#endregion

	#region Unity functions
	private void Update() {
		UpdateFlashlightPosition();

		CheckForEnemy();
	}
	#endregion
	
	#region Functions

	private void UpdateFlashlightPosition() {
		// Gets the mouse position in world space
		Vector2 mousePositionOnScreen = InputSystem.actions["point"].ReadValue<Vector2>();
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);

		// Rotates the camera around the pivot point
		float cameraAngleZ =
			-90 + (Mathf.Atan2(mousePosition.y - transform.position.y, mousePosition.x - transform.position.x) *
			       Mathf.Rad2Deg);
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, cameraAngleZ), 0.1f);
	}

	private void CheckForEnemy() {
		Dictionary<Collider2D, int> hitList = new Dictionary<Collider2D, int>();
		// How many degrees apart should each ray be
		float degreesPerRay = beamWidth / (rayAmount - 1);
		// Calculates the rotation as a vector2, (0deg = (1,0), 90deg = (0,1), 180deg = (-1,0), 270deg = (0,-1))
		Vector2 rotation = new Vector2(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad),
		                               Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad));
		for (int i = 0; i < rayAmount; i++) {
			// Calculates the offset from the center of the flashlight, uses rotation to know if the offset should be on the x or y-axis
			Vector2 offset = new Vector2((i * (flashlightWidth / (rayAmount - 1)) - flashlightWidth / 2) * rotation.x,
			                             (i * (flashlightWidth / (rayAmount - 1)) - flashlightWidth / 2) * rotation.y);
			// Calculates the startPosition of each line to be evenly spaced along the flashlight.
			Vector2 startPoint = new Vector2(lightOutput.position.x + offset.x, lightOutput.position.y + offset.y);
			//The normalEndPoint is the endPoint if the source was at (0,0 and was point up)
			Vector2 normalEndPoint =
				new Vector2(Mathf.Sin((2 * i * degreesPerRay - beamWidth) * Mathf.Deg2Rad) * range,
				            Mathf.Cos((2 * i * degreesPerRay - beamWidth) * Mathf.Deg2Rad) * range);
			// The normalEndPoint gets rotated and moved to match the flashlights transform.
			Vector2 endPoint = new Vector2(normalEndPoint.x * rotation.x + normalEndPoint.y * -rotation.y + startPoint.x,
			                               normalEndPoint.x * rotation.y + normalEndPoint.y * rotation.x + startPoint.y);
			Debug.DrawLine(startPoint, endPoint, Color.red);
			RaycastHit2D hit = Physics2D.Linecast(startPoint, endPoint);
			// Adds all colliders that hit the ray to a Dictionary and counts the number of times they hit.
			if (hit && hit.collider.gameObject.CompareTag("Enemy")) {
				if (hitList.ContainsKey(hit.collider)) {
					hitList[hit.collider]++;
					Debug.Log("test1");
				} else {
					hitList.Add(hit.collider, 1);
					Debug.Log("test2");
				}
			}
		}
		Debug.Log(hitList);
		foreach (KeyValuePair<Collider2D, int> hit in hitList) {
			hit.Key.gameObject.GetComponent<EnemyController>().UpdateText(hit.Value, hit.Value/(float)rayAmount);
		}
	}
	#endregion
}