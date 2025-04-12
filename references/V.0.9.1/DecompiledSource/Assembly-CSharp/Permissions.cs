using System;

[Serializable]
public class Permissions
{
	public string playerName;

	public int islandId;

	public bool isHost;

	public bool canTerraform;

	public bool canDamageTiles;

	public bool canOpenChests;

	public bool canPickup;

	public bool localPickup;

	public bool canInteractWithVehicles;

	public Permissions(string name, int id, bool isLocalPlayer)
	{
		isHost = isLocalPlayer;
		playerName = name;
		islandId = id;
		SetDefaultPermissions();
	}

	public bool CheckIfIsUser(string checkName, int checkId)
	{
		if (checkName == playerName)
		{
			return islandId == checkId;
		}
		return false;
	}

	private void SetDefaultPermissions()
	{
		canTerraform = UserPermissions.Instance.defaultPermission.canTerraform;
		canOpenChests = UserPermissions.Instance.defaultPermission.canOpenChests;
		canPickup = UserPermissions.Instance.defaultPermission.canPickup;
		canDamageTiles = UserPermissions.Instance.defaultPermission.canDamageTiles;
		canInteractWithVehicles = UserPermissions.Instance.defaultPermission.canInteractWithVehicles;
	}

	public bool CheckIfCanOpenChest()
	{
		return true;
	}

	public bool CheckIfCanPickUp()
	{
		return true;
	}

	public bool CheckIfCanTerraform()
	{
		return true;
	}

	public bool CheckIfCanDamgeTiles()
	{
		return true;
	}

	public bool CheckIfCanInteractWithVehicles()
	{
		return true;
	}
}
