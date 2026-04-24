using System.Collections;
using UnityEngine;
using FlashlightGame;
using Unity.Mathematics;

public class EnemyController : MonoBehaviour {
	#region Fields

	private static          DebugHandler Debug;
	private static readonly int          IsWalking = Animator.StringToHash("isWalking");
	private static readonly int          Fade      = Shader.PropertyToID("_Fade");
	private static readonly int          Attack    = Animator.StringToHash("Attack");

	private Rigidbody2D rb;

	[Header("Enemy Options")]
	[SerializeField] private bool isGrounded, isChasing, facingRight, flyingEnemy;
	[SerializeField] private float     detectionRange, baseSpeed,   maxHealth,  floatHeight;
	[SerializeField] private Transform lookPosition,   groundCheck, borderLeft, borderRight;
	[SerializeField] private LayerMask groundLayer;

	[Header("Sound")]
	[SerializeField] private string animatorName;
	[SerializeField] private float     soundInterval;
	[SerializeField] private AudioClip flap;

	[Header("Teleport Settings")]
	[SerializeField] private float teleportCooldown = 1.2f;
	
	[Header("Slow Down")]
	[SerializeField] private float slowDistance = 2f;
	[SerializeField] private float slowFactor = 0.5f;

	[SerializeField] private bool shouldRespawn;


