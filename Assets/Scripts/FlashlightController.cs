using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class FlashLightPreset {
	public float PresetDensity;
	public float PresetBeamWidth;
	public float PresetRange;
}


public class FlashlightController : MonoBehaviour {
	#region Fields

	[Header("Flashlight Settings")]
	[SerializeField] private float flashlightWidth = 45;
	[SerializeField] private int   rayAmount         = 100;
	[SerializeField] private float beamWidth         = 10;
	[SerializeField] private float range            = 10;
	[SerializeField] private float density;

	[Header("Light Output")]
	[SerializeField] private Transform lightOutput;

	[Header("SpotLight")]
	[SerializeField] private GameObject spotLightGameObject;

	//* Refs
	private LayerMask        excludePlayer;
	private FlashLightPreset laserPreset   = new FlashLightPreset();
	private FlashLightPreset defaultPreset = new FlashLightPreset();
	private InputAction      equipFlashlight1;
	private InputAction      equipFlashlight2;

	//* States
	private Light2D spotLight;
	private FlashLightPreset equippedFlashlight = new FlashLightPreset();
	#endregion

	#region Unity Functions

	private void Start() {
		if (spotLightGameObject == null) {
			Debug.LogError("FlashlightController: 'spotLightGameObject' is not assigned in the Inspector.", this);
		} else {
			spotLight = spotLightGameObject.GetComponent<Light2D>();
			if (spotLight == null) {
				Debug.LogError("FlashlightController: No Light2D component found on 'spotLightGameObject'.", this);
			}
		}

		excludePlayer = ~LayerMask.GetMask("Player");

		laserPreset.PresetDensity     = 1.5f;
		laserPreset.PresetBeamWidth   = 0.1f;
		laserPreset.PresetRange       = 100f;
		defaultPreset.PresetDensity   = 1.5f;
		defaultPreset.PresetBeamWidth = 20f;
		defaultPreset.PresetRange     = 10f;

		equipFlashlight1 = InputSystem.actions["Flashlight1"];
		equipFlashlight2 = InputSystem.actions["Flashlight2"];
	}

	private void Update() {
		UpdateSpotlight();
		UpdateFlashlightPosition();
		CheckForEnemy();
		CheckPlayerInputs();
		UpdateFlashlight();
	}

	#endregion

	#region Functions

	private void CheckPlayerInputs() {
		if (equipFlashlight1.triggered && equippedFlashlight != defaultPreset) {
			equippedFlashlight = defaultPreset;
		} else if (equipFlashlight2.triggered && equippedFlashlight != laserPreset) {
			equippedFlashlight = laserPreset;
		}
	}

	private void UpdateFlashlight() {
		density = Mathf.Lerp(density, equippedFlashlight.PresetDensity, Time.deltaTime * 10);
		beamWidth = Mathf.Lerp(beamWidth, equippedFlashlight.PresetBeamWidth, Time.deltaTime * 10);
		range = Mathf.Lerp(range, equippedFlashlight.PresetRange, Time.deltaTime * 10);
	}
	
	
	private void UpdateSpotlight() {
		spotLight.pointLightOuterAngle  = beamWidth * 2;
		spotLight.pointLightOuterRadius = range;
	}

	private void UpdateFlashlightPosition() {
		// Gets the mouse position in world space
		var mousePositionOnScreen = InputSystem.actions["point"].ReadValue<Vector2>();
		var mousePosition         = Camera.main!.ScreenToWorldPoint(new Vector3(mousePositionOnScreen.x, mousePositionOnScreen.y, 10f));

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
			//? Calculates the startpoint of each line.
			var startPointLinear = flashlightWidth - 2 * i * (flashlightWidth / rayAmount);
			//? Applies density to the startpoint.
			var startPointDensity =
				math.pow(Math.Abs(startPointLinear), density) / math.pow(flashlightWidth, density - 1);
			//? Finalizes the density calculation.
			var startPointNormal = startPointDensity * math.sign(startPointLinear);
			//? Applies rotation and offset to the startpoint.
			var startPoint =
				new Vector2(startPointNormal * rotation.x + lightOutput.position.x,
				            startPointNormal * rotation.y + lightOutput.position.y);


			//? Calculates the endPoint of each line.
			var endpointLinear = beamWidth - 2 * i * (beamWidth / rayAmount);
			//? Applies density to the endpoint.
			var endpointDensity = math.pow(Math.Abs(endpointLinear), density) / math.pow(beamWidth, density - 1);
			var endpointNormal  = endpointDensity                             * math.sign(endpointLinear);
			//? Applies rotation and offset to the endpoint.
			var endpointBend = new Vector2(math.sin(endpointNormal * math.TORADIANS) * range,
			                               math.cos(endpointNormal * math.TORADIANS) * range);
			var endPoint = new Vector2(endpointBend.x * rotation.x + endpointBend.y * -rotation.y + startPoint.x,
			                           endpointBend.x * rotation.y + endpointBend.y * rotation.x  + startPoint.y);


			//? Gizmo
			Debug.DrawLine(startPoint, endPoint, Color.red);

			//? Adds all colliders that hit the ray to a Dictionary and counts the number of times they hit.
			var hit = Physics2D.Linecast(startPoint, endPoint, excludePlayer);
			if (!hit ||
			    !(hit.collider.gameObject.CompareTag("Enemy") ||
			      hit.collider.gameObject.CompareTag("WeakPoint"))) continue;
			if (!hitList.TryAdd(hit.collider, 1)) {
				hitList[hit.collider]++;
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