using System;
using System.Diagnostics;
using UnityEngine;

public class OtherSettings : MonoBehaviour
{
	public void OpenSaveDirectory()
	{
		string persistentDataPath = Application.persistentDataPath;
		try
		{
			if (string.IsNullOrEmpty(persistentDataPath))
			{
				UnityEngine.Debug.LogError("The provided directory path is null or empty.");
				return;
			}
			string text = persistentDataPath.Replace('/', '\\');
			string arguments = "/select,\"" + text + "\"";
			Process.Start("explorer.exe", arguments);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Error opening directory: " + ex.Message);
		}
	}

	public void ClearPlayerPrefButton()
	{
		PlayerPrefs.DeleteAll();
		CustomNetworkManager.manage.disconectionScreenButton();
	}
}
