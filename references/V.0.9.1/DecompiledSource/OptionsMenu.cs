using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	public static OptionsMenu options;

	public Image fullscreenToggle;

	private List<Resolution> resolutions = new List<Resolution>();

	private bool firstSetUp = true;

	[Header("Quality Buttons")]
	public InvButton[] qualityButtons;

	public Color selectedColor;

	public Color baseColor;

	public InvButton[] chunkViewDistanceButtons;

	[Header("Sound Options")]
	public Image masterVolumeSlider;

	public Image masterVolumeBack;

	public Image volumeSlider;

	public Image volumeBack;

	public Image UIVolumeSlider;

	public Image UIVolumeBack;

	public Image musicVolumeSlider;

	public Image musicVolumeBack;

	public Image voiceChatVolumeSlider;

	public Image voiceChatVolumeBack;

	public TextMeshProUGUI cameraHorizontalSpeed;

	public TextMeshProUGUI cameraVerticleSpeed;

	public TextMeshProUGUI resolutionText;

	[Header("Control Options")]
	public GameObject invertXTick;

	public GameObject invertYTick;

	public GameObject cameraToggleTick;

	public GameObject cameraButtonTick;

	public GameObject rumbleTick;

	public GameObject autoDetectControllerTick;

	public GameObject mapNorthTick;

	public GameObject animateJournalOpenTick;

	public GameObject voiceChatOnTick;

	public GameObject voiceChatToggleTick;

	public GameObject staminaWheelTick;

	public GameObject use24hourTimeTick;

	public TextMeshProUGUI voiceChatToggleButtonText;

	[Header("Nametag Options")]
	public GameObject nameTagTick;

	[Header("Other Options")]
	public GameObject menuParent;

	public GameObject menuButtons;

	public GameObject optionWindow;

	[Header("Voice Options")]
	public Image voiceOnTick;

	public InvButton[] voiceSpeedButtons;

	public InvButton journalCloseButton;

	public bool openedInGame;

	public bool nameTagsOn;

	public UnityEvent nameTagSwitch = new UnityEvent();

	public int chunkViewDistance = 4;

	public int voiceSpeed;

	public bool voiceOn = true;

	public int textSpeed;

	public bool optionWindowOpen;

	public bool rumbleOn = true;

	public bool animateJournalOpen = true;

	public bool hostAllowVoiceChat = true;

	public bool voiceChatButtonIsToggle;

	public bool staminaWheelHidden;

	public CanvasScaler[] canvases;

	public Image canvasWideButton;

	public Image canvasNormalButton;

	public bool autoDetectOn = true;

	public int refreshRate;

	public bool mapFacesNorth;

	public GameObject highRefreshRate;

	public Image defaultRefreshRateButton;

	public Image highRefreshRateButton;

	public Transform resolutionButtonSpawnPos;

	public GameObject resolutionButton;

	public GameObject resolutionWindow;

	public float savedVoiceChatVolume = 1f;

	public bool use24HourTime;

	public GameObject[] languageButtons;

	public TMP_FontAsset[] allFonts;

	public TMP_FontAsset chineseFont;

	public TMP_FontAsset japaneseFont;

	public Image snapCursorCheckBox;

	private bool isUsingSnapCursor;

	private int showingNo;

	public static bool FullScreenMode
	{
		get
		{
			return Screen.fullScreen;
		}
		set
		{
			FullScreenMode fullscreenMode = ((!value) ? UnityEngine.FullScreenMode.Windowed : UnityEngine.FullScreenMode.ExclusiveFullScreen);
			Screen.fullScreen = value;
			Screen.SetResolution(Screen.width, Screen.height, fullscreenMode, options.refreshRate);
		}
	}

	private void Awake()
	{
		options = this;
	}

	private void Start()
	{
		nameTagSwitch.AddListener(setNameTagOnOff);
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (!resolutions.Contains(Screen.resolutions[i]) && !isResolutionInList(Screen.resolutions[i]))
			{
				resolutions.Add(Screen.resolutions[i]);
			}
		}
		for (int j = 0; j < resolutions.Count; j++)
		{
			if (resolutions[j].width == Screen.currentResolution.width && resolutions[j].height == Screen.currentResolution.height)
			{
				showingNo = j;
			}
		}
		resolutionText.text = resolutions[showingNo].width + " x " + resolutions[showingNo].height;
		StartCoroutine(createResolutionButtons());
		setUpVolumeSliders();
		fullscreenToggle.enabled = Screen.fullScreen;
		if (PlayerPrefs.HasKey("chunkDistance"))
		{
			chunkViewDistance = PlayerPrefs.GetInt("chunkDistance");
			NewChunkLoader.loader.setChunkDistance(chunkViewDistance);
		}
		if (PlayerPrefs.HasKey("snapCursorOn"))
		{
			if (PlayerPrefs.GetInt("snapCursorOn") == 1)
			{
				isUsingSnapCursor = false;
				Inventory.Instance.turnSnapCursorOnOff(isUsingSnapCursor: false);
			}
			else
			{
				isUsingSnapCursor = true;
				Inventory.Instance.turnSnapCursorOnOff(isUsingSnapCursor: true);
			}
		}
		else
		{
			isUsingSnapCursor = true;
			Inventory.Instance.turnSnapCursorOnOff(isUsingSnapCursor: true);
		}
		snapCursorCheckBox.enabled = isUsingSnapCursor;
		for (int k = 0; k < qualityButtons.Length; k++)
		{
			if (k == QualitySettings.GetQualityLevel())
			{
				qualityButtons[k].GetComponent<Image>().color = selectedColor;
			}
			else
			{
				qualityButtons[k].GetComponent<Image>().color = baseColor;
			}
		}
		selectChunkViewDistanceButton();
		if (PlayerPrefs.HasKey("invertXFree"))
		{
			setInvertX(PlayerPrefs.GetInt("invertXFree") == 1);
		}
		if (PlayerPrefs.HasKey("invertYFree"))
		{
			setInvertY(PlayerPrefs.GetInt("invertYFree") == 1);
		}
		else
		{
			setInvertY(isOn: true);
		}
		if (PlayerPrefs.HasKey("toggleLook"))
		{
			setCameraToggle(PlayerPrefs.GetInt("toggleLook") == 1);
		}
		else
		{
			setCameraToggle(isOn: true);
		}
		if (PlayerPrefs.HasKey("toggleNameTags"))
		{
			nameTagsOn = PlayerPrefs.GetInt("toggleNameTags") == 1;
			setNameTagOnOff();
		}
		if (PlayerPrefs.HasKey("voiceOn") && PlayerPrefs.GetInt("voiceOn") == 1)
		{
			turnOnOffVoice();
		}
		if (PlayerPrefs.HasKey("textSpeed"))
		{
			ChangeTextSpeed(PlayerPrefs.GetInt("textSpeed"));
		}
		else
		{
			voiceSpeedButtons[0].GetComponent<Image>().color = selectedColor;
		}
		if (PlayerPrefs.HasKey("rumbleOn") && PlayerPrefs.GetInt("rumbleOn") == 1)
		{
			swapRumble();
		}
		if (PlayerPrefs.HasKey("staminaWheelHidden") && PlayerPrefs.GetInt("staminaWheelHidden") == 1)
		{
			HideStaminaWheel(isHidden: true);
		}
		if (PlayerPrefs.HasKey("using24HourTime") && PlayerPrefs.GetInt("using24HourTime") == 1)
		{
			Use24HourTimeToggle(setUsing24HourTime: true);
		}
		rumbleTick.SetActive(rumbleOn);
		if (PlayerPrefs.HasKey("canvasScale"))
		{
			if (PlayerPrefs.GetInt("canvasScale") == 1)
			{
				makeCanvasWide();
			}
			else
			{
				canvasWideButton.color = baseColor;
				canvasNormalButton.color = selectedColor;
			}
		}
		else
		{
			canvasWideButton.color = baseColor;
			canvasNormalButton.color = selectedColor;
		}
		if (PlayerPrefs.HasKey("autoDetectController"))
		{
			if (PlayerPrefs.GetInt("autoDetectController") == 0)
			{
				autoDetectControllerTick.SetActive(value: true);
				autoDetectOn = true;
			}
			else
			{
				autoDetectControllerTick.SetActive(value: false);
				autoDetectOn = false;
			}
		}
		else
		{
			autoDetectControllerTick.SetActive(value: true);
		}
		if (PlayerPrefs.HasKey("mapFacesNorth") && PlayerPrefs.GetInt("mapFacesNorth") == 1)
		{
			mapFacesNorth = true;
			mapNorthTick.SetActive(mapFacesNorth);
		}
		if (PlayerPrefs.HasKey("cameraXSpeed"))
		{
			CameraController.control.horizontalSpeed = PlayerPrefs.GetFloat("cameraXSpeed");
		}
		changeHorizontalSpeed(0f);
		if (PlayerPrefs.HasKey("cameraYSpeed"))
		{
			CameraController.control.verticleSpeed = PlayerPrefs.GetFloat("cameraYSpeed");
		}
		changeVerticalSpeed(0f);
		if ((float)Screen.currentResolution.refreshRate > 61f)
		{
			highRefreshRate.SetActive(value: true);
			if (PlayerPrefs.HasKey("refreshRate"))
			{
				if (PlayerPrefs.GetInt("refreshRate") == 0)
				{
					useDefaultRefreshRate();
				}
				else
				{
					useHighRefreshRate();
				}
			}
		}
		else
		{
			useDefaultRefreshRate();
		}
		if (PlayerPrefs.HasKey("animateJournal"))
		{
			if (PlayerPrefs.GetInt("animateJournal") == 0)
			{
				animateJournalOpen = true;
			}
			else
			{
				animateJournalOpen = false;
			}
		}
		animateJournalOpenTick.SetActive(animateJournalOpen);
		if (PlayerPrefs.HasKey("allowVoiceChat"))
		{
			if (PlayerPrefs.GetInt("allowVoiceChat") == 0)
			{
				hostAllowVoiceChat = true;
			}
			else
			{
				hostAllowVoiceChat = false;
			}
			voiceChatOnTick.SetActive(hostAllowVoiceChat);
		}
		if (PlayerPrefs.HasKey("holdVoiceChatButton"))
		{
			if (PlayerPrefs.GetInt("holdVoiceChatButton") == 0)
			{
				voiceChatButtonIsToggle = true;
			}
			else
			{
				voiceChatButtonIsToggle = false;
			}
			voiceChatToggleTick.SetActive(voiceChatButtonIsToggle);
			if (voiceChatButtonIsToggle)
			{
				voiceChatToggleButtonText.text = string.Concat((LocalizedString)"OptionsWindow/Voice Chat Button", " - <size=10>", (LocalizedString)"OptionsWindow/PressToTalk");
			}
			else
			{
				voiceChatToggleButtonText.text = string.Concat((LocalizedString)"OptionsWindow/Voice Chat Button", " - <size=10>", (LocalizedString)"OptionsWindow/HoldToTalk");
			}
		}
		PopulateLanguageButtons();
		CheckForChineseAndJapanese();
	}

	private IEnumerator createResolutionButtons()
	{
		yield return null;
		for (int num = resolutions.Count - 1; num >= 0; num--)
		{
			Object.Instantiate(resolutionButton, resolutionButtonSpawnPos).GetComponent<ResolutionButton>().updateButton(resolutions[num]);
		}
	}

	public bool isResolutionInList(Resolution res)
	{
		for (int i = 0; i < resolutions.Count; i++)
		{
			if (res.width == resolutions[i].width && res.height == resolutions[i].height)
			{
				return true;
			}
		}
		return false;
	}

	public void animateJournalOpenButton()
	{
		animateJournalOpen = !animateJournalOpen;
		animateJournalOpenTick.SetActive(animateJournalOpen);
		if (animateJournalOpen)
		{
			PlayerPrefs.SetInt("animateJournal", 0);
		}
		else
		{
			PlayerPrefs.SetInt("animateJournal", 1);
		}
	}

	public void useHighRefreshRate()
	{
		Time.fixedDeltaTime = 0.01f;
		defaultRefreshRateButton.color = baseColor;
		highRefreshRateButton.color = selectedColor;
		PlayerPrefs.SetInt("refreshRate", 1);
	}

	public void useDefaultRefreshRate()
	{
		Time.fixedDeltaTime = 0.02f;
		defaultRefreshRateButton.color = selectedColor;
		highRefreshRateButton.color = baseColor;
		PlayerPrefs.SetInt("refreshRate", 0);
	}

	public void allowVoiceChatToggle()
	{
		hostAllowVoiceChat = !hostAllowVoiceChat;
		voiceChatOnTick.SetActive(hostAllowVoiceChat);
		if (hostAllowVoiceChat)
		{
			PlayerPrefs.SetInt("allowVoiceChat", 0);
		}
		else
		{
			PlayerPrefs.SetInt("allowVoiceChat", 1);
		}
		if ((bool)ProximityChatManager.manage && ProximityChatManager.manage.isServer)
		{
			ProximityChatManager.manage.NetworkvoiceChatOnForSever = hostAllowVoiceChat;
		}
	}

	public void allowVoiceChatButtonToggle()
	{
		voiceChatButtonIsToggle = !voiceChatButtonIsToggle;
		voiceChatToggleTick.SetActive(voiceChatButtonIsToggle);
		if (voiceChatButtonIsToggle)
		{
			PlayerPrefs.SetInt("holdVoiceChatButton", 0);
		}
		else
		{
			PlayerPrefs.SetInt("holdVoiceChatButton", 1);
		}
		if (voiceChatButtonIsToggle)
		{
			voiceChatToggleButtonText.text = string.Concat((LocalizedString)"OptionsWindow/Voice Chat Button", " - <size=10>", (LocalizedString)"OptionsWindow/PressToTalk");
		}
		else
		{
			voiceChatToggleButtonText.text = string.Concat((LocalizedString)"OptionsWindow/Voice Chat Button", " - <size=10>", (LocalizedString)"OptionsWindow/HoldToTalk");
		}
	}

	public void autoDetectOnOff()
	{
		autoDetectOn = !autoDetectOn;
		autoDetectControllerTick.SetActive(autoDetectOn);
		if (autoDetectOn)
		{
			PlayerPrefs.SetInt("autoDetectController", 0);
		}
		else
		{
			PlayerPrefs.SetInt("autoDetectController", 1);
		}
	}

	public void mapFaceNorth()
	{
		mapFacesNorth = !mapFacesNorth;
		mapNorthTick.SetActive(mapFacesNorth);
		if (mapFacesNorth)
		{
			PlayerPrefs.SetInt("mapFacesNorth", 1);
		}
		else
		{
			PlayerPrefs.SetInt("mapFacesNorth", 0);
		}
	}

	public void makeCanvasWide()
	{
		PlayerPrefs.SetInt("canvasScale", 1);
		for (int i = 0; i < canvases.Length; i++)
		{
			canvases[i].matchWidthOrHeight = 0.7f;
		}
		canvasWideButton.color = selectedColor;
		canvasNormalButton.color = baseColor;
	}

	public void makeCanvasNormal()
	{
		PlayerPrefs.SetInt("canvasScale", 0);
		for (int i = 0; i < canvases.Length; i++)
		{
			canvases[i].matchWidthOrHeight = 0.4f;
		}
		canvasWideButton.color = baseColor;
		canvasNormalButton.color = selectedColor;
	}

	public void clearAllPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}

	public void swapRumble()
	{
		rumbleOn = !rumbleOn;
		if (rumbleOn)
		{
			InputMaster.input.doRumble(0.5f, 10f);
			PlayerPrefs.SetInt("rumbleOn", 0);
		}
		else
		{
			InputMaster.input.stopRumble();
			PlayerPrefs.SetInt("rumbleOn", 1);
		}
		rumbleTick.SetActive(rumbleOn);
	}

	public void openOptionsMenu()
	{
		optionWindowOpen = true;
	}

	public void pressNameTagTogggle()
	{
		nameTagsOn = !nameTagsOn;
		if (nameTagsOn)
		{
			PlayerPrefs.SetInt("toggleNameTags", 1);
		}
		else
		{
			PlayerPrefs.SetInt("toggleNameTags", 0);
		}
		nameTagSwitch.Invoke();
	}

	private void setNameTagOnOff()
	{
		nameTagTick.SetActive(nameTagsOn);
	}

	private void setCameraToggle(bool isOn)
	{
		cameraToggleTick.SetActive(isOn);
		cameraButtonTick.SetActive(!isOn);
		if (isOn)
		{
			PlayerPrefs.SetInt("toggleLook", 1);
			CameraController.control.toggle = true;
		}
		else
		{
			PlayerPrefs.SetInt("toggleLook", 0);
			CameraController.control.toggle = false;
		}
	}

	private void setInvertX(bool isOn)
	{
		invertXTick.SetActive(isOn);
		if (isOn)
		{
			PlayerPrefs.SetInt("invertXFree", 1);
			CameraController.control.xMod = -1;
		}
		else
		{
			PlayerPrefs.SetInt("invertXFree", 0);
			CameraController.control.xMod = 1;
		}
	}

	private void setInvertY(bool isOn)
	{
		invertYTick.SetActive(isOn);
		if (isOn)
		{
			PlayerPrefs.SetInt("invertYFree", 1);
			CameraController.control.YMod = -1;
		}
		else
		{
			PlayerPrefs.SetInt("invertYFree", 0);
			CameraController.control.YMod = 1;
		}
	}

	public void switchToggle(bool onOrOff)
	{
		if (cameraToggleTick.activeInHierarchy)
		{
			setCameraToggle(isOn: false);
		}
		else
		{
			setCameraToggle(isOn: true);
		}
	}

	public void SwitchInvertX()
	{
		if (invertXTick.activeInHierarchy)
		{
			setInvertX(isOn: false);
		}
		else
		{
			setInvertX(isOn: true);
		}
	}

	public void SwitchInvertY()
	{
		if (invertYTick.activeInHierarchy)
		{
			setInvertY(isOn: false);
		}
		else
		{
			setInvertY(isOn: true);
		}
	}

	public void turnOnOffVoice()
	{
		voiceOn = !voiceOn;
		voiceOnTick.enabled = voiceOn;
		if (!voiceOn)
		{
			PlayerPrefs.SetInt("voiceOn", 1);
		}
		else
		{
			PlayerPrefs.SetInt("voiceOn", 0);
		}
	}

	public void PressStaminaWheelCheck()
	{
		HideStaminaWheel(!staminaWheelHidden);
	}

	public void Press24HourTimeCheck()
	{
		Use24HourTimeToggle(!use24HourTime);
	}

	private void HideStaminaWheel(bool isHidden)
	{
		staminaWheelHidden = isHidden;
		staminaWheelTick.SetActive(isHidden);
		if (staminaWheelHidden)
		{
			PlayerPrefs.SetInt("staminaWheelHidden", 1);
		}
		else
		{
			PlayerPrefs.SetInt("staminaWheelHidden", 0);
		}
	}

	private void Use24HourTimeToggle(bool setUsing24HourTime)
	{
		use24HourTime = setUsing24HourTime;
		use24hourTimeTick.SetActive(setUsing24HourTime);
		if (setUsing24HourTime)
		{
			PlayerPrefs.SetInt("using24HourTime", 1);
		}
		else
		{
			PlayerPrefs.SetInt("using24HourTime", 0);
		}
	}

	public void changeHorizontalSpeed(float dif)
	{
		CameraController.control.horizontalSpeed = Mathf.Clamp(CameraController.control.horizontalSpeed + dif, 0.5f, 6f);
		cameraHorizontalSpeed.text = CameraController.control.horizontalSpeed.ToString();
		PlayerPrefs.SetFloat("cameraXSpeed", CameraController.control.horizontalSpeed);
		if (CameraController.control.horizontalSpeed == 2f)
		{
			cameraHorizontalSpeed.text = ConversationGenerator.generate.GetOptionNameByTag("DEFAULT");
		}
	}

	public void changeVerticalSpeed(float dif)
	{
		CameraController.control.verticleSpeed = Mathf.Clamp(CameraController.control.verticleSpeed + dif, 0.25f, 3f);
		cameraVerticleSpeed.text = (CameraController.control.verticleSpeed * 2f).ToString();
		PlayerPrefs.SetFloat("cameraYSpeed", CameraController.control.verticleSpeed);
		if (CameraController.control.verticleSpeed == 1f)
		{
			cameraVerticleSpeed.text = ConversationGenerator.generate.GetOptionNameByTag("DEFAULT");
		}
	}

	public void ChangeTextSpeed(int newSpeed)
	{
		textSpeed = newSpeed;
		PlayerPrefs.SetInt("textSpeed", newSpeed);
		for (int i = 0; i < voiceSpeedButtons.Length; i++)
		{
			if (i == textSpeed)
			{
				voiceSpeedButtons[i].GetComponent<Image>().color = selectedColor;
			}
			else
			{
				voiceSpeedButtons[i].GetComponent<Image>().color = baseColor;
			}
		}
	}

	public void setChunkDistanceButton(int newChunkDistance)
	{
		PlayerPrefs.SetInt("chunkDistance", newChunkDistance);
		chunkViewDistance = newChunkDistance;
		NewChunkLoader.loader.setChunkDistance(chunkViewDistance);
		selectChunkViewDistanceButton();
	}

	public void openOptionsMenuInGame()
	{
		openOptionsMenu();
		optionWindow.SetActive(value: true);
		menuParent.SetActive(value: true);
		openedInGame = true;
	}

	public void closeOptionsMenuInGame()
	{
		optionWindowOpen = false;
		optionWindow.SetActive(value: false);
		Inventory.Instance.refreshMapAndWhistleButtons();
		if (openedInGame)
		{
			Inventory.Instance.setAsActiveCloseButton(journalCloseButton);
			menuParent.SetActive(value: false);
		}
		else
		{
			menuParent.SetActive(value: true);
			menuButtons.SetActive(value: true);
		}
		openedInGame = false;
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += OnChangeLanguage;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= OnChangeLanguage;
	}

	public void changeFullScreenMode()
	{
		FullScreenMode = !FullScreenMode;
		StartCoroutine(changeToggleDelay());
	}

	private IEnumerator changeToggleDelay()
	{
		yield return null;
		fullscreenToggle.enabled = Screen.fullScreen;
	}

	public void changeRefreshRate(int target)
	{
		refreshRate = target;
		Screen.SetResolution(resolutions[showingNo].width, resolutions[showingNo].height, Screen.fullScreen, refreshRate);
	}

	public void changeSnapCursorOnOff()
	{
		isUsingSnapCursor = !isUsingSnapCursor;
		Inventory.Instance.turnSnapCursorOnOff(isUsingSnapCursor);
		snapCursorCheckBox.enabled = isUsingSnapCursor;
	}

	public void ChangeResolutions(int dif)
	{
		showingNo += dif;
		if (showingNo > resolutions.Count - 1)
		{
			showingNo = 0;
		}
		if (showingNo < 0)
		{
			showingNo = resolutions.Count - 1;
		}
		resolutionText.text = resolutions[showingNo].width + " x " + resolutions[showingNo].height;
	}

	public void ApplyResolution()
	{
		Screen.SetResolution(resolutions[showingNo].width, resolutions[showingNo].height, Screen.fullScreen, 0);
	}

	public void ApplyResolution(Resolution newRes)
	{
		Screen.SetResolution(newRes.width, newRes.height, Screen.fullScreen, 0);
		resolutionText.text = newRes.width + " x " + newRes.height;
		resolutionWindow.SetActive(value: false);
	}

	private string ResToString(Resolution res)
	{
		return res.width + " x " + res.height;
	}

	public void changeLanguageEnglish()
	{
		LocalizationManager.CurrentLanguage = "English";
	}

	public void changeLanguageFrench()
	{
		LocalizationManager.CurrentLanguage = "French";
	}

	public void changeLanguageChinese()
	{
		LocalizationManager.CurrentLanguage = "Chinese";
	}

	public void changeLanguageRussian()
	{
		LocalizationManager.CurrentLanguage = "Russian";
	}

	public void setQuality(int newQuality)
	{
		QualitySettings.SetQualityLevel(newQuality, applyExpensiveChanges: true);
		for (int i = 0; i < qualityButtons.Length; i++)
		{
			if (i == newQuality)
			{
				qualityButtons[i].GetComponent<Image>().color = selectedColor;
			}
			else
			{
				qualityButtons[i].GetComponent<Image>().color = baseColor;
			}
		}
	}

	public void changeLanguage(string languageName)
	{
		LocalizationManager.CurrentLanguage = languageName;
	}

	public void changeSoundVolume(float dif)
	{
		changeSoundVolumeAmounts(dif);
		if (InputMaster.input.UISelectHeld())
		{
			StartCoroutine(holdVolumeButton(isVolume: true, dif));
		}
	}

	private void changeSoundVolumeAmounts(float dif)
	{
		float num = Mathf.Clamp(SoundManager.Instance.getSoundVolumeForChange() + dif, 0f, 4f);
		volumeSlider.fillAmount = num / 4f;
		SoundManager.Instance.setSoundEffectVolume(num);
		PlayerPrefs.SetFloat("soundVolume", num);
		PlayerPrefs.Save();
	}

	public void changeMasterVolume(float dif)
	{
		changeMasterVolumeAmounts(dif);
	}

	public void pressMasterVolumeBack()
	{
		float num = Mathf.Abs((masterVolumeBack.transform.position.x - masterVolumeBack.rectTransform.sizeDelta.x / 2f - Inventory.Instance.cursor.transform.position.x) / masterVolumeBack.rectTransform.sizeDelta.x);
		masterVolumeSlider.fillAmount = num;
		SoundManager.Instance.setMasterVolume(num * 4f);
		PlayerPrefs.SetFloat("masterVolume", num * 4f);
		PlayerPrefs.Save();
	}

	private void changeMasterVolumeAmounts(float dif)
	{
		float num = Mathf.Clamp(SoundManager.Instance.getMasterVolume() + dif, 0f, 4f);
		masterVolumeSlider.fillAmount = num / 4f;
		SoundManager.Instance.setMasterVolume(num);
		PlayerPrefs.SetFloat("masterVolume", num);
		PlayerPrefs.Save();
	}

	public void changeUIVolume(float dif)
	{
		changeUIVolumeAmounts(dif);
	}

	public void pressVolumeBack()
	{
		float num = Mathf.Abs((volumeBack.transform.position.x - volumeBack.rectTransform.sizeDelta.x / 2f - Inventory.Instance.cursor.transform.position.x) / volumeBack.rectTransform.sizeDelta.x);
		volumeSlider.fillAmount = num;
		SoundManager.Instance.setSoundEffectVolume(num * 4f);
		PlayerPrefs.SetFloat("soundVolume", num * 4f);
		PlayerPrefs.Save();
	}

	public void pressUIVolumeBack()
	{
		float num = Mathf.Abs((UIVolumeBack.transform.position.x - UIVolumeBack.rectTransform.sizeDelta.x / 2f - Inventory.Instance.cursor.transform.position.x) / UIVolumeBack.rectTransform.sizeDelta.x);
		UIVolumeSlider.fillAmount = num;
		SoundManager.Instance.setUiVolume(num * 4f);
		PlayerPrefs.SetFloat("uiVolume", num * 4f);
		PlayerPrefs.Save();
	}

	public void pressMusicVolumeBack()
	{
		float num = Mathf.Abs((musicVolumeBack.transform.position.x - musicVolumeBack.rectTransform.sizeDelta.x / 2f - Inventory.Instance.cursor.transform.position.x) / musicVolumeBack.rectTransform.sizeDelta.x);
		musicVolumeSlider.fillAmount = num;
		MusicManager.manage.changeVolume(num * 4f);
		PlayerPrefs.SetFloat("musicVolume", num * 4f);
		PlayerPrefs.Save();
	}

	public void pressVoiceVolumeBack()
	{
		float num = Mathf.Abs((voiceChatVolumeBack.transform.position.x - voiceChatVolumeBack.rectTransform.sizeDelta.x / 2f - Inventory.Instance.cursor.transform.position.x) / voiceChatVolumeBack.rectTransform.sizeDelta.x);
		voiceChatVolumeSlider.fillAmount = num;
		savedVoiceChatVolume = num * 4f;
		changeVoiceChatVolumeAmounts(0f);
	}

	private void changeUIVolumeAmounts(float dif)
	{
		float num = Mathf.Clamp(SoundManager.Instance.getUiVolumeForChange() + dif, 0f, 4f);
		UIVolumeSlider.fillAmount = num / 4f;
		SoundManager.Instance.setUiVolume(num);
		PlayerPrefs.SetFloat("uiVolume", num);
		PlayerPrefs.Save();
	}

	public void changeVoiceChatVolumeAmounts(float dif)
	{
		savedVoiceChatVolume = Mathf.Clamp(savedVoiceChatVolume + dif, 0f, 4f);
		if ((bool)ProximityChatManager.manage)
		{
			ProximityChatManager.manage.voiceVolumeMaster = savedVoiceChatVolume;
			ProximityChatManager.manage.volumeChangeEvent.Invoke();
		}
		voiceChatVolumeSlider.fillAmount = savedVoiceChatVolume / 4f;
		PlayerPrefs.SetFloat("voiceChatVolume", savedVoiceChatVolume);
		PlayerPrefs.Save();
	}

	public void changeMusicVolume(float dif)
	{
		changeMusicVolumeAmounts(dif);
		if (InputMaster.input.UISelectHeld())
		{
			StartCoroutine(holdVolumeButton(isVolume: false, dif));
		}
	}

	private void changeMusicVolumeAmounts(float dif)
	{
		float num = Mathf.Clamp(MusicManager.manage.musicMasterVolume + dif, 0f, 4f);
		musicVolumeSlider.fillAmount = num / 4f;
		MusicManager.manage.changeVolume(num);
		PlayerPrefs.SetFloat("musicVolume", num);
		PlayerPrefs.Save();
	}

	private IEnumerator holdVolumeButton(bool isVolume, float change)
	{
		float changeTimer = 0f;
		while (InputMaster.input.UISelectHeld())
		{
			if (changeTimer < 0.1f)
			{
				changeTimer += Time.deltaTime;
			}
			else
			{
				changeTimer = 0f;
				if (isVolume)
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.buttonSound);
					changeSoundVolumeAmounts(change);
				}
				else
				{
					changeMusicVolumeAmounts(change);
				}
			}
			yield return null;
		}
	}

	private void selectChunkViewDistanceButton()
	{
		for (int i = 0; i < chunkViewDistanceButtons.Length; i++)
		{
			if (i == chunkViewDistance - 4)
			{
				chunkViewDistanceButtons[i].GetComponent<Image>().color = selectedColor;
			}
			else
			{
				chunkViewDistanceButtons[i].GetComponent<Image>().color = baseColor;
			}
		}
	}

	public void setUpVolumeSliders()
	{
		if (PlayerPrefs.HasKey("masterVolume"))
		{
			float @float = PlayerPrefs.GetFloat("masterVolume");
			SoundManager.Instance.setMasterVolume(@float);
		}
		masterVolumeSlider.fillAmount = SoundManager.Instance.getMasterVolume() / 4f;
		if (PlayerPrefs.HasKey("soundVolume"))
		{
			float float2 = PlayerPrefs.GetFloat("soundVolume");
			SoundManager.Instance.setSoundEffectVolume(float2);
		}
		volumeSlider.fillAmount = SoundManager.Instance.getSoundVolumeForChange() / 4f;
		if (PlayerPrefs.HasKey("musicVolume"))
		{
			float float3 = PlayerPrefs.GetFloat("musicVolume");
			MusicManager.manage.changeVolume(float3);
		}
		musicVolumeSlider.fillAmount = MusicManager.manage.musicMasterVolume / 4f;
		if (PlayerPrefs.HasKey("uiVolume"))
		{
			float float4 = PlayerPrefs.GetFloat("uiVolume");
			SoundManager.Instance.setUiVolume(float4);
		}
		if (PlayerPrefs.HasKey("voiceChatVolume"))
		{
			savedVoiceChatVolume = PlayerPrefs.GetFloat("voiceChatVolume");
			changeVoiceChatVolumeAmounts(0f);
		}
		else
		{
			changeVoiceChatVolumeAmounts(0f);
		}
		UIVolumeSlider.fillAmount = SoundManager.Instance.getUiVolumeForChange() / 4f;
	}

	private void OnChangeLanguage()
	{
		changeHorizontalSpeed(0f);
		changeVerticalSpeed(0f);
		if (voiceChatButtonIsToggle)
		{
			voiceChatToggleButtonText.text = string.Concat((LocalizedString)"OptionsWindow/Voice Chat Button", " - <size=10>", (LocalizedString)"OptionsWindow/PressToTalk");
		}
		else
		{
			voiceChatToggleButtonText.text = string.Concat((LocalizedString)"OptionsWindow/Voice Chat Button", " - <size=10>", (LocalizedString)"OptionsWindow/HoldToTalk");
		}
		PopulateLanguageButtons();
		CheckForChineseAndJapanese();
	}

	private void CheckForChineseAndJapanese()
	{
		string currentLanguage = LocalizationManager.CurrentLanguage;
		if (currentLanguage.Contains("Japanese"))
		{
			ReorderFallbackForJapanese();
		}
		else if (currentLanguage.Contains("Chinese"))
		{
			ReorderFallbackForChinese();
		}
		else
		{
			ReorderFallbackForChinese();
		}
	}

	private void ReorderFallbackForJapanese()
	{
		for (int i = 0; i < allFonts.Length; i++)
		{
			allFonts[i].fallbackFontAssetTable.Remove(japaneseFont);
			allFonts[i].fallbackFontAssetTable.Insert(2, japaneseFont);
		}
	}

	private void ReorderFallbackForChinese()
	{
		for (int i = 0; i < allFonts.Length; i++)
		{
			allFonts[i].fallbackFontAssetTable.Remove(chineseFont);
			allFonts[i].fallbackFontAssetTable.Insert(2, chineseFont);
		}
	}

	public int GetCurrentLanguageID()
	{
		string currentLanguage = LocalizationManager.CurrentLanguage;
		int num = LocalizationManager.GetAllLanguages().IndexOf(currentLanguage);
		if (num == -1)
		{
			Debug.LogWarning("Current language not found in the language list.");
		}
		return num;
	}

	private void PopulateLanguageButtons()
	{
		int currentLanguageID = GetCurrentLanguageID();
		for (int i = 0; i < languageButtons.Length; i++)
		{
			HideLanguageArrowsOnButton(languageButtons[i], i == currentLanguageID);
			languageButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = ConversationGenerator.generate.GetLanguageNameByTag(i);
		}
	}

	private void HideLanguageArrowsOnButton(GameObject button, bool arrowsOn)
	{
		button.transform.Find("ArrowLeft").gameObject.SetActive(arrowsOn);
		button.transform.Find("ArrowRight").gameObject.SetActive(arrowsOn);
	}
}
