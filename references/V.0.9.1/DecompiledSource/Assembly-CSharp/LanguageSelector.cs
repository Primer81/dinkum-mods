using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class LanguageSelector : MonoBehaviour
{
	private void Start()
	{
		if (PlayerPrefs.HasKey("FirstLaunchLanguageChange"))
		{
			base.enabled = false;
			return;
		}
		SetLanguageBasedOnSystem();
		PlayerPrefs.SetString("FirstLaunchLanguageChange", LocalizationManager.CurrentLanguage);
	}

	private void SetLanguageBasedOnSystem()
	{
		Dictionary<SystemLanguage, string> obj = new Dictionary<SystemLanguage, string>
		{
			{
				SystemLanguage.English,
				"English"
			},
			{
				SystemLanguage.French,
				"French"
			},
			{
				SystemLanguage.Russian,
				"Russian"
			},
			{
				SystemLanguage.ChineseSimplified,
				"Chinese (Simplified)"
			},
			{
				SystemLanguage.Japanese,
				"Japanese"
			},
			{
				SystemLanguage.Korean,
				"Korean"
			},
			{
				SystemLanguage.ChineseTraditional,
				"Chinese (Traditional)"
			},
			{
				SystemLanguage.Thai,
				"Thai"
			},
			{
				SystemLanguage.Indonesian,
				"Indonesian"
			},
			{
				SystemLanguage.Italian,
				"Italian"
			},
			{
				SystemLanguage.Spanish,
				"Spanish (Latin Americas)"
			},
			{
				SystemLanguage.Portuguese,
				"Portuguese"
			},
			{
				SystemLanguage.Turkish,
				"Turkish"
			},
			{
				SystemLanguage.German,
				"German"
			}
		};
		SystemLanguage systemLanguage = Application.systemLanguage;
		if (obj.TryGetValue(systemLanguage, out var value) && LocalizationManager.HasLanguage(value))
		{
			Debug.Log("Language being set from OS to " + LocalizationManager.CurrentLanguage);
			LocalizationManager.CurrentLanguage = value;
		}
		else
		{
			Debug.Log("Language being set to english as a fallback");
			LocalizationManager.CurrentLanguage = "English";
		}
	}
}
