using UnityEngine;

public class checkpoint : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			PlayerData.Instance.Relieved = true;
			Debug.Log("PlayerData.Relieved");
		}
	}


	private void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			PlayerData.Instance.Relieved = false;
			Debug.Log("PlayerData.NotRelieved");
		}
	}
}