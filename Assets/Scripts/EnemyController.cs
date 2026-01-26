using TMPro;
using UnityEngine;

public class EnemyController : MonoBehaviour {
	#region Fields

	[Header("Refs")]
	[SerializeField] private TMP_Text overheadText;
	private Rigidbody2D rb;

	[Header("Enemy Options")]
	[SerializeField] private float detectionRange;
	[SerializeField] private float     enemySpeed;
	[SerializeField] private float     baseSpeed;
	[SerializeField] private float     maxHealth;
	[SerializeField] private Transform lookPosition;
	[SerializeField] private LayerMask groundLayer;
	//TEST
	[Header("Wall Detection")]
	[SerializeField] private float wallRayDistance = 3f;
	[SerializeField] private float     wallRayHeight = 1.2f;
	[SerializeField] private LayerMask wallLayer;

	[Header("Teleport Settings")]
	[SerializeField] private float teleportCooldown = 1.2f;
	[SerializeField] private float teleportOffsetY = 0.05f;

	[Header("Slow Down")]
	[SerializeField] private float slowDistance = 2f;
	[SerializeField] private float slowFactor = 0.5f;
	


	//* States
	private Vector3  teleportPoint;
	private Vector2? target;

	private                  float teleportTimer;
	private                  bool  canTeleport;
	private                  float health;
	[SerializeField] private bool  isChasing;
	[SerializeField] private bool  facingRight;

	#endregion

	#region Unity Functions

	private void Start() {
		rb        = GetComponent<Rigidbody2D>();
		health    = maxHealth;
		enemySpeed = baseSpeed;
	}

	private void Update() {
		GroundHitCheck();
		CheckClimbableWall();
		HandleTeleport();
		CheckForTarget();
		UpdateOverheadText();
		TurnEnemy();
		

		
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
			target  = playerPosition;
		} else {
			target  = null;
		}
	}
	private void GroundHitCheck() {
		var groundCheckHit = Physics2D.Raycast(lookPosition.position, Vector2.down, 0.2f, groundLayer);
		if (!groundCheckHit.collider & !isChasing) {
			baseSpeed = 2f;
			if (facingRight) {
				facingRight = false;
			} else if (!facingRight) {
				facingRight = true;
			}
			
		}
		if (!groundCheckHit.collider & isChasing) {
			baseSpeed = 0f;
		}
	}

	private void CheckWall() {
		var wallHit = Physics2D.Raycast(lookPosition.position, facingRight ? Vector2.right : Vector2.left, 0.2f, wallLayer);
		if (wallHit.collider) {
			if (facingRight) {
				facingRight = false;
			} else if (!facingRight) {
				facingRight = true;
			}
		}
	}
	private void CheckClimbableWall() {
		canTeleport = false;

		var origin    = (Vector2)transform.position + Vector2.up * wallRayHeight;
		var direction = facingRight ? Vector2.right : Vector2.left;

		Debug.DrawRay(origin, direction * wallRayDistance, Color.red);

		var climbableWallHit = Physics2D.Raycast(origin, direction, wallRayDistance, wallLayer);
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
			isChasing = false;
			Debug.Log("Enemy has no target");
			rb.linearVelocityX = (facingRight ? 1 : -1) * enemySpeed;
			return;
			
		}

		

		Debug.Log("Enemy has a target");

		isChasing = true;
		float dirX = target.Value.x - transform.position.x;
		facingRight = dirX > 0;
		rb.linearVelocityX = Mathf.Sign(dirX) * enemySpeed;



	}

	private void TurnEnemy() {
		if (facingRight)
			transform.localScale = new Vector3(1, 1, 1);
		else
			transform.localScale = new Vector3(-1, 1, 1);

	}


	private void UpdateOverheadText() {
		overheadText.text = $"Health: {health}/{maxHealth}";
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