using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;


public class FlashlightController : MonoBehaviour {
	#region Fields

	[Header("Flashlight Settings")]
	[SerializeField] private float flashlightWidth = 45;
	[SerializeField] private int   rayAmount         = 100;
	[SerializeField] private float beamWidth         = 10;
	[SerializeField] private float rangePreset       = 10;
	[SerializeField] private float scrollSensitivity = 10;

	[Header("Light Output")]
	[SerializeField] private Transform lightOutput;

	[Header("SpotLight")]
	[SerializeField] private GameObject spotLightGameObject;

	//* Refs
	private LayerMask   excludePlayer;
	private InputAction scrollAction;

	//* States
	private Vector2   totalScroll;
	private float     range;
	private Light2D spotLight;

	#endregion

	#region Unity functions

	private void Start() {
		spotLight     = spotLightGameObject.GetComponent<Light2D>();
		
		excludePlayer = ~LayerMask.GetMask("Player");
	}

	private void Awake() {
		scrollAction  = InputSystem.actions.FindAction("ScrollWheel");
	}

	private void Update() {
		UpdateSpotlight();
		UpdateFlashlightPosition();
		CheckForEnemy();
		CheckPlayerInputs();
	}

	#endregion

	#region Functions

	private void CheckPlayerInputs() {
		range = rangePreset / beamWidth;
		if (scrollAction.ReadValue<Vector2>().y == 0) return;
		beamWidth += scrollAction.ReadValue<Vector2>().y * scrollSensitivity;
	}

	private void UpdateSpotlight() {
		spotLight.pointLightOuterAngle = beamWidth * 2;
		spotLight.pointLightOuterRadius     = range;
	}

	private void UpdateFlashlightPosition() {
		// Gets the mouse position in world space
		var mousePositionOnScreen = InputSystem.actions["point"].ReadValue<Vector2>();
		var mousePosition         = Camera.main!.ScreenToWorldPoint(mousePositionOnScreen);

		// Rotates the camera around the pivot point
		var cameraAngleZ = -90 + (
			                         Mathf.Atan2(mousePosition.y - transform.position.y,
			                                     mousePosition.x - transform.position.x) *
			                         Mathf.Rad2Deg
		                         );

		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, cameraAngleZ), 0.1f);
	}

	private void CheckForEnemy() {
		var hitList = new Dictionary<Collider2D, int>();

		//? How many degrees apart should each ray be
		var degreesPerRay = beamWidth / (rayAmount - 1);

		//? Calculates the rotation as a vector2, (0deg = (1,0), 90deg = (0,1), 180deg = (-1,0), 270deg = (0,-1))
		var rotation = new Vector2(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad),
		                           Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad));

		for (var i = 0; i < rayAmount; i++) {
			//? Calculates the offset from the center of the flashlight, uses rotation to know if the offset should be on the x or y-axis
			var offset = new Vector2((i * (flashlightWidth / (rayAmount - 1)) - flashlightWidth / 2) * rotation.x,
			                         (i * (flashlightWidth / (rayAmount - 1)) - flashlightWidth / 2) * rotation.y);

			//? Calculates the startPosition of each line to be evenly spaced along the flashlight.
			var startPoint = new Vector2(lightOutput.position.x + offset.x, lightOutput.position.y + offset.y);

			//? The normalEndPoint is the endPoint if the source was at (0,0 and was point up)
			var normalEndPoint =
				new Vector2(Mathf.Sin((2 * i * degreesPerRay - beamWidth) * Mathf.Deg2Rad) * range,
				            Mathf.Cos((2 * i * degreesPerRay - beamWidth) * Mathf.Deg2Rad) * range);

			//? The normalEndPoint gets rotated and moved to match the flashlights transform.
			var endPoint = new Vector2(normalEndPoint.x * rotation.x + normalEndPoint.y * -rotation.y + startPoint.x,
			                           normalEndPoint.x * rotation.y + normalEndPoint.y * rotation.x  + startPoint.y);

			//? Gizmo
			Debug.DrawLine(startPoint, endPoint, Color.red);

			//? Adds all colliders that hit the ray to a Dictionary and counts the number of times they hit.
			var hit = Physics2D.Linecast(startPoint, endPoint, excludePlayer);
			if (!hit ||
			    !(hit.collider.gameObject.CompareTag("Enemy") ||
			      hit.collider.gameObject.CompareTag("WeakPoint"))) continue;
			if (!hitList.TryAdd(hit.collider, 1)) {
				hitList[hit.collider]++;
			} else {
			}
		}

		foreach (var hit in hitList) {
			if (hit.Key.gameObject.CompareTag("Enemy")) {
				hit.Key.gameObject.GetComponent<EnemyController>().UpdateHealth(hit.Value / (float)rayAmount);
			} else {
				hit.Key.gameObject.GetComponentInParent<BossController>().Hit(hit.Value / (float)rayAmount);
			}
		}
	}

	#endregion
}