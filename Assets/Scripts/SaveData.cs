using UnityEngine;

[System.Serializable]
public class SaveData {
	public int version;
	
	public int     health;
	public int     battery;
	public bool    isLookingRight;
	public Vector3 checkpointPosition;
	public long    lastSavedTicks;
}
