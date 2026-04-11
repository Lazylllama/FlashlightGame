using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using FlashlightGame;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[System.Serializable]
public class FlashLightPreset {
	public float density, beamWidth, range, intensity;
	public Color color;
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

[DisallowMultipleComponent]
public class FlashlightController : MonoBehaviour {
	#region Fields

	//* Static
	public static FlashlightController Instance;

	// ReSharper disable once InconsistentNaming
	private static DebugHandler Debug;

	[Header("Flashlight Settings")]
	[SerializeField] private float flashlightWidth = 45;
	[SerializeField] private int   rayAmount              = 100;
	[SerializeField] private float lerpTime               = 0.1f;
	[SerializeField] private int   maxReflections         = 3;     // limit bounce count to avoid infinite loops
	[SerializeField] private float reflectionOriginOffset = 0.01f; // offset to avoid insta re-hit on the same surface
	[SerializeField] private float minAngle;
	[SerializeField] private float maxAngle;


	[Header("Rig Settings")]
	[SerializeField] private Transform pupilBone;
	[SerializeField] private Transform playerTransform;
	[SerializeField] private Transform armLeft,                     armRight;
	[SerializeField] private Vector3   armLeftOffset,               armRightOffset;
	[SerializeField] private float     armLeftZRotMultiplierOver90, armLeftZRotMultiplierUnder90;

	[Header("Light Output")]
	[SerializeField] private Transform lightOutput;

	[Header("SpotLight")]
	[SerializeField] private GameObject lightGlowGameObject;
	[SerializeField] private GameObject spotLightGameObject;
	[SerializeField] private GameObject freeFormLightGameObject;

	[Header("Presets")]
	[Tooltip("Laserbeam Light")] [SerializeField]
	private FlashLightPreset laserPreset = new() {
		density   = 1.5f,
		beamWidth = 0.1f,
		range     = 100f,
		intensity = 20f,
		color     = new Color(1, 0, 0, 1)
	};

	//* Enabled flashlight default (white light)
	[Tooltip("Default Light")] [SerializeField]
	private FlashLightPreset defaultPreset = new() {
		density   = 1.5f,
		beamWidth = 20f,
		range     = 10f,
		intensity = 7f,
		color     = new Color(1f, 1f, 1f, 1)
	};

	//* Disabled flashlight default (nothing)
	[Tooltip("Disabled Light")] [SerializeField]
	private FlashLightPreset disabledPreset = new() {
		density   = 0f,
		beamWidth = 0f,
		range     = 0f,
		intensity = 0f,
		color     = new Color(1, 1f, 1f, 0)
	};

	[Header("Refs")]
	[SerializeField] private CameraController cameraController;

	private        LayerMask excludePlayer;
	private static bool FlashlightEnabled => PlayerData.Instance && PlayerData.Instance.FlashlightEnabled;
	private static bool IsLookingRight => PlayerData.Instance && PlayerData.Instance.IsLookingRight;
	private static bool HasPickedUpFlashlight => PlayerData.Instance && PlayerData.Instance.FlashlightModesUnlocked[1];

	//* States
	private bool      isListening, isGamepad;
	private Light2D   spotLight,   freeFormLight, glowLight;
	private Vector3[] lightPoints = { };
	private Coroutine flickerCoroutine;

	private          FlashLightPreset                    equippedFlashlight = new();
	private readonly Dictionary<Collider2D, int>         hitList            = new();
	private readonly Dictionary<RaycastObj, ReflectInfo> reflectList        = new();


	//* Active Flashlight Preset
	private FlashLightPreset activePreset = new FlashLightPreset();

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("FlashlightController");
	}

