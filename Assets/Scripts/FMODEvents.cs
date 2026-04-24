using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour {
	#region Fields

	public static FMODEvents Instance;

	#endregion

	#region FMOD Events

	public EventReference savedGame;
	public EventReference buttonPress;
	public EventReference openMenu,       closeMenu;
	public EventReference dialogueLetter, dialogueLastLetter;

	public EventReference enemyFlap, enemyFlyingDie, enemyFlyingAttack, bearFootstep, bearAttack, bearDeath;
	public EventReference playerFootstep;
	public EventReference flashlightToggle;
	public EventReference flashlightCrank;

	public EventReference crowsAmbience;
	public EventReference forestWindAmbience;

	public EventReference gameMusic;

	#endregion

	#region Unity Functions

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	#endregion
}