using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossController : MonoBehaviour {
	#region Fields

	[Header("Boss Settings")]
	[SerializeField] private float health;
	[SerializeField] private float weakpointChangeInterval;
	
	[Header("Text")]
	[SerializeField] private TMP_Text healthText;
	
	[Header("WeakPoints")]
	[SerializeField] private GameObject[] weakPoints;
	
	//* Refs
	private LayerMask groundLayer;
	
	//* States
	private float timeSinceLastChange;
	
	#endregion
	#region Unity Functions

	private void Start() {
		groundLayer = LayerMask.GetMask("Ground");
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
		OpenWeakPoint();
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
	
	#region Attack Functions
	
	
	
	#endregion
	
	#region Coroutines

	private IEnumerator OpenWeakpointAfter(float time) {
		yield return new WaitForSeconds(time);
		OpenWeakPoint();
	}
	
	#endregion
}
