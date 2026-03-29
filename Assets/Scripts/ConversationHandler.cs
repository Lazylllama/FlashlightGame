using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Conversation {
	public string[] Dialogue;
	public Sprite   OtherPartSprite;
	public bool     OtherPartStart;
	public string   OtherPartName;
}

public class ConversationHandler : MonoBehaviour {
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
		// lär dig instationata och använda klasser i c# du idiot
		// test.Dialogue = new[] {
		// 	"Hello there! aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "How are you doing today?", "Isn't this a nice day?" };
		// test.OtherPartStart = true;
		// test.OtherPartSprite = otherPartSprite;
		// test.OtherPartName = "Mango";

		var test = new Conversation() {
			Dialogue = new[] {
				"Hella there! aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
				"How are you doing today?", "Isn't this a nice day?",
				"Inte när thino inte kan instatiata klasser rätt jävla idiot"
			},
			OtherPartStart = true,
			OtherPartSprite = otherPartSprite,
			OtherPartName   = "Mango"
		};

		StartConversation(test);
	}

	private void Awake() => RegisterInstance(this);

	#endregion

	#region Functions

	public void SkipButtonPressed() {
		if (currentDialogueIndex >= currentConversation.Dialogue.Length - 1) {
			FinishConversation();
			return;
		}

		if (isTalking) {
			skipDialogue = true;
		} else {
			if (isTalking) {
				isTalking    = false;
				image.sprite = currentConversation.OtherPartSprite;

				nameBox.text = currentConversation.OtherPartName;
			} else {
				isTalking    = true;
				image.sprite = playerSprite;
				nameBox.text = "Player";
			}

			currentDialogueIndex++;
			StartCoroutine(TypeSentence(currentConversation.Dialogue[currentDialogueIndex]));
		}
	}

	private void RegisterInstance(ConversationHandler instance) {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
		} else {
			Instance = this;
		}
	}

	public void StartConversation(Conversation conversation) {
		if (conversation == null) throw new ArgumentNullException(nameof(conversation));
		currentConversation = conversation;
		conversationUI.SetActive(true);
		textBox.text                  = "";
		PlayerData.Instance.IsTalking = true;
		if (conversation.OtherPartStart) {
			image.sprite = conversation.OtherPartSprite;
			nameBox.text = conversation.OtherPartName;
			UpdateDialogue(conversation.Dialogue[currentDialogueIndex]);
		} else {
			image.sprite = playerSprite;
			nameBox.text = "Player";
			UpdateDialogue(conversation.Dialogue[currentDialogueIndex]);
		}
	}

	private void UpdateDialogue(string sentence) {
		if (currentDialogueIndex >= 0) {
			textBox.text = "";
			StartCoroutine(TypeSentence(sentence));
		}
	}

	IEnumerator TypeSentence(string sentence) {
		isTalking = true;
		for (int i = 1; i < sentence.Length; i++) {
			if (skipDialogue) {
				textBox.text = sentence;
				skipDialogue = false;
				isTalking    = false;
				yield break;
			}

			textBox.text = sentence.Substring(0, i);
			yield return new WaitForSeconds(conversationSpeed);
		}

		skipDialogue = false;
		isTalking    = false;
	}

	private void FinishConversation() {
		isTalking    = false;
		skipDialogue = false;
		conversationUI.SetActive(false);
		currentDialogueIndex = 0;
		currentConversation  = null;
		currentLetter        = 0;
		textBox.text         = "";
		nameBox.text         = "";
		image.sprite         = null;
	}

	#endregion
}