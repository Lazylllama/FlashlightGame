using UnityEngine;

public class RespawnManager : MonoBehaviour {
	public static RespawnManager Instance { get; private set; }

	public Vector3 respawnPoint;

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void SetRespawnPoint(Vector3 position) {
		respawnPoint = position;
		Debug.Log($"respawn point set to {position}");
	}

	public void Respawn(GameObject player) {
		player.transform.position = respawnPoint;
		Debug.Log("player respawned");
	}


}