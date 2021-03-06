using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimation : MonoBehaviour
{
	public GameObject RockPrefab;
	public float RockMinSpeed;
	public float RockMaxSpeed;
	public BoxCollider2D RockSpawnArea;
	public float SpawnRate;

	private List<GameObject> _rocks;
	private Queue<GameObject> _disabledRockPool;

	private void Awake()
	{
		_rocks = new List<GameObject>();
		_disabledRockPool = new Queue<GameObject>();
	}

	private void Start()
	{
		SpawnRock();
	}

	private void Update()
	{
		float p = Time.deltaTime * SpawnRate;

		if (Random.value < p)
		{
			SpawnRock();
		}

		HashSet<GameObject> remove = new HashSet<GameObject>();
		foreach (GameObject rock in _rocks)
		{
			if (rock.activeSelf && rock.transform.position.x > -RockSpawnArea.offset.x)
			{
				remove.Add(rock);
			}
		}

		foreach (GameObject rock in remove)
		{
			_rocks.Remove(rock);
			Destroy(rock);
		}
	}

	private void SpawnRock()
	{
		GameObject rock = Instantiate(RockPrefab, transform);
		_rocks.Add(rock);

		Vector2 min = RockSpawnArea.offset - RockSpawnArea.size / 2f;
		Vector2 max = RockSpawnArea.offset + RockSpawnArea.size / 2f;
		Vector3 localPosition = new Vector3(
			Mathf.Lerp(min.x, max.x, Random.value), Mathf.Lerp(min.y, max.y, Random.value)
		);
		localPosition.z = localPosition.y / RockSpawnArea.size.y;
		rock.transform.localPosition = localPosition;

		rock.GetComponent<Rigidbody2D>().velocity = Vector2.right * Random.Range(RockMinSpeed, RockMaxSpeed);

		rock.SetActive(true);
	}
}