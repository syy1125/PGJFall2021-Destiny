using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
	public GameObject PauseMenu;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch (GameManager.Instance.State)
			{
				case GameState.Active:
					GameManager.Instance.State = GameState.Menu;
					PauseMenu.SetActive(true);
					break;
				case GameState.Menu:
					GameManager.Instance.State = GameState.Active;
					PauseMenu.SetActive(false);
					break;
			}
		}
	}

	public void mainMenu()
	{
		SceneManager.LoadScene(0);
	}

	public void playGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void retryGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}