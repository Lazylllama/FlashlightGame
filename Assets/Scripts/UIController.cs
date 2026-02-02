using FlashlightGame;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour {
	#region Fields

	//* Instance *//
	public static UIController Instance;

	[SerializeField] private TMP_Text healthText;
	[SerializeField] private TMP_Text batteryText;

	#endregion

	#region Unity Functions

	private void Start() {
		RegisterInstance(this);
	}

	#endregion

	#region Functions

	private static void RegisterInstance(UIController instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;

			DebugHandler.Log("UIController initialized.");
		}
	}

	/// <summary>
	/// Update the UI with the current player data.
	/// </summary>
	public void UpdateUI() {
		healthText.text  = $"Health: {PlayerData.Instance.Health}";
		batteryText.text = $"Battery: {PlayerData.Instance.Battery}";
	}

	#endregion
}