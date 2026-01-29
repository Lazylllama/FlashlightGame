using System.Collections;
using TMPro;
using UnityEngine;
using FlashlightGame;

public class EnemyController : MonoBehaviour {
	#region Fields

	[Header("Refs")]
	[SerializeField] private TMP_Text overheadText;
	private Rigidbody2D rb;

	[Header("Enemy Options")]
	private float health;
	private                  SpriteRenderer enemySpriteRenderer;
	[SerializeField] private bool           isGrounded,     isChasing,  facingRight;
	[SerializeField] private float          detectionRange, enemySpeed, baseSpeed, maxHealth;
	[SerializeField] private Transform      lookPosition,   groundCheck;
	[SerializeField] private LayerMask      groundLayer;

	[Header("Teleport Settings")]
	[SerializeField] private float teleportCooldown = 1.2f;
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
		enemySpriteRenderer = GetComponent<SpriteRenderer>();
		rb                  = GetComponent<Rigidbody2D>();
		health              = maxHealth;
		enemySpeed          = baseSpeed;
	}

	private void Update() {
		isGrounded = Lib.Movement.GroundCheck(groundCheck.position, 0.2f);
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
		var check = Lib.Movement.LedgeCheck(lookPosition.position, 0.3f);

		if (!isChasing && !check.collider) return;

		enemySpeed = baseSpeed;

		if (facingRight & isGrounded) {
			facingRight = false;
		} else if (!facingRight & isGrounded) {
			facingRight = true;
		}
	}

	private void CheckWall() {
		var wallHit = Lib.Movement.WallCheck(lookPosition.position, facingRight);

		if (wallHit.collider != null && !isChasing) {
			if (facingRight & isGrounded) {
				facingRight = false;
			} else if (!facingRight & isGrounded) {
				facingRight = true;
			}
		}
	}

	private void CheckClimbableWall() {
		canTeleport = false;

		var climbPoint = Lib.Movement.GetWallClimbPoint(transform.position, facingRight);
		
		if (climbPoint.Position == Vector3.zero) {
			enemySpeed = baseSpeed;
			return;
		}
		
		teleportPoint = climbPoint.Position;
		canTeleport = true;
		StartCoroutine(FadeIn());

		if (climbPoint.Distance < slowDistance) {
			enemySpeed = baseSpeed * slowFactor;
			//fade out
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

		if (teleportTimer < teleportCooldown) return;
		transform.position = teleportPoint;
		StartCoroutine(FadeOut());
		teleportTimer = 0f;
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

	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		if (!canTeleport) return;
		Gizmos.DrawSphere(teleportPoint, 0.15f);
	}

	#endregion

	#region Coroutines

	private IEnumerator FadeIn() {
		var alphaVal = enemySpriteRenderer.color.a;
		var  tmp      = enemySpriteRenderer.color;

		while (enemySpriteRenderer.color.a > 0) {
			alphaVal                  -= 0.10f;
			tmp.a                     =  alphaVal;
			enemySpriteRenderer.color =  tmp;

			yield return new WaitForSeconds(0.05f);
		}
	}

	private IEnumerator FadeOut() {
		var alphaVal = enemySpriteRenderer.color.a;
		var tmp      = enemySpriteRenderer.color;

		while (enemySpriteRenderer.color.a < 1) {
			alphaVal                  += 0.10f;
			tmp.a                     =  alphaVal;
			enemySpriteRenderer.color =  tmp;

			yield return new WaitForSeconds(0.05f);
		}
	}

	#endregion
}