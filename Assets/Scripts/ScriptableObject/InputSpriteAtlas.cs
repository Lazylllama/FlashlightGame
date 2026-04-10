using System.Collections.Generic;
using FlashlightGame;
using UnityEngine;

[CreateAssetMenu(fileName = "InputSpriteAtlas", menuName = "Input/Input Sprite Atlas")]
public class InputSpriteAtlas : ScriptableObject {
	#region Fields

	[System.Serializable]
	public struct InputSprite {
		public InputHandler.InputActions inputName;
		public Sprite                    sprite;
	}

	public Lib.InputType inputType;

	private Dictionary<InputHandler.InputActions, Sprite> inputSpriteDict;

	[SerializeField] private Sprite            inputIcon;
	[SerializeField] private List<InputSprite> inputSprites;

	#endregion

	#region Functions

	public void MapSprites() {
		inputSpriteDict = new Dictionary<InputHandler.InputActions, Sprite>(inputSprites.Count);
		foreach (var inputSprite in inputSprites) {
			#if UNITY_EDITOR
			Debug.Log($"[DEBUG] [InputSpriteAtlas] Mapping {inputSprite.inputName} to {inputSprite.sprite.name}");
			#endif
			if (string.IsNullOrEmpty(inputSprite.inputName.ToString()) || inputSprite.sprite == null) return;
			inputSpriteDict.Add(inputSprite.inputName, inputSprite.sprite);
		}
	}

	public Sprite GetInputSprite(InputHandler.InputActions inputAction) {
		var sprite = inputSpriteDict.GetValueOrDefault(inputAction, null);
		if (inputSpriteDict == null || !sprite) MapSprites();
		return sprite;
	}

	public string GetInputName(InputHandler.InputActions inputAction) => inputAction.ToString();

	public Sprite GetInputLogoSprite() => inputIcon ? inputIcon : null;

	#endregion
}