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

[RequireComponent(typeof(GridEnvironment))]
[RequireComponent(typeof(PlayerMovementGrid))]
public class ObstacleGrid : MonoBehaviour
{
	public float DirectionThreshold = 0.5f;
	public float DragMoveTime = 0.1f;

	private GridEnvironment _grid;
	private PlayerMovementGrid _playerGrid;
	private List<ObstacleBlock> _blocks;

	private DragState _dragState;

	private ObstacleBlock _dragBlock;
	private Vector2 _dragMouseStart;
	private Vector2Int _dragRootStart;
	private int _dragExtentMin;
	private int _dragExtentMax;
	private HashSet<Vector2Int> _blockedPositions;

	private void Awake()
	{
		_grid = GetComponent<GridEnvironment>();
		_playerGrid = GetComponent<PlayerMovementGrid>();
		_blocks = GetComponentsInChildren<ObstacleBlock>().ToList();
	}

	private void Update()
	{
		Vector2 mousePosition = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		ObstacleBlock hoverBlock = GetBlockAt(Vector2Int.RoundToInt(mousePosition));

		if (Input.GetButtonDown("Fire1"))
		{
			if (hoverBlock != null && hoverBlock.Draggable)
			{
				// Initialize drag
				switch (hoverBlock.LockDragDirection)
				{
					case DragState.Idle:
					case DragState.Indeterminate:
						_dragState = DragState.Indeterminate;
						break;
					case DragState.Horizontal:
					case DragState.Vertical:
						_dragState = hoverBlock.LockDragDirection;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				_dragBlock = hoverBlock;
				_dragMouseStart = mousePosition;
				_dragRootStart = hoverBlock.RootPosition;
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

		Vector2 mouseOffset = mousePosition - _dragMouseStart;

		switch (_dragState)
		{
			case DragState.Idle:
				break;
			case DragState.Indeterminate:
				if (Mathf.Abs(mouseOffset.x) - Mathf.Abs(mouseOffset.y) > DirectionThreshold)
				{
					_dragState = DragState.Horizontal;
					goto case DragState.Horizontal;
				}
				else if (Mathf.Abs(mouseOffset.y) - Mathf.Abs(mouseOffset.x) > DirectionThreshold)
				{
					_dragState = DragState.Vertical;
					goto case DragState.Vertical;
				}

				break;
			case DragState.Horizontal:
			case DragState.Vertical:
				ComputeDragExtents();
				UseDragTarget(_dragRootStart + Vector2Int.RoundToInt(mouseOffset));
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public ObstacleBlock GetBlockAt(Vector2Int position)
	{
		return _blocks.FirstOrDefault(
			block => block.BlockedPositions.Any(blockedPosition => block.RootPosition + blockedPosition == position)
		);
	}

	private void UseDragTarget(Vector2Int targetRoot)
	{
		switch (_dragState)
		{
			case DragState.Horizontal:
				_dragBlock.RootPosition = new Vector2Int(
					Mathf.Clamp(targetRoot.x, _dragExtentMin, _dragExtentMax), _dragBlock.RootPosition.y
				);
				break;
			case DragState.Vertical:
				_dragBlock.RootPosition = new Vector2Int(
					_dragBlock.RootPosition.x, Mathf.Clamp(targetRoot.y, _dragExtentMin, _dragExtentMax)
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
				for (_dragExtentMin = _dragBlock.RootPosition.x - 1;
					_dragExtentMin >= _grid.BoundsMin.x;
					_dragExtentMin--)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragExtentMin, _dragBlock.RootPosition.y))) break;
				}

				for (_dragExtentMax = _dragBlock.RootPosition.x + 1;
					_dragExtentMax <= _grid.BoundsMax.x;
					_dragExtentMax++)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragExtentMax, _dragBlock.RootPosition.y))) break;
				}

				break;
			case DragState.Vertical:
				for (_dragExtentMin = _dragBlock.RootPosition.y - 1;
					_dragExtentMin >= _grid.BoundsMin.y;
					_dragExtentMin--)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragBlock.RootPosition.x, _dragExtentMin))) break;
				}

				for (_dragExtentMax = _dragBlock.RootPosition.y + 1;
					_dragExtentMax <= _grid.BoundsMax.y;
					_dragExtentMax++)
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
			position => !_grid.InBounds(position)
			            || _blockedPositions.Contains(position)
			            || _playerGrid.HasPlayerAt(position)
		))
		{
			return false;
		}

		return true;
	}
}