
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

public class FlashLightPreset {
 public float PresetDensity;
 public float PresetBeamWidth;
 public float PresetRange;
 public float PresetIntensity;
 public Color PresetColor;
}

public struct RaycastObj {
 public Vector2 Point;
 public Vector2 Normal;
}

public struct ReflectInfo {
 public Vector2 Origin;
 public Collider2D Collider;
}

public class FlashlightController : MonoBehaviour {
 #region Fields

 public static FlashlightController Instance;

 [Header("Flashlight Settings")]
 [SerializeField] private float updateInterval;
 [SerializeField] private float maxAngle        = 90;
 [SerializeField] private float flashlightWidth = 45;
 [SerializeField] private int   rayAmount       = 100;
 [SerializeField] private float beamWidth       = 10;
 [SerializeField] private float range           = 10;
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

 // Reflection controls
 [SerializeField] private int   maxReflections = 3; // limit bounce count to avoid infinite loops
 [SerializeField] private float reflectionOriginOffset = 0.01f; // small offset to avoid immediate re-hit of the same surface
 private int                    reflectionDepth;

 private Dictionary<Collider2D, int>       hitList     = new Dictionary<Collider2D, int>();
 private Dictionary<RaycastObj, ReflectInfo> reflectList = new Dictionary<RaycastObj, ReflectInfo>();

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
  if (laserSpotLightGameObject != null) {
   laserSpotLightGameObject.SetActive(beamWidth <= 2);
  } else {
   DebugHandler.Instance.Log("Laser spotlight GameObject not assigned; skipping laser toggle.", DebugHandler.DebugLevel.Warning);
  }
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

  if (isFacingRight && mousePosition.x - transform.position.x < 0) {
   mousePosition.x += 2 * (transform.position.x - mousePosition.x);
  } else if (!isFacingRight && mousePosition.x - transform.position.x > 0) {
   mousePosition.x += 2 * (transform.position.x - mousePosition.x);
  }

  // Rotates the camera around the pivot point
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

   DrawNewLine(startPoint, endPoint);
  }

  RegisterHitList();
  ProcessReflections();
 }

 private void ReflectRay(Vector2 inDirection, Vector2 inNormal, Vector2 origin, Collider2D sourceCollider = null) {
  // Stop if we've reached the max number of allowed reflections
  if (reflectionDepth >= maxReflections) return;
  reflectionDepth++;

  var reflectedDir = Vector2.Reflect(inDirection.normalized, inNormal.normalized);
  // Offset away from the surface along the surface normal, and slightly along the reflected direction
  var newOrigin = origin + inNormal.normalized * reflectionOriginOffset + reflectedDir * reflectionOriginOffset;
  DrawNewRay(newOrigin, reflectedDir * range, sourceCollider);
  RegisterHitList();
  // Do not call ProcessReflections() here; newly discovered mirror hits will be processed in the main ProcessReflections queue
 }

 private void DrawNewRay(Vector2 start, Vector2 direction, Collider2D ignoreCollider = null) {
  Debug.DrawRay(start, direction, Color.red);

  // Use the correct Raycast overload: normalized direction, explicit distance, and layer mask
  var dirNorm = direction.normalized;
  var dist    = direction.magnitude;
  var hit     = Physics2D.Raycast(start, dirNorm, dist, excludePlayer);

  if (!hit) return;
  // ignore immediate hit on the collider that produced this reflection
  if (hit.collider == ignoreCollider) return;
  if (!(hit.collider.gameObject.CompareTag("Enemy")     ||
        hit.collider.gameObject.CompareTag("WeakPoint") ||
        hit.collider.gameObject.CompareTag("Mirror"))) return;
  if (hit.collider.gameObject.CompareTag("Mirror")) {
   reflectList.TryAdd(new RaycastObj(){Point = hit.point, Normal = hit.normal},
                      new ReflectInfo { Origin = start, Collider = hit.collider });
  } else if (!hitList.TryAdd(hit.collider, 1)) {
   hitList[hit.collider]++;
  }
 }

 private void DrawNewLine(Vector2 start, Vector2 end) {
  //? Gizmo
  Debug.DrawLine(start, end, Color.red);

  //? Adds all colliders that hit the ray to a Dictionary and counts the number of times they hit.
  var hit = Physics2D.Linecast(start, end, excludePlayer);
  if (!hit ||
      !(hit.collider.gameObject.CompareTag("Enemy")     ||
        hit.collider.gameObject.CompareTag("WeakPoint") ||
        hit.collider.gameObject.CompareTag("Mirror"))) return;
  if (hit.collider.gameObject.CompareTag("Mirror")) {
   reflectList.TryAdd(new RaycastObj(){Point = hit.point, Normal = hit.normal},
                      new ReflectInfo { Origin = start, Collider = hit.collider });
  } else if (!hitList.TryAdd(hit.collider, 1)) {
   hitList[hit.collider]++;
  }
 }

 private void RegisterHitList() {
  List<Collider2D> removeList = new List<Collider2D>();
  foreach (var hit in hitList) {
   if (hit.Key.gameObject.CompareTag("Enemy")) {
    hit.Key.gameObject.GetComponent<EnemyController>().UpdateHealth(hit.Value / (float)rayAmount);
    removeList.Add(hit.Key);
   } else if (hit.Key.gameObject.CompareTag("WeakPoint")) {
    hit.Key.gameObject.GetComponentInParent<BossController>().Hit(hit.Value / (float)rayAmount);
    removeList.Add(hit.Key);
   }
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

 private void CastReflectionChain(Vector2 hitPoint, Vector2 normal, Vector2 origin, Collider2D sourceCollider, int depth, Queue<KeyValuePair<RaycastObj, ReflectInfo>> queue) {
  if (depth >= maxReflections) return;

  // Direction incoming -> reflect
  var inDir = Vector2.Normalize(hitPoint - origin);
  var reflectedDir = Vector2.Reflect(inDir, normal);
  var newOrigin = hitPoint + normal.normalized * reflectionOriginOffset + reflectedDir * reflectionOriginOffset;

  Debug.DrawRay(newOrigin, reflectedDir * range, Color.yellow);

  var dirNorm = reflectedDir.normalized;
  var hit = Physics2D.Raycast(newOrigin, dirNorm, range, excludePlayer);
  if (!hit) return;

  // ignore immediate bounce back onto the same collider
  if (hit.collider == sourceCollider) return;

  if (!(hit.collider.gameObject.CompareTag("Enemy")     ||
        hit.collider.gameObject.CompareTag("WeakPoint") ||
        hit.collider.gameObject.CompareTag("Mirror"))) return;

  if (hit.collider.gameObject.CompareTag("Mirror")) {
   // enqueue next mirror reflection (use newOrigin as the origin for the next incoming direction)
   var rObj = new RaycastObj { Point = hit.point, Normal = hit.normal };
   var rInfo = new ReflectInfo { Origin = newOrigin, Collider = hit.collider };
   queue.Enqueue(new KeyValuePair<RaycastObj, ReflectInfo>(rObj, rInfo));
   // process further in same chain (depth+1)
   CastReflectionChain(hit.point, hit.normal, newOrigin, hit.collider, depth + 1, queue);
  } else {
   // hit an enemy or weakpoint along the reflection
   if (!hitList.TryAdd(hit.collider, 1)) hitList[hit.collider]++;
  }
 }

 #endregion

}
