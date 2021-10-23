using System;
using System.Collections;
using UnityEngine;

public class ObstacleDrag : MonoBehaviour
{
	public LineRenderer PreviewLine;
	public float MoveTime;

	private Vector2Int _startPosition;
	private Vector3 _velocity;
	private bool? _vertical;
	private Vector3Int _target;

	private void Start()
	{
		_target = Vector3Int.RoundToInt(transform.position);
	}

	private void Update()
	{
		Vector2Int mouseGridPosition = Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));

		if (Input.GetButtonDown("Fire1"))
		{
			_startPosition = mouseGridPosition;
		}
		else if (Input.GetButton("Fire1"))
		{
			if (mouseGridPosition != _startPosition && _vertical == null)
			{
				_vertical = mouseGridPosition.y != _startPosition.y;
				Debug.Log(_vertical);
			}

			if (_vertical != null)
			{
				_target = _vertical.Value
					? new Vector3Int(_startPosition.x, mouseGridPosition.y, 0)
					: new Vector3Int(mouseGridPosition.x, _startPosition.y, 0);
			}
		}
		else if (Input.GetButtonUp("Fire1"))
		{
			_vertical = null;
		}

		transform.position = Vector3.SmoothDamp(transform.position, _target, ref _velocity, MoveTime);
	}
}