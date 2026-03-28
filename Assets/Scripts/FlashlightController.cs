using System;
using System.Collections.Generic;
using System.Linq;
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
	public bool       IsLightRay;
}

public class FlashlightController : MonoBehaviour {
	#region Fields

	public static  FlashlightController Instance;
	private static DebugHandler         fsDebug;

	[Header("Flashlight Settings")]
	[SerializeField] private float flashlightWidth = 45;
	[SerializeField] private int   rayAmount = 100;
	[SerializeField] private float lerpTime  = 0.1f;

	[Header("Light Output")]
	[SerializeField] private Transform lightOutput;

	[Header("SpotLight")]
	[SerializeField] private GameObject spotLightGameObject;
	[SerializeField] private GameObject freeFormLightGameObject;

	//* Refs
	[SerializeField] private Transform playerTransform;

	private LayerMask        excludePlayer;
	private FlashLightPreset laserPreset, defaultPreset, disabledPreset;

	private static bool FlashlightEnabled {
		get => PlayerData.Instance && PlayerData.Instance.FlashlightEnabled;
		set => throw new NotImplementedException();
	}

	//* States
	private bool             flashlightRotation;
	private bool             isFacingRight;
	private float            intensity;
	private Light2D          spotLight;
	private Light2D          freeFormLight;
	private FlashLightPreset equippedFlashlight = new FlashLightPreset();
	private Vector3[]        lightPoints        = { };
	private Vector3          flashlightPositionWhenFacingRight;
	private Vector3          flashLightPositionWhenFacingLeft;

	// Active Flashlight Preset
	private FlashLightPreset activePreset = new FlashLightPreset();

	// Reflection controls
	[SerializeField] private int maxReflections = 3; // limit bounce count to avoid infinite loops
	[SerializeField]
	private float reflectionOriginOffset = 0.01f; // small offset to avoid immediate re-hit of the same surface

	// States
	private readonly Dictionary<Collider2D, int>         hitList     = new Dictionary<Collider2D, int>();
	private readonly Dictionary<RaycastObj, ReflectInfo> reflectList = new Dictionary<RaycastObj, ReflectInfo>();

	#endregion

	#region Unity Functions

