using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using FlashlightGame;

public class FlashLightPreset {
	public float PresetDensity;
	public float PresetBeamWidth;
	public float PresetRange;
	public float PresetIntensity;
	public Color PresetColor;
}

public struct RaycastObj : IEquatable<RaycastObj> {
	public Vector2 Point;
	public Vector2 Normal;

	public bool Equals(RaycastObj other) => Point.Equals(other.Point) && Normal.Equals(other.Normal);

	public override bool Equals(object obj) => obj is RaycastObj other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Point, Normal);
}

public struct ReflectInfo {
	public Vector2    Origin;
	public Collider2D Collider;
}

public class FlashlightController : MonoBehaviour {
	#region Fields

	public static  FlashlightController Instance;
	private static DebugHandler         flDebug;

	[Header("Flashlight Settings")]
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

	// TODO: Replace with InputHandler asap pls 
	private InputAction equipFlashlight1, equipFlashlight2;

	//* States
	private bool             isFacingRight;
	private float            intensity;
	private Light2D          spotLight;
	private FlashLightPreset equippedFlashlight = new FlashLightPreset();

	// Reflection controls
	[SerializeField] private int maxReflections = 3; // limit bounce count to avoid infinite loops
	[SerializeField] private float reflectionOriginOffset = 0.01f; // small offset to avoid immediate re-hit of the same surface
	private int reflectionDepth;

	private Dictionary<Collider2D, int>         hitList     = new Dictionary<Collider2D, int>();
	private Dictionary<RaycastObj, ReflectInfo> reflectList = new Dictionary<RaycastObj, ReflectInfo>();

	#endregion

	#region Unity Functions

