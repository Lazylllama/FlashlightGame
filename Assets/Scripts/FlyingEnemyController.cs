using System.Collections;
using System.Net;
using TMPro;
using UnityEngine;
using FlashlightGame;

public class FlyingEnemyController : MonoBehaviour {
	#region Fields

	[Header("Refs")]
	[SerializeField] private TMP_Text overheadText;
	private Rigidbody2D rb;
	private LayerMask   wallLayerMask, groundLayer, pathfindingLayer;
	

	[Header("Enemy Options")]
	[SerializeField] private bool isGrounded, isChasing, facingRight;
	[SerializeField] private float     detectionRange, baseSpeed, maxHealth;
	[SerializeField] private Transform lookPosition,   groundCheck;

	[Header("Sound")]
	[SerializeField] private AudioManager.AudioName soundName;
	[SerializeField] private string animatorName;
	[SerializeField] private float  soundInterval;

	[Header("Teleport Settings")]
	[SerializeField] private float teleportCooldown = 1.2f;
	private float teleportTimer;

	[Header("Slow Down")]
	[SerializeField] private float slowDistance = 2f;
	[SerializeField] private float slowFactor = 0.5f;


	//* States
	private AudioSource audioSource;
	private Animator    animator;
	private Vector3     goToPoint;
	private Vector2?    target;
	private float       health, enemySpeed;
	private bool        canTeleport;
	private bool        isPathFinding;

	#endregion

	#region Unity Functions

	private void Start() {
		wallLayerMask = LayerMask.GetMask("ClimbWall","MantleWall", "Ground", "Box", "Wall");
		groundLayer   = LayerMask.GetMask("Ground");
		pathfindingLayer = LayerMask.GetMask("Ground", "Box");
		
		
		rb          = GetComponent<Rigidbody2D>();
		audioSource = GetComponent<AudioSource>();
		animator    = GetComponent<Animator>();
		health      = maxHealth;
		enemySpeed  = baseSpeed;

		if (soundInterval > 0) StartCoroutine(SoundRoutine());
	}

	private void Update() {
		if (isPathFinding) {
			GoToPoint(goToPoint);
			print("Pathfinding");
			return;
		}
		CheckForWall();
		CheckForTarget();
		UpdateOverheadText();
		TurnEnemy();
		CheckFloatHeight();
	}

	private void FixedUpdate() {
		ChaseTarget();
	}
	
	private void OnDrawGizmos() {
		if (isPathFinding) {
			Gizmos.color = Color.green;		
		} else {
			Gizmos.color = Color.red;
		}
		Gizmos.DrawWireSphere(goToPoint, 1);
	}

	#endregion

	#region Functions

	private void CheckFloatHeight() {
		var hit = Physics2D.Raycast(transform.position, Vector2.down, 3f, groundLayer);
		if (!hit.collider) {
			Debug.DrawRay(transform.position, Vector2.down * 3f, Color.red);
			rb.linearVelocityY = -1f;
		}

		if (hit.point.y + 3.05f > transform.position.y) {
			rb.linearVelocityY = 1f;
		}
		if (hit.point.y + 3.25f > transform.position.y && hit.point.y + 2.75f < transform.position.y) {
			rb.linearVelocityY = 0f;
		}
	}

	private void CheckForWall() {
		var hit = Physics2D.Raycast(transform.position, (facingRight? Vector2.right : Vector2.left), 0.8f, wallLayerMask);
		if (!hit.collider) {
			Debug.DrawRay(transform.position, (facingRight? Vector2.right : Vector2.left) * 0.8f, Color.red);
			return;
		}
		if (hit.collider.gameObject.layer == LayerMask.NameToLayer("ClimbWall") || hit.collider.gameObject.layer == LayerMask.NameToLayer("MantleWall") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Box")) {
			var rayOrigin = new Vector2(transform.position.x + (facingRight? 1.0f : -1.0f), transform.position.y + 300f);
			var wallHit = Physics2D.Raycast(rayOrigin,Vector2.down, 10000f ,pathfindingLayer);
			if (!wallHit.collider) {
				print("Test1");
				Debug.DrawRay(rayOrigin,Vector2.down * 10000f, Color.red, 10f);
				facingRight = !facingRight;
				return;
			} 
			if (wallHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) {
				print("Test2");
				Debug.DrawLine(rayOrigin, wallHit.point, Color.blue, 10f);
				goToPoint = new Vector3(wallHit.point.x, wallHit.point.y +1f, transform.position.z);
				GoToPoint(goToPoint);
				return;
			}
		} 
		facingRight = !facingRight;
		print("Test3");
	}

	private void GoToPoint(Vector3 point) {
		isPathFinding = true;
		if (transform.position.y >= point.y) {
			isPathFinding = false;
			rb.linearVelocityY = 0;
			return;
		}

		rb.linearVelocityX = 0;
		rb.linearVelocityY = 1;
	}

	private void CheckForTarget() {
		var playerPosition   = PlayerMovement.Instance.transform.position;
		var distanceToPlayer = Vector2.Distance(transform.position, playerPosition);

		target = (distanceToPlayer <= detectionRange) ? playerPosition : null;
	}

	private void ChaseTarget() {		
		if (target == null) {
			enemySpeed         = baseSpeed;
			isChasing          = false;
			if (!isPathFinding) rb.linearVelocityX = (facingRight ? 1 : -1) * enemySpeed;
			return;
		}

		isChasing  = true;
		enemySpeed = baseSpeed * 1.3f;
		var dirX = target.Value.x - transform.position.x;
		facingRight        = dirX > 0;
		rb.linearVelocityX = Mathf.Sign(dirX) * enemySpeed;
		rb.linearVelocityY = ((transform.position.y - PlayerMovement.Instance.transform.position.y) < 0? 1: -1)*enemySpeed ;
	}

	private void TurnEnemy() {
		if(isPathFinding) return;
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

	#endregion
}