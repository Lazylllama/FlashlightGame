using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class WallTrigger : SerializedMonoBehaviour
{
	[Header("ID")]
	[SerializeField] private string id;
	
	[Header("CameraBoundChanges")]
	[OdinSerialize] private CameraBoundsEventParameters cameraBoundColliders;

	private bool wallOpened = false;
	
	public void OpenWall() {
		if (wallOpened) return;
		wallOpened = true;
		GameController.Instance.wallTriggerEvent.Invoke(id);
		GameController.Instance.changeCameraBounds.Invoke(cameraBoundColliders);
	}
}