	private void Awake() {
		flDebug = new DebugHandler("PlayerMovement");

		//* Instance
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start() {
		spotLight     = spotLightGameObject.GetComponent<Light2D>();
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
		UpdateFlashlight();
		CheckPlayerInputs();
		UpdateSpotlight();
		CheckForEnemy();
	}

	#endregion

	#region Functions

	public void UpdateDirection() {
		if (PlayerData.Instance) isFacingRight = PlayerData.Instance.IsLookingRight;
		else flDebug.Log("PlayerData not found, cannot update direction.", DebugLevel.Fatal);
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
		if (laserSpotLightGameObject != null) {
			laserSpotLightGameObject.SetActive(beamWidth <= 2);
		} else {
			flDebug.Log("Laser spotlight GameObject not assigned; skipping laser toggle.", DebugLevel.Warning);
		}
	}

	private void UpdateSpotlight() {
		spotLight.pointLightOuterAngle  = beamWidth * 2;
		spotLight.color                 = color;
		spotLight.pointLightOuterRadius = range;
		spotLight.intensity             = intensity;
	}

	private void UpdateFlashlightPosition() {
		// TODO(@th1n0-i): Double check and also flipping sides is kinda annoying
		//? Gets mouse position in screen
		var mousePositionOnScreen = InputSystem.actions["point"].ReadValue<Vector2>();

		//? Get main camera
		var cam = Camera.main;
		if (!cam) return;

		Vector2 mousePosition;

		//* Unity discussions my savior :pray:
		// TODO: Simplify even more, lowkey too tired
		var ray    = cam.ScreenPointToRay(new Vector3(mousePositionOnScreen.x, mousePositionOnScreen.y, 0f));
		var planeZ = transform.position.z;
		var t      = (planeZ - ray.origin.z) / ray.direction.z;

		if (!Mathf.Approximately(ray.direction.z, 0f) && t > 0f) {
			var worldPoint = ray.GetPoint(t);
			mousePosition = new Vector2(worldPoint.x, worldPoint.y);
		} else {
			var screenZ = cam.WorldToScreenPoint(transform.position).z;
			if (screenZ <= 0f) screenZ = 10f;
			var wp = cam.ScreenToWorldPoint(new Vector3(mousePositionOnScreen.x, mousePositionOnScreen.y, screenZ));
			mousePosition = new Vector2(wp.x, wp.y);
		}

		//? Only point in the players direction, otherwise mirror
		switch (isFacingRight) {
			case true when mousePosition.x  - transform.position.x < 0:
			case false when mousePosition.x - transform.position.x > 0:
				mousePosition.x += 2 * (transform.position.x - mousePosition.x);
				break;
		}

		//? Rotate the camera around the pivot point
		var cameraAngleZ = -90 + (
			                         Mathf.Atan2(mousePosition.y - transform.position.y,
			                                     mousePosition.x - transform.position.x) *
			                         Mathf.Rad2Deg
		                         );

		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, cameraAngleZ), 0.03f);
	}

	private void CheckForEnemy() {
		// reset per-frame reflection depth counter
		reflectionDepth = 0;

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

			DrawNewLine(startPoint, endPoint);
		}

		RegisterHitList();
		ProcessReflections();
	}

	private void DrawNewLine(Vector2 start, Vector2 end) {
		//? Gizmo
		Debug.DrawLine(start, end, Color.red);

		//? Adds all colliders that hit the ray to a Dictionary and counts the number of times they hit.
		var hit = Physics2D.Linecast(start, end, excludePlayer);

		//? Null guard & only process relevant tags
		if (!hit || hit.collider.tag is not ("Enemy" or "WeakPoint" or "Mirror")) return;
		
		switch (hit.collider.tag) {
			case "Enemy" or "WeakPoint":
				if (hitList.TryAdd(hit.collider, 1)) break;
				hitList[hit.collider]++;
				break;
			case "Mirror":
				reflectList.TryAdd
					(new RaycastObj() { Point = hit.point, Normal = hit.normal },
					 new ReflectInfo { Origin = start, Collider   = hit.collider });
				break;
		}
	}

	private void RegisterHitList() {
		var removeList = new List<Collider2D>();

		foreach (var hit in hitList) {
			switch (hit.Key.gameObject.tag) {
				case "Enemy":
					hit.Key.gameObject.GetComponent<EnemyController>().UpdateHealth(hit.Value / (float)rayAmount);
					break;
				case "WeakPoint":
					hit.Key.gameObject.GetComponentInParent<BossController>().Hit(hit.Value / (float)rayAmount);
					break;
			}

			if (hit.Key.gameObject.tag is "Enemy" or "WeakPoint") removeList.Add(hit.Key);
		}

		foreach (var key in removeList) {
			hitList.Remove(key);
		}

		removeList.Clear();
	}

	private void ProcessReflections() {
		if (reflectList.Count == 0) return;

		// Use a queue so newly discovered mirror hits can be processed in the same frame (up to maxReflections)
		var queue = new Queue<KeyValuePair<RaycastObj, ReflectInfo>>(reflectList);
		reflectList.Clear();

		while (queue.Count > 0) {
			var pair = queue.Dequeue();
			// Start casting from the hit point using the original origin stored
			CastReflectionChain(pair.Key.Point, pair.Key.Normal, pair.Value.Origin, pair.Value.Collider, 0, queue);
		}

		// After processing all reflections, apply hits found along reflection rays
		RegisterHitList();
	}

	private void CastReflectionChain(
		Vector2                                      hitPoint,
		Vector2                                      normal,
		Vector2                                      origin,
		Collider2D                                   sourceCollider,
		int                                          depth,
		Queue<KeyValuePair<RaycastObj, ReflectInfo>> queue
	) {
		if (depth >= maxReflections) return;

		//? Direction incoming -> reflect
		var inDir = Vector2.Normalize(hitPoint - origin);
		var reflectedDir = Vector2.Reflect(inDir, normal);
		var newOrigin = hitPoint + normal.normalized * reflectionOriginOffset + reflectedDir * reflectionOriginOffset;

		Debug.DrawRay(newOrigin, reflectedDir * range, Color.yellow);

		var dirNorm = reflectedDir.normalized;
		var hit     = Physics2D.Raycast(newOrigin, dirNorm, range, excludePlayer);

		//? Null guard || ignore immediate bounce back onto the same collider
		if (!hit || hit.collider == sourceCollider) return;

		if (hit.collider.gameObject.tag is not ("Enemy" or "WeakPoint" or "Mirror")) return;

		if (hit.collider.gameObject.CompareTag("Mirror")) {
			//? enqueue next mirror reflection (use newOrigin as the origin for the next incoming direction)
			var rObj  = new RaycastObj { Point   = hit.point, Normal   = hit.normal };
			var rInfo = new ReflectInfo { Origin = newOrigin, Collider = hit.collider };

			queue.Enqueue(new KeyValuePair<RaycastObj, ReflectInfo>(rObj, rInfo));

			//? Process further in same chain (depth+1)
			CastReflectionChain(hit.point, hit.normal, newOrigin, hit.collider, depth + 1, queue);
		} else {
			//? Hit an enemy or weakpoint along the reflection
			if (!hitList.TryAdd(hit.collider, 1)) hitList[hit.collider]++;
		}
	}

	#endregion
}