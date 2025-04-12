using System.Collections.Generic;
using UnityEngine;

public class UserPermissions : MonoBehaviour
{
	public static UserPermissions Instance;

	public List<Permissions> AllPermissions = new List<Permissions>();

	public Permissions defaultPermission;

	private void Awake()
	{
		Instance = this;
	}

	public Permissions GetPermissions(string checkName, int checkIslandId, bool isHost)
	{
		for (int i = 0; i < AllPermissions.Count; i++)
		{
			if (AllPermissions[i].CheckIfIsUser(checkName, checkIslandId))
			{
				return AllPermissions[i];
			}
		}
		Permissions permissions = new Permissions(checkName, checkIslandId, isHost);
		AllPermissions.Add(permissions);
		return permissions;
	}

	private void OnDestroy()
	{
		Object.Destroy(Instance);
	}
}
