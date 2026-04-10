using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueRefs : MonoBehaviour {
	public static  DialogueRefs Instance;
	private static DebugHandler Debug;

	[Header("Text")]
	[SerializeField] public TextMeshProUGUI textBox;
	[SerializeField] public TextMeshProUGUI nameBox;
	[SerializeField] public TextMeshProUGUI skipText;

	[Header("Images")]
	[SerializeField] public RawImage fogImage;
	[SerializeField] public Image holyOverlayImage;
	[SerializeField] public Image normalImage;
	[SerializeField] public Image skipGlyph;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("DialogueRefs");
	}

	private void Awake() {
		Debug ??= new DebugHandler("DialogueRefs");

		if (Instance != null && Instance != this) {
			Debug.LogWarning("Multiple instances of DialogueRefs detected. Destroying duplicate.");
			Destroy(gameObject);
			return;
		}

		Instance = this;

		CheckRefs();
	}

	private void CheckRefs() {
		if (textBox == null)
			Debug.LogError("textBox is null!");

		if (nameBox == null)
			Debug.LogError("nameBox is null!");

		if (skipText == null)
			Debug.LogError("skipText is null!");

		if (normalImage == null)
			Debug.LogError("normalImage is null!");

		if (holyOverlayImage == null)
			Debug.LogError("holyOverlayImage is null!");

		if (fogImage == null)
			Debug.LogError("fogImage is null!");

		if (skipGlyph == null)
			Debug.LogError("skipGlyph is null!");
	}
}