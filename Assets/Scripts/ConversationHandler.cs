using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public struct Conversation {
	public string[] dialogue;
	public Sprite   otherPartSprite;
	public bool     otherPartStart;
	public string   otherPartName;
}

[ShowOdinSerializedPropertiesInInspector]
public class ConversationHandler : SerializedMonoBehaviour {
	#region Fields

	//? Instance
	public static ConversationHandler Instance;

	[Header("Refs")]
	[SerializeField] private GameObject conversationUI;
	[SerializeField] private TextMeshProUGUI textBox;
	[SerializeField] private TextMeshProUGUI nameBox;
	[SerializeField] private Image           image;
	[SerializeField] private Sprite          playerSprite, otherPartSprite;

	//? Settings
	[SerializeField] private float conversationSpeed = 0.05f;

	//? Conversations

	[OdinSerialize]
	private readonly Dictionary<string, Conversation> Conversations = new Dictionary<string, Conversation>();

	//? States
	private Conversation currentConversation;

	public  bool playerCanWalk = true;
	private int  currentDialogueIndex;
	private bool isTalking;
	private int  currentLetter;
	private bool skipDialogue;

	#endregion

	#region Unity Functions

	private void Start() {
		//StartConversation(test);
	}

	// private void Awake() => RegisterInstance(this);

	#endregion

	#region Functions

	// public void StartConversation(string conversationId) {
	// 	conversationId = conversationId.ToLower();
	// 	if (Conversations.ContainsKey(conversationId)) {
	// 		StartConversation(Conversations[conversationId]);
	// 	}
	// }
	//
	// public void SkipButtonPressed() {
	// 	if (currentDialogueIndex >= currentConversation.Dialogue.Length - 1) {
	// 		FinishConversation();
	// 		return;
	// 	}
	//
	// 	if (isTalking) {
	// 		skipDialogue = true;
	// 	} else {
	// 		if (isTalking) {
	// 			isTalking    = false;
	// 			image.sprite = currentConversation.OtherPartSprite;
	//
	// 			nameBox.text = currentConversation.OtherPartName;
	// 		} else {
	// 			isTalking    = true;
	// 			image.sprite = playerSprite;
	// 			nameBox.text = "Player";
	// 		}
	//
	// 		currentDialogueIndex++;
	// 		StartCoroutine(TypeSentence(currentConversation.Dialogue[currentDialogueIndex]));
	// 	}
	// }
	//
	// private void RegisterInstance(ConversationHandler instance) {
	// 	if (Instance != null && Instance != this) {
	// 		Destroy(gameObject);
	// 	} else {
	// 		Instance = this;
	// 	}
	// }
	//
	// public void StartConversation(Conversation conversation) {
	// 	if (conversation == null) throw new ArgumentNullException(nameof(conversation));
	// 	currentConversation = conversation;
	// 	conversationUI.SetActive(true);
	// 	textBox.text                  = "";
	// 	PlayerData.Instance.IsTalking = true;
	// 	if (conversation.otherPartStart) {
	// 		image.sprite = conversation.otherPartSprite;
	// 		nameBox.text = conversation.otherPartName;
	// 		UpdateDialogue(conversation.dialogue[currentDialogueIndex]);
	// 	} else {
	// 		image.sprite = playerSprite;
	// 		nameBox.text = "Player";
	// 		UpdateDialogue(conversation.dialogue[currentDialogueIndex]);
	// 	}
	// }
	//
	// private void UpdateDialogue(string sentence) {
	// 	if (currentDialogueIndex >= 0) {
	// 		textBox.text = "";
	// 		StartCoroutine(TypeSentence(sentence));
	// 	}
	// }
	//
	// IEnumerator TypeSentence(string sentence) {
	// 	isTalking = true;
	// 	for (int i = 1; i < sentence.Length; i++) {
	// 		if (skipDialogue) {
	// 			textBox.text = sentence;
	// 			skipDialogue = false;
	// 			isTalking    = false;
	// 			yield break;
	// 		}
	//
	// 		textBox.text = sentence.Substring(0, i);
	// 		yield return new WaitForSeconds(conversationSpeed);
	// 	}
	//
	// 	skipDialogue = false;
	// 	isTalking    = false;
	// }
	//
	// private void FinishConversation() {
	// 	isTalking    = false;
	// 	skipDialogue = false;
	// 	conversationUI.SetActive(false);
	// 	currentDialogueIndex = 0;
	// 	currentConversation  = null;
	// 	currentLetter        = 0;
	// 	textBox.text         = "";
	// 	nameBox.text         = "";
	// 	image.sprite         = null;
	// }

	#endregion
}