using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParallaxLayer {
	//? Settings
	[Range(0, 1)] public float parallaxEffectX;
	[Range(0, 1)] public float parallaxEffectY;
	[Range(0, 1)] public float speedMultiplier;
	public               bool  infiniteVertical = false;

	//? Layer
	public GameObject layer;
	public Transform  transform;

	//? Refs
	[HideInInspector] public Vector3 startPos;
	[HideInInspector] public float   textureSizeX;
	[HideInInspector] public float   textureSizeY;
}


public class ParallaxController : MonoBehaviour {
	#region Fields

	private static DebugHandler Debug;

	[Header("Camera")]
	[SerializeField] GameObject playerCam;

	[Header("Layers")]
	[SerializeField] private ParallaxLayer[] layers;

	//? States
	private Vector3 prevCamPos;

	#endregion

	#region Unity Functions

	private void Awake() {
		Debug = new DebugHandler("ParallaxController");
	}

	private void Start() {
		if (!playerCam) {
			Debug.LogError("Camera not assigned to ParallaxController.");
			enabled = false;
			return;
		}

		prevCamPos = playerCam.transform.position;

		foreach (var layer in layers) {
			layer.startPos = layer.transform.position;

			var spriteRenderer = layer.transform.GetComponent<SpriteRenderer>();

			if (!spriteRenderer) {
				Debug.LogException(new Exception("What the fucka re you doing get help"));
			} else {
				layer.textureSizeX = spriteRenderer.bounds.size.x;
				layer.textureSizeY = spriteRenderer.bounds.size.y;
			}
		}
	}

	private void LateUpdate() {
		var delta = playerCam.transform.position - prevCamPos;

		foreach (var layer in layers) {
			var mov = new Vector3(
			                      delta.x * layer.speedMultiplier,
			                      delta.y * layer.speedMultiplier,
			                      0);

			layer.transform.position += mov;

			if (layer.textureSizeX > 0f) {
				var distX = playerCam.transform.position.x - layer.transform.position.x;
				if (Mathf.Abs(distX) >= layer.textureSizeX) {
					var offsetX = distX % layer.textureSizeX;
					layer.transform.position =
						new Vector3(
						            playerCam.transform.position.x + offsetX,
						            layer.transform.position.y,
						            layer.transform.position.z
						           );
				}
			}

			if (layer.infiniteVertical && layer.textureSizeY > 0f) {
				var distY = playerCam.transform.position.y - layer.transform.position.y;
				if (Mathf.Abs(distY) >= layer.textureSizeY) {
					var offsetY = distY % layer.textureSizeY;
					layer.transform.position =
						new Vector3(
						            layer.transform.position.x,
						            playerCam.transform.position.y + offsetY,
						            layer.transform.position.z
						           );
				}
			}

			prevCamPos = playerCam.transform.position;
		}
	}

	#endregion
}