using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
	#region Fields

	//* Instance
	public static PlayerMovement Instance;

	//* Refs
	private InputAction moveAction;
	private Rigidbody2D playerRb;

	private static bool IsLookingRight {
		get => PlayerData.Instance.IsLookingRight;
		set => PlayerData.Instance.IsLookingRight = value;
	}

	//! Friction impacts speed *GREATLY*
	[Header("Movement Settings")]
	[SerializeField] private float maxSpeed, acceleration;

	[Header("Ground Check")]
	[SerializeField] private Transform groundCheckPosition;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float     groundCheckRadius;
	private                  bool      isGrounded;

	//* States
	private Vector2 moveInputVal;

	#endregion

	#region Unity Functions

	private void Start() {
		playerRb   = GetComponent<Rigidbody2D>();
		moveAction = InputSystem.actions.FindAction("Move");
	}

	private void Update() {
		GroundCheck();
	}

	private void FixedUpdate() {
		InputCheck();
		PerformMove();
	}

	//? Draws gizmo to visualize ground check status
	private void OnDrawGizmos() {
		Gizmos.color = isGrounded ? Color.green : Color.red;
		Gizmos.DrawWireSphere(groundCheckPosition.position, groundCheckRadius);
	}

	//? Set global instance
	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	#endregion

	#region Functions

	private void GroundCheck() {
		var hit = Physics2D.OverlapCircle(groundCheckPosition.position, groundCheckRadius, groundLayer);

		isGrounded = hit;
	}

	private void InputCheck() {
		moveInputVal = moveAction.ReadValue<Vector2>();

		IsLookingRight = moveInputVal.x switch {
			< 0 when IsLookingRight  => false,
			> 0 when !IsLookingRight => true,
			_                        => IsLookingRight
		};
		
		print(IsLookingRight);
	}

	private void PerformMove() {
		// playerRb.linearVelocityX = moveInputVal.x * playerSpeed;

		//* Revolutionary!?
		var inputSpeed      = moveInputVal.x * maxSpeed;
		var speedDifference = inputSpeed - playerRb.linearVelocityX;
		var finalForce      = speedDifference * acceleration;

		playerRb.AddForce(Vector2.right * finalForce, ForceMode2D.Force);
		print("Final Force:" + finalForce + ". Speed: " + playerRb.linearVelocityX + ". Input: " + inputSpeed + "");
	}

	#endregion
}