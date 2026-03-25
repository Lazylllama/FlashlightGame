using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FlashlightGame;

public class PlayerMovement : MonoBehaviour {
	#region Fields

	//* Instance
	public static           PlayerMovement Instance;
	private static          DebugHandler   Debug;
	private static readonly int            WalkingDirection = Animator.StringToHash("walkingDirection");

	[Header("Settings")]
	[SerializeField] private LayerMask groundLayer;

	//* Refs
	private InputAction        moveAction, mantleAction;
	private Rigidbody2D        playerRb;
	private ParticleController particleController;
	private Animator           playerAnimator;

	private static bool IsLookingRight {
		get => PlayerData.Instance && PlayerData.Instance.IsLookingRight;
		set {
			return;
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

	private readonly Dictionary<string, AudioManager.FootstepSurface> surfaceTags = new() {
		{ "DirtGround", AudioManager.FootstepSurface.Dirt },
		{ "ConcreteGround", AudioManager.FootstepSurface.Concrete },
		{ "GrassGround", AudioManager.FootstepSurface.Grass },
		{ "SandGround", AudioManager.FootstepSurface.Sand },
		{ "WoodGround", AudioManager.FootstepSurface.Wood }
	};

	[Header("Mantling")]
	[SerializeField] private Transform headLevelPosition;

	//* States
	private bool                         isGrounded;
	private bool                         canMantle;
	private Coroutine                    mantleRoutineState;
	private Vector2                      moveInputVal;
	private Vector2                      lastPosition;
	private AudioManager.FootstepSurface currentSurface;

	#endregion

	#region Unity Functions

	//? Set global instance
	private void Awake() => RegisterInstance(this);

	private void Start() {
		Debug = new DebugHandler("PlayerMovement");

		playerRb           = GetComponent<Rigidbody2D>();
		particleController = GetComponentInChildren<ParticleController>();
		playerAnimator     = GetComponentInChildren<Animator>();

		moveAction   = InputSystem.actions.FindAction("Move");
		mantleAction = InputSystem.actions.FindAction("MantleClimb");
	}

	private void Update() {
		RaycastChecks();
	}

	private void FixedUpdate() {
		if (!GameController.Instance.InActiveGame) return;
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
		var mantleCheckHit = Lib.Movement.MantleWallCheck(headLevelPosition.position, IsLookingRight);
		var climbCheckHit  = Lib.Movement.ClimbWallCheck(headLevelPosition.position, IsLookingRight);

		isGrounded = groundCheckHit;
		canMantle  = mantleCheckHit.collider;

		if (!groundCheckHit) return;
		currentSurface = surfaceTags.GetValueOrDefault(groundCheckHit.tag, AudioManager.FootstepSurface.Dirt);
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

		var delta = Vector2.Normalize(lastPosition - (Vector2)transform.position);

		if (delta != Vector2.zero && Mathf.Abs(delta.x) > 0.1f) {
			particleController.CrateMovement(delta.x);
			AudioManager.Instance.PlayFootstepSfx(currentSurface);
			playerAnimator.SetInteger(WalkingDirection, delta.x < 0 ? 1 : -1);
		} else {
			playerAnimator.SetInteger(WalkingDirection, 0);
		}
		
		lastPosition = transform.position;

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