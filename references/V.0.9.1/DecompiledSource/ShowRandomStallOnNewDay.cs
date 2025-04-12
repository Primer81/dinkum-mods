using UnityEngine;

public class ShowRandomStallOnNewDay : MonoBehaviour
{
	public GameObject[] toShowFromList;

	private void OnEnable()
	{
		if (!NetworkMapSharer.Instance)
		{
			return;
		}
		Random.InitState(NetworkMapSharer.Instance.mineSeed);
		int num = Random.Range(0, toShowFromList.Length);
		for (int i = 0; i < toShowFromList.Length; i++)
		{
			if (i == num)
			{
				toShowFromList[i].SetActive(value: true);
			}
			else
			{
				toShowFromList[i].SetActive(value: false);
			}
		}
	}
}
