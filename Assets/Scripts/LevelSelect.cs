using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
	public GameObject ButtonPrefab;
	public string[] LevelNames;
	public Transform BackButton;

	private void Start()
	{
		for (int i = 0; i < LevelNames.Length; i++)
		{
			int levelIndex = i + 1;
			GameObject levelButton = Instantiate(ButtonPrefab, transform);
			levelButton.GetComponentInChildren<TMP_Text>().text = LevelNames[i];
			levelButton.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(levelIndex));
			levelButton.transform.localPosition = Vector3.down * i * 40;
		}

		BackButton.transform.localPosition = Vector3.down * LevelNames.Length * 40;
	}
}