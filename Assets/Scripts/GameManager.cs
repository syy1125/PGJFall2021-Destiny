using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
	Active,
	Menu,
	Success,
	Failure
}

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	private GameState _state;
	public AudioSource winDing;

	public GameState State
	{
		get => _state;
		set
		{
			_state = value;

			switch (value)
			{
				case GameState.Success:
					OnSuccess.Invoke();
					winDing.Play();
					break;
				case GameState.Failure:
					OnFailure.Invoke();
					break;
			}
		}
	}

	public UnityEvent OnSuccess;
	public UnityEvent OnFailure;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			State = GameState.Active;
		}
		else if (Instance != this)
		{
			Destroy(this);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}