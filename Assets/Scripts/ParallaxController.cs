using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParallaxLayer {
	//? Settings
	[Range(0, 1)] public float speedMultiplier;
	public               bool  infiniteVertical   = false;
	public               bool  infiniteHorizontal = true;

	//? Layer
	public Transform transform;

	//? Refs
	[HideInInspector] public List<Transform> tiles = new List<Transform>();
	[HideInInspector] public float           textureSizeX;
	[HideInInspector] public float           textureSizeY;
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
			layer.tiles.Clear();

			foreach (Transform child in layer.transform) {
				layer.tiles.Add(child);
			}

			if (layer.tiles.Count <= 0) continue;

			var spriteRenderer = layer.tiles[0].GetComponent<SpriteRenderer>();

			if (spriteRenderer == null) {
				Debug.LogWarning($"Tile '{layer.tiles[0].name}' in layer '{layer.transform.name}' is missing a SpriteRenderer.");
				return;
			}

			layer.textureSizeX = spriteRenderer.bounds.size.x;
			layer.textureSizeY = spriteRenderer.bounds.size.y;
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

			if (layer.infiniteHorizontal && layer.textureSizeX > 0f) {
				foreach (var tile in layer.tiles) {
					var distX = tile.position.x - playerCam.transform.position.x;

					//? If the tile is too far to the left, move it to the right end of the layer, and vice versa
					// Should work in theory i hope (plz work)
					if (Mathf.Abs(distX) >= layer.textureSizeX) {
						tile.position += new Vector3(-layer.textureSizeX * layer.tiles.Count, 0, 0);
					} else if (Mathf.Abs(distX) <= -layer.textureSizeX) {
						tile.position += new Vector3(layer.textureSizeX * layer.tiles.Count, 0, 0);
					}
				}
			}

			if (layer.infiniteVertical && layer.textureSizeY > 0f) {
				foreach (var tile in layer.tiles) {
					var distY = tile.position.y - playerCam.transform.position.y;

					//? If the tile is too far down, move it to the top end of the layer, and vice versa
					if (Mathf.Abs(distY) >= layer.textureSizeY) {
						tile.position += new Vector3(0, -layer.textureSizeY * layer.tiles.Count, 0);
					} else if (Mathf.Abs(distY) <= -layer.textureSizeY) {
						tile.position += new Vector3(0, layer.textureSizeY * layer.tiles.Count, 0);
					}
				}
			}

			prevCamPos = playerCam.transform.position;
		}
	}

	#endregion
}