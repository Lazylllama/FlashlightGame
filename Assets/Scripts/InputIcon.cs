using System;
using FlashlightGame;
using UnityEngine;

//? Needs an sprite to replace (duh)
[RequireComponent(typeof(SpriteRenderer))]
public class InputIcon : MonoBehaviour {
	#region Fields

	[SerializeField] private InputHandler.InputActions inputAction;

	private SpriteRenderer spriteRenderer;

	private bool isInitialized;

	#endregion

	#region Unity Functions

	private void FixedUpdate() {
		if (!isInitialized) Initialize();
	}

	private void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void OnEnable() => Initialize();

	private void OnDisable() {
		if (!isInitialized || InputHandler.Instance == null) return;
		InputHandler.Instance.inputChange.RemoveListener(RefreshSprite);
	}

	#endregion

	#region Functions

	private void RefreshSprite(Lib.InputType inputType = default) {
		var sprite = InputHandler.Instance.GetSprite(inputAction);
		spriteRenderer.sprite  = sprite;
	}

	private void Initialize() {
		if (InputHandler.Instance == null) return;
		InputHandler.Instance.inputChange.AddListener(RefreshSprite);
		RefreshSprite();
		isInitialized = true;
	}

	#endregion
}