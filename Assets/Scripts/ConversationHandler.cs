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
	private string[] currentDialogue;
	public  bool     playerCanWalk = true;
	private int      currentDialogueIndex;
	private bool     isTalking;
	private int      currentSentenceLength;
	
	#endregion

	#region Unity Functions

	private void Start() {
		var  test = new Conversation();
		test.Dialogue = new[] {
			"Hello there!", "How are you doing today?", "Isn't this a nice day?" };
		test.OtherPartStart = true;
		test.OtherPartSprite = playerSprite;
		test.OtherPartName = "Mango";
		StartConversation(test);
	}

	#endregion

	#region Functions

	public void SkipButtonPressed() {
		if (isTalking) {
			currentDialogueIndex = currentSentenceLength;
		} else {
			currentDialogueIndex++;
			StartCoroutine(TypeSentence(currentDialogue[currentDialogueIndex]));
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
		currentDialogue = conversation.Dialogue;
		conversationUI.SetActive(true);
		textBox.text = "";
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
			currentSentenceLength = sentence.Length;
			StartCoroutine(TypeSentence(sentence));
		}
	}

	IEnumerator TypeSentence(string sentence) {
		print("mango");
		isTalking = true;
		for (int i = 1; i < sentence.Length; i++) {
			print(sentence.Substring(0, i));
			textBox.text = sentence.Substring(0, i);
			yield return new WaitForSeconds(conversationSpeed);
		}
		isTalking = false;
	}

	#endregion
}