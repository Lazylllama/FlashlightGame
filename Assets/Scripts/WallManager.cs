using System;
using SteamTools;
using UnityEngine;

public class WallManager : MonoBehaviour
{
	[Header("ID")]
	[SerializeField] private string id;
	
	private bool isInitialized = false;

	private void Update() {
		if (!isInitialized) Init();
	}

	private void Init() {
		GameController.Instance.wallTriggerEvent.AddListener(OpenWall);
		isInitialized = true;
	}

	private void OpenWall(string _id) {
		if (id != _id) return;
		gameObject.SetActive(false);	
	}
}
