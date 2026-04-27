using System;
using UnityEngine;
using UnityEngine.UI;

public class EndScript : MonoBehaviour
{
	public  Image     endingImage;
	private Transform player;
	public  bool      playerInZone = false;
	public  float     fadeStartX;
	public  float     fadeEndX;
	public  float     cineBoxesEndX;

	void Start()
	{
		Collider2D col = GetComponent<Collider2D>();
		fadeStartX    = col.bounds.min.x + 40f; //Allow black bars to start fading in before the player reaches the start of the collider
		fadeEndX      = col.bounds.max.x;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			player       = other.transform;
			playerInZone = true;
			CinematicBars.instance.ShowBars();
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
			playerInZone = false;
		
	}

	void Update()
	{
		if (!playerInZone || player == null) return;

		float t = Mathf.InverseLerp(fadeStartX, fadeEndX, player.position.x);

		Color c = endingImage.color;
		c.a               = t;
		endingImage.color = c;
	}
}