	private void Awake() {
		Debug ??= new DebugHandler("FlashlightController");

		//* Instance
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start() {
		//* Refs
		excludePlayer = ~LayerMask.GetMask("Player", "FlashlightIgnore");
		spotLight     = spotLightGameObject.GetComponent<Light2D>();
		freeFormLight = freeFormLightGameObject.GetComponent<Light2D>();
		glowLight     = lightGlowGameObject.GetComponent<Light2D>();
	}


	private void Update() {
		if (!isListening) Initialize();

		LerpFlashlightPreset(!FlashlightEnabled ? disabledPreset : equippedFlashlight);
		UpdateFlashlightPosition();
		CheckPlayerInputs();

		//* Update Spotlight
		spotLight.pointLightOuterAngle  = activePreset.beamWidth * 2;
		spotLight.color                 = activePreset.color;
		spotLight.pointLightOuterRadius = activePreset.range;
		spotLight.intensity             = activePreset.intensity;

		if (!FlashlightEnabled) return;
		CheckForEnemy();
		if (equippedFlashlight == laserPreset) LaserLightRay();
	}

	#endregion

	#region Functions

	private void Initialize() {
		if (!InputHandler.Instance) return;
		InputHandler.Instance.inputChange.AddListener((newType) =>
			                                              isGamepad = newType is not (Lib.InputType.KeyboardMouse
				                                                          or Lib.InputType.Unknown));
		isListening = true;
	}

	private void CheckPlayerInputs() {
		//TODO: Use events....
		if (!PlayerData.Instance) return;
		equippedFlashlight = PlayerData.Instance.FlashlightMode == 1 ? defaultPreset : laserPreset;

		//? Make sure flashlight is enabled if not flickering ofc
		var targetLight = equippedFlashlight == laserPreset ? freeFormLight : spotLight;
		if (flickerCoroutine == null) targetLight.enabled = FlashlightEnabled;

		freeFormLightGameObject.SetActive(FlashlightEnabled && equippedFlashlight == laserPreset);
		spotLightGameObject.SetActive(FlashlightEnabled     && equippedFlashlight != laserPreset);
		lightGlowGameObject.SetActive(FlashlightEnabled);
	}

	private void LerpFlashlightPreset(FlashLightPreset targetPreset) {
		if (targetPreset == null || activePreset == targetPreset) return;

		activePreset = new FlashLightPreset() {
			density = Mathf.Lerp(activePreset.density, targetPreset.density, Time.deltaTime * 10),
			beamWidth = Mathf.Lerp(activePreset.beamWidth, targetPreset.beamWidth,
			                       Time.deltaTime * 10),
			range = Mathf.Lerp(activePreset.range, targetPreset.range, Time.deltaTime * 10),
			intensity = Mathf.Lerp(activePreset.intensity, targetPreset.intensity,
			                       Time.deltaTime * 10),
			color = Color.Lerp(activePreset.color, targetPreset.color, Time.deltaTime * 10)
		};
	}

	private void UpdateFlashlightPosition() {
		var cameraAngleZ = 0f;

		if (!HasPickedUpFlashlight) {
		}

		if (!isGamepad) {
			var playerPos = playerTransform.position;

			//? Gets mouse position in screen
			var mousePositionOnScreen = InputSystem.actions["point"].ReadValue<Vector2>();

			//? Get main camera
			var cam = Camera.main;
			if (!cam) return;

			Vector2 mousePosition;

			//* Unity discussions my savior :pray:
			// TODO: Simplify even more, lowkey too tired
			Ray ray;

			try {
				ray = cam.ScreenPointToRay(new Vector3(mousePositionOnScreen.x, mousePositionOnScreen.y, 0f));
			} catch (Exception e) {
				Debug.LogWarning($"ScreenPointToRay failed: {e.Message}");
				return;
			}

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

			//? Rotate the flashlight around the pivot point
			cameraAngleZ = -90 + (
				                     Mathf.Atan2(mousePosition.y - playerTransform.position.y,
				                                 mousePosition.x - playerTransform.position.x) *
				                     Mathf.Rad2Deg
			                     );
			if (Mathf.Atan2(mousePosition.y - playerPos.y,
			                mousePosition.x - playerPos.x) * Mathf.Rad2Deg > 90 ||
			    Mathf.Atan2(mousePosition.y - playerPos.y,
			                mousePosition.x - playerPos.x) * Mathf.Rad2Deg < -90) {
				PlayerData.Instance.IsLookingRight = false;
				cameraController.CallTurnCamera(true);
			} else {
				PlayerData.Instance.IsLookingRight = true;
				cameraController.CallTurnCamera(false);
			}
		} else {
			var deadZone = Preferences.Input.LookInputDeadZone;
			var input    = InputHandler.Instance.ReadValue(InputHandler.InputActions.FlashlightDirection);

			if (Mathf.Abs(input.x) < deadZone || Mathf.Abs(input.y) < deadZone) return;

			var dir = InputHandler.Instance.ReadValue(InputHandler.InputActions.FlashlightDirection);
			if (dir.sqrMagnitude < 1e-6f) {
				cameraAngleZ = transform.eulerAngles.z;
			} else {
				cameraAngleZ = -90f + Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			}

			if (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg > 90 ||
			    Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg < -90) PlayerData.Instance.IsLookingRight = false;
			else PlayerData.Instance.IsLookingRight                                                 = true;
		}

		var safeZonePosition = GetSafeZonePosition();

		transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, safeZonePosition, lerpTime));
		lightGlowGameObject.transform.eulerAngles =
			new Vector3(0, IsLookingRight ? 0 : 180,
			            Mathf.LerpAngle(lightGlowGameObject.transform.eulerAngles.z,
			                            safeZonePosition * (IsLookingRight ? 1 : -1), lerpTime));

