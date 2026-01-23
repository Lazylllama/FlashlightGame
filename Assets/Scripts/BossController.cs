using TMPro;
using UnityEngine;

public class BossController : MonoBehaviour {
	#region Fields
	[Header("Text")]
	[SerializeField] private TMP_Text healthText;
	
	//* States
	private float health;
	#endregion
	
	#region Functions
	public void Hit(float damage) {
		health -= damage;
		UpdateText();
	}
	
	private void UpdateText() {
		healthText.text = "{health}";
	}
	#endregion
}
