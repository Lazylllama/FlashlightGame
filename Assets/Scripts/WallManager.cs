using System.Collections;
using UnityEngine;

public class WallManager : MonoBehaviour {
	[Header("ID")]
	[SerializeField] private string id;

	[Header("Sprites")]
	[SerializeField] private GameObject unbroken;
	[SerializeField] private GameObject broken;

	[SerializeField] private Collider2D hitCollider;

	private bool isInitialized = false;

	//? Refs
	private ParticleSystem ps;

	private void Awake() {
		ps = GetComponentInChildren<ParticleSystem>();
	}

	private void Update() {
		if (!isInitialized) Init();
	}

	private void Init() {
		GameController.Instance.wallTriggerEvent.AddListener(OpenWall);
		isInitialized = true;
	}

	private void OpenWall(string _id) {
		if (id != _id) return;
		ps.Play();
		StartCoroutine(Delay());
	}

	private IEnumerator Delay() {
		if (TutorialHandler.Instance.isTutorialActive && TutorialHandler.Instance.activeTutorialObjectIndex == 4) {
			TutorialHandler.Instance.ShowTutorial(2);
			ConversationHandler.Instance.StartConversation("FirstDoorBroken");
		}

		yield return new WaitForSeconds(2f);

		unbroken.SetActive(false);
		broken.SetActive(true);

		hitCollider.enabled = false;
		enabled             = false;
	}
}