using UnityEngine;

public class SpecialEventObjects : MonoBehaviour
{
	public GameObject[] eventObjects;

	private void OnEnable()
	{
		bool active = TownEventManager.manage.IsJohnsAnniversary();
		for (int i = 0; i < eventObjects.Length; i++)
		{
			eventObjects[i].SetActive(active);
		}
	}
}
