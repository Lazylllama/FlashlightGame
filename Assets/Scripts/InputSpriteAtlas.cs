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

	[SerializeField] private Sprite inputIcon;
	[SerializeField] private List<InputSprite> inputSprites;

	#endregion

	#region Functions

	public void MapSprites() {
		inputSpriteDict = new Dictionary<InputHandler.InputActions, Sprite>(inputSprites.Count);
		foreach (var inputSprite in inputSprites) {
			Debug.Log($"Mapping {inputSprite.inputName} to {inputSprite.sprite}");
			if (string.IsNullOrEmpty(inputSprite.inputName.ToString()) || inputSprite.sprite == null) return;
			inputSpriteDict.Add(inputSprite.inputName, inputSprite.sprite);
		}
	}

	public Sprite GetInputSprite(InputHandler.InputActions inputAction) {
		var sprite = inputSpriteDict.GetValueOrDefault(inputAction, null);
		if (inputSpriteDict == null || !sprite) MapSprites();
		return sprite;
	}

	public Sprite GetInputLogoSprite() => inputIcon ? inputIcon : null;

	#endregion
}