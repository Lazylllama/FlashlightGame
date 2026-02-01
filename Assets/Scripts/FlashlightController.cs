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
	public float PresetIntensity;
	public Color PresetColor;
}


public class FlashlightController : MonoBehaviour {
	#region Fields
	public static FlashlightController Instance;

	[Header("Flashlight Settings")]
	[SerializeField] private float maxAngle = 90;
	[SerializeField] private float flashlightWidth = 45;
	[SerializeField] private int   rayAmount = 100;
	[SerializeField] private float beamWidth = 10;
	[SerializeField] private float range     = 10;
	[SerializeField] private float density;
	[SerializeField] private Color color;

	[Header("Light Output")]
	[SerializeField] private Transform lightOutput;

	[Header("SpotLight")]
	[SerializeField] private GameObject spotLightGameObject;
	[SerializeField] private GameObject laserSpotLightGameObject;

	//* Refs
	private LayerMask        excludePlayer;
	private FlashLightPreset laserPreset   = new FlashLightPreset();
	private FlashLightPreset defaultPreset = new FlashLightPreset();
	private InputAction      equipFlashlight1;
	private InputAction      equipFlashlight2;

	//* States
	private bool             isFacingRight;
	private float            intensity;
	private Light2D          spotLight;
	private FlashLightPreset equippedFlashlight = new FlashLightPreset();

	#endregion

	#region Unity Functions

	private void Awake() {
		//* Instance
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}
	
	private void Start() {
		spotLight = spotLightGameObject.GetComponent<Light2D>();


		excludePlayer = ~LayerMask.GetMask("Player");

		laserPreset.PresetDensity     = 1.5f;
		laserPreset.PresetBeamWidth   = 0.1f;
		laserPreset.PresetRange       = 100f;
		laserPreset.PresetIntensity   = 20f;
		laserPreset.PresetColor       = new Color(1, 0, 0, 1);
		defaultPreset.PresetDensity   = 1.5f;
		defaultPreset.PresetBeamWidth = 20f;
		defaultPreset.PresetRange     = 10f;
		defaultPreset.PresetIntensity = 7f;
		defaultPreset.PresetColor     = new Color(1, 0.94f, 0.55f, 1);

		equipFlashlight1 = InputSystem.actions["Flashlight1"];
		equipFlashlight2 = InputSystem.actions["Flashlight2"];
	}

	private void Update() {
		UpdateFlashlightPosition();
		CheckForEnemy();
		CheckPlayerInputs();
		UpdateFlashlight();
		UpdateSpotlight();
	}

	#endregion

	#region Functions

	public void UpdateDirection() {
		if (PlayerData.Instance) isFacingRight = PlayerData.Instance.IsLookingRight;
		else DebugHandler.Instance.Log("PlayerData not found, cannot update direction.", DebugHandler.DebugLevel.Fatal);

	}

	private void CheckPlayerInputs() {
		if (equipFlashlight1.triggered && equippedFlashlight != defaultPreset) {
			equippedFlashlight = defaultPreset;
		} else if (equipFlashlight2.triggered && equippedFlashlight != laserPreset) {
			equippedFlashlight = laserPreset;
		}
	}

	private void UpdateFlashlight() {
		density   = Mathf.Lerp(density,   equippedFlashlight.PresetDensity,   Time.deltaTime * 10);
		beamWidth = Mathf.Lerp(beamWidth, equippedFlashlight.PresetBeamWidth, Time.deltaTime * 10);
		range     = Mathf.Lerp(range,     equippedFlashlight.PresetRange,     Time.deltaTime * 10);
		intensity = Mathf.Lerp(intensity, equippedFlashlight.PresetIntensity, Time.deltaTime * 10);
		color = new Color(Mathf.Lerp(color.r, equippedFlashlight.PresetColor.r, Time.deltaTime * 10),
		                  Mathf.Lerp(color.g, equippedFlashlight.PresetColor.g, Time.deltaTime * 10),
		                  Mathf.Lerp(color.b, equippedFlashlight.PresetColor.b, Time.deltaTime * 10),
		                  1);
		laserSpotLightGameObject.SetActive(beamWidth <= 2);
	}

	private void UpdateSpotlight() {
		spotLight.pointLightOuterAngle  = beamWidth * 2;
		spotLight.color                 = color;
		spotLight.pointLightOuterRadius = range;
		spotLight.intensity             = intensity;
	}

	private void UpdateFlashlightPosition() {
		// Gets the mouse position in world space
		var mousePositionOnScreen = InputSystem.actions["point"].ReadValue<Vector2>();
		var mousePosition =
			Camera.main!.ScreenToWorldPoint(new Vector3(mousePositionOnScreen.x, mousePositionOnScreen.y, 10f));

		// Rotates the camera around the pivot point
		var cameraAngleZ = -90 + (
			                         Mathf.Atan2(mousePosition.y - transform.position.y,
			                                     mousePosition.x - transform.position.x) *
			                         Mathf.Rad2Deg
		                         );

		Debug.Log(cameraAngleZ);
		Debug.Log(isFacingRight);
		if (isFacingRight) {
			if (cameraAngleZ > -90 + maxAngle && cameraAngleZ <= 90) cameraAngleZ = -90 + maxAngle;
			Debug.Log(cameraAngleZ);
			if (cameraAngleZ < -90 - maxAngle && cameraAngleZ >= -270) cameraAngleZ = -90 - maxAngle;
			Debug.Log(cameraAngleZ);
		} else {
			if (cameraAngleZ < 90 - maxAngle && cameraAngleZ >= -90) cameraAngleZ = 90 - maxAngle;
			Debug.Log(cameraAngleZ);
			if (cameraAngleZ > -90 - maxAngle && cameraAngleZ < -90) cameraAngleZ = 90 + maxAngle;
			Debug.Log(cameraAngleZ);
		}
		
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, cameraAngleZ), 0.03f);
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
				//Debug.Log("Test 1");
			} else {
				//Debug.Log("Test 2");
			}
		}

		//Debug.Log(hitList);

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