using UnityEngine;

public class PlayerAnimEventsHandler : MonoBehaviour {
	// Attempt climb
	public void ClimbAnimationFinished() => PlayerMovement.Instance.ClimbAnimationFinished();

	// Play footstep sound
	public void OnFootstep() =>
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.playerFootstep, transform.position + (Vector3.down * 2f));
}