using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FlashlightGame;
using Sirenix.OdinInspector;

public class PlayerMovement : MonoBehaviour {
	#region Fields

	//* Instance
	public static  PlayerMovement Instance;
	private static DebugHandler   Debug;

	//* Hash
	private static readonly int WalkingDirection = Animator.StringToHash("walkingDirection");

	//* Refs
	private Rigidbody2D        playerRb;
	private ParticleController particleController;
	private Animator           playerAnimator;

	//? Aiming right and walking left will cause the player to walk blindly/backwards.
	private static bool IsLookingRight => PlayerData.Instance && PlayerData.Instance.IsLookingRight;
	private static bool IsWalkingRight {
		get => PlayerData.Instance && PlayerData.Instance.IsWalkingRight;
		set {
			if (PlayerData.Instance) {
				PlayerData.Instance.IsWalkingRight = value;
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
	[ReadOnly] [SerializeField] public  AudioManager.FootstepSurface currentSurface;
	[ReadOnly] [SerializeField] private Vector2                      moveInputVal;
	private                             bool                         isGrounded, canMantle;
	private                             Coroutine                    mantleRoutineState;
	private                             Vector2                      lastPosition;

	#endregion

	#region Unity Functions

	//? Set global instance
	private void Awake() => RegisterInstance(this);

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("PlayerMovement");
	}

	private void Start() {
		Debug ??= new DebugHandler("PlayerMovement");

		playerRb           = GetComponent<Rigidbody2D>();
		particleController = GetComponentInChildren<ParticleController>();
		playerAnimator     = GetComponentInChildren<Animator>();

		lastPosition = transform.position;
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
		Gizmos.color = Lib.Movement.WallCheck(headLevelPosition.position, IsWalkingRight).collider
			               ? Color.green
			               : Color.red;
		Gizmos.DrawLine(headLevelPosition.position,
		                headLevelPosition.position + (IsWalkingRight ? Vector3.right : Vector3.left) *
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
		var mantleCheckHit = Lib.Movement.MantleWallCheck(headLevelPosition.position, IsWalkingRight);

		isGrounded = groundCheckHit;
		canMantle  = mantleCheckHit.collider;

		if (!groundCheckHit) return;
		currentSurface = surfaceTags.GetValueOrDefault(groundCheckHit.tag, AudioManager.FootstepSurface.Dirt);
		AudioManager.Instance.SetFootstepSurface(currentSurface);
	}

	private void InputCheck() {
		moveInputVal = InputHandler.Instance.ReadValue(InputHandler.InputActions.Move);
		if (Mathf.Abs(moveInputVal.x) < Preferences.Input.MoveInputDeadZone) return;
		IsWalkingRight = moveInputVal.x switch {
			< 0 when IsWalkingRight  => false,
			> 0 when !IsWalkingRight => true,
			_                        => IsWalkingRight
		};
	}

	private void PerformMove() {
		var inputSpeed      = moveInputVal.x * maxSpeed;
		var speedDifference = inputSpeed - playerRb.linearVelocityX;
		var finalForce      = speedDifference * acceleration;

		var movement = (Vector2)transform.position - lastPosition;
		var moveX    = movement.x;

		if (Mathf.Abs(moveX) > 0.05f) {
			// If moving in the same direction the player is looking, it's "forward" (1), otherwise "backward" (-1)
			var movingRight   = moveX > 0f;
			var walkDirection = movingRight == IsLookingRight ? 1 : -1;

			//? Particles very broken
			//particleController.CrateMovement(moveX);

			playerAnimator.SetInteger(WalkingDirection, walkDirection);
		} else {
			playerAnimator.SetInteger(WalkingDirection, 0);
		}

		lastPosition = transform.position;

		playerRb.AddForce(Vector2.right * finalForce, ForceMode2D.Force);

		//! Do not include in prod builds, only use when testing accel :)
		// Debug.LogKv("PerformMove", DebugLevel.Debug, new object[] {
		// 	"inputSpeed", inputSpeed,
		// 	"speedDifference", speedDifference,
		// 	"finalForce", finalForce
		// });
	}

	/// <summary>
	/// Called by InputHandler, attempts to mantle.
	/// </summary>
	public void Mantle() {
		Debug.LogKv("Mantle", DebugLevel.Debug, new object[] {
			"isGrounded", isGrounded,
			"canMantle", canMantle
		});

		if (!isGrounded || !canMantle || mantleRoutineState != null) return;
		mantleRoutineState = StartCoroutine(MantleRoutine());
	}

	public void Respawn(Vector3 position) {
		playerRb.linearVelocity = Vector2.zero;
		transform.position      = position;
	}

	#endregion

	#region Coroutines

	private IEnumerator MantleRoutine() {
		var mantle = Lib.Movement.GetWallMantlePoint(headLevelPosition.position, IsWalkingRight);

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