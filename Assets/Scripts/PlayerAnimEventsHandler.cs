using UnityEngine;

public class PlayerAnimEventsHandler : MonoBehaviour {
	// Play footstep sound
	public void OnFootstep() =>
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.playerFootstep, transform.position + (Vector3.down * 2f));
}