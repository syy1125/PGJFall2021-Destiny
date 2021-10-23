using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ObstacleBlock : MonoBehaviour
{
	public Vector2Int RootPosition;
	public List<Vector2Int> BlockedPositions = new List<Vector2Int>(new[] { Vector2Int.zero });
	public bool Draggable = true;
	public float MoveTime = 0.1f;

	private ObstacleGrid _grid;
	private Vector3 _velocity;

	private void Awake()
	{
		_grid = GetComponentInParent<ObstacleGrid>();
	}

	private void Update()
	{
		if (!Application.IsPlaying(gameObject))
		{
			transform.localPosition = new Vector3(RootPosition.x, RootPosition.y, 0);
			return;
		}

		transform.localPosition = Vector3.SmoothDamp(
			transform.localPosition, new Vector3(RootPosition.x, RootPosition.y), ref _velocity, MoveTime
		);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Color.magenta;
		foreach (Vector2Int blockedPosition in BlockedPositions)
		{
			Gizmos.DrawWireCube(
				transform.TransformPoint(new Vector3(blockedPosition.x, blockedPosition.y)),
				new Vector3(0.8f, 0.8f, 0.8f)
			);
		}
	}
}