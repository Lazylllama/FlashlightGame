using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[Serializable]
public struct Conversation {
	public             string[] dialogue;
	public             bool     preventMovement;
	public             bool     playerIncluded;
	public             bool     otherPartStart;
	public             bool     otherPartHolyOverlay;
	[CanBeNull] public Sprite   otherPartSprite;
	[CanBeNull] public string   otherPartName;
}

[ShowOdinSerializedPropertiesInInspector]
public class ConversationHandler : SerializedMonoBehaviour {
	#region Fields

	//? Instance
	public static  ConversationHandler Instance;
	private static DebugHandler        Debug;

	[Header("Refs")]
	[SerializeField] private GameObject conversationUI;
	[SerializeField] private TextMeshProUGUI textBox,     nameBox, skipText;
	[SerializeField] private Image           normalImage, holyOverlayImage;
	[SerializeField] private RawImage        fogImage;
	[SerializeField] private Image           skipGlyph;

	[SerializeField] private Sprite playerSprite;

	//? Settings
	[SerializeField] private float conversationSpeed = 0.05f;

	//? Conversations

	[OdinSerialize] private readonly Dictionary<string, Conversation> Conversations = new();

	//? States
	private Conversation currentConversation;
	public  bool         pressedProceed;

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("AudioManager");
	}

	private void Awake() {
		Debug = new DebugHandler("AudioManager");

		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start() {
		nameBox.text = "";
		textBox.text = "";

		normalImage.color      = Color.clear;
		holyOverlayImage.color = Color.clear;
		fogImage.color         = Color.clear;
		skipGlyph.color        = Color.clear;
		skipText.color         = Color.clear;
	}

	#endregion

	#region Functions

	public void StartConversation(string conversationId) {
		if (PlayerData.Instance.InConversation) Debug.LogError("Already in conversation! Cannot start a new one.");
		else Debug.Log($"Starting conversation: {conversationId}");
		StartCoroutine(StartConversationRoutine(conversationId));
	}

	private void ToggleDisplay(bool visible, Conversation conversation = default) {
		var color = visible ? Color.white : Color.clear;

		if (visible) {
			holyOverlayImage.enabled = conversation.otherPartHolyOverlay;
			normalImage.enabled      = !conversation.otherPartHolyOverlay;

			normalImage.sprite      = conversation.otherPartSprite;
			holyOverlayImage.sprite = conversation.otherPartSprite;
		} else {
			textBox.text = "";
			nameBox.text = "";
		}

		LeanTween.color(normalImage.rectTransform, color, 1f)
		         .setEase(LeanTweenType.easeInOutQuad);
		LeanTween.color(holyOverlayImage.rectTransform, color, 1f)
		         .setEase(LeanTweenType.easeInOutQuad);
		LeanTween.color(fogImage.rectTransform, color, 2f)
		         .setEase(LeanTweenType.easeInOutQuad);

		LeanTween.color(skipGlyph.rectTransform, new Color(.3f, .3f, .3f, visible ? 1 : 0), 1f)
		         .setEase(LeanTweenType.easeInOutQuad);

		LeanTween.value(skipText.gameObject, visible ? 0 : 1, visible ? 1 : 0, 2f).setOnUpdate(UpdateTextAlpha);
	}

	private void UpdateTextAlpha(float value) {
		skipText.color = new Color(.3f, .3f, .3f, value);
	}

	#endregion

	#region Coroutines

	private IEnumerator StartConversationRoutine(string conversationId) {
		//* Null-guards
		if (conversationId == null) yield break;
		Conversations.TryGetValue(conversationId, out var conversation);
		if (conversation.dialogue.Length == 0) yield break;

		//* Feature implementation check
		if (conversation.playerIncluded) {
			Debug.LogException(new Exception("You haven't implemented player included conversations yet dipshit."));
			yield break;
		}

		//* Update state
		PlayerData.Instance.InConversation = true;
		if (conversation.preventMovement)
			PlayerData.Instance.PreventMovement = true;
		currentConversation = conversation;

		//* Fade in dialogue UI
		ToggleDisplay(true, conversation);

		yield return new WaitForSecondsRealtime(1f);

		foreach (var letter in conversation.otherPartName.Length > 0 ? conversation.otherPartName : "???") {
			nameBox.text += letter;
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dialogueLetter);
			yield return new WaitForSecondsRealtime(conversationSpeed);
		}

		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dialogueLastLetter);

		yield return new WaitForSecondsRealtime(1f);

		foreach (var part in conversation.dialogue) {
			textBox.text = "";
			foreach (var letter in part) {
				textBox.text += letter;

				if (pressedProceed) {
					Debug.Log("Finishing part early");
					pressedProceed = false;
					textBox.text   = part;
					break;
				} else {
					AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dialogueLetter);
				}

				yield return new WaitForSecondsRealtime(conversationSpeed);
			}

			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dialogueLastLetter);
			Debug.Log("Finished part: " + part);

			while (!pressedProceed) yield return null;

			pressedProceed = false;

			yield return new WaitForSecondsRealtime(1f);
		}

		while (pressedProceed) yield return null;

		//* Fade out dialogue UI
		ToggleDisplay(false);
		PlayerData.Instance.InConversation = false;
		if (conversation.preventMovement)
			PlayerData.Instance.PreventMovement = false;

		Debug.Log("Finished conversation");
	}

	#endregion
}