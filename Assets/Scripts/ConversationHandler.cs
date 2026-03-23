using UnityEngine;

public class ConversationHandler : MonoBehaviour
{
	#region Fields
	public static  ConversationHandler Instance;
	#endregion
	#region Unity Functions
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
        
	}

	// Update is called once per frame
	void Update()
	{
        
	}
	#endregion
	
	#region Functions 
	private void RegisterInstance(PlayerMovement instance) {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
		} else {
			Instance = this;
		}
	}
	#endregion
	
	
   
}
