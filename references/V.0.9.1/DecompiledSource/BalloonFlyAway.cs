using System.Collections;
using UnityEngine;

public class BalloonFlyAway : MonoBehaviour
{
	public Transform balloonToFly;

	public Transform cameraPos;

	public GameObject invisibleWalls;

	private void OnEnable()
	{
		if ((bool)NetworkMapSharer.Instance.localChar)
		{
			StartCoroutine(StartFlyAway());
		}
	}

	private IEnumerator StartFlyAway()
	{
		invisibleWalls.SetActive(value: true);
		while (!NetworkMapSharer.Instance)
		{
			yield return null;
		}
		while (!NetworkMapSharer.Instance.localChar)
		{
			yield return null;
		}
		yield return null;
		yield return null;
		while (ConversationManager.manage.IsConversationActive || WeatherManager.Instance.IsMyPlayerInside)
		{
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		while (balloonToFly.position.y < 5f)
		{
			balloonToFly.position += Vector3.up * Time.deltaTime;
			yield return null;
		}
		while (balloonToFly.position.y < 55f)
		{
			yield return null;
			balloonToFly.position = balloonToFly.position + Vector3.up * Time.deltaTime * 2f + Vector3.left * Time.deltaTime * 2f;
		}
		while (Vector3.Distance(NetworkMapSharer.Instance.localChar.transform.position, invisibleWalls.transform.position) < 15f)
		{
			yield return null;
		}
		invisibleWalls.SetActive(value: false);
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(0, TownManager.manage.startingDockPosition[0], TownManager.manage.startingDockPosition[1]);
	}
}
