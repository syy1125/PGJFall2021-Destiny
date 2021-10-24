using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public Toggle ToggleTimer;
	public Toggle ToggleMoveLimit;

	private void Start()
	{
		ToggleTimer.isOn = PlayerPrefs.GetInt("UseTimer", 0) == 1;
		ToggleMoveLimit.isOn = PlayerPrefs.GetInt("UseMoveLimit", 0) == 1;
		ToggleTimer.onValueChanged.AddListener(OnUseTimerChanged);
		ToggleMoveLimit.onValueChanged.AddListener(OnUseMoveLimitChanged);
	}

	private void OnUseTimerChanged(bool useTimer)
	{
		PlayerPrefs.SetInt("UseTimer", useTimer ? 1 : 0);
	}

	private void OnUseMoveLimitChanged(bool useMoveLimit)
	{
		PlayerPrefs.SetInt("UseMoveLimit", useMoveLimit ? 1 : 0);
	}

	public void StartGame()
	{
		SceneManager.LoadScene(1);
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}