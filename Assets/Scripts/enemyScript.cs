using TMPro;
using UnityEngine;

public class enemyScript : MonoBehaviour {
	[Header("Movement")]
	public float baseSpeed;
	public           float     moveSpeed;
	[SerializeField] Transform lookPosition;
	[SerializeField] LayerMask groundLayer;

	[Header("Wall Detection")]
	[SerializeField] float wallRayDistance = 3f;
	[SerializeField] float     wallRayHeight = 1.2f;
	[SerializeField] LayerMask wallLayer;

	[Header("Teleport Settings")]
	[SerializeField] float teleportCooldown = 1.2f;
	[SerializeField] float teleportOffsetY = 0.05f;

	[Header("Slow Down")]
	[SerializeField] float slowDistance = 2f;
	[SerializeField] float slowFactor = 0.5f;

	[Header("Enemy Stats")]
	public float enemyHealth = 100f;
	public           float           maxHealth = 100f;
	[SerializeField] TextMeshProUGUI healthText;

	Rigidbody2D enemyrb;
	Vector3     teleportPoint;
	bool        canTeleport;
	float       teleportTimer;

	public bool WantToDrop;
	public bool drawNumber;

	private void Start() {
		enemyHealth = maxHealth;
		moveSpeed   = baseSpeed;
		enemyrb     = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		CheckWall();
		HandleTeleport();
		MoveEnemy();
		UpdateOverheadText();

		var groundCheckHit = Physics2D.Raycast(lookPosition.position, Vector2.down, 0.2f, groundLayer);
		if (!groundCheckHit.collider) {
			transform.rotation *= Quaternion.Euler(0, 180, 0);
		}

		if (!drawNumber) return;
		RandomNumber();
		drawNumber = false;
	}

	private void MoveEnemy() {
		enemyrb.linearVelocityX = transform.right.x * moveSpeed;
	}

	private void CheckWall() {
		canTeleport = false;

		var origin    = (Vector2)transform.position + Vector2.up * wallRayHeight;
		var direction = transform.right;

		Debug.DrawRay(origin, direction * wallRayDistance, Color.red);

		var wallCheckHit = Physics2D.Raycast(origin, direction, wallRayDistance, wallLayer);
		if (!wallCheckHit) {
			moveSpeed = baseSpeed;
			return;
		}

		var bounds = wallCheckHit.collider.bounds;
		teleportPoint = new Vector3(
		                            bounds.center.x,
		                            bounds.max.y + teleportOffsetY,
		                            transform.position.z
		                           );

		canTeleport = true;
		Debug.DrawLine(origin, wallCheckHit.point, Color.green);


		var distance = wallCheckHit.distance;
		if (distance < slowDistance) {
			moveSpeed = baseSpeed * slowFactor;
		} else {
			moveSpeed = baseSpeed;
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

	private void RandomNumber() {
		WantToDrop = (Random.Range(0, 2) == 0) ? true : false;
	}

	private void UpdateOverheadText() {
		healthText.text = $"Health: {enemyHealth}/{maxHealth}";
	}

	void OnDrawGizmos() {
		if (!canTeleport) return;

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(teleportPoint, 0.15f);
	}
}