using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct PlayerToken
{
	public Transform PlayerTransform;
	public Vector2Int Position;
	public string HorizontalAxis;
	public string VerticalAxis;
	public TMP_Text MoveLimitDisplay;
	[NonSerialized]
	public int MoveCount;
}

[ExecuteInEditMode]
[RequireComponent(typeof(GridEnvironment))]
[RequireComponent(typeof(ObstacleGrid))]
public class PlayerMovementGrid : MonoBehaviour
{
	public AnimationCurve MoveCurve;
	public AnimationCurve FailedMoveCurve;
	public float MoveTime = 0.2f;
	public float MoveCooldown = 0.05f;
	public int MoveLimit = 10;
	public PlayerToken[] Players;

	private Coroutine[] _moveCoroutines;
	private GridEnvironment _grid;
	private ObstacleGrid _obstacleGrid;
	private bool _useMoveLimit;

	private void Awake()
	{
		_moveCoroutines = new Coroutine[Players.Length];
		_grid = GetComponent<GridEnvironment>();
		_obstacleGrid = GetComponent<ObstacleGrid>();
		_useMoveLimit = PlayerPrefs.GetInt("UseMoveLimit", 0) == 1;
	}

	private void Start()
	{
		if (Application.IsPlaying(gameObject))
		{
			for (int i = 0; i < Players.Length; i++)
			{
				if (Players[i].MoveLimitDisplay != null)
				{
					if (_useMoveLimit)
					{
						Players[i].MoveLimitDisplay.text = MoveLimit.ToString();
					}
					else
					{
						Players[i].MoveLimitDisplay.gameObject.SetActive(false);
					}
				}
			}
		}
	}

	private void Update()
	{
		if (!Application.IsPlaying(gameObject))
		{
			if (Players == null) return;

			foreach (PlayerToken playerToken in Players)
			{
				if (playerToken.PlayerTransform != null)
				{
					playerToken.PlayerTransform.localPosition = new Vector3(
						playerToken.Position.x, playerToken.Position.y
					);
				}
			}

			return;
		}

		if (GameManager.Instance.State == GameState.Active)
		{
			for (int i = 0; i < Players.Length; i++)
			{
				if (_moveCoroutines[i] != null) continue;
				if (_useMoveLimit && Players[i].MoveCount >= MoveLimit) continue;

				Vector2 move = new Vector2(
					Input.GetAxisRaw(Players[i].HorizontalAxis), Input.GetAxisRaw(Players[i].VerticalAxis)
				);

				Vector2Int target = Players[i].Position;

				if (Mathf.Abs(move.x) > Mathf.Epsilon)
				{
					target += new Vector2Int(move.x > 0 ? 1 : -1, 0);
				}
				else if (Mathf.Abs(move.y) > Mathf.Epsilon)
				{
					target += new Vector2Int(0, move.y > 0 ? 1 : -1);
				}

				if (target != Players[i].Position)
				{
					if (IsValidTarget(target))
					{
						_moveCoroutines[i] =
							StartCoroutine(MovePlayerVisual(i, Players[i].Position, target));

						Players[i].Position = target;
						Players[i].MoveCount++;

						if (Players[i].MoveLimitDisplay != null)
						{
							Players[i].MoveLimitDisplay.text = (MoveLimit - Players[i].MoveCount).ToString();
						}
					}
					else
					{
						_moveCoroutines[i] = StartCoroutine(FailedMoveVisual(i, Players[i].Position, target));
					}
				}
			}

			if (Players.Select(player => player.Position).Distinct().Count() == 1)
			{
				// Players overlap, end game
				GameManager.Instance.State = GameState.Success;
			}

			if (_useMoveLimit && Players.All(player => player.MoveCount >= MoveLimit))
			{
				// All player moves exhausted, game fails
				GameManager.Instance.State = GameState.Failure;
			}
		}
	}

	private bool IsValidTarget(Vector2Int targetPosition)
	{
		return _grid.InBounds(targetPosition) && _obstacleGrid.GetBlockAt(targetPosition) == null;
	}

	private IEnumerator MovePlayerVisual(int index, Vector2Int fromPosition, Vector2Int toPosition)
	{
		float startTime = Time.time;
		float endTime = startTime + MoveTime;

		while (Time.time < endTime)
		{
			Players[index].PlayerTransform.localPosition = Vector2.Lerp(
				fromPosition, toPosition,
				MoveCurve.Evaluate(Mathf.InverseLerp(startTime, endTime, Time.time))
			);
			yield return null;
		}

		Players[index].PlayerTransform.localPosition = new Vector3(toPosition.x, toPosition.y);

		yield return new WaitForSeconds(MoveCooldown);

		_moveCoroutines[index] = null;
	}

	private IEnumerator FailedMoveVisual(int index, Vector2Int fromPosition, Vector2Int toPosition)
	{
		float startTime = Time.time;
		float endTime = startTime + MoveTime;

		while (Time.time < endTime)
		{
			Players[index].PlayerTransform.localPosition = Vector2.Lerp(
				fromPosition, toPosition,
				FailedMoveCurve.Evaluate(Mathf.InverseLerp(startTime, endTime, Time.time))
			);
			yield return null;
		}

		Players[index].PlayerTransform.localPosition = new Vector3(fromPosition.x, fromPosition.y);

		yield return new WaitForSeconds(MoveCooldown);

		_moveCoroutines[index] = null;
	}

	public bool HasPlayerAt(Vector2Int position)
	{
		return Players.Any(player => player.Position == position);
	}
}