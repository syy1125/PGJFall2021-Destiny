using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
	public float Delay;
	private float _remainingTime;
	public UnityEvent OnTimerEnd;

	public TMP_Text TimeDisplay;

	private void OnEnable()
	{
		_remainingTime = Delay;
	}

	private void Update()
	{
		_remainingTime -= Time.deltaTime;
		if (TimeDisplay != null)
		{
			TimeDisplay.text = Mathf.Max(_remainingTime, 0f).ToString("0.0");
		}

		if (_remainingTime <= 0)
		{
			enabled = false;
			OnTimerEnd.Invoke();
		}
	}
}