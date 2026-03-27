using System.Collections;
using TMPro;
using UnityEngine;
using FlashlightGame;
using Unity.Mathematics;

public class EnemyController : MonoBehaviour {
	#region Fields

	private static DebugHandler Debug;
	
	[Header("Refs")]
	[SerializeField] private TMP_Text       overheadText;
	private                  Rigidbody2D rb;

	[Header("Enemy Options")]
	[SerializeField] private bool isGrounded, isChasing, facingRight, flyingEnemy;
	[SerializeField] private float     detectionRange, baseSpeed, maxHealth, floatHeight;
	[SerializeField] private Transform lookPosition,   groundCheck, borderLeft, borderRight;
	[SerializeField] private LayerMask groundLayer;

	[Header("Sound")]
	[SerializeField] private AudioManager.AudioName soundName;
	[SerializeField] private string animatorName;
	[SerializeField] private float  soundInterval;

	[Header("Teleport Settings")]
	[SerializeField] private float teleportCooldown = 1.2f;
	private                  float teleportTimer;

	[Header("Slow Down")]
	[SerializeField] private float slowDistance = 2f;
	[SerializeField] private float slowFactor = 0.5f;


	//* States
	private AudioSource audioSource;
	private Animator    animator;
	private Vector2?    target;
	private Vector3     teleportPoint, pathFindPoint, borderLeftPos, borderRightPos;
	private float       health, enemySpeed;
	private bool        canTeleport;
	private Coroutine   teleportRoutineState, pathfindingRoutineState;

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("EnemyController");
	}

	private void Start() {
		rb             = GetComponent<Rigidbody2D>();
		audioSource    = GetComponent<AudioSource>();
		animator       = GetComponent<Animator>();
		health         = maxHealth;
		enemySpeed     = baseSpeed;
		borderLeftPos  = borderLeft.position;
		borderRightPos = borderRight.position;

		if (flyingEnemy) rb.gravityScale = 0;

		if (soundInterval > 0) StartCoroutine(SoundRoutine());
	}

	private void Update() {
		isGrounded = Lib.Movement.GroundCheck(groundCheck.position, 0.2f);
		CheckForTarget();
		UpdateOverheadText();
		TurnEnemy();
		CheckMantleWall();
		CheckWall();
		CheckBorder();
		if(!flyingEnemy) LedgeCheck();
		if (flyingEnemy) CheckFloatHeight();
	}

	private void FixedUpdate() {
		ChaseTarget();
	}
	
	private void OnCollisionEnter2D(Collision2D other) {
		if (PlayerData.Instance.IsInvulnerable == false && other.gameObject.CompareTag("Player")) {
			PlayerData.Instance.UpdateHealth(25);
		}
	}

	#endregion

	#region Functions

	private void CheckBorder() {
		if (transform.position.x < borderLeftPos.x) {
			facingRight = true;
		} else if (transform.position.x > borderRightPos.x) {
			facingRight = false;
		}
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
		

		if (!wallHit.collider || isChasing || teleportRoutineState != null) return;

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

		Debug.Log(mantlePoint);
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
				print("ERROR: Pathfind ground not found!");
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

	private void UpdateOverheadText() {
		overheadText.text                 = $"Health: {health}/{maxHealth}";
		overheadText.transform.localScale = facingRight ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
	}

	public void UpdateHealth(float amount) {
		health -= amount;
		UpdateOverheadText();

		if (health <= 0) Destroy(gameObject);
	}

	private void CheckFloatHeight() {
		var hit = Physics2D.Raycast(transform.position, Vector2.down, 10000f, groundLayer);
		if (!hit.collider) {
			print("ERROR: Float ground not found!");
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

	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		if (!canTeleport) return;
		Gizmos.DrawSphere(teleportPoint, 0.15f);
	}

	#endregion

	#region Coroutines

	private IEnumerator SoundRoutine() {
		var interval = Mathf.Max(0.01f, soundInterval);

		// repeat while the enemy is alive
		while (health > 0f) {
			Debug.Log("Playing enemy sound.");

			if (audioSource) {
				if (animator && animatorName.Length > 0)
					animator.SetTrigger(animatorName);
				AudioManager.Instance.PlaySfx(soundName, audioSource);
			} else {
				Debug.LogWarning("AudioSource component missing on EnemyController. Sound will not play.");
			}

			// wait for the configured interval before playing again
			yield return new WaitForSeconds(interval);
		}
	}
	
	private IEnumerator HandleTeleport() {
		yield return new WaitForSecondsRealtime(teleportCooldown);

		transform.position = teleportPoint;

		teleportTimer        = 0f;
		teleportRoutineState = null;
	}

	private IEnumerator HandlePathFinding() {
		bool finishedX = false, finishedY = false;
		while (!(finishedY && finishedX)) {
			if (transform.position.y < pathFindPoint.y) rb.linearVelocityY = baseSpeed;
			else {
				rb.linearVelocityY = 0;
				finishedY          = true;
			}
			if (math.abs(transform.position.x - pathFindPoint.x) > 0.1f) {
				rb.linearVelocityX = facingRight ? baseSpeed : -baseSpeed;
			} else {
				rb.linearVelocityX = 0;
				finishedX          = true;
			}

			yield return new WaitForEndOfFrame();
		}

		pathfindingRoutineState = null;
	}

	#endregion
}