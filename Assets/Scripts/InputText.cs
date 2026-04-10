using System;
using FlashlightGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputText : MonoBehaviour {
	#region Fields

	private static DebugHandler Debug;

	[SerializeField] private InputHandler.InputActions inputAction;
	[SerializeField] private bool                      getInputDisplayName;
	[SerializeField] private string                    prefix, suffix;

	private TextMeshProUGUI text;

	private bool isInitialized;

	#endregion

	#region Unity Functions

	private void FixedUpdate() {
		if (!isInitialized) Initialize();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("InputText");
	}

	private void Awake() {
		Debug ??= new DebugHandler("InputText");

		text = GetComponent<TextMeshProUGUI>();

		if (text) return;
		Debug.LogError("[ERROR] [InputText] InputText requires a TextMeshProUGUI component.");
		enabled = false;
	}

	private void OnEnable() => Initialize();

	private void OnDisable() {
		if (!isInitialized || !InputHandler.Instance) return;
		InputHandler.Instance.inputChange.RemoveListener(RefreshText);
	}

	#endregion

	#region Functions

	private void RefreshText(Lib.InputType inputType = default) {
		text.text = getInputDisplayName
			            ? prefix + InputHandler.Instance.GetInputDisplayName(inputAction) + suffix
			            : prefix + InputHandler.Instance.GetInputName(inputAction) + suffix;
	}

	private void Initialize() {
		if (!InputHandler.Instance) return;
		InputHandler.Instance.inputChange.AddListener(RefreshText);
		RefreshText();
		isInitialized = true;
	}

	#endregion
}