using System;
using FlashlightGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BusSlider : MonoBehaviour {
	public enum BusType {
		UI,
		Sfx,
		Music,
		Master,
		Ambience
	}

	[SerializeField] private BusType busType;

	private Slider slider;

	private void Awake() {
		slider = GetComponentInChildren<Slider>();
	}

	private void Update() {
		switch (busType) {
			case BusType.Master:
				slider.value = Preferences.Mixer.MasterVolume;
				break;
			case BusType.UI:
				slider.value = Preferences.Mixer.UIVolume;
				break;
			case BusType.Sfx:
				slider.value = Preferences.Mixer.SfxVolume;
				break;
			case BusType.Music:
				slider.value = Preferences.Mixer.MusicVolume;
				break;
			case BusType.Ambience:
				slider.value = Preferences.Mixer.AmbienceVolume;
				break;
			default:
				Debug.LogError("[ERROR] [BusSlider] BusType not found");
				throw new ArgumentOutOfRangeException();
		}
	}

	public void OnSliderValueChanged() => PlayerPrefsHandler.UpdateBusValue(busType, slider.value);
	
}