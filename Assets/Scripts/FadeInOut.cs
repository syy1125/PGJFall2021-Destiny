using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
	public float FadeTime = 0.5f;
	public CanvasGroup[] Sequence;
	public UnityEvent OnSequenceEnd;

	private int _index;

	private void OnEnable()
	{
		foreach (CanvasGroup canvasGroup in Sequence)
		{
			canvasGroup.alpha = 0f;
		}

		StartCoroutine(Fade(Sequence[0], 1f, FadeTime));
	}

	private IEnumerator Fade(CanvasGroup group, float endAlpha, float time)
	{
		float startTime = Time.time;
		float endTime = startTime + time;
		float startAlpha = group.alpha;

		while (Time.time < endTime)
		{
			group.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.InverseLerp(startTime, endTime, Time.time));
			yield return null;
		}

		group.alpha = endAlpha;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
		{
			_index++;
			if (_index < Sequence.Length)
			{
				StartCoroutine(Fade(Sequence[_index], 1f, FadeTime));
			}
			else
			{
				StopAllCoroutines();

				foreach (CanvasGroup canvasGroup in Sequence)
				{
					StartCoroutine(Fade(canvasGroup, 0f, FadeTime));
				}

				Invoke(nameof(OnFadeOut), FadeTime);
			}
		}
	}

	private void OnFadeOut()
	{
		foreach (CanvasGroup canvasGroup in Sequence)
		{
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}

		OnSequenceEnd.Invoke();
	}

	public void FadeOutGraphic(Graphic target)
	{
		target.raycastTarget = false;
		target.CrossFadeAlpha(0f, FadeTime, true);
	}
}