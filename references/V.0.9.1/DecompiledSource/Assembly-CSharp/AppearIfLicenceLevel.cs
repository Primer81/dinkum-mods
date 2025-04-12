using UnityEngine;

public class AppearIfLicenceLevel : MonoBehaviour
{
	public LicenceManager.LicenceTypes licenceType;

	public int licenceLevel;

	public float percentChanceOfShowing;

	public GameObject toShow;

	public TileObject myTileObject;

	private void OnEnable()
	{
		if (!NetworkMapSharer.Instance)
		{
			return;
		}
		if (LicenceManager.manage.allLicences[(int)licenceType].getCurrentLevel() >= licenceLevel && base.transform.position.y >= 0f)
		{
			Random.InitState((int)(base.transform.position.x + base.transform.position.z) + NetworkMapSharer.Instance.mineSeed + (int)base.transform.position.y);
			if (!WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[myTileObject.xPos, myTileObject.yPos]].isPath && Random.Range(0f, 100f) <= percentChanceOfShowing)
			{
				toShow.SetActive(value: true);
			}
			else
			{
				toShow.SetActive(value: false);
			}
		}
		else
		{
			toShow.SetActive(value: false);
		}
	}
}
