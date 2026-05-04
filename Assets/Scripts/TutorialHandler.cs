using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialObject {
	FlashlightPickup,
	Laser,
	Mirror,
	Crank,
	Mantle
}

public class TutorialHandler : MonoBehaviour {
	#region Fields

	public static TutorialHandler Instance;

	private Dictionary<TutorialObject, GameObject> tutorialObjects;

	[SerializeField] private List<GameObject> rawObjects;

	public bool           isTutorialActive;
	public TutorialObject activeTutorialObjectIndex;

	#endregion

	#region Unity Functions

	private void Awake() {
		RegisterInstance(this);

		tutorialObjects = new Dictionary<TutorialObject, GameObject>() {
			{ TutorialObject.FlashlightPickup, rawObjects[0] },
			{ TutorialObject.Laser, rawObjects[1] },
			{ TutorialObject.Mirror, rawObjects[2] },
			{ TutorialObject.Crank, rawObjects[3] },
			{ TutorialObject.Mantle, rawObjects[4] }
		};
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

	public void ShowTutorial(TutorialObject key) {
		if (isTutorialActive) HideTutorial();
		isTutorialActive          = true;
		activeTutorialObjectIndex = key;
		tutorialObjects.TryGetValue(key, out var obj);
		if (obj) obj.SetActive(true);
	}

	public void HideTutorial() {
		if (!isTutorialActive) return;
		isTutorialActive = false;
		tutorialObjects.TryGetValue(activeTutorialObjectIndex, out var obj);
		if (obj) obj.SetActive(false);
	}

	#endregion
}