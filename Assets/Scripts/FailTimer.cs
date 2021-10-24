using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FailTimer : MonoBehaviour
{
	public float Delay;
	private float _remainingTime;

	public TMP_Text TimeDisplay;

	private void OnEnable()
	{
		_remainingTime = Delay;
	}

	private void Start()
	{
		if (PlayerPrefs.GetInt("UseTimer", 0) != 1)
		{
			if (TimeDisplay != null)
			{
				TimeDisplay.gameObject.SetActive(false);
			}

			enabled = false;
		}
	}

	private void Update()
	{
		if (GameManager.Instance.State != GameState.Active) return;
		
		_remainingTime -= Time.deltaTime;
		if (TimeDisplay != null)
		{
			TimeDisplay.text = Mathf.Max(_remainingTime, 0f).ToString("0.0");
		}

		if (_remainingTime <= 0)
		{
			enabled = false;
			GameManager.Instance.State = GameState.Failure;
		}
	}
}