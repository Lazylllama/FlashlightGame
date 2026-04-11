using System;
using System.Collections;
using SteamTools;
using UnityEngine;

public class WallManager : MonoBehaviour
{
	[Header("ID")]
	[SerializeField] private string id;
	
	private bool isInitialized = false;
	
	//? Refs
	private ParticleSystem ps;

	private void Awake() {
		ps = GetComponentInChildren<ParticleSystem>();
	}

	private void Update() {
		if (!isInitialized) Init();
	}

	private void Init() {
		GameController.Instance.wallTriggerEvent.AddListener(OpenWall);
		isInitialized = true;
	}

	private void OpenWall(string _id) {
		if (id != _id) return;
		ps.Play();
		StartCoroutine(Delay());
	}

	private IEnumerator Delay() {
		yield return new WaitForSeconds(2f);
		gameObject.SetActive(false);
	}
}
