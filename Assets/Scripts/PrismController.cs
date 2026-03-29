using System;
using System.Collections.Generic;
using UnityEngine;

public class PrismController : MonoBehaviour
{
	#region Fields
	// Settings
	[SerializeField] private float health;
	[SerializeField] private int   rayAmount;
	[SerializeField] private float range;
	[SerializeField] private float damageMultiplier;

	private LayerMask prismLayerMask;
	
	// States
	private Dictionary<Collider2D, int> hitList = new Dictionary<Collider2D, int>();
	#endregion
	
	#region Unity Functions

	private void Start() {
		prismLayerMask = ~LayerMask.GetMask("Player","Prism");
	}

	#endregion
	
	#region Functions
	public void Hit(float damage) {
		health -= damage;
		if (health <= 0) {
			Explode();
		}
	}

	private void Explode() {
		for (int i = 0; i < rayAmount; i++) {
			float angle = (360f / rayAmount) * i;
			Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
			DrawNewRay(transform.position,direction);
		}
		if (hitList.Count > 0) RegisterHitList();
		Destroy(gameObject);
	}
	
	private void DrawNewRay(Vector2 start, Vector2 direction) {
		
		var hit = Physics2D.Raycast(start, direction, range, prismLayerMask);
		
		if (hit.collider) Debug.DrawLine(start,hit.point, Color.red, 100);
		if (!hit.collider) Debug.DrawRay(start, direction * range, Color.red, 100);

		
		print("test1");
		if (!hit || hit.collider.tag is not ("Enemy" or "WeakPoint")) return;
		print("test2");
		if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("WeakPoint")) {
			print("test3");
			if (hitList.TryAdd(hit.collider, 1)) return; 
			hitList[hit.collider]++;
		}
	}
	

	private void RegisterHitList() {
		var removeList = new List<Collider2D>();

		foreach (var hit in hitList) {
			switch (hit.Key.gameObject.tag) {
				case "Enemy":
					hit.Key.gameObject.GetComponent<EnemyController>().UpdateHealth(hit.Value / (float)rayAmount);
					break;
				case "WeakPoint":
					hit.Key.gameObject.GetComponentInParent<BossController>().Hit(hit.Value * damageMultiplier / (float)rayAmount);
					break; 
			}
			if (hit.Key.gameObject.tag is "Enemy" or "WeakPoint") removeList.Add(hit.Key);
		}
		foreach (var key in removeList) {
			hitList.Remove(key);
		}
		removeList.Clear();
	}

	#endregion
}
