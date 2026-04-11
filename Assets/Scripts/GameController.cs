using FlashlightGame;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour {
	#region Fields

	public static  GameController     Instance;
	private static DebugHandler       Debug;
	public         UnityEvent<string> leverEvent, wallTriggerEvent;
	public         UnityEvent<CameraBoundsEventParameters> changeCameraBounds;


	//* Data *//
	public bool InActiveGame        { get; set; } = false;

	//* State *//

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("GameController");
	}

	private void Awake() {
		Debug ??= new DebugHandler("GameController");

		if (leverEvent == null) leverEvent = new UnityEvent<string>();
		leverEvent.AddListener((string id) => Debug.Log(id));
		
		if (wallTriggerEvent == null) wallTriggerEvent = new UnityEvent<string>();
		leverEvent.AddListener((string id) => Debug.Log(id));
		
		if(changeCameraBounds == null) changeCameraBounds = new UnityEvent<CameraBoundsEventParameters>();
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