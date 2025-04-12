using UnityEngine;

public class EffectedByWind : MonoBehaviour
{
	public Transform faceWindDir;

	public Animator animatorWindSpeed;

	private void Start()
	{
		if ((bool)animatorWindSpeed)
		{
			animatorWindSpeed.SetFloat("Offset", Random.Range(0f, 1f));
		}
	}

	private void Update()
	{
		if ((bool)animatorWindSpeed && (bool)WeatherManager.Instance)
		{
			animatorWindSpeed.SetFloat("WindSpeed", 1f + WeatherManager.Instance.windMgr.CurrentWindSpeed * 5f);
		}
	}

	public void newDayWeatherCheck()
	{
		if ((bool)faceWindDir && (bool)WeatherManager.Instance)
		{
			faceWindDir.LookAt(faceWindDir.transform.position + WeatherManager.Instance.windMgr.WindDirection);
		}
	}

	private void OnEnable()
	{
		if ((bool)faceWindDir)
		{
			WorldManager.Instance.changeDayEvent.AddListener(newDayWeatherCheck);
		}
		newDayWeatherCheck();
	}

	private void OnDisable()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(newDayWeatherCheck);
	}
}
