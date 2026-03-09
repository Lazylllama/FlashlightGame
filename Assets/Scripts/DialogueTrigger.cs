using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class MyArray1
{
	public string[] dialogue;
	public Sprite   sprite;
	public string   names;
}


public class DialogueTrigger: MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    
    public int npcindex = 0;
    public int dialogueindex = 0;

    public TextMeshProUGUI nPCnameText;

    public MyArray1[] array1;


    public float wordSpeed;
    public bool playerIsClose;
    public Image sImage;



    InputAction nextLineAction;

    void Start()
    {
        dialogueText.text = "";
        nextLineAction    = InputSystem.actions.FindAction("NextLine");
    }
    
    void Update()
    {

        
        sImage.sprite = array1[npcindex].sprite;
        nPCnameText.text = array1[npcindex].names;
        if (nextLineAction.WasPerformedThisFrame() && playerIsClose)
        {
            if (!dialoguePanel.activeInHierarchy)
            {
                
                dialoguePanel.SetActive(true);
                StartCoroutine(Typing());
            }
            else if (dialogueText.text == array1[npcindex].dialogue[dialogueindex])
            {
                NextLine();
                
            }
            
        }
        
        
    }


    private void RemoveText()
    {
	    dialogueText.text = "";
	    dialogueindex     = 0;
	    npcindex          = 0;
	    dialoguePanel.SetActive(false);
    }

    IEnumerator Typing()
    {
	    foreach (char letter in array1[npcindex].dialogue[dialogueindex])
	    {
		    dialogueText.text += letter;
		    yield return new WaitForSeconds(wordSpeed);
	    }
    }

    private void NextLine()
    {
	    if (dialogueindex < array1[npcindex].dialogue.Length - 1)
	    {
		    dialogueindex++;
		    dialogueText.text = "";
		    StartCoroutine(Typing());
	    }
	    else
	    {
		    dialogueindex = 0;

		    if (npcindex < array1.Length - 1)
		    {
			    npcindex++;
			    dialogueText.text = "";
			    StartCoroutine(Typing());
		    }
		    else
		    {
			    RemoveText();
		    }
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

