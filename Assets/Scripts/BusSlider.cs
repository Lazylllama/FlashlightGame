using System;
using FlashlightGame;
using UnityEngine;
using UnityEngine.UI;

public class BusSlider : MonoBehaviour {
	public enum BusType {
		UIVolume,
		SfxVolume,
		MusicVolume,
		MasterVolume,
		AmbienceVolume
	}

	[SerializeField] private BusType busType;

	private Slider slider;

	private void Awake() {
		slider = GetComponentInChildren<Slider>();
	}

	private void Update() {
		switch (busType) {
			case BusType.MasterVolume:
				slider.value = Preferences.Mixer.MasterVolume;
				break;
			case BusType.UIVolume:
				slider.value = Preferences.Mixer.UIVolume;
				break;
			case BusType.SfxVolume:
				slider.value = Preferences.Mixer.SfxVolume;
				break;
			case BusType.MusicVolume:
				slider.value = Preferences.Mixer.MusicVolume;
				break;
			case BusType.AmbienceVolume:
				slider.value = Preferences.Mixer.AmbienceVolume;
				break;
			default:
				Debug.LogError("[ERROR] [BusSlider] BusType not found");
				throw new ArgumentOutOfRangeException();
		}
	}

	public void OnSliderValueChanged() => PlayerPrefsHandler.UpdateBusValue(busType, slider.value);
}