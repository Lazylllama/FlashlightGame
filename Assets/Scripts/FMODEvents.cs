using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour {
	#region Fields

	public static FMODEvents Instance;

	#endregion

	#region FMOD Events

	public EventReference savedGame;
	public EventReference buttonPress;

	public EventReference playerFootstep;

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