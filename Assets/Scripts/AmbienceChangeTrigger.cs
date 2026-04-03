using UnityEngine;

public class AmbienceChangeTrigger : MonoBehaviour {
	[SerializeField]               private string parameterName;
	[SerializeField] [Range(0, 1)] private float  parameterValue;

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			AudioManager.Instance.SetAmbienceParameter(parameterName, parameterValue);
		}
	}
}