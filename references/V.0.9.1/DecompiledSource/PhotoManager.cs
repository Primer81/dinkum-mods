using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CinematicEffects;
using UnityStandardAssets.ImageEffects;

public class PhotoManager : MonoBehaviour
{
	public enum photographObjects
	{
		Animal,
		NPC,
		Player,
		Carryable,
		Location
	}

	public static PhotoManager manage;

	public GameObject cameraWindow;

	private AudioSource cameraZoomSound;

	public Camera photoCamera;

	private int resWidth = 640;

	private int resHeight = 512;

	public RawImage showPhotoPos;

	public Image takePhotoFlashEffect;

	private RenderTexture photoTexture;

	public bool cameraViewOpen;

	public bool photoTabOpen;

	public GlobalFog photoFog;

	public RawImage previousPhotoFrame;

	public List<PhotoDetails> savedPhotos = new List<PhotoDetails>();

	public PhotoDetails[] displayedPhotos;

	public Transform photoTabWindow;

	public Transform previewPhotoWindows;

	public List<InvPhoto> invPhotoFrames = new List<InvPhoto>();

	public InvPhoto blownUpPhoto;

	public LayerMask myPhotosInterest;

	public RectTransform photoFrameMovement;

	public GameObject showPhotoButton;

	public ConversationObject noPhotoGivenConvo;

	[Header("Photo Journal Tab----------")]
	public InvButton tabCloseButton;

	public GameObject photoPreviewPrefab;

	public Transform photoButtonSpawnPos;

	public GameObject blownUpWindow;

	public TextMeshProUGUI blownUpPhotoText;

	public TMP_InputField photoNameField;

	public TextMeshProUGUI photoNamePlaceHolder;

	[Header("Give Photo things----------")]
	public Animator animToDisableOnGive;

	public GameObject[] objectsToHideOnGive;

	public Transform windowToMove;

	public Transform moveToWhenGive;

	public Transform moveBackAfterGive;

	public Transform tripodTransform;

	public GameObject underwaterEffect;

	public PickUpNotification closeWindowNotification;

	public PickUpNotification hideHudNotification;

	public bool usingTripod;

	public UnityStandardAssets.CinematicEffects.DepthOfField def;

	private bool givingToNpc;

	private PostOnBoard givingPost;

	private int placingInFrameNo = -1;

	public bool canMoveCam = true;

	private bool hidingHud;

	private int showingPhotosFrom;

