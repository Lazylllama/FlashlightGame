using TMPro;
using UnityEngine;

public class EnemyController : MonoBehaviour {
	#region Fields

	[Header("Refs")]
	[SerializeField] private TMP_Text overheadText;
	private Rigidbody2D rb;

	[Header("Enemy Options")]
	private float health;
	[SerializeField] private bool      isGrounded,     isChasing,  facingRight;
	[SerializeField] private float     detectionRange, enemySpeed, baseSpeed, maxHealth;
	[SerializeField] private Transform lookPosition,   groundCheck;
	[SerializeField] private LayerMask groundLayer;

	[Header("Wall Detection")]
	[SerializeField] private float wallRayDistance = 3f;
	[SerializeField] private float     wallRayHeight = 1.2f;
	[SerializeField] private LayerMask climbableWallLayer, wallLayer;

	[Header("Teleport Settings")]
	[SerializeField] private float teleportCooldown = 1.2f;
	[SerializeField] private float teleportOffsetY = 0.05f;
	private                  float teleportTimer;
	private                  bool  canTeleport;
	
	[Header("Slow Down")]
	[SerializeField] private float slowDistance = 2f;
	[SerializeField] private float slowFactor = 0.5f;

	
	//* States
	private Vector3  teleportPoint;
	private Vector2? target;

	#endregion

	#region Unity Functions

	private void Start() {
		rb         = GetComponent<Rigidbody2D>();
		health     = maxHealth;
		enemySpeed = baseSpeed;
	}

	private void Update() {
		GroundCheck();
		LedgeCheck();
		HandleTeleport();
		CheckForTarget();
		UpdateOverheadText();
		TurnEnemy();
		CheckClimbableWall();
		CheckWall();
	}

	private void FixedUpdate() {
		ChaseTarget();
	}

	#endregion

	#region Functions

	private void CheckForTarget() {
		var playerPosition   = PlayerMovement.Instance.transform.position;
		var distanceToPlayer = Vector2.Distance(transform.position, playerPosition);

		if (distanceToPlayer <= detectionRange) {
			target = playerPosition;
		} else {
			target = null;
		}
	}

	private void LedgeCheck() {
		var ledgeHit = Physics2D.Raycast(lookPosition.position, Vector2.down, 0.3f, groundLayer);
		if (!ledgeHit.collider & !isChasing) {
			enemySpeed = baseSpeed;
			if (facingRight & isGrounded) {
				facingRight = false;
			} else if (!facingRight & isGrounded) {
				facingRight = true;
			}
		}
	}

	private void GroundCheck() {
		var groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer);
		if (groundHit.collider) {
			isGrounded = true;
		} else {
			isGrounded = false;
		}
	}

	private void CheckWall() {
		var wallHit = Physics2D.Raycast(lookPosition.position, facingRight ? Vector2.right : Vector2.left, 2f, wallLayer);
		Debug.DrawRay(lookPosition.position, facingRight ? Vector2.right : Vector2.left * 2f, Color.yellow);
		if (wallHit.collider & !isChasing) {
			if (facingRight & isGrounded) {
				facingRight = false;
			} else if (!facingRight & isGrounded) {
				facingRight = true;
			}
		}
	}

	private void CheckClimbableWall() {
		canTeleport = false;

		var origin    = (Vector2)transform.position + Vector2.up * wallRayHeight;
		var direction = facingRight ? Vector2.right : Vector2.left;

		Debug.DrawRay(origin, direction * wallRayDistance, Color.red);

		var climbableWallHit = Physics2D.Raycast(origin, direction, wallRayDistance, climbableWallLayer);
		if (!climbableWallHit) {
			enemySpeed = baseSpeed;
			return;
		}

		var bounds = climbableWallHit.collider.bounds;
		teleportPoint = new Vector3(
		                            bounds.center.x,
		                            bounds.max.y + teleportOffsetY,
		                            transform.position.z
		                           );

		canTeleport = true;
		enemySpeed  = 0f;
		Debug.DrawLine(origin, climbableWallHit.point, Color.green);
		var distance = climbableWallHit.distance;
		if (distance < slowDistance) {
			enemySpeed = baseSpeed * slowFactor;
		} else {
			enemySpeed = baseSpeed;
		}

	}

	private void HandleTeleport() {
		if (!canTeleport) {
			teleportTimer = 0f;
			return;
		}

		teleportTimer += Time.deltaTime;

		if (!(teleportTimer >= teleportCooldown)) return;
		transform.position = teleportPoint;
		teleportTimer      = 0f;
	}

	private void ChaseTarget() {
		if (target == null) {
			enemySpeed = baseSpeed;
			isChasing          = false;
			rb.linearVelocityX = (facingRight ? 1 : -1) * enemySpeed;
			return;
		}

		isChasing  = true;
		enemySpeed = enemySpeed * 2f;
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
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		if (!canTeleport) return;
		Gizmos.DrawSphere(teleportPoint, 0.15f);
	}

	#endregion
}