using System;
using UnityEngine;

public class MirrorController : MonoBehaviour
{
	#region Fields

	[Header("Positions")]
	[SerializeField] private Transform positionA, positionB;

	private Vector3 posA, posB;

	[Header("Settings")]
	[SerializeField] private string id;
	
	//? States
	private bool isInPosition;
	private bool posAIsTarget;
	private bool isInitialized;
	#endregion
	#region Unity Functions
    private void Start()
    {
        posA = new Vector3(positionA.position.x, positionA.position.y, transform.position.z);
        posB = new Vector3(positionB.position.x, positionB.position.y, transform.position.z);
        if (posA == posB) {
	        Debug.Log("Mirror positions need to be moved!");
        }
    }
    
    private void Update() {
	    if(!isInitialized) Initialize();
	    if (isInPosition) return;
	    UpdatePositon();
    }
    
    #endregion
    #region private Functions
    
    private void Initialize() {
	    if (!GameController.Instance) return;
	    GameController.Instance.leverEvent.AddListener(ChangeTarget);
	    isInitialized = true;
    }

    private void ChangeTarget(string eventId) {
	    print("received");
	    if(!string.Equals(eventId, id, StringComparison.CurrentCultureIgnoreCase)) return;
	    print("passed");
	    posAIsTarget = !posAIsTarget;
    }

    private void UpdatePositon() {
		LeanTween.move(gameObject, (posAIsTarget? posA : posB), 1).setEase(LeanTweenType.easeInOutQuint);
    }
    #endregion
    
}
