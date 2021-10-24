using UnityEngine;

public class EndTrigger : MonoBehaviour
{
	public GameObject Endscrn;
	public AudioSource winDing;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			Debug.Log("Collided");
			Endscrn.gameObject.SetActive(true);
			winDing.Play();
		}
	}
}