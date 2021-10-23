using UnityEngine;

public class GridMovement2 : MonoBehaviour
{
	public float MoveTime = 0.4f;
	public float MoveInterval = 0.5f;

	private Vector3 _startPosition;
	private Vector3Int _offset;
	private Vector3 _position;
	private Vector3 _velocity;
	private float _moveCooldown;

	private void Start()
	{
		_startPosition = transform.position;
	}

	private void Update()
	{
		Vector2 input = new Vector2(Input.GetAxis("Horizontal2"), Input.GetAxis("Vertical2"));

		if (_moveCooldown <= 0f)
		{
			if (Mathf.Abs(input.x) > Mathf.Epsilon)
			{
				_offset.x += input.x > 0 ? 1 : -1;
				_moveCooldown = MoveInterval;
			}
			else if (Mathf.Abs(input.y) > Mathf.Epsilon)
			{
				_offset.y += input.y > 0 ? 1 : -1;
				_moveCooldown = MoveInterval;
			}
		}
		else
		{
			_moveCooldown -= Time.deltaTime;
		}

		transform.position = Vector3.SmoothDamp(transform.position, _startPosition + _offset, ref _velocity, MoveTime);
	}
}