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

public class ObstacleGrid : MonoBehaviour
{
	public Vector2Int BoundsMin;
	public Vector2Int BoundsMax;
	public float DragMoveTime = 0.1f;

	private List<ObstacleBlock> _blocks;

	private DragState _dragState;

	private ObstacleBlock _dragBlock;
	private Vector2Int _dragStart; // The mouse position where the drag started.
	private Vector2Int _dragHandle; // The mouse position relative to the block's root position.
	private int _dragExtentMin;
	private int _dragExtentMax;
	private HashSet<Vector2Int> _blockedPositions;

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
				_blockedPositions = new HashSet<Vector2Int>(
					_blocks
						.Where(block => block != _dragBlock)
						.SelectMany(
							block => block.BlockedPositions.Select(localPosition => block.RootPosition + localPosition)
						)
				);
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
			case DragState.Vertical:
				ComputeDragExtents();
				UseDragTarget(mousePosition);
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

	private void UseDragTarget(Vector2Int anchor)
	{
		Vector2Int targetRootPosition = anchor - _dragHandle;

		switch (_dragState)
		{
			case DragState.Horizontal:
				_dragBlock.RootPosition = new Vector2Int(
					Mathf.Clamp(targetRootPosition.x, _dragExtentMin, _dragExtentMax), _dragBlock.RootPosition.y
				);
				break;
			case DragState.Vertical:
				_dragBlock.RootPosition = new Vector2Int(
					_dragBlock.RootPosition.x, Mathf.Clamp(targetRootPosition.y, _dragExtentMin, _dragExtentMax)
				);
				break;
			default:
				Debug.LogError(
					$"UseDragTarget called with drag state {_dragState}. It should only be called when the direction of drag has been determined."
				);
				break;
		}
	}

	private void ComputeDragExtents()
	{
		switch (_dragState)
		{
			case DragState.Horizontal:
				for (_dragExtentMin = _dragBlock.RootPosition.x - 1; _dragExtentMin >= BoundsMin.x; _dragExtentMin--)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragExtentMin, _dragBlock.RootPosition.y))) break;
				}

				for (_dragExtentMax = _dragBlock.RootPosition.x + 1; _dragExtentMax <= BoundsMax.x; _dragExtentMax++)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragExtentMax, _dragBlock.RootPosition.y))) break;
				}

				break;
			case DragState.Vertical:
				for (_dragExtentMin = _dragBlock.RootPosition.y - 1; _dragExtentMin >= BoundsMin.y; _dragExtentMin--)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragBlock.RootPosition.x, _dragExtentMin))) break;
				}

				for (_dragExtentMax = _dragBlock.RootPosition.y + 1; _dragExtentMax <= BoundsMax.y; _dragExtentMax++)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragBlock.RootPosition.x, _dragExtentMax))) break;
				}

				break;
			default:
				Debug.LogError(
					$"ComputeDragExtents called with drag state {_dragState}. It should only be called when the direction of drag has been determined."
				);
				break;
		}

		_dragExtentMin++;
		_dragExtentMax--;
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
			            || _blockedPositions.Contains(position)
		))
		{
			return false;
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