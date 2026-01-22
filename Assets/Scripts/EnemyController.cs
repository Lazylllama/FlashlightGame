using TMPro;
using UnityEngine;

public class EnemyController : MonoBehaviour {
	#region Fields
	
	[Header("Refs")]
	[SerializeField] private TMP_Text overheadText;
	private Rigidbody2D rb;

	[Header("Enemy Options")]
	[SerializeField] private float detectionRange;
	[SerializeField] private float enemySpeed;
	[SerializeField] private float maxHealth;

	//* States
	private Vector2? target;
	private float    health;

	#endregion

	#region Unity Functions

	private void Start() {
		rb = GetComponent<Rigidbody2D>();
		health = maxHealth;
	}

	private void Update() {
		CheckForTarget();
		UpdateOverheadText();
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

	private void ChaseTarget() {
		if (target == null) {
			Debug.Log("Enemy has no target");
			rb.linearVelocityX = 0;
			return;
		}

		Debug.Log("Enemy has a target");

		var direction = ((Vector2)target - (Vector2)transform.position).normalized;

		rb.linearVelocityX = direction.x * enemySpeed;
	}

	private void UpdateOverheadText() {
		overheadText.text = $"Health: {health}/{maxHealth}";
	}

	public void UpdateHealth(float amount) {
		health += amount;
		UpdateOverheadText();
	}

	#endregion
}