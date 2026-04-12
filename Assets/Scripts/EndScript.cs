using System;
using UnityEngine;

public class EndScript : MonoBehaviour
{
	#region Unity Functions

	private void OnCollisionEnter2D(Collision2D other) {
		print("collision");
		if (!other.gameObject.CompareTag("Player")) return;
		
		PlayerData.Instance.PreventMovement = true;
		ConversationHandler.Instance.StartConversation("End");
	}

	#endregion
}
