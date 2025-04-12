using System.Collections;
using UnityEngine;
using UnityStandardAssets.CinematicEffects;
using UnityStandardAssets.ImageEffects;

public class CameraController : MonoBehaviour
{
	public static CameraController control;

	public int freeCamYLimitMax = 140;

	public int freeCamYLimitMaxInsideBuildings = 50;

	public int freeCamYLimitMin = -30;

	public int aimCamYLimitMax = 140;

	public int aimCamYLimitMin = -30;

	private float aimCamDistance = 2f;

	private float defaultCamDistance = 12f;

	private float vehicleFollowCamDistance = 18f;

	private Vehicle followingVehicle;

	public Camera mainCamera;

	public CameraShake myShake;

	public int playerNo = 1;

	private Transform followTransform;

	public Transform Camera_Y;

	public Transform cameraTrans;

	private float Yrotation = 35f;

	public UnityStandardAssets.CinematicEffects.DepthOfField def;

	private float camDistance = 12f;

	public NewChunkLoader newChunkLoader;

	public cameraWonderOnMenu wondering;

	private bool conversationCam;

	private float followSpeed = 0.15f;

	public bool inCar;

	private bool cameraLocked = true;

	public AudioSource riverNoiseAudio;

	public AudioSource oceanNoiseAudio;

	public AudioSource landAmbientAudio;

	public AudioClip riverSound;

	public AudioClip oceanSound;

	public FadeBlackness blackFadeAnim;

	private bool lockCameraForTransition;

	private bool freeCamOn;

	private bool aimCamOn;

	private Vector3 freeCamPos = Vector3.zero;

	private float turnSpeed;

	private bool freeCam;

	private bool zoomOnChar;

	private float savedCamDistance = 12f;

	private bool distanceLock;

	private CharMovement followingChar;

	public bool flyCamOn;

	public GameObject undergroundSoundZone;

	private Transform targetfacing;

	public GlobalFog fogScript;

	public CameraHudHint cameraHint;

	public int xMod = 1;

	public int YMod = 1;

	public bool toggle;

	public LayerMask cameraCollisions;

	public Vector3 freePosition;

	public Quaternion freeRotation;

	public RectTransform waterEffect;

	public RectTransform underWaterEffect;

	public RectTransform waterCanvas;

	public GameObject waterUnderSide;

	public float horizontalSpeed = 2f;

	public float verticleSpeed = 2f;

	public Material waterBlur;

	private Coroutine underWaterRoutine;

	private Vector3 cameraLocalPosition;

	private float farPlaneNo = 7.6f;

	public float foggyDayRollInDistance;

	public float normalFogDistance;

	private Vector3 followVel = Vector3.one;

	public float landAmbienceMax = 0.25f;

	public AudioClip dayTimeAmbience;

	public AudioClip undergroundAmbience;

	public AudioClip lavaUndergroundAmbience;

	public AudioClip nightTimeAmbience;

	private bool inFreeViewBeforeAim;

	private RaycastHit cameraHit;

	private float distance = 7f;

	private float maxDistance = 12f;

	private float maxSpeed = 10f;

	public bool cameraShowingSomething;

	public float checkDistance = 0.3f;

	private void Awake()
	{
		control = this;
	}

	private void OnDestroy()
	{
		control = null;
		RenderSettings.fog = false;
	}

	private void Start()
	{
		StartCoroutine(cameraSwitchControl());
		StartCoroutine(playAmbientSounds());
		foggyDayRollInDistance = (float)(20 * NewChunkLoader.loader.getChunkDistance()) - 20f;
	}

