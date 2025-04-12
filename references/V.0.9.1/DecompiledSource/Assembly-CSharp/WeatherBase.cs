using UnityEngine;

public class WeatherBase : MonoBehaviour
{
	protected WeatherManager weatherMgr;

	public bool IsActive { get; protected set; }

	public virtual void SetUp(WeatherManager weatherMgr)
	{
		this.weatherMgr = weatherMgr;
		IsActive = false;
	}

	public virtual void SetActive(bool active)
	{
		if (!IsActive && active)
		{
			Show();
		}
		if (IsActive && !active)
		{
			Hide();
		}
		IsActive = active;
	}

	protected virtual void Show()
	{
	}

	protected virtual void Hide()
	{
	}
}