	private int showingBlowUp;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		def = photoCamera.GetComponent<UnityStandardAssets.CinematicEffects.DepthOfField>();
		displayedPhotos = new PhotoDetails[MuseumManager.manage.paintingsOnDisplay.Length];
		RenderTexture renderTexture = new RenderTexture(resWidth, resHeight, 24);
		photoCamera.targetTexture = renderTexture;
		showPhotoPos.texture = renderTexture;
		photoFog = photoCamera.GetComponent<GlobalFog>();
		cameraZoomSound = photoCamera.GetComponent<AudioSource>();
	}

	public void resetMaterialsToSeeThrough()
	{
		for (int i = 0; i < WeatherManager.Instance.windMgr.windyMaterials.Length; i++)
		{
			if (i != 4 && i != 1)
			{
				WeatherManager.Instance.windMgr.windyMaterials[i].SetFloat("_MaxDistance", 6f);
			}
		}
	}

	public void doDepthOfFieldOnUp()
	{
		if (def.blur.farRadius > 0f && Vector3.Angle(Vector3.up, photoCamera.transform.forward) < 80f)
		{
			def.blur.farRadius = Mathf.Clamp(def.blur.farRadius - Time.deltaTime * 10f, 0f, 10f);
		}
		else if (def.blur.farRadius < 10f)
		{
			def.blur.farRadius = Mathf.Clamp(def.blur.farRadius + Time.deltaTime * 10f, 0f, 10f);
		}
	}

	public void openCameraView(Transform tripodPos = null, bool isDrone = false)
	{
		tripodTransform = tripodPos;
		usingTripod = tripodPos;
		takePhotoFlashEffect.enabled = false;
		cameraViewOpen = true;
		cameraWindow.SetActive(cameraViewOpen);
		photoCamera.gameObject.SetActive(cameraViewOpen);
		CameraController.control.mainCamera.gameObject.SetActive(!cameraViewOpen);
		showPhotoPos.gameObject.SetActive(cameraViewOpen);
		if (tripodPos != null)
		{
			photoCamera.transform.position = tripodPos.position;
		}
		StartCoroutine(cameraOpen(tripodPos, isDrone));
		TileHighlighter.highlight.off = true;
	}

	public void closeCameraView()
	{
		takePhotoFlashEffect.enabled = false;
		cameraViewOpen = false;
		cameraWindow.SetActive(cameraViewOpen);
		photoCamera.gameObject.SetActive(cameraViewOpen);
		CameraController.control.mainCamera.gameObject.SetActive(!cameraViewOpen);
		showPhotoPos.gameObject.SetActive(cameraViewOpen);
		TileHighlighter.highlight.off = false;
		if (hidingHud)
		{
			swapShowingHud();
		}
	}

	public bool isGivingToNPC()
	{
		return givingToNpc;
	}

	public void openPhotoTab(bool showingNPC = false, int selectingForFrame = -1)
	{
		placingInFrameNo = selectingForFrame;
		givingToNpc = showingNPC;
		if (showingNPC)
		{
			hideWindowOnGive();
			MenuButtonsTop.menu.subMenuButtonsWindow.gameObject.SetActive(value: false);
			showPhotoButton.gameObject.SetActive(value: true);
			if (selectingForFrame == -1)
			{
				givingPost = BulletinBoard.board.checkMissionsCompletedForNPC(ConversationManager.manage.lastConversationTarget.GetComponent<NPCIdentity>().NPCNo);
				GiveNPC.give.givingPost = givingPost;
			}
		}
		else
		{
			showPhotoButton.gameObject.SetActive(value: false);
			givingPost = null;
		}
		photoTabOpen = true;
		photoTabWindow.gameObject.SetActive(photoTabOpen);
		closeBlownUpWindow();
	}

	public void populatePhotoButtons()
	{
		for (int i = 0; i < savedPhotos.Count; i++)
		{
			InvPhoto component = Object.Instantiate(photoPreviewPrefab, photoButtonSpawnPos).GetComponent<InvPhoto>();
			component.fillPhotoImage(loadPhoto(savedPhotos[i].photoName), i);
			invPhotoFrames.Add(component);
			component.transform.SetAsFirstSibling();
		}
	}

	public void createNewButtonForPhoto()
	{
		InvPhoto component = Object.Instantiate(photoPreviewPrefab, photoButtonSpawnPos).GetComponent<InvPhoto>();
		component.fillPhotoImage(loadPhoto(savedPhotos[savedPhotos.Count - 1].photoName), savedPhotos.Count - 1);
		invPhotoFrames.Add(component);
		component.transform.SetAsFirstSibling();
	}

	public void giveButtonPress()
	{
		if (placingInFrameNo == -1)
		{
			if (givingPost.checkIfPhotoShownIsCorrect(savedPhotos[showingBlowUp]))
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givingPost.getPostPostsById().onGivenItemConvo);
			}
			else
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givingPost.getPostPostsById().onGivenWrongPhotoConvo);
			}
			givingPost = null;
		}
		else
		{
			MuseumManager.manage.donatePhoto(placingInFrameNo, showingBlowUp);
		}
		closeWhenGive();
		showPhotoButton.gameObject.SetActive(value: false);
		closePhotoTab();
		MenuButtonsTop.menu.closeSubMenu();
	}

	public void closeWhenGive()
	{
		if (givingToNpc)
		{
			givingToNpc = false;
			windowToMove.SetParent(moveBackAfterGive);
			windowToMove.localPosition = Vector2.zero;
			moveToWhenGive.gameObject.SetActive(value: false);
		}
	}

	public void hideWindowOnGive()
	{
		windowToMove.SetParent(moveToWhenGive);
		windowToMove.localPosition = Vector2.zero;
		moveToWhenGive.gameObject.SetActive(value: true);
	}

	public void letNPCKeepPhoto()
	{
		savedPhotos.RemoveAt(showingBlowUp);
		Object.Destroy(invPhotoFrames[showingBlowUp].gameObject);
		invPhotoFrames.RemoveAt(showingBlowUp);
		for (int i = 0; i < invPhotoFrames.Count; i++)
		{
			invPhotoFrames[i].updatePhotoId(i);
		}
	}

	public void closePhotoTab()
	{
		if (givingToNpc)
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, noPhotoGivenConvo);
			closeWhenGive();
			givingPost = null;
			showPhotoButton.gameObject.SetActive(value: false);
		}
		photoTabOpen = false;
		closeBlownUpWindow();
		photoTabWindow.gameObject.SetActive(photoTabOpen);
		blownUpWindow.gameObject.SetActive(value: false);
	}

	public static string ScreenShotName(int width, int height, string name)
	{
		SaveLoad.saveOrLoad.createPhotoDir();
		return $"{SaveLoad.saveOrLoad.saveSlot()}/Photos/{name}.png";
	}

	public string getPhotoDateAndTime()
	{
		return WorldManager.Instance.year + "." + WorldManager.Instance.month + "." + WorldManager.Instance.week + "." + WorldManager.Instance.day + "." + RealWorldTimeLight.time.currentHour + "." + RealWorldTimeLight.time.currentMinute;
	}

	private void waitAndReactivateViewfinder()
	{
		photoCamera.targetTexture = photoTexture;
		showPhotoPos.texture = photoTexture;
		RenderTexture.active = photoTexture;
		photoCamera.enabled = false;
		photoCamera.enabled = true;
		canMoveCam = true;
	}

	public void showingHud(bool showHud)
	{
		GraphicRaycaster[] casters = Inventory.Instance.casters;
		for (int i = 0; i < casters.Length; i++)
		{
			casters[i].GetComponent<Canvas>().enabled = showHud;
		}
	}

	public void swapShowingHud()
	{
		hidingHud = !hidingHud;
		if (hidingHud)
		{
			photoCamera.targetTexture = null;
			showingHud(showHud: false);
			return;
		}
		showingHud(showHud: true);
		photoTexture = new RenderTexture(resWidth, resHeight, 24);
		photoCamera.targetTexture = photoTexture;
		showPhotoPos.texture = photoTexture;
	}

	private IEnumerator cameraOpen(bool onTripod = false, bool isDrone = false)
	{
		float yRot = 10f;
		float zoom = 0f;
		float desiredZoom = 0f;
		for (int i = 0; i < WeatherManager.Instance.windMgr.windyMaterials.Length; i++)
		{
			if (i != 4)
			{
				WeatherManager.Instance.windMgr.windyMaterials[i].SetFloat("_MaxDistance", 0f);
			}
		}
		while (cameraViewOpen)
		{
			if (!isDrone && InputMaster.input.Use())
			{
				if (hidingHud)
				{
					swapShowingHud();
				}
				else
				{
					takePhoto();
					canMoveCam = false;
					SoundManager.Instance.play2DSound(SoundManager.Instance.cameraSound);
					NetworkMapSharer.Instance.localChar.CmdTakePhotoSound(photoCamera.transform.position);
					yield return StartCoroutine(takePhotoEffects());
					waitAndReactivateViewfinder();
				}
			}
			if (Inventory.Instance.usingMouse)
			{
				closeWindowNotification.fillButtonPrompt(ConversationGenerator.generate.GetToolTip("Tip_CloseCamera"), Input_Rebind.RebindType.Other);
				hideHudNotification.fillButtonPrompt(ConversationGenerator.generate.GetToolTip("Tip_HideHud"), Input_Rebind.RebindType.SwapCameraMode);
			}
			else
			{
				closeWindowNotification.fillButtonPrompt(ConversationGenerator.generate.GetToolTip("Tip_CloseCamera"), closeWindowNotification.controllerY);
				hideHudNotification.fillButtonPrompt(ConversationGenerator.generate.GetToolTip("Tip_HideHud"), hideHudNotification.controllerRightStick);
			}
			if (InputMaster.input.Other() || InputMaster.input.Journal() || InputMaster.input.OpenInventory() || InputMaster.input.OpenMap())
			{
				closeCameraView();
				Inventory.Instance.quickBarLocked(isLocked: false);
			}
			if (usingTripod && Vector3.Distance(NetworkMapSharer.Instance.localChar.transform.position, photoCamera.transform.position) > 60f)
			{
				closeCameraView();
				Inventory.Instance.quickBarLocked(isLocked: false);
			}
			if (InputMaster.input.SwapCamera())
			{
				swapShowingHud();
			}
			if (WorldManager.Instance.isPositionOnMap(photoCamera.transform.position))
			{
				if (WorldManager.Instance.waterMap[Mathf.RoundToInt(photoCamera.transform.position.x / 2f), Mathf.RoundToInt(photoCamera.transform.position.z / 2f)] && photoCamera.transform.position.y <= 0.6f)
				{
					underwaterEffect.SetActive(value: true);
				}
				else
				{
					underwaterEffect.SetActive(value: false);
				}
			}
			else if (photoCamera.transform.position.y <= 0.6f)
			{
				underwaterEffect.SetActive(value: true);
			}
			else
			{
				underwaterEffect.SetActive(value: false);
			}
			if (canMoveCam)
			{
				doDepthOfFieldOnUp();
				if (InputMaster.input.RBHeld())
				{
					desiredZoom = Mathf.Clamp(desiredZoom + 50f * Time.deltaTime, 0f, 60f);
				}
				if (InputMaster.input.LBHeld())
				{
					desiredZoom = Mathf.Clamp(desiredZoom - 50f * Time.deltaTime, 0f, 60f);
				}
				desiredZoom = Mathf.Clamp(desiredZoom + InputMaster.input.getScrollWheel() * Time.deltaTime, 0f, 60f);
				zoom = Mathf.Lerp(zoom, desiredZoom, Time.deltaTime * 10f);
				photoCamera.fieldOfView = 70f - zoom;
				if (isDrone)
				{
					photoCamera.transform.position = tripodTransform.position;
					photoCamera.transform.rotation = tripodTransform.rotation;
				}
				else
				{
					float num = 0f - InputMaster.input.getRightStick().y;
					float num2 = 0f - InputMaster.input.getMousePosOld().y;
					if (Inventory.Instance.usingMouse)
					{
						num = num2;
					}
					yRot = Mathf.Clamp(yRot + num, -25f, 45f);
					if (!onTripod)
					{
						photoCamera.transform.position = new Vector3(CameraController.control.transform.position.x, NetworkMapSharer.Instance.localChar.myEquip.holdPos.position.y, CameraController.control.transform.position.z) + Vector3.up * 0.85f + CameraController.control.transform.forward * 1.75f;
					}
					photoCamera.transform.rotation = CameraController.control.transform.rotation * Quaternion.Euler(yRot, 0f, 0f);
					if (Mathf.Abs(desiredZoom - zoom) > 1.5f)
					{
						AudioSource audioSource = cameraZoomSound;
						float volume = (cameraZoomSound.volume = Mathf.Lerp(0.5f * SoundManager.Instance.GetGlobalSoundVolume(), 0f, Time.deltaTime * 50f));
						audioSource.volume = volume;
						cameraZoomSound.pitch = Mathf.Clamp(zoom / 30f + 2f, 2f, 4f);
						cameraZoomSound.Play();
					}
					else
					{
						cameraZoomSound.volume = Mathf.Lerp(cameraZoomSound.volume, 0f, Time.deltaTime * 10f);
						if (cameraZoomSound.volume <= 0.05f)
						{
							cameraZoomSound.Stop();
						}
					}
				}
			}
			float y = InputMaster.input.getLeftStick().y;
			float x = InputMaster.input.getLeftStick().x;
			if (!onTripod && y == 0f && x == 0f)
			{
				NetworkMapSharer.Instance.localChar.transform.rotation = Quaternion.Lerp(NetworkMapSharer.Instance.localChar.transform.rotation, CameraController.control.transform.rotation, Time.deltaTime * 3f);
			}
			yield return null;
		}
		resetMaterialsToSeeThrough();
		cameraZoomSound.Stop();
	}

	public bool isGameObjectVisible(GameObject target)
	{
		if ((bool)target.GetComponentInChildren<Renderer>() && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(photoCamera), target.GetComponentInChildren<Renderer>().bounds))
		{
			return true;
		}
		return false;
	}

	public List<PhotographedObject> getPhotosContents()
	{
		List<PhotographedObject> list = new List<PhotographedObject>();
		List<Transform> list2 = new List<Transform>();
		if (Physics.Raycast(photoCamera.transform.position, photoCamera.transform.forward, out var hitInfo, 12f) && !list2.Contains(hitInfo.transform.root) && isGameObjectVisible(hitInfo.transform.root.gameObject))
		{
			list2.Add(hitInfo.transform.root);
			checkTransform(hitInfo.transform.root, list);
		}
		Collider[] array = Physics.OverlapSphere(photoCamera.transform.position + photoCamera.transform.forward * 25.5f, 25f, myPhotosInterest);
		for (int i = 0; i < array.Length; i++)
		{
			if (!list2.Contains(array[i].transform.root) && isGameObjectVisible(array[i].transform.root.gameObject))
			{
				list2.Add(array[i].transform.root);
				checkTransform(array[i].transform.root, list);
			}
		}
		return list;
	}

	public void takePhoto()
	{
		photoTexture = new RenderTexture(resWidth, resHeight, 24);
		photoCamera.targetTexture = photoTexture;
		Texture2D texture2D = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, mipChain: false);
		photoCamera.Render();
		RenderTexture.active = photoTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, resWidth, resHeight), 0, 0);
		string photoDateAndTime = getPhotoDateAndTime();
		PhotoDetails photoDetails = new PhotoDetails(photoDateAndTime, getPhotosContents(), NetworkMapSharer.Instance.localChar.transform.position, 1);
		savedPhotos.Add(photoDetails);
		byte[] bytes = texture2D.EncodeToJPG();
		File.WriteAllBytes(ScreenShotName(resWidth, resHeight, photoDateAndTime), bytes);
		BulletinBoard.board.checkAllMissionsForPhotoRequestsOnNewPhoto(photoDetails);
		createNewButtonForPhoto();
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.Photographer);
	}

	private IEnumerator takePhotoEffects()
	{
		float progress = 0f;
		takePhotoFlashEffect.enabled = true;
		takePhotoFlashEffect.color = Color.white;
		while (progress <= 1f)
		{
			takePhotoFlashEffect.color = Color.Lerp(Color.white, Color.clear, progress);
			progress += Time.deltaTime;
			yield return null;
		}
		takePhotoFlashEffect.enabled = false;
	}

	public Texture2D loadPhoto(string photoName)
	{
		Texture2D texture2D = null;
		if (File.Exists(ScreenShotName(resWidth, resHeight, photoName)))
		{
			byte[] data = File.ReadAllBytes(ScreenShotName(resWidth, resHeight, photoName));
			texture2D = new Texture2D(resWidth, resHeight);
			texture2D.LoadImage(data);
			previousPhotoFrame.texture = texture2D;
		}
		return texture2D;
	}

	public Texture2D loadPhotoFromByteArray(byte[] array)
	{
		Texture2D texture2D = new Texture2D(resWidth, resHeight);
		texture2D.LoadImage(array);
		return texture2D;
	}

	public byte[] getByteArrayForTransfer(string photoName)
	{
		if (File.Exists(ScreenShotName(resWidth, resHeight, photoName)))
		{
			return File.ReadAllBytes(ScreenShotName(resWidth, resHeight, photoName));
		}
		return null;
	}

	public void setPhotoTabCloseButtonAsLastCloseButton()
	{
		if (!isGivingToNPC())
		{
			Inventory.Instance.setAsLastActiveCloseButton(tabCloseButton);
		}
	}

	public void blowUpImage(int frameNo)
	{
		photoButtonSpawnPos.gameObject.SetActive(value: false);
		showingBlowUp = frameNo;
		blownUpPhoto.fillPhotoImage(loadPhoto(savedPhotos[showingBlowUp].photoName), showingBlowUp);
		blownUpWindow.SetActive(value: true);
		blownUpPhotoText.text = manage.savedPhotos[frameNo].getIslandName() + "\n";
		TextMeshProUGUI textMeshProUGUI = blownUpPhotoText;
		textMeshProUGUI.text = textMeshProUGUI.text + manage.savedPhotos[frameNo].getDateString() + "\n" + manage.savedPhotos[frameNo].getTimeString();
		if (savedPhotos[showingBlowUp].photoNickname == "" || savedPhotos[showingBlowUp].photoNickname == null)
		{
			savedPhotos[showingBlowUp].photoNickname = ConversationGenerator.generate.GetJournalNameByTag("UntitledPhotoName");
		}
		photoNameField.text = savedPhotos[showingBlowUp].photoNickname;
		photoNamePlaceHolder.text = savedPhotos[showingBlowUp].photoNickname;
	}

	public void renamePhoto()
	{
		savedPhotos[showingBlowUp].photoNickname = photoNameField.text;
		invPhotoFrames[showingBlowUp].updatePhotoId(showingBlowUp);
	}

	public void closeBlownUpWindow()
	{
		setPhotoTabCloseButtonAsLastCloseButton();
		photoButtonSpawnPos.gameObject.SetActive(value: true);
		blownUpWindow.SetActive(value: false);
	}

	public void closeBlownUpWindowAndSaveName()
	{
		renamePhoto();
		closeBlownUpWindow();
	}

	public void deleteBlownUpImage()
	{
		savedPhotos.RemoveAt(showingBlowUp);
		Object.Destroy(invPhotoFrames[showingBlowUp].gameObject);
		invPhotoFrames.RemoveAt(showingBlowUp);
		for (int i = 0; i < invPhotoFrames.Count; i++)
		{
			invPhotoFrames[i].updatePhotoId(i);
		}
		closeBlownUpWindow();
	}

	public void donatePhoto(int photoId)
	{
		savedPhotos.RemoveAt(photoId);
		Object.Destroy(invPhotoFrames[photoId].gameObject);
		invPhotoFrames.RemoveAt(photoId);
		for (int i = 0; i < invPhotoFrames.Count; i++)
		{
			invPhotoFrames[i].updatePhotoId(i);
		}
	}

	public void checkTransform(Transform transformToCheck, List<PhotographedObject> inPicture)
	{
		AnimalAI componentInParent = transformToCheck.GetComponentInParent<AnimalAI>();
		if ((bool)componentInParent)
		{
			inPicture.Add(new PhotographedObject(componentInParent));
		}
		NPCIdentity componentInParent2 = transformToCheck.GetComponentInParent<NPCIdentity>();
		if ((bool)componentInParent2)
		{
			inPicture.Add(new PhotographedObject(componentInParent2));
		}
		EquipItemToChar componentInParent3 = transformToCheck.GetComponentInParent<EquipItemToChar>();
		if ((bool)componentInParent3)
		{
			inPicture.Add(new PhotographedObject(componentInParent3));
		}
		PickUpAndCarry componentInParent4 = transformToCheck.GetComponentInParent<PickUpAndCarry>();
		if ((bool)componentInParent4)
		{
			inPicture.Add(new PhotographedObject(componentInParent4));
		}
	}
}
