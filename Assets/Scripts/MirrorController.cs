using UnityEngine;

public class MirrorController : MonoBehaviour
{
	#region Fields

	[Header("Positions")]
	[SerializeField] private Transform positionA, positionB;

	private Vector2 posA, posB;

	[Header("Settings")]
	[SerializeField] private float speed;
	
	//? States
	private bool isInPosition;
	private bool posAOnTop;
	private bool posAIsTarget;
	#endregion
	#region Unity Functions
    void Start()
    {
        posA = new Vector2(positionA.position.x, positionA.position.y);
        posB = new Vector2(positionB.position.x, positionB.position.y);
        if (posA == posB) {
	        Debug.Log("Mirror positions need to be moved!");
        } else if (posA.y > posB.y) {
	        posAOnTop = true;
        }
    }
    
    void Update() {
	    if (isInPosition) return;
	    UpdatePositon();
    }
    
    #endregion
    #region private Functions

    private void UpdatePositon() {
	    speed                *= (posAIsTarget ? 1f : -1f) * (posAOnTop ? 1f : -1f);
	    transform.position = 
    }
    #endregion
}
