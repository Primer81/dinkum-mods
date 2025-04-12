using System.Collections.Generic;
using UnityEngine;

public class InstancerManager : MonoBehaviour
{
	public struct InstanceData
	{
		public Mesh mesh;

		public Material material;

		public Matrix4x4 matrix;
	}

	public static InstancerManager instance;

	private Dictionary<string, List<InstanceData>> instanceGroups;

	private MaterialPropertyBlock propertyBlock;

	private const int MaxInstancesPerBatch = 1023;

	private static List<Matrix4x4> reusableMatrixList = new List<Matrix4x4>(1023);

	private void Awake()
	{
		instance = this;
		instanceGroups = new Dictionary<string, List<InstanceData>>();
		propertyBlock = new MaterialPropertyBlock();
	}

	public void RegisterInstance(string groupName, Mesh mesh, Material material, Matrix4x4 matrix)
	{
		if (!instanceGroups.ContainsKey(groupName))
		{
			instanceGroups[groupName] = new List<InstanceData>();
		}
		instanceGroups[groupName].Add(new InstanceData
		{
			mesh = mesh,
			material = material,
			matrix = matrix
		});
	}

	public void UnregisterInstance(string groupName, Matrix4x4 matrix)
	{
		if (!instanceGroups.ContainsKey(groupName))
		{
			return;
		}
		List<InstanceData> list = instanceGroups[groupName];
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].matrix == matrix)
			{
				list.RemoveAt(num);
				break;
			}
		}
	}

	public void DrawInstances()
	{
		foreach (KeyValuePair<string, List<InstanceData>> instanceGroup in instanceGroups)
		{
			Dictionary<(Mesh, Material), List<Matrix4x4>> dictionary = new Dictionary<(Mesh, Material), List<Matrix4x4>>();
			foreach (InstanceData item in instanceGroup.Value)
			{
				(Mesh, Material) key = (item.mesh, item.material);
				if (!dictionary.ContainsKey(key))
				{
					dictionary[key] = new List<Matrix4x4>();
				}
				dictionary[key].Add(item.matrix);
			}
			foreach (KeyValuePair<(Mesh, Material), List<Matrix4x4>> item2 in dictionary)
			{
				List<Matrix4x4> value = item2.Value;
				int count = value.Count;
				for (int i = 0; i < count; i += 1023)
				{
					int count2 = Mathf.Min(1023, count - i);
					reusableMatrixList.Clear();
					reusableMatrixList.AddRange(value.GetRange(i, count2));
					Graphics.DrawMeshInstanced(item2.Key.Item1, 0, item2.Key.Item2, reusableMatrixList.ToArray(), count2, propertyBlock);
				}
			}
		}
	}

	private void Update()
	{
		DrawInstances();
	}
}
