using UnityEngine;

public class ChangeAnimatorOffsetOnStart : MonoBehaviour
{
	public float specificOffset;

	private void OnEnable()
	{
		ChangeOffset();
	}

	public void ChangeOffset()
	{
		if (specificOffset == 0f)
		{
			GetComponent<Animator>().SetFloat("Offset", Random.Range(0f, 1f));
		}
		else
		{
			GetComponent<Animator>().SetFloat("Offset", specificOffset);
		}
	}
}
