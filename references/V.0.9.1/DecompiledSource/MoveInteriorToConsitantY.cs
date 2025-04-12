using System.Collections;
using UnityEngine;

public class MoveInteriorToConsitantY : MonoBehaviour
{
	public float yPosToMoveTo;

	public void OnEnable()
	{
		base.transform.position = new Vector3(base.transform.position.x, yPosToMoveTo, base.transform.position.z);
		StartCoroutine(MoveDelayForChangingLevels());
	}

	private IEnumerator MoveDelayForChangingLevels()
	{
		float timer = 0f;
		while (timer <= 1f)
		{
			yield return null;
			timer += Time.deltaTime;
			base.transform.position = new Vector3(base.transform.position.x, yPosToMoveTo, base.transform.position.z);
		}
	}
}
