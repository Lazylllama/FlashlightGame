using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	#region Fields

	public static AudioManager Instance;

	[SerializeField] private float stepInterval;

	//? Clip list public to be able to get the length of a clip in other scripts
	[SerializeField] public  List<AudioClip> audioClipList;
	[SerializeField] public  List<AudioClip> stepsClipList;
	[SerializeField] private AudioSource     sfxSource;

	private          Dictionary<FootstepSurface, List<AudioClip>> stepsClips = new();
	private readonly Dictionary<AudioName, AudioClip>             audioClips = new();

	//? Map index to names
	public enum AudioName {
		CoinCollect, //0
	};

	public enum FootstepSurface {
		Concrete,
		Dirt,
		Grass,
		Sand,
		Wood
	}

	//* States *//
	private Coroutine footstepCoroutine;

	#endregion

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;

		InitializeDictionary();
	}

	private void Start() {
		stepsClips = new Dictionary<FootstepSurface, List<AudioClip>> {
			{ FootstepSurface.Concrete, stepsClipList.GetRange(0, 8) },
			{ FootstepSurface.Dirt, stepsClipList.GetRange(7,     5) },
			{ FootstepSurface.Grass, stepsClipList.GetRange(12,   5) },
			{ FootstepSurface.Sand, stepsClipList.GetRange(17,    8) },
			{ FootstepSurface.Wood, stepsClipList.GetRange(25,    5) }
		};
	}

	private void InitializeDictionary() {
		for (var i = 0; i < audioClipList.Count; i++) {
			if (i < System.Enum.GetValues(typeof(AudioName)).Length) {
				audioClips[(AudioName)i] = audioClipList[i];
			}
		}
	}

	/// <summary>
	/// Plays a sound effect
	/// </summary>
	/// <param name="audioName">Audio Clip Name - AudioManager.AudioName[]</param>
	/// <param name="volume">Sound Volume - 0 to 1 (float)</param>
	public void PlaySfx(AudioName audioName, float volume = 1f) {
		if (audioClips.TryGetValue(audioName, out var clip)) {
			sfxSource.PlayOneShot(clip, volume);
		} else {
			Debug.LogWarning($"AudioClip {audioName} not found in AudioManager dictionary!");
		}
	}

	public void PlayFootstepSfx(FootstepSurface surface, float volume = 1f) {
		if (footstepCoroutine != null) return;
		footstepCoroutine = StartCoroutine(PlayFootstepSfxCoroutine(surface, volume));
	}

	#region Coroutines

	private IEnumerator PlayFootstepSfxCoroutine(FootstepSurface surface, float volume = 1f) {
		if (stepsClips.TryGetValue(surface, out var clips) && clips.Count > 0) {
			var clip = clips[UnityEngine.Random.Range(0, clips.Count)];
			sfxSource.PlayOneShot(clip, volume);
		} else {
			Debug.LogWarning($"No footstep clips found for surface: {surface}");
		}

		yield return new WaitForSeconds(stepInterval);

		footstepCoroutine = null;
	}

	#endregion
}