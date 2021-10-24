using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DragDirectionState
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
	public GameObject DragPreviewIndicator;
	public GameObject DragPreviewVerticalIndicator;
	public GameObject DragPreviewHorizontalIndicator;
	public GameObject DragMouseIndicator;
	public GameObject DragVerticalIndicator;
	public GameObject DragHorizontalIndicator;

	public bool AllowDrag { get; set; }

	private GridEnvironment _grid;
	private PlayerMovementGrid _playerGrid;
	private List<ObstacleBlock> _blocks;

	private DragDirectionState _dragState;

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
		AllowDrag = true;
	}

	private void Update()
	{
		Vector2 mousePosition = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		ObstacleBlock hoverBlock = GetBlockAt(Vector2Int.RoundToInt(mousePosition));

		if (AllowDrag && Input.GetButtonDown("Fire1"))
		{
			if (hoverBlock != null && hoverBlock.Draggable)
			{
				// Initialize drag
				switch (hoverBlock.LockDragDirection)
				{
					case DragDirectionState.Idle:
					case DragDirectionState.Indeterminate:
						_dragState = DragDirectionState.Indeterminate;
						break;
					case DragDirectionState.Horizontal:
					case DragDirectionState.Vertical:
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

		if (!AllowDrag || Input.GetButtonUp("Fire1"))
		{
			_dragState = DragDirectionState.Idle;
		}

		Vector2 mouseOffset = mousePosition - _dragMouseStart;

		switch (_dragState)
		{
			case DragDirectionState.Idle:
				break;
			case DragDirectionState.Indeterminate:
				if (Mathf.Abs(mouseOffset.x) - Mathf.Abs(mouseOffset.y) > DirectionThreshold)
				{
					_dragState = DragDirectionState.Horizontal;
					goto case DragDirectionState.Horizontal;
				}
				else if (Mathf.Abs(mouseOffset.y) - Mathf.Abs(mouseOffset.x) > DirectionThreshold)
				{
					_dragState = DragDirectionState.Vertical;
					goto case DragDirectionState.Vertical;
				}

				break;
			case DragDirectionState.Horizontal:
			case DragDirectionState.Vertical:
				ComputeDragExtents();
				UseDragTarget(_dragRootStart + Vector2Int.RoundToInt(mouseOffset));
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void LateUpdate()
	{
		if (DragPreviewIndicator == null || DragMouseIndicator == null) return;
		Vector3 previewPosition = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		previewPosition.z = -1f;
		ObstacleBlock hoverBlock = GetBlockAt(Vector2Int.RoundToInt(previewPosition));

		DragPreviewIndicator.transform.localPosition = previewPosition;
		DragMouseIndicator.transform.localPosition = previewPosition;

		switch (_dragState)
		{
			case DragDirectionState.Idle:
				if (AllowDrag && hoverBlock != null && hoverBlock.Draggable)
				{
					Cursor.visible = false;
					DragPreviewIndicator.SetActive(true);
					DragPreviewVerticalIndicator.SetActive(
						hoverBlock.LockDragDirection != DragDirectionState.Horizontal
					);
					DragPreviewHorizontalIndicator.SetActive(
						hoverBlock.LockDragDirection != DragDirectionState.Vertical
					);
				}
				else
				{
					Cursor.visible = true;
					DragPreviewIndicator.SetActive(false);
				}

				DragMouseIndicator.SetActive(false);
				break;
			case DragDirectionState.Indeterminate:
				Cursor.visible = false;
				DragPreviewIndicator.SetActive(false);
				DragMouseIndicator.SetActive(true);
				DragHorizontalIndicator.SetActive(true);
				DragVerticalIndicator.SetActive(true);
				break;
			case DragDirectionState.Horizontal:
				Cursor.visible = false;
				DragPreviewIndicator.SetActive(false);
				DragMouseIndicator.SetActive(true);
				DragHorizontalIndicator.SetActive(true);
				DragVerticalIndicator.SetActive(false);
				break;
			case DragDirectionState.Vertical:
				Cursor.visible = false;
				DragPreviewIndicator.SetActive(false);
				DragMouseIndicator.SetActive(true);
				DragHorizontalIndicator.SetActive(false);
				DragVerticalIndicator.SetActive(true);
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
			case DragDirectionState.Horizontal:
				_dragBlock.RootPosition = new Vector2Int(
					Mathf.Clamp(targetRoot.x, _dragExtentMin, _dragExtentMax), _dragBlock.RootPosition.y
				);
				break;
			case DragDirectionState.Vertical:
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
		Vector2Int min = new Vector2Int(
			_dragBlock.BlockedPositions.Min(pos => pos.x), _dragBlock.BlockedPositions.Min(pos => pos.y)
		);
		Vector2Int max = new Vector2Int(
			_dragBlock.BlockedPositions.Max(pos => pos.x), _dragBlock.BlockedPositions.Max(pos => pos.y)
		);

		switch (_dragState)
		{
			case DragDirectionState.Horizontal:
				for (_dragExtentMin = _dragBlock.RootPosition.x - 1;
					_dragExtentMin >= _grid.BoundsMin.x - min.x;
					_dragExtentMin--)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragExtentMin, _dragBlock.RootPosition.y))) break;
				}

				for (_dragExtentMax = _dragBlock.RootPosition.x + 1;
					_dragExtentMax <= _grid.BoundsMax.x - max.x;
					_dragExtentMax++)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragExtentMax, _dragBlock.RootPosition.y))) break;
				}

				break;
			case DragDirectionState.Vertical:
				for (_dragExtentMin = _dragBlock.RootPosition.y - 1;
					_dragExtentMin >= _grid.BoundsMin.y - min.y;
					_dragExtentMin--)
				{
					if (!IsDropTargetClear(new Vector2Int(_dragBlock.RootPosition.x, _dragExtentMin))) break;
				}

				for (_dragExtentMax = _dragBlock.RootPosition.y + 1;
					_dragExtentMax <= _grid.BoundsMax.y - max.y;
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