		armLeft.eulerAngles =
			new Vector3(0, IsLookingRight ? 0 : 180,
			            Mathf.LerpAngle(armLeft.eulerAngles.z,
			                            (IsLookingRight ? safeZonePosition : -safeZonePosition) + GetLeftArmOffset(),
			                            lerpTime));
		armRight.eulerAngles =
			new Vector3(0, IsLookingRight ? 0 : 180,
			            Mathf.LerpAngle(armRight.eulerAngles.z,
			                            (IsLookingRight ? safeZonePosition : -safeZonePosition) + armRightOffset.z,
			                            lerpTime));
		return;

		float GetSafeZonePosition() {
			//? actualAngle: 0 = right, 90 = up, 180/-180 = left, -90 = down (if that makes sense)
			var actualAngle = ConvertAngle(cameraAngleZ);

			//? uncomment when needed
			/*Debug.LogKv("GetSafeZonePosition", DebugLevel.Debug, new object[] {
				"actualAngle", actualAngle,
				"IsLookingRight", IsLookingRight,
			});*/

			var clampedActual = ClampToSide(actualAngle, minAngle, maxAngle);

			// convert back to cameraAngleZ space
			return ConvertAngleInverse(clampedActual);

			float ConvertAngle(float angle) {
				if (IsLookingRight) {
					return angle + 180f;
				}

				if (angle < 0) {
					angle += 360f;
				}

				angle = angle * -1f + 180f;
				return angle;
			}

			float ConvertAngleInverse(float angle) {
				if (IsLookingRight) {
					return angle - 180f;
				}

				angle = (angle - 180f) * -1f;

				if (angle < 360) {
					angle -= 360f;
				}

				return angle;
			}

			float ClampToSide(float angle, float min, float max) {
				if (actualAngle < min) {
					return min;
				}

				if (actualAngle > max) {
					return max;
				}

				return actualAngle;
			}
		}

		// normalize to [-180,180)
		//static float NormalizeAngle(float a) => Mathf.Repeat(a + 180f, 360f) - 180f;

		float GetLeftArmOffset() {
			if (IsLookingRight) {
				return cameraAngleZ > -90
					       ? armLeftOffset.z * armLeftZRotMultiplierOver90
					       : armLeftOffset.z * armLeftZRotMultiplierUnder90;
			} else {
				return cameraAngleZ > 0
					       ? armLeftOffset.z * armLeftZRotMultiplierOver90
					       : armLeftOffset.z * armLeftZRotMultiplierUnder90;
			}
		}
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
				math.pow(Math.Abs(startPointLinear), activePreset.density) /
				math.pow(flashlightWidth,            activePreset.density - 1);

			//? Finalizes the density calculation.
			var startPointNormal = startPointDensity * math.sign(startPointLinear);

			//? Applies rotation and offset to the startpoint.
			var lightPos = lightOutput.position;
			var startPoint =
				new Vector2(startPointNormal * rotation.x + lightPos.x,
				            startPointNormal * rotation.y + lightPos.y);


			//? Calculates the endPoint of each line.
			var endpointLinear = activePreset.beamWidth - 2 * i * (activePreset.beamWidth / rayAmount);

