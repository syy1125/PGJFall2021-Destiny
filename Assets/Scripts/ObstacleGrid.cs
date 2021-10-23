using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DragState
{
	Idle,
	Indeterminate,
	Horizontal,
	Vertical
}

public struct DragExtent
{
	public int Left;
	public int Right;
	public int Up;
	public int Down;
}

public class ObstacleGrid : MonoBehaviour
{
	public Vector2Int BoundsMin;
	public Vector2Int BoundsMax;

	private List<ObstacleBlock> _blocks;

	private DragState _dragState;

	private ObstacleBlock _dragBlock;
	private Vector2Int _dragStart; // The mouse position where the drag started.
	private Vector2Int _dragHandle; // The mouse position relative to the block's root position.
	private DragExtent
		_dragExtent; // Determines how far the block can go in any particular direction. Computed at drag start.

	private void Awake()
	{
		_blocks = GetComponentsInChildren<ObstacleBlock>().ToList();
	}

	private void Update()
	{
		Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2Int mousePosition = Vector2Int.RoundToInt(transform.InverseTransformPoint(worldPosition));
		ObstacleBlock hoverBlock = GetBlock(mousePosition);

		if (Input.GetButtonDown("Fire1"))
		{
			if (hoverBlock != null && hoverBlock.Draggable)
			{
				// Initialize drag
				_dragState = DragState.Indeterminate;

				_dragBlock = hoverBlock;
				_dragStart = mousePosition;
				_dragHandle = mousePosition - hoverBlock.RootPosition;
			}
		}

		if (Input.GetButtonUp("Fire1"))
		{
			_dragState = DragState.Idle;
		}

		switch (_dragState)
		{
			case DragState.Idle:
				break;
			case DragState.Indeterminate:
				Vector2Int delta = mousePosition - _dragStart;

				if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
				{
					_dragState = DragState.Horizontal;
					goto case DragState.Horizontal;
				}
				else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
				{
					_dragState = DragState.Vertical;
					goto case DragState.Vertical;
				}

				break;
			case DragState.Horizontal:
				TryDragBlockTo(new Vector2Int(mousePosition.x, _dragStart.y));
				break;
			case DragState.Vertical:
				TryDragBlockTo(new Vector2Int(_dragStart.x, mousePosition.y));
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private ObstacleBlock GetBlock(Vector2Int position)
	{
		return _blocks.FirstOrDefault(
			block => block.BlockedPositions.Any(blockedPosition => block.RootPosition + blockedPosition == position)
		);
	}

	private void TryDragBlockTo(Vector2Int anchor)
	{
		Vector2Int targetRootPosition = anchor - _dragHandle;
		Vector2Int offset = targetRootPosition - _dragBlock.RootPosition;

		// For the drag to be valid, all intermediate positions must be clear
		Debug.Assert(offset.x == 0 || offset.y == 0, "At least one offset axis must be zero");
		if (offset.x > 0)
		{
			for (int x = 1; x <= offset.x; x++)
			{
				if (!IsDropTargetClear(_dragBlock.RootPosition + new Vector2Int(x, 0))) return;
			}
		}
		else if (offset.x < 0)
		{
			for (int x = -1; x >= offset.x; x--)
			{
				if (!IsDropTargetClear(_dragBlock.RootPosition + new Vector2Int(x, 0))) return;
			}
		}
		else if (offset.y > 0)
		{
			for (int y = 1; y <= offset.y; y++)
			{
				if (!IsDropTargetClear(_dragBlock.RootPosition + new Vector2Int(0, y))) return;
			}
		}
		else // offset.y < 0
		{
			for (int y = -1; y >= offset.y; y--)
			{
				if (!IsDropTargetClear(_dragBlock.RootPosition + new Vector2Int(0, y))) return;
			}
		}

		_dragBlock.RootPosition = targetRootPosition;
	}

	private bool IsDropTargetClear(Vector2Int targetRootPosition)
	{
		List<Vector2Int> targetPositions =
			_dragBlock.BlockedPositions
				.Select(localPosition => targetRootPosition + localPosition)
				.ToList();

		if (targetPositions.Any(
			position => position.x < BoundsMin.x
			            || position.x > BoundsMax.x
			            || position.y < BoundsMin.y
			            || position.y > BoundsMax.y
		))
		{
			return false;
		}

		foreach (ObstacleBlock otherBlock in _blocks.Where(block => block != _dragBlock))
		{
			if (otherBlock.BlockedPositions
				.Select(localPosition => otherBlock.RootPosition + localPosition)
				.Intersect(targetPositions)
				.Any())
			{
				return false;
			}
		}

		return true;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Color.red;

		Vector2 center = (Vector2) (BoundsMin + BoundsMax) / 2f;
		Vector2 size = BoundsMax - BoundsMin + Vector2Int.one;
		Gizmos.DrawWireCube(center, size);
	}
}