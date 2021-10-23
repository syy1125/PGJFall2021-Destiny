using UnityEngine;

public class GridEnvironment : MonoBehaviour
{
	public Vector2Int BoundsMin;
	public Vector2Int BoundsMax;

	public bool InBounds(Vector2Int point)
	{
		return point.x >= BoundsMin.x && point.x <= BoundsMax.x && point.y >= BoundsMin.y && point.y <= BoundsMax.y;
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Color.red;

		Vector2 center = (Vector2) (BoundsMin + BoundsMax) / 2f;
		Vector2 size = BoundsMax - BoundsMin + Vector2Int.one;
		Gizmos.DrawWireCube(transform.TransformPoint(center), size);
	}
}