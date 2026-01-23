using System.Collections;
using TMPro;
using UnityEngine;

public class BossController : MonoBehaviour {
	#region Fields

	[Header("Boss Settings")]
	[SerializeField] private float health;
	[SerializeField] private float weakpointChangeInterval;
	[SerializeField] private float weakpointClosedFor; //? The amount of time all weakpoints are closed between changes.
	
	[Header("Text")]
	[SerializeField] private TMP_Text healthText;
	
	[Header("WeakPoints")]
	[SerializeField] private GameObject[] weakPoints;
	
	//* States
	public float timeSinceLastChange;
	#endregion
	#region unity functions

	private void Start() {
		timeSinceLastChange = weakpointChangeInterval;
		CloseWeakPoints();
	}
	
	private void Update() {
		timeSinceLastChange += Time.deltaTime;
		if (timeSinceLastChange >= weakpointChangeInterval) CloseWeakPoints();
	}
	
	#endregion
	#region Functions

	private void CloseWeakPoints() {
		timeSinceLastChange = 0;
		foreach (var t in weakPoints) {
			t.tag                       = "Untagged";
			t.GetComponent<SpriteRenderer>().color = Color.red;
		}
		StartCoroutine(OpenWeakpointAfter(weakpointClosedFor));
	}

	private void OpenWeakPoint() {
		GameObject openWeakPoint = weakPoints[Random.Range(0, weakPoints.Length)];
		openWeakPoint.tag                                  = "WeakPoint";
		openWeakPoint.GetComponent<SpriteRenderer>().color = Color.green;
	}

	public void Hit(float damage) {
		health -= damage;
		UpdateText();
	}
	
	private void UpdateText() {
		healthText.text = health.ToString();
	}
	#endregion
	#region Enumerators

	private IEnumerator OpenWeakpointAfter(float time) {
		yield return new WaitForSeconds(time);
		OpenWeakPoint();
	}
	
	#endregion
}
