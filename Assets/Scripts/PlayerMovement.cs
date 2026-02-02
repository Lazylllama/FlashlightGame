using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FlashlightGame;

public class PlayerMovement : MonoBehaviour {
	#region Fields

	//* Instance
	public static PlayerMovement Instance;

	[Header("Settings")]
	[SerializeField] private LayerMask groundLayer;

	//* Refs
	private InputAction moveAction, mantleAction;
	private Rigidbody2D playerRb;

	private static bool IsLookingRight {
		get => PlayerData.Instance && PlayerData.Instance.IsLookingRight;
		set {
			if (PlayerData.Instance) {
				PlayerData.Instance.IsLookingRight = value;
			}
		}
	}

	//! Friction impacts speed *GREATLY*
	[Header("Movement Settings")]
	[SerializeField] private float maxSpeed, acceleration;

	[Header("Ground Check")]
	[SerializeField] private Transform groundCheckPosition;
	[SerializeField] private float groundCheckRadius;

	[Header("Mantling")]
	[SerializeField] private Transform headLevelPosition;
	[SerializeField] private float mantleCheckDistance;

	//* States
	private bool      isGrounded;
	private bool      canMantle;
	private Coroutine mantleRoutineState;
	private Vector2   moveInputVal;

	#endregion

	#region Unity Functions

	private void Start() {
		playerRb     = GetComponent<Rigidbody2D>();
		moveAction   = InputSystem.actions.FindAction("Move");
		mantleAction = InputSystem.actions.FindAction("MantleClimb");
	}

	private void Update() {
		RaycastChecks();
	}

	private void FixedUpdate() {
		InputCheck();
		PerformMove();
	}

	//? Set global instance
	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
		} else {
			Instance = this;
		}
	}

	#endregion

	#region Functions

	private void RaycastChecks() {
		var groundCheckHit = Physics2D.OverlapCircle(groundCheckPosition.position, groundCheckRadius, groundLayer);
		var mantleCheckHit = Lib.Movement.WallCheck(headLevelPosition.position, IsLookingRight);

		isGrounded = groundCheckHit;
		canMantle  = mantleCheckHit;
	}

	private void InputCheck() {
		moveInputVal = moveAction.ReadValue<Vector2>();
		IsLookingRight = moveInputVal.x switch {
			< 0 when IsLookingRight  => false,
			> 0 when !IsLookingRight => true,
			_                        => IsLookingRight
		};

		if (mantleAction.IsPressed()) Mantle();
	}

	private void PerformMove() {
		var inputSpeed      = moveInputVal.x * maxSpeed;
		var speedDifference = inputSpeed - playerRb.linearVelocityX;
		var finalForce      = speedDifference * acceleration;

		playerRb.AddForce(Vector2.right * finalForce, ForceMode2D.Force);


		DebugHandler.LogKv("PerformMove", DebugLevel.Debug, new object[] {
			"inputSpeed", inputSpeed,
			"speedDifference", speedDifference,
			"finalForce", finalForce
		});
	}

	private void Mantle() {
		DebugHandler.LogKv("Mantle", DebugLevel.Debug, new object[] {
			"isGrounded", isGrounded,
			"canMantle", canMantle,
			"mantleRoutineState", mantleRoutineState
		});

		if (!isGrounded || !canMantle || mantleRoutineState != null) return;
		mantleRoutineState = StartCoroutine(MantleRoutine());
	}

	#endregion

	#region Coroutines

	private IEnumerator MantleRoutine() {
		var mantle = Lib.Movement.GetWallClimbPoint(transform.position, IsLookingRight);

		if (mantle.Position == Vector3.zero) {
			DebugHandler.Log("Mantle point invalid, cancelling mantle.", DebugLevel.Warning);
			mantleRoutineState = null;
			yield break;
		}

		transform.position = mantle.Position;
		mantleRoutineState = null;
	}

	#endregion
}