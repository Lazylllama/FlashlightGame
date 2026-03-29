using FlashlightGame;
using UnityEngine;

public class GameController : MonoBehaviour {
	#region Fields

	public static  GameController Instance;
	private static DebugHandler   Debug;


	//* Data *//
	public bool InActiveGame { get; set; } = false;

	//* State *//

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("PlayerData");
	}

	private void Start() => RegisterInstance(this);

	#endregion

	#region Functions

	public void StartNewGame() {
		Debug.Log("Starting new game...", DebugLevel.Debug);
		UIController.Instance.InitiateGameStart();
	}


	/// Register the PlayerData instance.
	private static void RegisterInstance(GameController instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;

			Debug.Log("PlayerData initialized.");
		}
	}

	#endregion
}