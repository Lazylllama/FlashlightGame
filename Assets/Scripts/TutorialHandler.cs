using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour {
	#region Fields

	public static TutorialHandler Instance;

	[SerializeField] private List<GameObject> tutorialObjects;

	public bool isTutorialActive;
	public int  activeTutorialObjectIndex;

	#endregion

	#region Unity Functions

	private void Awake() {
		RegisterInstance(this);
	}

	#endregion

	#region Functions

	private void RegisterInstance(TutorialHandler instance) {
		if (Instance && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;
		}
	}

	public void ShowTutorial(int index) {
		if (isTutorialActive) HideTutorial();
		isTutorialActive          = true;
		activeTutorialObjectIndex = index;
		tutorialObjects[index].SetActive(true);
	}

	public void HideTutorial() {
		print("hide tutorial");
		if (!isTutorialActive) return;
		isTutorialActive = false;
		tutorialObjects[activeTutorialObjectIndex].SetActive(false);
	}

	#endregion
}