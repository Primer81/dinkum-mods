using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ModChecker : MonoBehaviour
{
	public static ModChecker check;

	public GameObject modPopUpWindow;

	private void Awake()
	{
		check = this;
	}

	public void checkForMods()
	{
		Process[] processesByName = Process.GetProcessesByName("Dinkum");
		List<string> list = new List<string>();
		Process[] array = processesByName;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (object module in array[i].Modules)
			{
				list.Add(module.ToString());
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].Contains("BepInEx"))
			{
				MonoBehaviour.print("FOUND BEPINEX");
				if (!PlayerPrefs.HasKey("ModsInstalled"))
				{
					modPopUpWindow.SetActive(value: true);
					PlayerPrefs.SetInt("ModsInstalled", 1);
				}
			}
		}
	}
}
