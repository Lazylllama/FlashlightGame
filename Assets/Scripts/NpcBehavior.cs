using System.Collections;
using TMPro;
using UnityEngine;

public class NpcBehavior : MonoBehaviour
{
	public   GameObject      dialoguePanel;
	public   TextMeshProUGUI dialogueText;
	public   string[]        dialogue;
	[SerializeField] private int             index = 0;

	public float wordSpeed;
	public bool  playerIsClose;


	void Start()
	{
		dialogueText.text = "";
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.E) && playerIsClose)
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
		if (Input.GetKeyDown(KeyCode.Q) && dialoguePanel.activeInHierarchy)
		{
			RemoveText();
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
