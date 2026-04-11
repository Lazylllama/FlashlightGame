using System;
using FlashlightGame;
using UnityEngine;
using UnityEngine.UI;

public class InputIcon : MonoBehaviour {
	#region Fields

	private static DebugHandler Debug;

	[SerializeField] private InputHandler.InputActions inputAction;
	[SerializeField] private bool                      getInputIcon, isInWorldSpace;
	[SerializeField] private float                     maxDist;

	private SpriteRenderer spriteRenderer;
	private Image          uiImage;
	private GameObject player;

	private bool isInitialized;
	private bool IsUiImage => uiImage != null;

	#endregion

	#region Unity Functions

	private void FixedUpdate() {
		if (!isInitialized) Initialize();
		if (isInWorldSpace) GetOpacity();
	}

	private void GetOpacity() {
		var dist = Vector2.Distance(player.transform.position, transform.position);

		if (dist < maxDist) spriteRenderer.color = new Color(1f, 1f, 1f, Math.Clamp(1f - (dist / maxDist), 0f, 1f));
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInit() {
		Debug = new DebugHandler("InputIcon");
	}

	private void Awake() {
		Debug ??= new DebugHandler("InputIcon");

		uiImage        = GetComponent<Image>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		player = GameObject.FindGameObjectWithTag("Player");
		
		if(Vector2.Distance(player.transform.position, transform.position) > maxDist && isInWorldSpace) spriteRenderer.color = new Color(1f, 1f, 1f, 0f);

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
		var sprite = getInputIcon
			             ? InputHandler.Instance.GetInputLogoSprite()
			             : InputHandler.Instance.GetSprite(inputAction);

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