using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
	public static ScreenFader Instance;

	private Image image;

	private void Awake()
	{
		Instance = this;
		image    = GetComponent<Image>();
	}

	public IEnumerator FadeOut(float duration)
	{
		float t = 0;
		while (t < duration)
		{
			t += Time.deltaTime;
			SetAlpha(t / duration);
			yield return null;
		}
	}

	public IEnumerator FadeIn(float duration)
	{
		float t = duration;
		while (t > 0)
		{
			t -= Time.deltaTime;
			SetAlpha(t / duration);
			yield return null;
		}
	}

	private void SetAlpha(float alpha)
	{
		Color c = image.color;
		c.a         = alpha;
		image.color = c;
	}
}