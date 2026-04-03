using System;
using System.Collections.Generic;
using FlashlightGame;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;


public class AudioManager : MonoBehaviour {
	#region Fields

	public static  AudioManager Instance;
	private static DebugHandler debug;

	private Bus masterBus, musicBus, ambienceBus, sfxBus, uiBus;

	public enum FootstepSurface {
		Concrete,
		Dirt,
		Grass,
		Sand,
		Wood
	}

	public enum MusicTrack {
		MainMenu,
		Game
	}

	private readonly List<EventInstance> eventInstances = new();

	private EventInstance ambienceEventInstance, musicEventInstance;

	//* States *//
	private Coroutine footstepCoroutine;

	#endregion

	private void OnDestroy() => CleanUp();

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;

		masterBus   = RuntimeManager.GetBus("bus:/");
		musicBus    = RuntimeManager.GetBus("bus:/Music");
		ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
		sfxBus      = RuntimeManager.GetBus("bus:/SFX");
		uiBus       = RuntimeManager.GetBus("bus:/UI");
	}

	private void Start() {
		InitializeMusic(FMODEvents.Instance.gameMusic);

		SetAmbienceParameter("wind_intensity", 0.2f);
	}

	private void Update() {
		masterBus.setVolume(Preferences.Mixer.MasterVolume);
		musicBus.setVolume(Preferences.Mixer.MusicVolume);
		ambienceBus.setVolume(Preferences.Mixer.AmbienceVolume);
		sfxBus.setVolume(Preferences.Mixer.SfxVolume);
		uiBus.setVolume(Preferences.Mixer.UIVolume);
	}

	#region Functions

	/// <summary>
	/// Handles all Audio initialization that needs to be done when the game starts. Called from UIController on game init.
	/// </summary>
	public void GameStarted() {
		SetMusicTrack(AudioManager.MusicTrack.Game);

		InitializeAmbience(FMODEvents.Instance.crowsAmbience);
		InitializeAmbience(FMODEvents.Instance.forestWindAmbience);
	}

	/// <summary>
	/// Play a sound once from a position
	/// </summary>
	/// <param name="sound"></param>
	/// <param name="worldPosition"></param>
	public void PlayOneShot(EventReference sound, Vector3 worldPosition) {
		RuntimeManager.PlayOneShot(sound, worldPosition);
	}

	/// <summary>
	/// Change a parameter in the ambience event instance
	/// </summary>
	/// <param name="paramName">FMOD Parameter</param>
	/// <param name="value"></param>
	public void SetAmbienceParameter(string paramName, float value) {
		ambienceEventInstance.setParameterByName(paramName, value);
	}

	/// <summary>
	/// Change the MusicTrack parameter in the music event instance.
	/// </summary>
	/// <param name="track">AudioManager.MusicTrack</param>
	public void SetMusicTrack(MusicTrack track) {
		musicEventInstance.setParameterByName("MusicTrack", (int)track);
	}

	/// <summary>
	/// Set bus volume in FMOD and preferences.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public void SetBusVolume(BusSlider.BusType type, float value) {
		switch (type) {
			case BusSlider.BusType.UI:
				uiBus.setVolume(value);
				Preferences.Mixer.UIVolume = value;
				break;
			case BusSlider.BusType.Sfx:
				sfxBus.setVolume(value);
				Preferences.Mixer.SfxVolume = value;
				break;
			case BusSlider.BusType.Music:
				musicBus.setVolume(value);
				Preferences.Mixer.MusicVolume = value;
				break;
			case BusSlider.BusType.Master:
				ambienceBus.setVolume(value);
				Preferences.Mixer.MasterVolume = value;
				break;
			case BusSlider.BusType.Ambience:
				ambienceBus.setVolume(value);
				Preferences.Mixer.AmbienceVolume = value;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}
	}

	private void InitializeAmbience(EventReference ambienceEvent) {
		ambienceEventInstance = CreateInstance(ambienceEvent);
		ambienceEventInstance.start();
	}

	private void InitializeMusic(EventReference musicEvent) {
		musicEventInstance = CreateInstance(musicEvent);
		musicEventInstance.start();
	}

	private EventInstance CreateInstance(EventReference eventReference) {
		var eventInstance = RuntimeManager.CreateInstance(eventReference);
		eventInstances.Add(eventInstance);
		return eventInstance;
	}

	private void CleanUp() {
		foreach (var instance in eventInstances) {
			instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			instance.release();
		}
	}

	#endregion
}