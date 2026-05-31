using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsRoll : MonoBehaviour {
	[SerializeField] private float movementDist;

	private void Start() {
		LeanTween.moveY(gameObject, transform.position.y + (movementDist * 1.5f), 20f)
		         .setEase(LeanTweenType.linear)
		         .setOnComplete(() => { SceneManager.LoadScene("Main"); });
	}
}