	//* States
	private Collider2D capsuleCollider;
	private Material          material;
	private AudioSource       audioSource;
	private Animator          animator;
	private Vector2?          target;
	private Vector3           teleportPoint, pathFindPoint, borderLeftPos, borderRightPos, spawnPoint;
	private float             health,        enemySpeed;
	private bool              canTeleport;
	private int               playerCollisionCount;
	private bool              collidingWithPlayer;
	private Coroutine         teleportRoutineState, pathfindingRoutineState, deathHandlerRoutineState;

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("EnemyController");
	}

	private void Awake() {
		Debug = new DebugHandler("EnemyController");
	}

	private void Start() {
		rb              = GetComponent<Rigidbody2D>();
		audioSource     = GetComponent<AudioSource>();
		animator        = GetComponentInChildren<Animator>();
		health          = maxHealth;
		enemySpeed      = baseSpeed;
		borderLeftPos    = borderLeft.position;
		borderRightPos  = borderRight.position;
		spawnPoint      = transform.position;
		capsuleCollider = GetComponent<Collider2D>();
		material        = GetComponentInChildren<SpriteRenderer>().material;

		if (flyingEnemy) rb.gravityScale = 0;

		if (soundInterval > 0) StartCoroutine(SoundRoutine());
	}

	private void Update() {
		if (deathHandlerRoutineState != null) return;
		isGrounded = Lib.Movement.GroundCheck(groundCheck.position, 0.2f);
		if (!CheckBorder()) {
			CheckForTarget();
		}
		TurnEnemy();
		CheckMantleWall();
		CheckWall();
		if (!flyingEnemy) LedgeCheck();
		if (flyingEnemy) CheckFloatHeight();

		if (shouldRespawn) {
			Respawn();
		}

		if (collidingWithPlayer && !PlayerData.Instance.IsInvulnerable) {
			PlayerData.Instance.UpdateHealth(-20);
			animator.SetTrigger(Attack);
		}
	}

	private void FixedUpdate() {
		if (deathHandlerRoutineState != null) return;
		ChaseTarget();
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject != null && other.gameObject.CompareTag("Player")) {
			playerCollisionCount++;
			collidingWithPlayer = playerCollisionCount > 0;
		}
	}

	private void OnCollisionExit2D(Collision2D other) {
		if (other.gameObject != null && other.gameObject.CompareTag("Player")) {
			playerCollisionCount = Mathf.Max(0, playerCollisionCount - 1);
			collidingWithPlayer  = playerCollisionCount > 0;
		}
	}

	#endregion

	#region Functions

	private bool CheckBorder() {
		if (transform.position.x < borderLeftPos.x) {
			facingRight = true;
			return true;
		} else if (transform.position.x > borderRightPos.x) {
			facingRight = false;
			return true;
		}

		return false;
	}

	private void CheckForTarget() {
		var playerPosition   = PlayerMovement.Instance.transform.position;
		var distanceToPlayer = Vector2.Distance(transform.position, playerPosition);

		target = (distanceToPlayer <= detectionRange) ? playerPosition : null;
	}

	private void LedgeCheck() {
		var check = Lib.Movement.LedgeCheck(lookPosition.position, 0.3f);

		if (!check.collider && !isChasing) {
			enemySpeed = baseSpeed;
			if (facingRight && isGrounded) {
				facingRight = false;
			} else if (!facingRight && isGrounded) {
				facingRight = true;
			}
		}
	}

	private void CheckWall() {
		var wallHit = Lib.Movement.WallCheck(lookPosition.position, facingRight);


		if (!wallHit.collider || isChasing || teleportRoutineState != null || pathfindingRoutineState != null) return;

		facingRight = facingRight switch {
			true when isGrounded  => false,
			false when isGrounded => true,
			_                     => facingRight
		};
	}

	private void CheckMantleWall() {
		if (teleportRoutineState != null || pathfindingRoutineState != null) return;
		if (!Lib.Movement.MantleWallCheck(lookPosition.position, facingRight)) return;

		var mantlePoint = Lib.Movement.GetWallMantlePoint(transform.position, facingRight);

		if (mantlePoint.Position == Vector3.zero) {
			enemySpeed = baseSpeed;
			return;
		}

		if (!flyingEnemy) {
			teleportPoint        = mantlePoint.Position;
			teleportRoutineState = StartCoroutine(HandleTeleport());
		} else {
			var pathfindHit = Physics2D.Raycast(mantlePoint.Position, Vector2.down, 10000f, groundLayer);
			if (!pathfindHit.collider) {
				Debug.LogError("ERROR: Pathfind ground not found!");
				return;
			}

			pathFindPoint = new Vector3(pathfindHit.point.x, pathfindHit.point.y + floatHeight, transform.position.z);
			pathfindingRoutineState = StartCoroutine(HandlePathFinding());
		}


		if (mantlePoint.Distance < slowDistance) {
			enemySpeed = baseSpeed * slowFactor;
			//fade out
		} else {
			enemySpeed = baseSpeed;
		}
	}

	private void ChaseTarget() {
		if (pathfindingRoutineState != null) return;
		if (target == null) {
			enemySpeed         = baseSpeed;
			isChasing          = false;
			rb.linearVelocityX = (facingRight ? 1 : -1) * enemySpeed;
			return;
		}

		isChasing  = true;
		enemySpeed = baseSpeed * 1.3f;
		var dirX = target.Value.x - transform.position.x;
		facingRight        = dirX > 0;
		rb.linearVelocityX = Mathf.Sign(dirX) * enemySpeed;
	}

	private void TurnEnemy() {
		transform.localScale = facingRight ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
	}

	

	public void UpdateHealth(float amount) {
		health -= amount;

		if (health <= 0) {
			deathHandlerRoutineState = StartCoroutine(DeathHandler());
		}
	}

	private void CheckFloatHeight() {
		if (pathfindingRoutineState != null) return;
		var hit = Physics2D.Raycast(transform.position, Vector2.down, 10000f, groundLayer);
		if (!hit.collider) {
			Debug.LogError("ERROR: Float ground not found!");
			return;
		}

		var desiredY = hit.point.y + floatHeight;
		if (transform.position.y < desiredY - 0.1f) {
			rb.linearVelocityY = baseSpeed;
		} else if (transform.position.y > desiredY + 0.1f) {
			rb.linearVelocityY = -baseSpeed;
		} else {
			rb.linearVelocityY = 0;
		}
	}

	public void Respawn() {
		gameObject.SetActive(true);
		transform.position       = spawnPoint;
		health                   = maxHealth;
		pathfindingRoutineState  = null;
		teleportRoutineState     = null;
		deathHandlerRoutineState = null;
		target                   = null;
		isChasing                = false;
		collidingWithPlayer      = false;
		playerCollisionCount     = 0;
		material.SetFloat(Fade, 1f);
		capsuleCollider.enabled = true;
		rb.bodyType             = RigidbodyType2D.Dynamic;
		animator.SetBool(IsWalking, true);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		if (!canTeleport) return;
		Gizmos.DrawSphere(teleportPoint, 0.15f);
	}

	#endregion

	#region Coroutines

	private IEnumerator DeathHandler() {
		rb.bodyType             = RigidbodyType2D.Kinematic;
		rb.linearVelocity       = Vector2.zero;
		capsuleCollider.enabled = false;
		animator.SetBool(IsWalking, false);
		for (float fade = 1; fade > 0; fade -= Time.deltaTime / 2) {
			material.SetFloat(Fade, fade);
			yield return new WaitForEndOfFrame();
		}

		gameObject.SetActive(false);
	}

	private IEnumerator HandleTeleport() {
		yield return new WaitForSecondsRealtime(teleportCooldown);
		transform.position   = teleportPoint;
		teleportRoutineState = null;
	}

	private IEnumerator HandlePathFinding() {
		bool finishedX = false, finishedY = false;
		while (!finishedX || !finishedY) {
			if (transform.position.y < pathFindPoint.y) {
				rb.linearVelocityY = baseSpeed;
			} else {
				rb.linearVelocityY = 0;
				finishedY          = true;
			}

			if (math.abs(transform.position.x - pathFindPoint.x) > 0.1f) {
				rb.linearVelocityX = facingRight ? baseSpeed : -baseSpeed;
			} else {
				rb.linearVelocityX = 0;
				finishedX          = true;
			}

			yield return new WaitForFixedUpdate();
		}

		pathfindingRoutineState = null;
	}

	#endregion
}