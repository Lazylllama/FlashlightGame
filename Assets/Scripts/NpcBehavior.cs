using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcBehavior : MonoBehaviour
{
	public   GameObject      dialoguePanel;
	public   TextMeshProUGUI dialogueText;
	public   string[]        dialogue;
	[SerializeField] private int             index = 0;

	public float wordSpeed;
	public bool  playerIsClose;

	InputAction nextLineAction;

	void Start()
	{
		nextLineAction = InputSystem.actions.FindAction("NextLine");
		dialogueText.text = "";
	}
	
	void Update()
	{
		if (nextLineAction.WasPerformedThisFrame() && playerIsClose)
		{
			if (!dialoguePanel.activeInHierarchy)
			{
				dialoguePanel.SetActive(true);
				StartCoroutine(Typing());
			}
			else if (dialogueText.text == dialogue[index])
			{
				NextLine();
			}

		}
	}

	private void RemoveText()
	{
		dialogueText.text = "";
		index             = 0;
		dialoguePanel.SetActive(false);
	}

	IEnumerator Typing()
	{
		foreach(char letter in dialogue[index])
		{
			dialogueText.text += letter;
			yield return new WaitForSeconds(wordSpeed);
		}
	}

	private void NextLine()
	{
		if (index < dialogue.Length - 1)
		{
			index++;
			dialogueText.text = "";
			StartCoroutine(Typing());
		}
		else
		{
			RemoveText();
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			playerIsClose = true;
		}
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			playerIsClose = false;
			RemoveText();
		}
	}
}
