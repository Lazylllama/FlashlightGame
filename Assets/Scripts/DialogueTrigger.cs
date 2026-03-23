using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class DialogueArray
{
	public string[]   dialogue;
	public Sprite     sprite;
	public string     names;
	public GameObject characterObj;
	public Vector2    cMove;
	public float      cMoveSpeed;

}

//THE MOVE PART IS UNDER BETA TESTING GUYS NOT RELIABLE 

public class DialogueTrigger: MonoBehaviour
{
	#region Fields
	
	public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    
    public int npcIndex = 0;
    public int dialogueIndex = 0;

    public TextMeshProUGUI nPCnameText;

    public float wordSpeed;
    public bool  playerIsClose;
    public Image sImage;
    public bool  noMovement;
    
    public DialogueArray[] dArray;

    InputAction nextLineAction;

    
    
    #endregion

    #region Unity Functions
    
    void Start()
    {
        dialogueText.text = "";
        nextLineAction    = InputSystem.actions.FindAction("Interact");
    }
    
    void Update() {

	    InitiateDialogue();
	    UpdateUI();
	    MoveToTarget(npcIndex);
	    CheckMovementLock();
    }
    #endregion
	
    #region Functions

    void InitiateDialogue() {
	    if (nextLineAction.WasPerformedThisFrame() && playerIsClose)
	    {
		    if (!dialoguePanel.activeInHierarchy)
		    {
		        
			    dialoguePanel.SetActive(true);
			    StartCoroutine(Typing());
		    }
		    else if (dialogueText.text == dArray[npcIndex].dialogue[dialogueIndex])
		    {
			    NextLine();
		    }
	    }
    }
    
    

    private void ResetDialogue()
    {
	    dialogueText.text = "";
	    dialogueIndex     = 0;
	    npcIndex          = 0;
	    dialoguePanel.SetActive(false);
    }

    private void UpdateUI() {
	    sImage.sprite    = dArray[npcIndex].sprite;
	    nPCnameText.text = dArray[npcIndex].names;
    }
    
    void MoveToTarget(int index)
    {
	    Vector3 cur = dArray[index].characterObj.transform.position;

	    Vector3 target = new Vector3(
	                                 dArray[index].cMove.x,
	                                 dArray[index].cMove.y,
	                                 cur.z
	                                );

	    dArray[index].characterObj.transform.position =
		    Vector3.MoveTowards(
		                        cur,
		                        target,
		                        dArray[index].cMoveSpeed * Time.deltaTime   // constant speed
		                       );
    }


    private void NextLine()
    {
	    if (dialogueIndex < dArray[npcIndex].dialogue.Length - 1)
	    {
		    dialogueIndex++;
		    dialogueText.text = "";
		    StartCoroutine(Typing());
	    }
	    else
	    {
		    dialogueIndex = 0;

		    if (npcIndex < dArray.Length - 1)
		    {
			    npcIndex++;
			    dialogueText.text = "";
			    StartCoroutine(Typing());
		    }
		    else
		    {
			    ResetDialogue();
		    }
	    }
    }

    void CheckMovementLock() {
	    if (noMovement && dialoguePanel.activeInHierarchy)
	    {
		    PlayerMovement.Instance.enabled = false;
	    } else {
		    PlayerMovement.Instance.enabled = true;
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
            ResetDialogue();
        }
    }
    #endregion

    #region Coroutines

    IEnumerator Typing()
    {
	    foreach (char letter in dArray[npcIndex].dialogue[dialogueIndex])
	    {
		    dialogueText.text += letter;
		    yield return new WaitForSeconds(wordSpeed);
	    }
    }

    #endregion
}