			//? Applies density to the endpoint.
			var endpointDensity = math.pow(Math.Abs(endpointLinear), activePreset.density) /
			                      math.pow(activePreset.beamWidth,   activePreset.density - 1);
			var endpointNormal = endpointDensity * math.sign(endpointLinear);

			//? Applies rotation and offset to the endpoint.
			var endpointBend = new Vector2(math.sin(endpointNormal * math.TORADIANS) * activePreset.range,
			                               math.cos(endpointNormal * math.TORADIANS) * activePreset.range);
			var endPoint = new Vector2(endpointBend.x * rotation.x + endpointBend.y * -rotation.y + startPoint.x,
			                           endpointBend.x * rotation.y + endpointBend.y * rotation.x  + startPoint.y);

			DrawNewLine(startPoint, endPoint);
		}

		RegisterHitList();
		ProcessReflections();
	}

	private void LaserLightRay() {
		var start = (Vector2)(lightOutput.position);
		lightPoints = new[] { (Vector3)start };
		DrawNewRay(start, transform.up, true);
	}

	private void DrawNewRay(Vector2 start, Vector2 direction, bool isLightRay = false) {
		var hit = Physics2D.Raycast(start, direction, activePreset.range, excludePlayer);

		if (hit.collider) Debug.DrawLine(start, hit.point, Color.red);
		else Debug.DrawRay(start, direction * activePreset.range, Color.red);

		switch (isLightRay) {
			case true when !hit:
				lightPoints = lightPoints
				              .Append(new Vector3(start.x + direction.x * activePreset.range,
				                                  start.y + direction.y * activePreset.range, 0))
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

	private void CastReflectionChain(Vector2 hitPoint, Vector2 normal, Vector2 origin, Collider2D sourceCollider,
	                                 int     depth,    Queue<KeyValuePair<RaycastObj, ReflectInfo>> queue,
	                                 bool    isLightRay = false) {
		while (true) {
			if (depth >= maxReflections) return;

			//? Direction incoming -> reflect
			var inDir        = Vector2.Normalize(hitPoint - origin);
			var reflectedDir = Vector2.Reflect(inDir, normal);
			var newOrigin = hitPoint + normal.normalized * reflectionOriginOffset +
			                reflectedDir                 * reflectionOriginOffset;


			var dirNorm = reflectedDir.normalized;
			var hit     = Physics2D.Raycast(newOrigin, dirNorm, activePreset.range, excludePlayer);


			if (!hit.collider)
				Debug.DrawRay(newOrigin, dirNorm * activePreset.range, Color.red);
			else
				Debug.DrawLine(newOrigin, hit.point, Color.green);

			switch (isLightRay) {
				case true when !hit:
					lightPoints = lightPoints
					              .Append(new Vector3(newOrigin.x + reflectedDir.x * activePreset.range,
					                                  newOrigin.y + reflectedDir.y * activePreset.range, 0))
					              .ToArray();
					SetLightPosition(lightPoints);
					break;
				case true when !hit.collider.gameObject.CompareTag("Mirror"):
					lightPoints = lightPoints.Append(new Vector3(hit.point.x, hit.point.y, 0)).ToArray();
					SetLightPosition(lightPoints);
					break;
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
				hitPoint       = hit.point;
				normal         = hit.normal;
				origin         = newOrigin;
				sourceCollider = hit.collider;
				depth          = depth + 1;
				continue;
			} else {
				//? Hit an enemy or weakpoint along the reflection
				if (!hitList.TryAdd(hit.collider, 1)) hitList[hit.collider]++;
			}

			break;
		}
	}

	public void LowBatteryWarning(bool isLow) {
		if (isLow) {
			flickerCoroutine = StartCoroutine(Flicker());
		} else {
			StopCoroutine(flickerCoroutine);
		}
	}

	#endregion

	#region Coroutines

	private IEnumerator Flicker() {
		while (true) {
			if (!FlashlightEnabled) {
				yield return null;
				continue;
			}

			var targetLight = equippedFlashlight == laserPreset ? freeFormLight : spotLight;

			targetLight.enabled = false;
			glowLight.enabled   = false;
			yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
			targetLight.enabled = true;
			glowLight.enabled   = true;
			yield return new WaitForSeconds(Random.Range(0.2f, 1f));
		}
	}

	#endregion
}