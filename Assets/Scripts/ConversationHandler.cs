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
	[SerializeField] private GameObject      conversationUI;
	[SerializeField] private TextMeshProUGUI textBox;
	[SerializeField] private TextMeshProUGUI nameBox;
	[SerializeField] private Image           image;
	[SerializeField] private Sprite          playerSprite, otherPartSprite;
	
	//? Settings
	[SerializeField] private float conversationSpeed = 0.05f;
	
	//? States
	private Conversation currentConversation;
	
	public  bool     playerCanWalk = true;
	private int      currentDialogueIndex;
	private bool     isTalking;
	private int      currentLetter;
	private bool     skipDialogue;
	private bool     playerIsTalking;
	
	#endregion

	#region Unity Functions

	private void Start() { 
		var  test = new Conversation();
		test.Dialogue = new[] {
			"Hello there! aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "How are you doing today?", "Isn't this a nice day?" };
		test.OtherPartStart = true;
		test.OtherPartSprite = playerSprite;
		test.OtherPartName = "Mango";
		StartConversation(test);
	}
	
	private void Awake() => RegisterInstance(this);
	
	#endregion

	#region Functions

	public void SkipButtonPressed() {
		if (isTalking) {
			skipDialogue = true;
		} else {
			if (playerIsTalking) {
				playerIsTalking = false;
				image.sprite = currentConversation.OtherPartSprite;
				nameBox.text = currentConversation.OtherPartName;
			} else {
				playerIsTalking = true;
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
		textBox.text = "";
		if (conversation.OtherPartStart) {
			playerIsTalking = false;
			image.sprite = conversation.OtherPartSprite;
			nameBox.text = conversation.OtherPartName;
			UpdateDialogue(conversation.Dialogue[currentDialogueIndex]);
		} else {
			playerIsTalking = true;
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
			print(skipDialogue);
			if (skipDialogue) {
				textBox.text = sentence;
				skipDialogue = false;
				isTalking = false;
				yield break;
			}
			print(sentence.Substring(0, i));
			textBox.text = sentence.Substring(0, i);
			yield return new WaitForSeconds(conversationSpeed);
		}
		skipDialogue = false;
		isTalking = false;
	}

	#endregion
}