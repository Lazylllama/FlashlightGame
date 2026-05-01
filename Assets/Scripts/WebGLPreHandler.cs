using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WebGLPreHandler : MonoBehaviour {
	[FMODUnity.BankRef] public List<string> banks;
	
	[SerializeField] private Button loadButton;

	public void LoadBanks() {
		loadButton.interactable = false; 
		foreach (var b in banks) {
			FMODUnity.RuntimeManager.LoadBank(b, true);
			Debug.Log("[DEBUG] [WebGLPreHandler] Loaded bank " + b);
		}

		/*
		    For Chrome / Safari browsers / WebGL.  Reset audio on response to user interaction (LoadBanks is called from a button press), to allow audio to be heard.
		*/
		FMODUnity.RuntimeManager.CoreSystem.mixerSuspend();
		FMODUnity.RuntimeManager.CoreSystem.mixerResume();

		StartCoroutine(CheckBanksLoaded());
	}

	private static IEnumerator CheckBanksLoaded() {
		while (!FMODUnity.RuntimeManager.HaveAllBanksLoaded) {
			yield return null;
		}

		SceneManager.LoadScene("Main", LoadSceneMode.Single);
	}
}