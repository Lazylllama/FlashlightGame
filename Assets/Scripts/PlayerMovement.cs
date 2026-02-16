using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FlashlightGame;

public class PlayerMovement : MonoBehaviour {
	#region Fields

	//* Instance
	public static  PlayerMovement Instance;
	private static DebugHandler   Debug;

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

	//* States
	private bool      isGrounded;
	private bool      canMantle;
	private Coroutine mantleRoutineState;
	private Vector2   moveInputVal;

	#endregion

	#region Unity Functions

	//? Set global instance
	private void Awake() => RegisterInstance(this);

	private void Start() {
		Debug = new DebugHandler("PlayerMovement");

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

	private void OnDrawGizmos() {
		//? Ground Check
		Gizmos.color = Lib.Movement.GroundCheck(groundCheckPosition.position, groundCheckRadius)
			               ? Color.green
			               : Color.red;
		Gizmos.DrawWireSphere(groundCheckPosition.position, groundCheckRadius);

		//? Wall Check
		Gizmos.color = Lib.Movement.WallCheck(headLevelPosition.position, IsLookingRight).collider
			               ? Color.green
			               : Color.red;
		Gizmos.DrawLine(headLevelPosition.position,
		                headLevelPosition.position + (IsLookingRight ? Vector3.right : Vector3.left) *
		                Lib.Movement.WallCheckDistance);
	}

	#endregion

	#region Functions

	private void RegisterInstance(PlayerMovement instance) {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
		} else {
			Instance = this;
		}
	}

	private void RaycastChecks() {
		var groundCheckHit = Lib.Movement.GroundCheck(groundCheckPosition.position, groundCheckRadius);
		var mantleCheckHit = Lib.Movement.WallCheck(headLevelPosition.position, IsLookingRight);

		isGrounded = groundCheckHit;
		canMantle  = mantleCheckHit.collider;
	}

	private void InputCheck() {
		moveInputVal = moveAction.ReadValue<Vector2>();
		IsLookingRight = moveInputVal.x switch {
			< 0 when IsLookingRight  => false,
			> 0 when !IsLookingRight => true,
			_                        => IsLookingRight
		};

		if (mantleAction.WasPressedThisFrame()) Mantle();
	}

	private void PerformMove() {
		var inputSpeed      = moveInputVal.x * maxSpeed;
		var speedDifference = inputSpeed - playerRb.linearVelocityX;
		var finalForce      = speedDifference * acceleration;

		playerRb.AddForce(Vector2.right * finalForce, ForceMode2D.Force);


		//! TODO(@lazylllama): Do not include in prod builds :)
		Debug.LogKv("PerformMove", DebugLevel.Debug, new object[] {
			"inputSpeed", inputSpeed,
			"speedDifference", speedDifference,
			"finalForce", finalForce
		});
	}

	private void Mantle() {
		Debug.LogKv("Mantle", DebugLevel.Debug, new object[] {
			"isGrounded", isGrounded,
			"canMantle", canMantle
		});

		if (!isGrounded || !canMantle || mantleRoutineState != null) return;
		mantleRoutineState = StartCoroutine(MantleRoutine());
	}

	#endregion

	#region Coroutines

	private IEnumerator MantleRoutine() {
		var mantle = Lib.Movement.GetWallClimbPoint(transform.position, IsLookingRight);

		if (mantle.Position == Vector3.zero) {
			Debug.Log("Mantle point invalid, cancelling mantle.", DebugLevel.Warning);
			mantleRoutineState = null;
			yield break;
		}

		transform.position = mantle.Position;
		mantleRoutineState = null;
	}

	#endregion
}