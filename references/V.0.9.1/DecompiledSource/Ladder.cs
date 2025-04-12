using UnityEngine;

public class Ladder : MonoBehaviour
{
	public GameObject[] ladderLevels;

	public Transform ladderCollider;

	public ASound climbSound;

	public void SetToLevel(int level)
	{
		level = Mathf.Clamp(level, 3, ladderLevels.Length);
		for (int i = 0; i < ladderLevels.Length; i++)
		{
			if (i < level)
			{
				ladderLevels[i].SetActive(value: true);
			}
			else
			{
				ladderLevels[i].SetActive(value: false);
			}
		}
		ladderCollider.localPosition = new Vector3(ladderCollider.localPosition.x, (float)level / 2f, ladderCollider.localPosition.z);
		ladderCollider.localScale = new Vector3(ladderCollider.localScale.x, level, ladderCollider.localScale.z);
	}
}
