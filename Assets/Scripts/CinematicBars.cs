using System;
using UnityEngine;
using UnityEngine.UI;

public class CinematicBars : MonoBehaviour
{
	public static CinematicBars instance;
	
	public RectTransform topBar;
	public RectTransform bottomBar;
	public float         barHeight = 4000f;
	public float         speed     = 2f;

	private bool  show = false;
	private float t    = 0f;

	private void Awake() {
		if (instance && instance != this) {
			Destroy(gameObject);
		} else {
			instance = this;
		}
	}

	void Update()
	{
		t = Mathf.MoveTowards(t, show ? 1f : 0f, Time.deltaTime * speed);

		topBar.sizeDelta    = new Vector2(topBar.sizeDelta.x,    Mathf.Lerp(0, barHeight, t));
		bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, Mathf.Lerp(0, barHeight, t));
	}
	

	public void ShowBars() => show = true;
	public void HideBars() => show = false;
}