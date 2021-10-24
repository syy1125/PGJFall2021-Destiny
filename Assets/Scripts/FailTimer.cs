﻿using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FailTimer : MonoBehaviour
{
	public float Delay;
	private float _remainingTime;

	public TMP_Text TimeDisplay;
	public Image TimeChart;

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

			if (TimeChart != null)
			{
				TimeChart.gameObject.SetActive(false);
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
			TimeDisplay.text = Mathf.Max(_remainingTime, 0f).ToString("0.000");
		}

		if (TimeChart != null)
		{
			TimeChart.fillAmount = _remainingTime / Delay;
		}

		if (_remainingTime <= 0)
		{
			enabled = false;
			GameManager.Instance.State = GameState.Failure;
		}
	}
}