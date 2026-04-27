using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlashlightGame;
using Sirenix.OdinInspector;

public class PlayerMovement : MonoBehaviour {
	#region Fields

	//* Instance
	public static  PlayerMovement Instance;
	private static DebugHandler   Debug;

	//* Hash
	private static readonly int WalkingDirection = Animator.StringToHash("walkingDirection");
	private static readonly int IsFalling        = Animator.StringToHash("isFalling");
	private static readonly int HasFlashlight    = Animator.StringToHash("hasFlashlight");
	private static readonly int StartClimb       = Animator.StringToHash("startClimb");

	//* Refs
	private Rigidbody2D playerRb;
	private Animator    playerAnimator;

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
	private                             bool                         isGrounded;
	private                             Coroutine                    mantleRoutineState;
	private                             Vector2                      lastPosition;

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("PlayerMovement");
	}

	private void Start() {
		Debug ??= new DebugHandler("PlayerMovement");

		lastPosition = transform.position;
	}

	private void Awake() {
		RegisterInstance(this);

		//? Required in other start functions
		playerRb       = GetComponent<Rigidbody2D>();
		playerAnimator = GetComponentInChildren<Animator>();
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

		isGrounded = groundCheckHit;

		if (!groundCheckHit && !Lib.Movement.WallCheck(gameObject.transform.position, IsWalkingRight).collider) {
			playerAnimator.SetBool(IsFalling, true);
		} else {
			playerAnimator.SetBool(IsFalling, false);
		}

		if (!groundCheckHit) return;
		currentSurface = surfaceTags.GetValueOrDefault(groundCheckHit.tag, AudioManager.FootstepSurface.Dirt);
		AudioManager.Instance.SetFootstepSurface(currentSurface);
	}

	private void InputCheck() {
		moveInputVal = InputHandler.Instance.ReadValue(InputHandler.InputActions.Move);
		if (Mathf.Abs(moveInputVal.x) < Preferences.Input.MoveInputDeadZone) return;
		if (PlayerData.Instance.PreventMovement) moveInputVal = Vector2.zero;
		IsWalkingRight = moveInputVal.x switch {
			< 0 when IsWalkingRight  => false,
			> 0 when !IsWalkingRight => true,
			_                        => IsWalkingRight
		};
	}

	private void PerformMove() {
		var inputSpeed = (Mathf.Abs(moveInputVal.x) > Preferences.Input.MoveInputDeadZone ? moveInputVal.x : 0) *
		                 maxSpeed;
		var speedDifference = inputSpeed - playerRb.linearVelocityX;
		var finalForce      = speedDifference * acceleration;

		var movement = (Vector2)transform.position - lastPosition;
		var moveX    = movement.x;

		if (Mathf.Abs(moveX) > 0.039f) {
			// If moving in the same direction the player is looking, it's "forward" (1), otherwise "backward" (-1)
			var movingRight   = moveX > 0f;
			var walkDirection = movingRight == IsLookingRight ? 1 : -1;

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
	/// Set flashlight to true in animator.
	/// </summary>
	public void PickupFlashlight() => playerAnimator.SetBool(HasFlashlight, true);

	/// <summary>
	/// Tries to climb the object in front
	/// </summary>
	public void TryClimb() {
		var mantleCheckHit = Lib.Movement.MantleWallCheck(headLevelPosition.position, IsWalkingRight);
		if (!mantleCheckHit.collider || !PlayerData.Instance.FlashlightModesUnlocked[1]) return;

		PlayerData.Instance.PreventMovement = true;
		playerAnimator.SetTrigger(StartClimb);
	}

	/// <summary>
	/// Teleports the actual player to where the animation stopped and allows momvent
	/// </summary>
	public void ClimbAnimationFinished() {
		transform.position                  += new Vector3(1.631f, 2.429f) + new Vector3(0.7f, 0.7f);
		PlayerData.Instance.PreventMovement =  false;
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