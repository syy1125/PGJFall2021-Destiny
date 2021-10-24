using System.Collections;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestinyEasterEgg : MonoBehaviour
{
	public Sprite AltSprite;
	private static bool _useAltSprite;

	private void Start()
	{
		if (SceneManager.GetActiveScene().buildIndex == 0)
		{
			StartCoroutine(AcceptEasterEgg());
		}
		else
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();
			if (_useAltSprite && spriteRenderer != null)
			{
				spriteRenderer.sprite = AltSprite;
			}
		}
	}

	private IEnumerator AcceptEasterEgg()
	{
		while (true)
		{
			yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.D));
			yield return null;

			yield return new WaitUntil(() => Input.anyKeyDown);
			if (!Input.GetKeyDown(KeyCode.E)) continue;
			yield return null;

			yield return new WaitUntil(() => Input.anyKeyDown);
			if (!Input.GetKeyDown(KeyCode.S)) continue;
			yield return null;

			yield return new WaitUntil(() => Input.anyKeyDown);
			if (!Input.GetKeyDown(KeyCode.T)) continue;
			yield return null;

			yield return new WaitUntil(() => Input.anyKeyDown);
			if (!Input.GetKeyDown(KeyCode.I)) continue;
			yield return null;

			yield return new WaitUntil(() => Input.anyKeyDown);
			if (!Input.GetKeyDown(KeyCode.N)) continue;
			yield return null;

			yield return new WaitUntil(() => Input.anyKeyDown);
			if (!Input.GetKeyDown(KeyCode.Y)) continue;

			_useAltSprite = true;
			GetComponent<TMP_Text>().enabled = true;
			yield break;
		}
	}
}