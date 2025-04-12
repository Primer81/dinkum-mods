using System.Collections.Generic;
using UnityEngine;

public class NPCPathFinding : MonoBehaviour
{
	public class Node
	{
		public Vector2Int Position;

		public float GCost;

		public float HCost;

		public Node Parent;

		public float FCost => GCost + HCost;

		public Node(Vector2Int position)
		{
			Position = position;
		}
	}

	public Vector2Int[] directions = new Vector2Int[4]
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0)
	};

	private bool IsWalkable(Vector2Int currentPos, Vector2Int targetPos)
	{
		if (WorldManager.Instance.heightMap[currentPos.x, currentPos.y] != WorldManager.Instance.heightMap[targetPos.x, targetPos.y] && Mathf.Abs(WorldManager.Instance.heightMap[currentPos.x, currentPos.y] - WorldManager.Instance.heightMap[targetPos.x, targetPos.y]) >= 2)
		{
			return false;
		}
		if (WorldManager.Instance.onTileMap[targetPos.x, targetPos.y] == -1)
		{
			return true;
		}
		if (WorldManager.Instance.onTileMap[targetPos.x, targetPos.y] < -1)
		{
			Vector2 vector = WorldManager.Instance.findMultiTileObjectPos(targetPos.x, targetPos.y);
			if (WorldManager.Instance.onTileMap[(int)vector.x, (int)vector.y] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[(int)vector.x, (int)vector.y]].tileObjectBridge)
			{
				return true;
			}
			return false;
		}
		if (WorldManager.Instance.waterMap[targetPos.x, targetPos.y] && WorldManager.Instance.heightMap[targetPos.x, targetPos.y] <= -1)
		{
			return false;
		}
		if (WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[targetPos.x, targetPos.y]].walkable)
		{
			return true;
		}
		if (WorldManager.Instance.onTileStatusMap[targetPos.x, targetPos.y] == 1 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[targetPos.x, targetPos.y]].tileOnOff && WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[targetPos.x, targetPos.y]].tileOnOff.isGate)
		{
			return true;
		}
		return false;
	}

	public bool CanReach(Vector2Int start, Vector2Int end, int margin = 30)
	{
		int minX = Mathf.Max(0, Mathf.Min(start.x, end.x) - margin);
		int maxX = Mathf.Min(WorldManager.Instance.GetMapSize(), Mathf.Max(start.x, end.x) + margin);
		int minY = Mathf.Max(0, Mathf.Min(start.y, end.y) - margin);
		int maxY = Mathf.Min(WorldManager.Instance.GetMapSize() - 1, Mathf.Max(start.y, end.y) + margin);
		Vector2Int start2 = start;
		Vector2Int end2 = end;
		return CanReachInBounds(start2, end2, minX, maxX, minY, maxY);
	}

	private bool CanReachInBounds(Vector2Int start, Vector2Int end, int minX, int maxX, int minY, int maxY)
	{
		List<Node> list = new List<Node>();
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		Node item = new Node(start);
		list.Add(item);
		while (list.Count > 0)
		{
			Node node = list[0];
			for (int i = 1; i < list.Count; i++)
			{
				if (list[i].FCost < node.FCost || (list[i].FCost == node.FCost && list[i].HCost < node.HCost))
				{
					node = list[i];
				}
			}
			list.Remove(node);
			hashSet.Add(node.Position);
			if (node.Position == end)
			{
				return true;
			}
			Vector2Int[] array = directions;
			foreach (Vector2Int vector2Int in array)
			{
				Vector2Int neighborPos = node.Position + vector2Int;
				if (neighborPos.x >= minX && neighborPos.x <= maxX && neighborPos.y >= minY && neighborPos.y <= maxY && IsWalkable(node.Position, neighborPos) && !hashSet.Contains(neighborPos))
				{
					float num = node.GCost + Vector2Int.Distance(node.Position, neighborPos);
					Node node2 = list.Find((Node n) => n.Position == neighborPos);
					if (node2 == null)
					{
						node2 = new Node(neighborPos);
						node2.GCost = num;
						node2.HCost = Vector2Int.Distance(neighborPos, end);
						node2.Parent = node;
						list.Add(node2);
					}
					else if (num < node2.GCost)
					{
						node2.GCost = num;
						node2.Parent = node;
					}
				}
			}
		}
		return false;
	}
}
