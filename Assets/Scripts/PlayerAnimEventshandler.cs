using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAnimEventshandler : MonoBehaviour {
	public void OnFootstep() {
		AudioManager.Instance.PlayFootstepSfx(PlayerMovement.Instance.currentSurface);
	}
}