	private void Awake() {
		fsDebug = new DebugHandler("FlashlightController");

		//* Instance
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start() {
		//* Init
		flashlightPositionWhenFacingRight = transform.localPosition;
		flashLightPositionWhenFacingLeft =
			new Vector3(-flashlightPositionWhenFacingRight.x, flashlightPositionWhenFacingRight.y, 0);

		//* Refs
		excludePlayer = ~LayerMask.GetMask("Player");
		spotLight     = spotLightGameObject.GetComponent<Light2D>();
		freeFormLight = freeFormLightGameObject.GetComponent<Light2D>();

		//* Laser flashlight (red beam)
		laserPreset = new FlashLightPreset() {
			PresetDensity   = 1.5f,
			PresetBeamWidth = 0.1f,
			PresetRange     = 100f,
			PresetIntensity = 20f,
			PresetColor     = new Color(1, 0, 0, 1)
		};

		//* Enabled flashlight default (white light)
		defaultPreset = new FlashLightPreset() {
			PresetDensity   = 1.5f,
			PresetBeamWidth = 20f,
			PresetRange     = 10f,
			PresetIntensity = 7f,
			PresetColor     = new Color(0.5f, 0.1f, 0.55f, 1)
		};

		//* Disabled flashlight default (nothing)
		disabledPreset = new FlashLightPreset() {
			PresetDensity   = 0f,
			PresetBeamWidth = 0f,
			PresetRange     = 0f,
			PresetIntensity = 0f,
			PresetColor     = new Color(1, 1f, 1f, 0)
		};
	}


	private void Update() {
		UpdateFlashlight();
		UpdateFlashlightPosition();
		CheckPlayerInputs();
		UpdateSpotlight();

		if (!FlashlightEnabled) return;
		CheckForEnemy();
		if (equippedFlashlight == laserPreset) LaserLightRay();
	}

	#endregion

	#region Functions

	public void UpdateDirection() {
		if (PlayerData.Instance) isFacingRight = PlayerData.Instance.IsLookingRight;
		else fsDebug.Log("PlayerData not found, cannot update direction.", DebugLevel.Fatal);
		//if (isFacingRight) transform.localPosition = flashlightPositionWhenFacingRight;
		//else transform.localPosition = flashLightPositionWhenFacingLeft;
	}

	private void CheckPlayerInputs() {
		if (!PlayerData.Instance) return;
		equippedFlashlight = PlayerData.Instance.FlashlightMode == 1 ? defaultPreset : laserPreset;
		if (equippedFlashlight == laserPreset) {
			freeFormLightGameObject.SetActive(true);
			spotLightGameObject.SetActive(false);
		} else {
			freeFormLightGameObject.SetActive(false);
			spotLightGameObject.SetActive(true);
		}
	}

	private void LerpFlashlight(FlashLightPreset targetPreset) {
		if (targetPreset == null || activePreset == targetPreset) return;

		activePreset = new FlashLightPreset() {
			PresetDensity = Mathf.Lerp(activePreset.PresetDensity, targetPreset.PresetDensity, Time.deltaTime * 10),
			PresetBeamWidth = Mathf.Lerp(activePreset.PresetBeamWidth, targetPreset.PresetBeamWidth,
			                             Time.deltaTime * 10),
			PresetRange = Mathf.Lerp(activePreset.PresetRange, targetPreset.PresetRange, Time.deltaTime * 10),
			PresetIntensity = Mathf.Lerp(activePreset.PresetIntensity, targetPreset.PresetIntensity,
			                             Time.deltaTime * 10),
			PresetColor = Color.Lerp(activePreset.PresetColor, targetPreset.PresetColor, Time.deltaTime * 10)
		};
	}

	private void UpdateFlashlight() {
		LerpFlashlight(!FlashlightEnabled ? disabledPreset : equippedFlashlight);
	}

	private void UpdateSpotlight() {
		spotLight.pointLightOuterAngle  = activePreset.PresetBeamWidth * 2;
		spotLight.color                 = activePreset.PresetColor;
		spotLight.pointLightOuterRadius = activePreset.PresetRange;
		spotLight.intensity             = activePreset.PresetIntensity;
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

		//? Rotate the flahlight around the pivot point
		var cameraAngleZ = -90 + (
			                         Mathf.Atan2(mousePosition.y - transform.position.y,
			                                     mousePosition.x - transform.position.x) *
			                         Mathf.Rad2Deg
		                         );

		if (Mathf.Atan2(mousePosition.y - playerTransform.position.y, mousePosition.x - playerTransform.position.x) *
		    Mathf.Rad2Deg > 90 ||
		    Mathf.Atan2(mousePosition.y - playerTransform.position.y, mousePosition.x - playerTransform.position.x) *
		    Mathf.Rad2Deg < -90) PlayerData.Instance.IsLookingRight = false;
		else PlayerData.Instance.IsLookingRight                     = true;
		transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, cameraAngleZ, lerpTime));
	}

	private void CheckForEnemy() {
		//? Calculates the rotation as a vector2, (0deg = (1,0), 90deg = (0,1), 180deg = (-1,0), 270deg = (0,-1))
		var rotation = new Vector2(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad),
		                           Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad));

		for (var i = 0; i < rayAmount; i++) {
			//? Calculates the startpoint of each line.
			var startPointLinear = flashlightWidth - 2 * i * (flashlightWidth / rayAmount);

			//? Applies density to the startpoint.
			var startPointDensity =
				math.pow(Math.Abs(startPointLinear), activePreset.PresetDensity) /
				math.pow(flashlightWidth,            activePreset.PresetDensity - 1);

			//? Finalizes the density calculation.
			var startPointNormal = startPointDensity * math.sign(startPointLinear);

			//? Applies rotation and offset to the startpoint.
			var startPoint =
				new Vector2(startPointNormal * rotation.x + lightOutput.position.x,
				            startPointNormal * rotation.y + lightOutput.position.y);


			//? Calculates the endPoint of each line.
			var endpointLinear = activePreset.PresetBeamWidth - 2 * i * (activePreset.PresetBeamWidth / rayAmount);

			//? Applies density to the endpoint.
			var endpointDensity = math.pow(Math.Abs(endpointLinear),     activePreset.PresetDensity) /
			                      math.pow(activePreset.PresetBeamWidth, activePreset.PresetDensity - 1);
			var endpointNormal = endpointDensity * math.sign(endpointLinear);

			//? Applies rotation and offset to the endpoint.
			var endpointBend = new Vector2(math.sin(endpointNormal * math.TORADIANS) * activePreset.PresetRange,
			                               math.cos(endpointNormal * math.TORADIANS) * activePreset.PresetRange);
			var endPoint = new Vector2(endpointBend.x * rotation.x + endpointBend.y * -rotation.y + startPoint.x,
			                           endpointBend.x * rotation.y + endpointBend.y * rotation.x  + startPoint.y);

			DrawNewLine(startPoint, endPoint);
		}

		RegisterHitList();
		ProcessReflections();
	}

	private void LaserLightRay() {
		lightPoints = new[] { transform.position };
		DrawNewRay(transform.position, transform.up, true);
	}

	private void DrawNewRay(Vector2 start, Vector2 direction, bool isLightRay = false) {
		var hit = Physics2D.Raycast(start, direction, activePreset.PresetRange, excludePlayer);

		if (hit.collider) Debug.DrawLine(start, hit.point, Color.red);
		else Debug.DrawRay(start, direction * activePreset.PresetRange, Color.red);

		switch (isLightRay) {
			case true when !hit:
				lightPoints = lightPoints
				              .Append(new Vector3(start.x + direction.x * activePreset.PresetRange,
				                                  start.y + direction.y * activePreset.PresetRange, 0))
				              .ToArray();
				SetLightPosition(lightPoints);
				return;
			case true when !hit.collider.CompareTag("Mirror"):
				lightPoints = lightPoints.Append(new Vector3(hit.point.x, hit.point.y, 0)).ToArray();
				SetLightPosition(lightPoints);
				return;
			case true:
				lightPoints = lightPoints.Append(new Vector3(hit.point.x, hit.point.y, 0)).ToArray();
				break;
		}

		switch (hit.collider.tag) {
			case "Enemy" or "WeakPoint" or "Prism":
				if (hitList.TryAdd(hit.collider, 1)) break;
				hitList[hit.collider]++;
				break;
			case "Mirror":
				reflectList.TryAdd
					(new RaycastObj() { Point = hit.point, Normal = hit.normal },
					 new ReflectInfo { Origin = start, Collider   = hit.collider, IsLightRay = isLightRay });
				break;
			default:
				return;
		}

		ProcessReflections();
	}

	private void DrawNewLine(Vector2 start, Vector2 end, bool isLightRay = false) {
		//? Adds all colliders that hit the ray to a Dictionary and counts the number of times they hit.
		var hit = Physics2D.Linecast(start, end, excludePlayer);

		//? Gizmo
		if (hit.collider) Debug.DrawLine(start,  hit.point, Color.red);
		if (!hit.collider) Debug.DrawLine(start, end,       Color.red);

		//? Null guard
		if (!hit) return;

		switch (hit.collider.tag) {
			case "Enemy" or "WeakPoint" or "Prism":
				if (hitList.TryAdd(hit.collider, 1)) break;
				hitList[hit.collider]++;
				break;
			case "Mirror":
				reflectList.TryAdd
					(new RaycastObj() { Point = hit.point, Normal = hit.normal },
					 new ReflectInfo { Origin = start, Collider   = hit.collider });
				break;
			default:
				return;
		}
	}

	private void SetLightPosition(Vector3[] oldLightPoints) {
		var newLightPoints      = new Vector3[] { };
		var reversedLightPoints = oldLightPoints.Reverse().ToArray();

		foreach (var point in reversedLightPoints) {
			newLightPoints = newLightPoints.Append(point).ToArray();
			newLightPoints = newLightPoints.Reverse().ToArray();
			newLightPoints = newLightPoints.Append(point).ToArray();
		}

		freeFormLight.SetShapePath(newLightPoints);
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
				case "Prism":
					hit.Key.gameObject.GetComponent<PrismController>().Hit(hit.Value / (float)rayAmount);
					break;
			}

			if (hit.Key.gameObject.tag is "Enemy" or "WeakPoint" or "Prism") removeList.Add(hit.Key);
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
			CastReflectionChain(pair.Key.Point, pair.Key.Normal, pair.Value.Origin, pair.Value.Collider, 0, queue,
			                    pair.Value.IsLightRay);
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
		Queue<KeyValuePair<RaycastObj, ReflectInfo>> queue,
		bool                                         isLightRay = false
	) {
		if (depth >= maxReflections) return;

		//? Direction incoming -> reflect
		var inDir = Vector2.Normalize(hitPoint - origin);
		var reflectedDir = Vector2.Reflect(inDir, normal);
		var newOrigin = hitPoint + normal.normalized * reflectionOriginOffset + reflectedDir * reflectionOriginOffset;


		var dirNorm = reflectedDir.normalized;
		var hit     = Physics2D.Raycast(newOrigin, dirNorm, activePreset.PresetRange, excludePlayer);


		if (!hit.collider) Debug.DrawRay(newOrigin, dirNorm * activePreset.PresetRange, Color.red);
		else Debug.DrawLine(newOrigin, hit.point, Color.green);

		if (isLightRay && !hit) {
			lightPoints = lightPoints
			              .Append(new Vector3(newOrigin.x + reflectedDir.x * activePreset.PresetRange,
			                                  newOrigin.y + reflectedDir.y * activePreset.PresetRange, 0)).ToArray();
			SetLightPosition(lightPoints);
		} else if (isLightRay && !hit.collider.gameObject.CompareTag("Mirror")) {
			lightPoints = lightPoints.Append(new Vector3(hit.point.x, hit.point.y, 0)).ToArray();
			SetLightPosition(lightPoints);
		}

		//? Null guard || ignore immediate bounce back onto the same collider
		if (!hit || hit.collider == sourceCollider) return;

		if (isLightRay) {
			lightPoints = lightPoints.Append(new Vector3(hit.point.x, hit.point.y, 0)).ToArray();
		}

		if (hit.collider.gameObject.tag is not ("Enemy" or "WeakPoint" or "Prism" or "Mirror")) return;

		if (hit.collider.gameObject.CompareTag("Mirror")) {
			//? enqueue next mirror reflection (use newOrigin as the origin for the next incoming direction)
			var rObj  = new RaycastObj { Point   = hit.point, Normal   = hit.normal };
			var rInfo = new ReflectInfo { Origin = newOrigin, Collider = hit.collider };

			queue.Enqueue(new KeyValuePair<RaycastObj, ReflectInfo>(rObj, rInfo));

			//? Process further in same chain (depth+1)
			CastReflectionChain(hit.point, hit.normal, newOrigin, hit.collider, depth + 1, queue, isLightRay);
		} else {
			//? Hit an enemy or weakpoint along the reflection
			if (!hitList.TryAdd(hit.collider, 1)) hitList[hit.collider]++;
		}
	}

	#endregion
}