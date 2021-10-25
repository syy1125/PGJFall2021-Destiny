using UnityEngine;

public class IntroDisable : MonoBehaviour
{
	private static bool _disabled;

	private void Start()
	{
		if (_disabled)
		{
			gameObject.SetActive(false);
		}
	}

	private void OnDisable()
	{
		_disabled = true;
	}
}