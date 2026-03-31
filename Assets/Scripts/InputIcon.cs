using System;
using FlashlightGame;
using UnityEngine;
using UnityEngine.UI;

public class InputIcon : MonoBehaviour {
	#region Fields

	[SerializeField] private InputHandler.InputActions inputAction;
	[SerializeField] private bool                      getInputIcon;

	private SpriteRenderer spriteRenderer;
	private Image          uiImage;

	private bool isInitialized;
	private bool IsUiImage => uiImage != null;

	#endregion

	#region Unity Functions

	private void FixedUpdate() {
		if (!isInitialized) Initialize();
	}

	private void Awake() {
		uiImage 	  = GetComponent<Image>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		
		if (!spriteRenderer && uiImage == null) {
			Debug.LogError("[ERROR] [InputIcon] InputIcon requires either a SpriteRenderer or an Image component.");
			enabled = false;
		} else if (spriteRenderer && uiImage != null) {
			Debug.LogError("[ERROR] [InputIcon] InputIcon should not have both SpriteRenderer and Image components. Please remove one.");
			enabled = false;
		}
	}

	private void OnEnable() => Initialize();

	private void OnDisable() {
		if (!isInitialized || !InputHandler.Instance) return;
		InputHandler.Instance.inputChange.RemoveListener(RefreshSprite);
	}

	#endregion

	#region Functions

	private void RefreshSprite(Lib.InputType inputType = default) {
		var sprite = getInputIcon ? InputHandler.Instance.GetInputLogoSprite() : InputHandler.Instance.GetSprite(inputAction);

		if (IsUiImage) uiImage.sprite = sprite;
		else spriteRenderer.sprite    = sprite;
	}

	private void Initialize() {
		if (!InputHandler.Instance) return;
		InputHandler.Instance.inputChange.AddListener(RefreshSprite);
		RefreshSprite();
		isInitialized = true;
	}

	#endregion
}