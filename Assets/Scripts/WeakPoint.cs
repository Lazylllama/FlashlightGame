using UnityEngine;

public class WeakPoint : MonoBehaviour
{
	#region Functions
	public void Hit(float damage) {
		gameObject.GetComponentInParent<BossController>().Hit(damage);
	}
	#endregion
}
