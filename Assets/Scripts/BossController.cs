using System.Collections;
using TMPro;
using UnityEngine;


public class BossController : MonoBehaviour {
	#region Fields

	[Header("Boss Settings")]
	[SerializeField] private float health;
	[SerializeField] private float weakpointChangeInterval;


	[Header("Text")]
	[SerializeField] private TMP_Text healthText;

	[Header("WeakPoints")]
	[SerializeField] private GameObject[] weakPoints;

	//* Refs *//
	private LayerMask groundLayer;

	//* States *//
	private float timeSinceLastChange;

	#endregion

	#region unity functions

	private void Start() {
		groundLayer         = LayerMask.GetMask("Ground");
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
			t.tag = "Untagged";
			SpriteRenderer spriteRenderer = t.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null) {
				spriteRenderer.color = Color.red;
			} else {
				Debug.LogWarning($"WeakPoint GameObject '{t.name}' is missing a SpriteRenderer component.", t);
			}
		}

		OpenWeakPoint();
	}

	private void OpenWeakPoint() {
		GameObject openWeakPoint = weakPoints[Random.Range(0, weakPoints.Length)];
		openWeakPoint.tag = "WeakPoint";
		SpriteRenderer spriteRenderer = openWeakPoint.GetComponent<SpriteRenderer>();
		if (spriteRenderer != null) {
			spriteRenderer.color = Color.green;
		} else {
			Debug.LogWarning($"WeakPoint GameObject '{openWeakPoint.name}' is missing a SpriteRenderer component.",
			                 openWeakPoint);
		}
	}

	public void Hit(float damage) {
		health -= damage;
		UpdateText();
	}

	private void UpdateText() {
		healthText.text = health.ToString("F0");
	}

	#endregion

	#region AttackFunctions

	#endregion
}