using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[Serializable]
public struct Conversation {
	public             string[] dialogue;
	public             bool     preventMovement;
	public             bool     finnIncluded;
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
	//! Non-serialized fields are set in the UI element under "DialogueRefs"
	private TextMeshProUGUI textBox, nameBox, skipText;
	private Image    normalImage, holyOverlayImage, gradientImage;
	private RawImage fogImage;
	private Image    skipGlyph;
	private Color    gradientColor;

	[SerializeField] private Sprite playerSprite;

	//? Settings
	[Header("Settings")]
	[SerializeField] private float conversationSpeed = 0.05f;
	[SerializeField] private float  pauseTime          = 1f;
	[SerializeField] private string pauseString        = "<pause>";
	[SerializeField] private string halfPauseString    = "<halfpause>";
	private                  char   pauseCharacter     = '|';
	private                  char   halfPauseCharacter = '~';

	//? Conversations
	[OdinSerialize] private readonly Dictionary<string, Conversation> Conversations = new();

	//? States
	public  bool pressedProceed;
	private bool ready;

	#endregion

	#region Unity Functions

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("ConversationHandler");
	}

	private void Awake() {
		Debug = new DebugHandler("ConversationHandler");

		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start() {
		SetDialogueRefs();

		nameBox.text = "";
		textBox.text = "";

		normalImage.color      = Color.clear;
		holyOverlayImage.color = Color.clear;
		fogImage.color         = Color.clear;
		skipGlyph.color        = Color.clear;
		skipText.color         = Color.clear;
		gradientImage.color    = Color.clear;

		normalImage.enabled      = false;
		holyOverlayImage.enabled = false;
		fogImage.enabled         = false;
		gradientImage.enabled    = false;
		skipGlyph.enabled        = false;
		skipText.enabled         = false;
	}

	#endregion

	#region Functions

	public void StartConversation(string conversationId) {
		if (PlayerData.Instance.InConversation || !ready)
			Debug.LogError("Already in conversation! Cannot start a new one.");
		else Debug.Log($"Starting conversation: {conversationId}");
		ready = false;
		StartCoroutine(StartConversationRoutine(conversationId));
	}

	private void ToggleDisplay(bool visible, Conversation conversation = default) {
		var color = visible ? Color.white : Color.clear;

		fogImage.enabled      = conversation.otherPartHolyOverlay;
		gradientImage.enabled = !conversation.otherPartHolyOverlay;
		skipGlyph.enabled     = true;
		skipText.enabled      = true;

		if (visible) {
			holyOverlayImage.enabled = conversation is { otherPartStart: true, otherPartHolyOverlay: true };
			normalImage.enabled = conversation is { otherPartStart: true, otherPartHolyOverlay: false } or
			                                      { otherPartStart: false };

			normalImage.sprite      = conversation.otherPartStart ? conversation.otherPartSprite : playerSprite;
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

		LeanTween.color(gradientImage.rectTransform, visible ? gradientColor : Color.clear, 2f)
		         .setEase(LeanTweenType.easeInOutQuad);

		LeanTween.color(skipGlyph.rectTransform, new Color(.3f, .3f, .3f, visible ? 1 : 0), 1f)
		         .setEase(LeanTweenType.easeInOutQuad);

		LeanTween.value(skipText.gameObject, visible ? 0 : 1, visible ? 1 : 0, 1f)
		         .setOnUpdate(UpdateSkipTextAlpha)
		         .setEase(LeanTweenType.easeInOutQuad);
	}

	private void UpdateSkipTextAlpha(float value) {
		skipText.color = new Color(.3f, .3f, .3f, value);
	}

	private string ParseText(string text, bool clean = false) {
		var parsedText = text;

		parsedText = parsedText.Replace(pauseString,     clean ? "" : pauseCharacter.ToString());
		parsedText = parsedText.Replace(halfPauseString, clean ? "" : halfPauseCharacter.ToString());

		return parsedText;
	}

	private static void PlaySound(bool lastLetter = false) =>
		AudioManager.Instance.PlayOneShot(lastLetter
			                                  ? FMODEvents.Instance.dialogueLastLetter
			                                  : FMODEvents.Instance.dialogueLetter);


	private void UpdateSkipText(bool finished) {
		skipText.text = finished ? "TO PROCEED" : "TO SKIP";
	}

	private void HandleStartEnd(bool start, Conversation conversation) {
		PlayerData.Instance.InConversation = start;
		if (conversation.preventMovement)
			PlayerData.Instance.PreventMovement = start;
	}

	private void CharacterSwap(bool toFinn, Conversation conversation) {
		normalImage.sprite       = toFinn ? playerSprite : conversation.otherPartSprite;
		normalImage.enabled      = toFinn || !conversation.otherPartHolyOverlay;
		holyOverlayImage.enabled = !toFinn && conversation.otherPartHolyOverlay;

		nameBox.text = toFinn ? "Finn" : conversation.otherPartName;
		textBox.text = "";
	}

	/*
	 * This is a workaround for the fact that this script is using SerializedMonoBehaviour instead of MonoBehaviour,
	 * which means that this gameObject can't be a prefab or a child of one. Instead, to ease the workload, the DialogueRefs
	 * MonoBehaviour is placed in the prefab, and we use its values here. Not ideal, but it would be even less ideal to
	 * not have the serialization and allat for the conversations :pray:
	 *
	 * https://odininspector.com/patch-notes/2-0-14-0
	 *
	 * TL;DR: This script can't be in a prefab due to it being a SerializedMonoBehaviour. Known Odin flaw but okay tradeoff.
	 */
	private void SetDialogueRefs() {
		if (!DialogueRefs.Instance) {
			Debug.LogError("DialogueRefs instance not found! Make sure you have a working DialogueRefs script in your scene.");
			return;
		}

		textBox          = DialogueRefs.Instance.textBox;
		nameBox          = DialogueRefs.Instance.nameBox;
		skipText         = DialogueRefs.Instance.skipText;
		normalImage      = DialogueRefs.Instance.normalImage;
		holyOverlayImage = DialogueRefs.Instance.holyOverlayImage;
		fogImage         = DialogueRefs.Instance.fogImage;
		gradientImage    = DialogueRefs.Instance.gradientImage;
		skipGlyph        = DialogueRefs.Instance.skipGlyph;
		gradientColor    = DialogueRefs.Instance.gradientColor;
	}

	#endregion

	#region Coroutines

	private IEnumerator WaitForPlayer() {
		if (!PlayerData.Instance.InConversation) yield break;
		UpdateSkipText(true);
		while (!pressedProceed) yield return null;
		UpdateSkipText(false);
		pressedProceed = false;
	}

	private IEnumerator StartConversationRoutine(string conversationId) {
		//* Null-guards
		if (conversationId == null) yield break;
		Conversations.TryGetValue(conversationId, out var conversation);
		if (conversation.dialogue.Length == 0) yield break;

		//* Update states and fade in dialogue UI
		HandleStartEnd(true, conversation);
		ToggleDisplay(true, conversation);

		yield return new WaitForSecondsRealtime(1f);

		//* Write character name
		var startName = conversation.otherPartStart ? conversation.otherPartName : "Finn";
		foreach (var letter in startName.Length > 0 ? startName : "???") {
			nameBox.text += letter;
			PlaySound();
			yield return new WaitForSecondsRealtime(conversationSpeed);
		}

		PlaySound(true);

		yield return new WaitForSecondsRealtime(1f);

		var nextPartIndex     = 0;
		var otherPartSpeaking = conversation.otherPartStart;

		foreach (var part in conversation.dialogue) {
			nextPartIndex++;

			//* Parse dialogue text
			var parsedPart = ParseText(part);
			var cleanPart  = ParseText(part, true);

			//* Clear textbox
			textBox.text = "";

			//* Loop through parsed text
			foreach (var letter in parsedPart) {
				//? If the letter is a pause character, wait for the pause time and dont add it to the textbox
				if (letter == pauseCharacter || letter == halfPauseCharacter) {
					PlaySound(true);
					yield return new WaitForSecondsRealtime(letter == pauseCharacter ? pauseTime : pauseTime / 2);
				} else {
					PlaySound();
					textBox.text += letter;
				}

				//? If the player pressed the next sentence button, finish the part early
				if (pressedProceed) {
					Debug.Log("Finishing part early");
					pressedProceed = false;
					textBox.text   = cleanPart;
					break;
				}

				//? Otherwise, wait for the next letter
				yield return new WaitForSecondsRealtime(conversationSpeed);
			}

			//* After the loop has finished, play the last letter sound.
			PlaySound(true);
			Debug.Log("Finished part: " + part);

			if (conversation.dialogue.Length <= nextPartIndex) continue;
			yield return WaitForPlayer();

			if (conversation is not { finnIncluded: true, otherPartStart: true }) continue;
			CharacterSwap(otherPartSpeaking, conversation);
			otherPartSpeaking = !otherPartSpeaking;
		}

		yield return WaitForPlayer();

		//* Update states and fade out dialogue UI
		ToggleDisplay(false);
		HandleStartEnd(false, conversation);

		if (conversationId == "End") {
			Debug.LogException(new Exception("Game ended conversation reached. Goodbye player."));
			yield return new WaitForSecondsRealtime(3f);
			UIController.Instance.FadeToBlack(20f);
			yield return new WaitForSecondsRealtime(4f);
			SceneManager.LoadScene("Main");
		}

		ready = true;
		Debug.Log("Finished conversation");
	}

	#endregion
}