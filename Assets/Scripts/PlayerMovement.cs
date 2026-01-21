using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
	#region Fields

	//* Instance
	public static PlayerMovement Instance;

	//* Refs
	private InputAction jumpAction, moveAction;
	private Rigidbody2D playerRb;

	[Header("Movement Settings")]
	[SerializeField] private float jumpForce, playerSpeed;

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
		jumpAction = InputSystem.actions.FindAction("Jump");
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

		if (hit) {
			isGrounded = true;
		} else {
			isGrounded = false;
		}
	}

	private void InputCheck() {
		moveInputVal = moveAction.ReadValue<Vector2>();
		if (jumpAction.IsPressed()) PerformJump();
	}

	private void PerformJump() {
		switch (isGrounded) {
			case true:
				playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
				break;
		}
	}

	private void PerformMove() {
		playerRb.linearVelocityX = moveInputVal.x * playerSpeed;
	}

	#endregion
}