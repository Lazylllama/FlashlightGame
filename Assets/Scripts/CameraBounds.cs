using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Cinemachine;
using UnityEngine;

public class CameraBounds : SerializedMonoBehaviour
{
	//? Instance
	public static CameraBounds Instance;
	
	[Header("CameraBoundGameObjects ")]
	[OdinSerialize] private Dictionary<string, GameObject> cameraBoundGameObjects;
	
	[Header("CameraBoundColliders")]
	[OdinSerialize] private Dictionary<string, Collider2D> cameraBoundColliders;
	
	//? States
	private bool       isInitialized;
	private GameObject currentColliderObject;

	private void Awake() {
		if (Instance == null) Instance = this;
	}

	private void Update() {
		if(!isInitialized) Init();
	}

	private void Init() {
		GameController.Instance.changeCameraBounds.AddListener(ChangeCameraBounds);
	}

	private void ChangeCameraBounds(CameraBoundsEventParameters parameters) {
		var currentCamera = CameraController.Instance.currentCamera;
		var currentConfiner = currentCamera.GetComponent<CinemachineConfiner2D>();
		if(parameters.ChangeColliderObject) {
			currentConfiner.BoundingShape2D = cameraBoundGameObjects[parameters.NewColliderObject].GetComponent<CompositeCollider2D>();
		}

		foreach (var collider in parameters.CollidersToTurnOff) {
			cameraBoundColliders[collider].enabled = false;
		}

		foreach (var collider in parameters.CollidersToTurnOn) {
			cameraBoundColliders[collider].enabled = true;
		}
		currentConfiner.InvalidateBoundingShapeCache();
	}
}

public class CameraBoundsEventParameters {
	public string[] CollidersToTurnOff;
	public string[] CollidersToTurnOn;
	public string   NewColliderObject;
	public bool     ChangeColliderObject;
}