	private IEnumerator cameraSwitchControl()
	{
		while (Inventory.Instance.menuOpen || TownManager.manage.firstConnect)
		{
			yield return null;
		}
		updateCameraSwitchPrompt();
		while (true)
		{
			if (!ChatBox.chat.chatOpen && !Inventory.Instance.isMenuOpen())
			{
				cameraHint.gameObject.SetActive(value: true);
				if (InputMaster.input.SwapCamera() && !ConversationManager.manage.IsConversationActive && !PhotoManager.manage.cameraViewOpen)
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.cameraSwitch);
					swapFreeCam();
					updateCameraSwitchPrompt();
				}
			}
			else
			{
				cameraHint.gameObject.SetActive(value: false);
			}
			yield return null;
		}
	}

	public void setCamDistanceForDeedPlacement()
	{
		camDistance = 12f;
	}

	public void swapFlyCam(bool followCam = true)
	{
		if (flyCamOn)
		{
			stopFlyCam();
		}
		else
		{
			startFlyCam(followCam);
		}
	}

	private void startFlyCam(bool follow)
	{
		flyCamOn = true;
		cameraLocalPosition = cameraTrans.localPosition;
		cameraTrans.parent = null;
		cameraTrans.GetComponent<camerFlys>().charFollows = follow;
		cameraTrans.GetComponent<camerFlys>().enabled = true;
		def.focus.transform = null;
		def.enabled = false;
		myShake.enabled = false;
		if (follow)
		{
			NetworkMapSharer.Instance.localChar.lockCharOnFreeCam();
		}
		SetFollowTransform(cameraTrans);
		_ = freePosition != Vector3.zero;
	}

	private void stopFlyCam()
	{
		flyCamOn = false;
		myShake.enabled = true;
		cameraTrans.parent = Camera_Y;
		cameraTrans.localRotation = Quaternion.Euler(new Vector3(50f, 0f, 0f));
		cameraTrans.localPosition = cameraLocalPosition;
		cameraTrans.GetComponent<camerFlys>().enabled = false;
		def.focus.transform = base.transform;
		SetFollowTransform(NetworkMapSharer.Instance.localChar.transform);
		NetworkMapSharer.Instance.localChar.unlocklockCharOnFreeCam();
	}

	public void clearFreeCam()
	{
		freePosition = Vector3.zero;
	}

	public void saveFreeCam()
	{
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosX", cameraTrans.transform.position.x);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosY", cameraTrans.transform.position.y);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosZ", cameraTrans.transform.position.z);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotX", cameraTrans.transform.eulerAngles.x);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotY", cameraTrans.transform.eulerAngles.y);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotZ", cameraTrans.transform.eulerAngles.z);
	}

	public void loadFreeCam()
	{
		freePosition = new Vector3(PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosX"), PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosY"), PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosZ"));
		freeRotation = Quaternion.Euler(PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotX"), PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotY"), PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotZ"));
		if (flyCamOn)
		{
			cameraTrans.position = freePosition;
			cameraTrans.rotation = freeRotation;
		}
	}

	public void updateCameraSwitchPrompt()
	{
		cameraHint.updateIcon(freeCam);
	}

	public void updateDepthOfFieldAndFog(int newChunkDistance)
	{
		normalFogDistance = (float)(20 * newChunkDistance) - 20f;
		RenderSettings.fogStartDistance = 0f;
		farPlaneNo = 7.6f - (float)(newChunkDistance - 4) * 1.9f;
		if (RealWorldTimeLight.time.underGround)
		{
			if (RealWorldTimeLight.time.mineLevel == 2)
			{
				fogScript.startDistance = 40f;
				RenderSettings.fogEndDistance = 80f;
			}
			else
			{
				fogScript.startDistance = 15f;
				RenderSettings.fogEndDistance = 40f;
			}
			RealWorldTimeLight.time.waterMat.SetFloat("_FogDistance", 25f);
		}
		else
		{
			if (foggyDayRollInDistance < normalFogDistance)
			{
				fogScript.startDistance = foggyDayRollInDistance;
				RenderSettings.fog = true;
			}
			else
			{
				fogScript.startDistance = normalFogDistance;
				RenderSettings.fog = false;
			}
			RenderSettings.fogEndDistance = (float)(20 * newChunkDistance) - 20f + 25f;
			RealWorldTimeLight.time.waterMat.SetFloat("_FogDistance", 25 * newChunkDistance);
		}
		def.focus.farPlane = farPlaneNo;
	}

	public void SetFogUnderground(bool isUnderground)
	{
		fogScript.excludeFarPixels = !isUnderground;
		PhotoManager.manage.photoFog.excludeFarPixels = !isUnderground;
	}

	public bool isFreeCamOn()
	{
		return freeCam;
	}

	public void swapFreeCam()
	{
		freeCam = !freeCam;
		if (freeCam)
		{
			cameraTrans.localPosition = new Vector3(0f, 8f, -8f);
			Camera_Y.localRotation = Quaternion.Euler(0f, 0f, 0f);
			Camera_Y.localPosition = new Vector3(0f, 1f, 0f);
			def.focus.farPlane = farPlaneNo;
			def.focus.fStops = 5.2f;
			def.focus.nearPlane = 0.05f;
			newChunkLoader.transform.localPosition = new Vector3(0f, 0f, 20f);
			if ((bool)WeatherManager.Instance && !WeatherManager.Instance.IsMyPlayerInside)
			{
				fogScript.enabled = true;
			}
			def.blur.farRadius = 10f;
		}
		else
		{
			Camera_Y.localPosition = new Vector3(0f, 0f, 0f);
			Camera_Y.localRotation = Quaternion.Euler(0f, 0f, 0f);
			newChunkLoader.transform.localPosition = Vector3.zero;
		}
	}

	public void faceTarget(Transform target)
	{
		targetfacing = target;
	}

	public AudioClip GetUndergroundAmbienceClip(int level)
	{
		if (level == 2)
		{
			return lavaUndergroundAmbience;
		}
		return undergroundAmbience;
	}

	private void LateUpdate()
	{
		if ((bool)followTransform && followTransform.parent != null)
		{
			if (aimCamOn)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, followTransform.position + followTransform.right * 1.5f + Vector3.up, ref followVel, followSpeed);
			}
			else
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, followTransform.position, ref followVel, followSpeed * 2f);
			}
		}
	}

	private void FixedUpdate()
	{
		if (!followTransform || lockCameraForTransition)
		{
			return;
		}
		if (followTransform.parent == null)
		{
			if (aimCamOn)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, followTransform.position + followTransform.right * 1.5f + Vector3.up, ref followVel, followSpeed / 10f);
			}
			else
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, followTransform.position, ref followVel, followSpeed);
			}
		}
		controlCam();
	}

	public void SetFollowTransform(Transform followthis, float newFollowSpeed = 0.15f)
	{
		followSpeed = newFollowSpeed;
		followTransform = followthis;
		followingChar = followthis.GetComponentInParent<CharMovement>();
		if ((bool)followingChar && underWaterRoutine == null)
		{
			underWaterRoutine = StartCoroutine(CameraUnderWaterEffects());
		}
	}

	public void moveToFollowing(bool slowFade = false)
	{
		base.transform.position = followTransform.position;
		if (slowFade)
		{
			blackFadeAnim.fadeTime = 1f;
			blackFadeAnim.fadeOut();
		}
		else
		{
			blackFadeAnim.fadeTime = 0.5f;
			blackFadeAnim.fadeOut();
		}
	}

	public void shakeScreen(float trauma0to1)
	{
		myShake.addToTrauma(trauma0to1);
	}

	public void shakeScreenMax(float trauma0to1, float max)
	{
		myShake.addToTraumaMax(trauma0to1, max);
	}

	public bool isInAimCam()
	{
		return aimCamOn;
	}

	public void enterAimCamera(bool turnOnCrossHeir = true)
	{
		if (!aimCamOn)
		{
			inFreeViewBeforeAim = freeCam;
			if (!freeCam)
			{
				swapFreeCam();
			}
			if (turnOnCrossHeir)
			{
				Crossheir.cross.turnOnCrossheir();
			}
			else
			{
				Crossheir.cross.turnOffCrossheir();
			}
			maxDistance = aimCamDistance;
		}
		aimCamOn = true;
	}

	public void exitAimCamera()
	{
		if (aimCamOn)
		{
			Crossheir.cross.turnOffCrossheir();
			aimCamOn = false;
			if (freeCam != inFreeViewBeforeAim)
			{
				swapFreeCam();
			}
			StartCoroutine(SmoothZoomOutAimCam());
			Cursor.lockState = CursorLockMode.Confined;
		}
	}

	private IEnumerator SmoothZoomOutAimCam()
	{
		float timer = 0f;
		float startingMaxDistance = maxDistance;
		while (timer < 0.25f && !aimCamOn)
		{
			timer += Time.deltaTime;
			maxDistance = Mathf.Lerp(startingMaxDistance, getCurrentCamMaxDistance(), timer);
			yield return null;
		}
		if (!aimCamOn)
		{
			maxDistance = getCurrentCamMaxDistance();
		}
	}

	public float getCurrentCamMaxDistance()
	{
		if ((bool)followingVehicle && followingVehicle.useCameraVehicleDistance)
		{
			return vehicleFollowCamDistance;
		}
		if (aimCamOn)
		{
			return aimCamDistance;
		}
		return defaultCamDistance;
	}

	public void ConnectVehicle(Vehicle newVehicle)
	{
		followingVehicle = newVehicle;
		maxDistance = getCurrentCamMaxDistance();
	}

	private void controlCam()
	{
		if (aimCamOn)
		{
			Vector3 forward = cameraTrans.position + cameraTrans.forward * 100f - base.transform.position;
			forward.y = 0f;
			Quaternion b = Quaternion.LookRotation(forward);
			followTransform.root.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * 250f);
		}
		if (flyCamOn)
		{
			base.transform.rotation = Quaternion.Euler(0f, cameraTrans.eulerAngles.y, 0f);
			return;
		}
		if (Inventory.Instance.CanMoveCharacter() && !conversationCam && !zoomOnChar && !RenderMap.Instance.mapOpen)
		{
			float num = InputMaster.input.getMousePosOld().x;
			float num2 = 0f - InputMaster.input.getMousePosOld().y;
			if (!Inventory.Instance.usingMouse || (num == 0f && num2 == 0f))
			{
				num = InputMaster.input.getRightStick().x;
				num2 = 0f - InputMaster.input.getRightStick().y;
			}
			if (InputMaster.input.TriggerLookHeld() || PhotoManager.manage.cameraViewOpen || aimCamOn || toggle)
			{
				if (PhotoManager.manage.cameraViewOpen && !PhotoManager.manage.canMoveCam)
				{
					num = 0f;
					num2 = 0f;
				}
			}
			else if (Inventory.Instance.usingMouse)
			{
				num = 0f;
				num2 = 0f;
			}
			if (!cameraLocked && !Inventory.Instance.invOpen)
			{
				num = InputMaster.input.getMousePosOld().x;
				num2 = InputMaster.input.getMousePosOld().y;
			}
			if (freeCam)
			{
				num2 *= (float)YMod;
				checkCameraForCollisions();
			}
			num *= (float)xMod;
			float num3 = horizontalSpeed * num;
			turnSpeed = Mathf.Lerp(turnSpeed, num3, Time.deltaTime * 15f);
			if (!targetfacing || num3 != 0f)
			{
				base.transform.Rotate(0f, turnSpeed, 0f);
			}
			else
			{
				Quaternion b2 = Quaternion.LookRotation((base.transform.position - new Vector3(targetfacing.position.x, base.transform.position.y, targetfacing.position.z)).normalized);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b2, Time.deltaTime * 8f);
			}
			if (freeCam)
			{
				Yrotation += num2 * Time.deltaTime * (55f * verticleSpeed);
				if (RealWorldTimeLight.time.IsLocalPlayerInside)
				{
					Yrotation = Mathf.Clamp(Yrotation, freeCamYLimitMin, freeCamYLimitMaxInsideBuildings);
				}
				else if (aimCamOn)
				{
					Yrotation = Mathf.Clamp(Yrotation, aimCamYLimitMin, aimCamYLimitMax);
				}
				else
				{
					Yrotation = Mathf.Clamp(Yrotation, freeCamYLimitMin, freeCamYLimitMax);
				}
				Camera_Y.localEulerAngles = Vector3.left * Yrotation;
			}
			else if (!distanceLock)
			{
				if (!Physics.Raycast(cameraTrans.position, cameraTrans.forward * num2, out cameraHit, 1f, cameraCollisions))
				{
					camDistance = Mathf.Clamp(camDistance + num2 / 5f, 5f, 15f);
				}
				cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * camDistance - Vector3.forward * camDistance, Time.deltaTime * 10f);
			}
			else
			{
				cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * camDistance - Vector3.forward * camDistance, Time.deltaTime);
			}
		}
		if (ConversationManager.manage.IsConversationActive && !conversationCam && !ConversationManager.manage.lastConversationTarget.isSign)
		{
			StartCoroutine(zoomInConvo());
		}
		doDepthOfFieldBlur();
	}

	private void doDepthOfFieldBlur()
	{
		if (ConversationManager.manage.IsConversationActive && !conversationCam && !ConversationManager.manage.lastConversationTarget.isSign)
		{
			if (def.blur.farRadius != 10f)
			{
				def.blur.farRadius = 10f;
			}
		}
		else if (aimCamOn)
		{
			def.blur.farRadius = Mathf.Clamp(def.blur.farRadius - Time.deltaTime * 10f, 0f, 10f);
		}
		else if (def.blur.farRadius > 0f && Vector3.Angle(base.transform.forward, Camera_Y.forward) > 45f)
		{
			def.blur.farRadius = Mathf.Clamp(def.blur.farRadius - Time.deltaTime * 10f, 0f, 10f);
		}
		else if (def.blur.farRadius < 10f)
		{
			def.blur.farRadius = Mathf.Clamp(def.blur.farRadius + Time.deltaTime * 10f, 0f, 10f);
		}
	}

	public void zoomInOnCharForChair()
	{
		StartCoroutine(ZoomInOnChair());
	}

	public void checkCameraForCollisionsZoom(Vector3 desiredLocalPosition, float amount)
	{
		Vector3 vector = Vector3.Lerp(cameraTrans.localPosition, desiredLocalPosition, amount);
		Vector3 normalized = (Camera_Y.TransformPoint(vector) - Camera_Y.position).normalized;
		float num = Vector3.Distance(Camera_Y.TransformPoint(vector), Camera_Y.position);
		Debug.DrawLine(Camera_Y.position, Camera_Y.position + normalized * num, Color.white);
		if (Physics.Linecast(Camera_Y.position, Camera_Y.position + normalized * num, out var hitInfo, cameraCollisions))
		{
			distance = Mathf.Clamp(hitInfo.distance * 0.87f, 0.5f, num);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(normalized * distance), amount);
		}
		else
		{
			cameraTrans.localPosition = vector;
		}
	}

	private IEnumerator ZoomInOnChair()
	{
		zoomOnChar = true;
		Vector3 originalLocalPosition = cameraTrans.localPosition;
		Quaternion originalRotation = base.transform.rotation;
		_ = followTransform.eulerAngles;
		Vector3 desiredPos = base.transform.position - followTransform.forward - base.transform.position;
		Vector3 camYPos = Camera_Y.transform.localPosition;
		Quaternion camYRot = Camera_Y.localRotation;
		if (!freeCam)
		{
			def.focus.fStops = 32f;
		}
		def.enabled = true;
		float velocity = 0f;
		float amount = 0f;
		float smoother = 1f;
		while (HairDresserMenu.menu.hairMenuOpen)
		{
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, Quaternion.Euler(-20f, 0f, 0f), amount);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, new Vector3(0f, 1f, 0f), amount);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(desiredPos), amount);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * 5f - Vector3.forward * 5f, amount);
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 2.9f, amount);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, 14.6f, amount);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 4.4f, amount);
			amount = Mathf.SmoothDamp(amount, 1f, ref velocity, smoother);
			float value = smoother + Time.deltaTime * 10f;
			smoother = Mathf.Clamp(value, 1f, 25f);
		}
		for (float returnTo = 0f; returnTo < 1f; returnTo += Mathf.Clamp01(Time.deltaTime * 2f))
		{
			if (HairDresserMenu.menu.hairMenuOpen)
			{
				break;
			}
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, camYRot, returnTo);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, originalRotation, returnTo);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, camYPos, returnTo);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, originalLocalPosition, returnTo);
			if (!freeCam)
			{
				def.focus.fStops = Mathf.Lerp(def.focus.fStops, 32f, returnTo);
				continue;
			}
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 5.2f, returnTo);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, farPlaneNo, returnTo);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 0.05f, returnTo);
		}
		if (!freeCam)
		{
			def.enabled = false;
		}
		Camera_Y.localRotation = camYRot;
		Camera_Y.localPosition = camYPos;
		zoomOnChar = false;
	}

	public IEnumerator zoomOnCharacterInventory()
	{
		zoomOnChar = true;
		Vector3 originalLocalPosition = cameraTrans.localPosition;
		Quaternion originalRotation = base.transform.rotation;
		_ = followTransform.eulerAngles;
		Vector3 desiredPos = base.transform.position - followTransform.forward - base.transform.position;
		while (Inventory.Instance.invOpen || HairDresserMenu.menu.hairMenuOpen || BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
		{
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, Quaternion.Euler(-20f, 0f, 0f), Time.deltaTime * 4f);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, new Vector3(0f, 1f, 2f), Time.deltaTime * 3f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(desiredPos), Time.deltaTime * 6f);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * 5f - Vector3.forward * 5f, Time.deltaTime * 6f);
			yield return null;
		}
		for (float returnTo = 0f; returnTo < 1f; returnTo += Time.deltaTime)
		{
			if (Inventory.Instance.invOpen)
			{
				break;
			}
			if (HairDresserMenu.menu.hairMenuOpen)
			{
				break;
			}
			if (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
			{
				break;
			}
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Quaternion.Euler(-20f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), returnTo);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, originalRotation, returnTo);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, originalLocalPosition, returnTo);
			Camera_Y.localPosition = Vector3.Lerp(new Vector3(-3f, 1f, 2f), new Vector3(0f, 0f, 0f), returnTo);
		}
		Camera_Y.localRotation = Quaternion.Euler(0f, 0f, 0f);
		zoomOnChar = false;
	}

	public IEnumerator zoomInFishOrBug()
	{
		zoomOnChar = true;
		Vector3 originalLocalPosition = cameraTrans.localPosition;
		Quaternion originalRotation = base.transform.rotation;
		_ = followTransform.eulerAngles;
		Vector3 desiredPos = base.transform.position - followTransform.forward - base.transform.position;
		Vector3 camYPos = Camera_Y.transform.localPosition;
		Quaternion camYRot = Camera_Y.localRotation;
		if (!freeCam)
		{
			def.focus.fStops = 32f;
		}
		def.enabled = true;
		float velocity = 0f;
		float amount = 0f;
		float smoother = 1f;
		while (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen || (ConversationManager.manage.IsConversationActive && ConversationManager.manage.lastConversationTarget.isSign))
		{
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, Quaternion.Euler(-20f, 0f, 0f), amount);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, new Vector3(0f, 1f, 0f), amount);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(desiredPos), amount);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * 5f - Vector3.forward * 5f, amount);
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 2.9f, amount);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, 14.6f, amount);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 4.4f, amount);
			amount = Mathf.SmoothDamp(amount, 1f, ref velocity, smoother);
			float value = smoother + Time.deltaTime * 10f;
			smoother = Mathf.Clamp(value, 1f, 25f);
		}
		for (float returnTo = 0f; returnTo < 1f; returnTo += Mathf.Clamp01(Time.deltaTime * 2f))
		{
			if (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
			{
				break;
			}
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, camYRot, returnTo);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, originalRotation, returnTo);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, camYPos, returnTo);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, originalLocalPosition, returnTo);
			if (!freeCam)
			{
				def.focus.fStops = Mathf.Lerp(def.focus.fStops, 32f, returnTo);
				continue;
			}
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 5.2f, returnTo);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, farPlaneNo, returnTo);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 0.05f, returnTo);
		}
		if (!freeCam)
		{
			def.enabled = false;
		}
		Camera_Y.localRotation = camYRot;
		Camera_Y.localPosition = camYPos;
		zoomOnChar = false;
	}

	private IEnumerator zoomInConvo()
	{
		conversationCam = true;
		Vector3 originalLocalPosition = cameraTrans.localPosition;
		Quaternion originalRotation = base.transform.rotation;
		_ = followTransform.eulerAngles;
		_ = Vector3.zero;
		Vector3 camYPos = Camera_Y.transform.localPosition;
		Quaternion camYRot = Camera_Y.localRotation;
		Vector3 desiredLocalPosition = Vector3.up * 4f - Vector3.forward * 3f;
		Vector3 desiredPos2;
		if (Vector3.Distance(followTransform.position + followTransform.right + followTransform.forward, cameraTrans.position) < Vector3.Distance(followTransform.position - followTransform.right + followTransform.forward, cameraTrans.position))
		{
			desiredPos2 = followTransform.forward - followTransform.right;
			desiredLocalPosition += Vector3.right * Mathf.Clamp(Vector3.Distance(base.transform.position, ConversationManager.manage.lastConversationTarget.transform.position), 0f, 0f);
		}
		else
		{
			desiredPos2 = followTransform.forward + followTransform.right;
			desiredLocalPosition -= Vector3.right * Mathf.Clamp(Vector3.Distance(base.transform.position, ConversationManager.manage.lastConversationTarget.transform.position), 0f, 0f);
		}
		desiredPos2 = new Vector3(desiredPos2.x, 0f, desiredPos2.z);
		if (!freeCam)
		{
			def.focus.fStops = 32f;
		}
		def.enabled = true;
		float velocity = 0f;
		float amount = 0f;
		float smoother = 3f;
		while ((ConversationManager.manage.IsConversationActive || GiveNPC.give.giveWindowOpen) && !zoomOnChar)
		{
			yield return null;
			amount = Mathf.SmoothDamp(amount, 1f, ref velocity, smoother);
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, Quaternion.Euler(-20f, 0f, 0f), amount);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, new Vector3(0f, 1f, 0f), amount);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(desiredPos2), amount);
			checkCameraForCollisionsZoom(desiredLocalPosition, amount);
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 2.9f, amount);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, 14.6f, amount);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 4.4f, amount);
			float value = smoother + Time.deltaTime * 10f;
			smoother = Mathf.Clamp(value, 3f, 35f);
		}
		float returnTo = 0f;
		while (returnTo < 1f && !ConversationManager.manage.IsConversationActive && !GiveNPC.give.giveWindowOpen && !zoomOnChar)
		{
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, camYRot, returnTo);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, originalRotation, returnTo);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, camYPos, returnTo);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, originalLocalPosition, returnTo);
			returnTo += Time.deltaTime / 1.5f;
			if (!freeCam)
			{
				def.focus.fStops = Mathf.Lerp(def.focus.fStops, 32f, returnTo);
				continue;
			}
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 5.2f, returnTo);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, farPlaneNo, returnTo);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 0.05f, returnTo);
		}
		if (!freeCam)
		{
			def.enabled = false;
		}
		Camera_Y.localRotation = camYRot;
		Camera_Y.localPosition = camYPos;
		conversationCam = false;
	}

	public void startFishing()
	{
		distanceLock = true;
		savedCamDistance = camDistance;
		StartCoroutine("fishingZoomIn");
	}

	public void stopFishing()
	{
		if (distanceLock)
		{
			camDistance = savedCamDistance;
			distanceLock = false;
		}
	}

	public void zoomInOnAnimalHouse()
	{
		if (!distanceLock)
		{
			savedCamDistance = camDistance;
			distanceLock = true;
		}
		camDistance = 6f;
		cameraTrans.localPosition = Vector3.up * 9f - Vector3.forward * 9f;
	}

	public void stopZoomInOnAnimalHouse()
	{
		if (distanceLock)
		{
			camDistance = savedCamDistance;
			distanceLock = false;
		}
	}

	private IEnumerator fishingZoomIn()
	{
		while (distanceLock && Vector3.Distance(base.transform.position, followTransform.position) > 6f)
		{
			yield return null;
		}
		if (distanceLock)
		{
			camDistance = 5f;
		}
	}

	public void showOffPos(int xPos, int yPos)
	{
		StartCoroutine(moveCameraToShowPos(xPos, yPos));
	}

	public void moveCameraPointerToTileObject(int xPos, int yPos)
	{
		if (!WorldManager.Instance.isPositionOnMap(xPos, yPos))
		{
			return;
		}
		int num = WorldManager.Instance.rotationMap[xPos, yPos];
		if (WorldManager.Instance.onTileMap[xPos, yPos] > -1)
		{
			TileObject tileObject = WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]];
			switch (num)
			{
			case 1:
				base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				FarmAnimalMenu.menu.selectorTrans.position = new Vector3(xPos * 2 + 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2 + 2);
				break;
			case 2:
				base.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
				FarmAnimalMenu.menu.selectorTrans.position = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2 + tileObject.GetXSize());
				break;
			case 3:
				base.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
				FarmAnimalMenu.menu.selectorTrans.position = new Vector3(xPos * 2 + tileObject.GetXSize(), WorldManager.Instance.heightMap[xPos, yPos], yPos * 2 + tileObject.GetYSize());
				break;
			case 4:
				base.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
				FarmAnimalMenu.menu.selectorTrans.position = new Vector3(xPos * 2 + tileObject.GetYSize() + 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2 + 2);
				break;
			}
		}
	}

	public void checkCameraForCollisions()
	{
		Vector3 vector = -mainCamera.transform.forward;
		Debug.DrawLine(Camera_Y.position, Camera_Y.position + vector * (maxDistance + 4f), Color.blue);
		if (Physics.Linecast(Camera_Y.position, Camera_Y.position + vector * (maxDistance + 4f), out cameraHit, cameraCollisions))
		{
			distance = Mathf.Clamp(cameraHit.distance * 0.87f, 0.5f, maxDistance);
			if (cameraHit.normal.y > 0.9f && cameraHit.normal.y < 1.1f)
			{
				cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(vector * distance) + Vector3.up, Time.deltaTime * maxSpeed * 2f);
			}
			else
			{
				cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(vector * distance), Time.deltaTime * maxSpeed * 2f);
			}
		}
		else
		{
			distance = maxDistance;
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(vector * distance), Time.deltaTime * maxSpeed);
		}
	}

	public void checkCameraForCollisionsOld()
	{
		Vector3 normalized = (Vector3.up / 1.5f - Camera_Y.forward).normalized;
		Debug.DrawLine(Camera_Y.position, Camera_Y.position + normalized * maxDistance, Color.blue);
		if (Physics.Linecast(Camera_Y.position, Camera_Y.position + normalized * maxDistance, out cameraHit, cameraCollisions))
		{
			distance = Mathf.Clamp(cameraHit.distance * 0.87f, 0.5f, maxDistance);
		}
		else
		{
			distance = maxDistance;
		}
		cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(normalized * distance), Time.deltaTime * 15f);
	}

	private IEnumerator moveCameraToShowPos(int xpos, int yPos)
	{
		if (flyCamOn)
		{
			yield break;
		}
		while (!NetworkMapSharer.Instance.nextDayIsReady)
		{
			yield return null;
		}
		cameraShowingSomething = true;
		bool isFreeCam = freeCam;
		Quaternion startingRot = base.transform.rotation;
		float startingZoom = camDistance;
		if (isFreeCam)
		{
			swapFreeCam();
		}
		if (WeatherManager.Instance.IsMyPlayerInside)
		{
			RealWorldTimeLight.time.goOutside();
		}
		NetworkMapSharer.Instance.localChar.attackLockOn(isOn: true);
		bool isKinimeaticNow = NetworkMapSharer.Instance.localChar.GetComponent<Rigidbody>().isKinematic;
		NetworkMapSharer.Instance.localChar.GetComponent<Rigidbody>().isKinematic = true;
		moveCameraPointerToTileObject(xpos, yPos);
		SetFollowTransform(FarmAnimalMenu.menu.selectorTrans);
		base.transform.position = followTransform.position;
		NewChunkLoader.loader.forceInstantUpdateAtPos();
		float timer = 4f;
		camDistance = 12f;
		while (CharLevelManager.manage.levelUpWindowOpen)
		{
			yield return null;
		}
		if (WorldManager.Instance.onTileStatusMap[xpos, yPos] > 0)
		{
			if (NPCManager.manage.npcStatus[WorldManager.Instance.onTileStatusMap[xpos, yPos] - 1].hasMet)
			{
				NotificationManager.manage.makeTopNotification(string.Format(ConversationGenerator.generate.GetNotificationText("NameVisiting"), NPCManager.manage.NPCDetails[WorldManager.Instance.onTileStatusMap[xpos, yPos] - 1].GetNPCName()));
			}
			else
			{
				NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("SomeoneVisiting"));
			}
		}
		else
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NoOneVisiting"));
		}
		while (timer > 0f)
		{
			base.transform.Rotate(Vector3.up, 0.05f);
			camDistance += 0.01f;
			timer -= Time.deltaTime;
			yield return null;
		}
		if (WeatherManager.Instance.IsMyPlayerInside)
		{
			RealWorldTimeLight.time.goInside();
		}
		SetFollowTransform(NetworkMapSharer.Instance.localChar.transform);
		NetworkMapSharer.Instance.localChar.attackLockOn(isOn: false);
		control.moveToFollowing(slowFade: true);
		NewChunkLoader.loader.forceInstantUpdateAtPos();
		yield return null;
		if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails != null)
		{
			HouseManager.manage.findHousesOnDisplay(NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.xPos, NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.yPos).refreshHouseTiles(firstTime: true);
		}
		NetworkMapSharer.Instance.localChar.GetComponent<Rigidbody>().isKinematic = isKinimeaticNow;
		base.transform.rotation = startingRot;
		camDistance = startingZoom;
		cameraTrans.localPosition = Vector3.up * camDistance - Vector3.forward * camDistance;
		if (isFreeCam)
		{
			swapFreeCam();
		}
		cameraShowingSomething = false;
	}

	public void lockCamera(bool locked)
	{
		lockCameraForTransition = locked;
	}

	private IEnumerator playAmbientSounds()
	{
		while (Inventory.Instance.menuOpen)
		{
			yield return null;
		}
		while (true)
		{
			if ((bool)RealWorldTimeLight.time && !RealWorldTimeLight.time.underGround)
			{
				if (base.transform.position.y < -5f)
				{
					riverNoiseAudio.volume = 0f;
					oceanNoiseAudio.volume = 0f;
				}
				else
				{
					if ((bool)followingChar && followingChar.underWater)
					{
						riverNoiseAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.1f, Time.deltaTime * 3f);
						oceanNoiseAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.1f, Time.deltaTime * 3f);
						landAmbientAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.1f, Time.deltaTime * 3f);
					}
					else
					{
						riverNoiseAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.8f, Time.deltaTime * 3f);
						oceanNoiseAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.8f, Time.deltaTime * 3f);
						landAmbientAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 1f, Time.deltaTime * 3f);
					}
					if ((float)NewChunkLoader.loader.riverTilesInCharChunk >= 45f)
					{
						riverNoiseAudio.volume = Mathf.Lerp(riverNoiseAudio.volume, 0.07f * Mathf.Clamp(NewChunkLoader.loader.waterTilesNearChar, 0f, 500f) / 500f * SoundManager.Instance.GetGlobalSoundVolume(), Time.deltaTime * 3f);
					}
					else
					{
						riverNoiseAudio.volume = Mathf.Lerp(riverNoiseAudio.volume, 0.035f * Mathf.Clamp(NewChunkLoader.loader.waterTilesNearChar, 0f, 500f) / 500f * SoundManager.Instance.GetGlobalSoundVolume(), Time.deltaTime * 3f);
					}
					oceanNoiseAudio.volume = Mathf.Lerp(oceanNoiseAudio.volume, 0.1f * Mathf.Clamp(NewChunkLoader.loader.oceanTilesNearChar, 0f, 800f) / 800f * SoundManager.Instance.GetGlobalSoundVolume(), Time.deltaTime * 3f);
					landAmbientAudio.volume = Mathf.Lerp(landAmbientAudio.volume, landAmbienceMax * (1f - Mathf.Clamp(NewChunkLoader.loader.oceanTilesNearChar + NewChunkLoader.loader.waterTilesNearChar, 0f, 1000f) / 1000f) * SoundManager.Instance.GetGlobalSoundVolume(), Time.deltaTime * 3f);
				}
			}
			else
			{
				landAmbientAudio.volume = Mathf.Lerp(landAmbientAudio.volume, landAmbienceMax * SoundManager.Instance.GetGlobalSoundVolume(), Time.deltaTime * 3f);
				riverNoiseAudio.volume = 0f;
				oceanNoiseAudio.volume = 0f;
			}
			yield return null;
		}
	}

	public bool IsCloseToCamera(Vector3 checkPos)
	{
		if ((checkPos - base.transform.position).sqrMagnitude < 225f)
		{
			return true;
		}
		return false;
	}

	public bool IsCloseToCamera50(Vector3 checkPos)
	{
		if ((checkPos - base.transform.position).sqrMagnitude < 2500f)
		{
			return true;
		}
		return false;
	}

	public bool wearingDivingHelmet()
	{
		if (EquipWindow.equip.hatSlot.itemNo == NetworkMapSharer.Instance.localChar.divingHelmet.getItemId())
		{
			return true;
		}
		return false;
	}

	private IEnumerator CameraUnderWaterEffects()
	{
		Fisheye wobble = mainCamera.GetComponent<Fisheye>();
		while (true)
		{
			if (!WeatherManager.Instance.IsMyPlayerInside && cameraTrans.position.y <= 1f && WorldManager.Instance.waterMap[Mathf.RoundToInt(cameraTrans.position.x / 2f), Mathf.RoundToInt(cameraTrans.position.z / 2f)])
			{
				waterEffect.gameObject.SetActive(value: true);
				wobble.enabled = true;
				waterUnderSide.SetActive(value: true);
				float randomWobbleX = 0f;
				float randomWobbleY = 0f;
				while (!WeatherManager.Instance.IsMyPlayerInside && cameraTrans.position.y <= 1f && WorldManager.Instance.waterMap[Mathf.RoundToInt(cameraTrans.position.x / 2f), Mathf.RoundToInt(cameraTrans.position.z / 2f)])
				{
					Vector3 position = mainCamera.transform.position + mainCamera.transform.forward * 0.3f;
					position.y = 0.61f;
					float y = Mathf.Clamp(Mathf.Abs(((Vector2)mainCamera.WorldToScreenPoint(position)).y / (float)Screen.height) * waterCanvas.sizeDelta.y, 0f, Screen.height);
					waterEffect.sizeDelta = new Vector2(waterEffect.sizeDelta.x, y);
					if (wearingDivingHelmet())
					{
						wobble.strengthX = 0f;
						wobble.strengthY = 0f;
						waterBlur.SetFloat("_Size", 0.1f);
					}
					else
					{
						wobble.strengthX = Mathf.Lerp(wobble.strengthX, randomWobbleX, Time.deltaTime);
						wobble.strengthY = Mathf.Lerp(wobble.strengthY, randomWobbleY, Time.deltaTime);
						waterBlur.SetFloat("_Size", 1f);
					}
					if (Mathf.Abs(wobble.strengthX - randomWobbleX) <= 0.01f)
					{
						randomWobbleX = Random.Range(0.05f, 0.35f);
					}
					if (Mathf.Abs(wobble.strengthY - randomWobbleY) <= 0.01f)
					{
						randomWobbleY = Random.Range(0.05f, 0.35f);
					}
					yield return null;
				}
				wobble.enabled = false;
				waterEffect.gameObject.SetActive(value: false);
				waterUnderSide.SetActive(value: false);
			}
			else if (waterEffect.gameObject.activeInHierarchy)
			{
				wobble.enabled = false;
				waterEffect.gameObject.SetActive(value: false);
				waterUnderSide.SetActive(value: false);
			}
			yield return null;
		}
	}
}
