using System;

[Serializable]
public class SignSave
{
	public SignDetails[] savedSigns = new SignDetails[0];

	public void saveSigns()
	{
		savedSigns = SignManager.manage.allSigns.ToArray();
	}

	public void loadSigns()
	{
		if (savedSigns != null)
		{
			for (int i = 0; i < savedSigns.Length; i++)
			{
				SignManager.manage.allSigns.Add(savedSigns[i]);
			}
		}
	}
}
