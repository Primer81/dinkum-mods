using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CinematicEffects;

public class StatusManager : MonoBehaviour
{
	public enum BuffType
	{
		healthRegen,
		staminaRegen,
		fullBuff,
		miningBuff,
		loggingBuff,
		huntingBuff,
		farmingBuff,
		fishingBuff,
		defenceBuff,
		speedBuff,
		xPBuff,
		sickness,
		swimBuff,
		sitting,
		sleepless,
		wellrested,
		diligent,
		charged,
		healthTickSpeedIncrease,
		fireResistance
	}

	public static StatusManager manage;

	public GameObject reviveWindow;

	public GameObject reviveButton;

	public Transform heartWindow;

	public GameObject heartContainerPrefab;

	public HeartContainer[] allContainers;

	public Transform statusWindow;

	public InvSlotAnimator healthBubbleBounce;

	public InvSlotAnimator statusBubbleBounce;

	public ASound faintSound;

	public Image healthBar;

	public Image staminaBar;

	public Damageable connectedDamge;

	public ASound heartHealSound;

	public TonemappingColorGrading worldColor;

	private float stamina = 50f;

	private float staminaMax = 50f;

	public bool tired;

	public bool dead;

	private Animator playerAnim;

	private float changespeed = 1f;

	private TonemappingColorGrading.ColorGradingSettings worldColorSettings;

	public ASound lowHealthSound;

	public GameObject healthBarToHide;

	public GameObject staminaBarToHide;

	public RectTransform staminaBarRect;

	public RectTransform healthBarRect;

	public ConversationObject faintedOnFirstDayConvoSO;

	public GameObject lateTiredStatusIcon;

	public Image damageVignette;

	private Coroutine damageRoutine;

	[Header("Fullness -------------")]
	public RectTransform fullnessIconRect;

	public RectTransform healthTickIconRect;

	public Image fullnessIcon;

	public Image healthTickIcon;

	public TextMeshProUGUI healthTickAmount;

	public Image healthTickAmountBack;

	public Sprite[] fullnessSpriteStages;

	public int currentFullness;

	public Sprite[] buffIconsSprite;

	public Sprite buffLevel2Sprite;

	public Sprite buffLevel3Sprite;

	public int snagsEaten;

	private Color staminaDefaultColour;

	private float staminaSignBounceTimer;

	public GameObject staminaRing;

	public Image staminaRingFill;

	public StoredFoodType[] eatenFoods;

	public BuffIcon[] buffIcons;

	public string[] buffNames;

	public string[] buffDescs;

	public BuffIcon foodTickIcon;

	public BuffIcon staminaTickIcon;

	private float damageAmountShown;

	private float addFullnessAmount = 120f;

	private bool staminaPunishment;

	private float stopStaminaRegenTimer = 0.8f;

	private float stopStaminaRegenTimerMax = 0.8f;

	private int staminaDrainCounter = 2;

	private float healthRegenTimer;

	private Coroutine fillExtraBarRoutine;

	private Buff[] currentBuffs = new Buff[5];

	private static WaitForSeconds sec = new WaitForSeconds(1f);

	private static WaitForSeconds tick = new WaitForSeconds(1f);

	private float staminaRegenFromFood;

	private int healthRegenFromFood;

	private void Awake()
	{
		worldColorSettings = worldColor.colorGrading;
		worldColor.colorGrading = worldColorSettings;
		manage = this;
	}

	private void Start()
	{
		currentBuffs = new Buff[Enum.GetValues(typeof(BuffType)).Length];
		staminaDefaultColour = staminaBar.color;
		StartCoroutine(lateTiredStatus());
		StartCoroutine(FullnessStatus());
		SetBuffIconText();
	}

	public void SetBuffIconText()
	{
		foodTickIcon.SetUpBuffIcon(foodTickIcon.icon.sprite, ConversationGenerator.generate.GetBuffNameText(0), ConversationGenerator.generate.GetBuffDescText(0));
		staminaTickIcon.SetUpBuffIcon(staminaTickIcon.icon.sprite, ConversationGenerator.generate.GetBuffNameText(1), ConversationGenerator.generate.GetBuffDescText(1));
		for (int i = 0; i < buffIconsSprite.Length; i++)
		{
			buffIcons[i].SetUpBuffIcon(buffIconsSprite[i], ConversationGenerator.generate.GetBuffNameText(i), ConversationGenerator.generate.GetBuffDescText(i));
		}
	}

	private void Update()
	{
		if (!connectedDamge)
		{
			return;
		}
		if (!dead && connectedDamge.health <= 0)
		{
			die();
		}
		changeFillAmount(healthBar, (float)connectedDamge.health / (float)connectedDamge.maxHealth);
		changeFillAmount(staminaBar, stamina / staminaMax);
		if (stamina < staminaMax && Inventory.Instance.CanMoveCharacter())
		{
			if (!OptionsMenu.options.staminaWheelHidden)
			{
				staminaRing.SetActive(value: true);
			}
			changeFillAmount(staminaRingFill, stamina / staminaMax);
		}
		else
		{
			staminaRing.SetActive(value: false);
		}
		if (staminaPunishment)
		{
			staminaBar.color = Color.Lerp(Color.red, staminaDefaultColour, staminaBar.fillAmount);
		}
		else
		{
			staminaBar.color = staminaDefaultColour;
		}
		if (staminaSignBounceTimer < 0.5f)
		{
			staminaSignBounceTimer += Time.deltaTime;
		}
	}

	private void LateUpdate()
	{
		if (!OptionsMenu.options.staminaWheelHidden && stamina < staminaMax && (bool)connectedDamge)
		{
			staminaRing.transform.position = CameraController.control.mainCamera.WorldToScreenPoint(NetworkMapSharer.Instance.localChar.charRendererTransform.transform.position + Vector3.up * 2.5f + CameraController.control.mainCamera.transform.right * 1.5f);
		}
	}

	public void changeFillAmount(Image toFill, float fillToShow)
	{
		if (toFill.fillAmount != fillToShow)
		{
			if (toFill.fillAmount < fillToShow)
			{
				toFill.fillAmount = Mathf.Clamp(toFill.fillAmount + Time.deltaTime * 4f, 0f, fillToShow);
			}
			else
			{
				toFill.fillAmount = Mathf.Clamp(toFill.fillAmount - Time.deltaTime * 4f, fillToShow, 1f);
			}
		}
	}

	public void takeDamageUIChanges(int amountTaken)
	{
		healthBubbleBounce.UpdateSlotContents();
		if (connectedDamge.health != connectedDamge.maxHealth)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TakeDamage, amountTaken);
			addToDamageVignette((float)amountTaken * 3f);
			InputMaster.input.doRumble(Mathf.Clamp((float)amountTaken / 10f, 0f, 0.75f));
		}
		healthRegenTimer = 0f;
	}

	public bool isTooFull()
	{
		if (getBuffLevel(BuffType.sickness) >= 1)
		{
			return true;
		}
		return currentFullness == 3 + Mathf.Clamp(snagsEaten, 0, 2);
	}

	private IEnumerator lateTiredStatus()
	{
		while (true)
		{
			yield return null;
			if (RealWorldTimeLight.time.currentHour == 0 && NetworkMapSharer.Instance.nextDayIsReady)
			{
				lateTiredStatusIcon.SetActive(value: true);
				while (RealWorldTimeLight.time.currentHour == 0 && NetworkMapSharer.Instance.nextDayIsReady)
				{
					lateTiredStatusIcon.transform.localPosition = staminaBarRect.localPosition + new Vector3(staminaBarRect.sizeDelta.x + 25f, 0f, 0f);
					if (getBuffLevel(BuffType.sleepless) == 0)
					{
						if (staminaMax != 10f)
						{
							addTempPoints(0, 0f);
						}
						if (stamina > 10f)
						{
							changeStamina(-0.05f);
						}
					}
					else
					{
						if (staminaMax != 30f)
						{
							addTempPoints(0, 0f);
						}
						if (stamina > 30f)
						{
							changeStamina(-0.05f);
						}
					}
					yield return null;
				}
				lateTiredStatusIcon.SetActive(value: false);
			}
			if (!NetworkMapSharer.Instance.nextDayIsReady)
			{
				lateTiredStatusIcon.SetActive(value: false);
				while (!NetworkMapSharer.Instance.nextDayIsReady)
				{
					yield return null;
				}
				lateTiredStatusIcon.SetActive(value: false);
			}
		}
	}

	private IEnumerator takeDamageRoutine()
	{
		Color colourToSet = damageVignette.color;
		damageVignette.enabled = true;
		while (damageAmountShown > 0f)
		{
			damageAmountShown = Mathf.Clamp(damageAmountShown - Time.deltaTime * 25f, 0f, 100f);
			colourToSet.a = damageAmountShown / 100f;
			damageVignette.color = colourToSet;
			yield return null;
		}
		damageVignette.enabled = false;
		damageRoutine = null;
	}

	public void addToDamageVignette(float amount)
	{
		damageAmountShown += amount;
		if (damageRoutine == null)
		{
			StartCoroutine(takeDamageRoutine());
		}
	}

	public void AddToFullness()
	{
		currentFullness++;
		currentFullness = Mathf.Clamp(currentFullness, 0, 3 + Mathf.Clamp(snagsEaten, 0, 2));
	}

	public void RemoveFullness()
	{
		currentFullness--;
		currentFullness = Mathf.Clamp(currentFullness, 0, 3 + Mathf.Clamp(snagsEaten, 0, 2));
	}

	private IEnumerator FullnessStatus()
	{
		while (true)
		{
			yield return null;
			fullnessIconRect.transform.localPosition = staminaBarRect.localPosition + new Vector3(staminaBarRect.sizeDelta.x, 0f, 0f);
			healthTickIconRect.transform.localPosition = healthBarRect.localPosition + new Vector3(healthBarRect.sizeDelta.x, 0f, 0f);
			fullnessIcon.fillAmount = (float)currentFullness / (3f + (float)Mathf.Clamp(snagsEaten, 0, 2));
			if ((bool)NetworkMapSharer.Instance.localChar)
			{
				HandleStaminaRegen();
				HandleHealthRegen();
			}
			if (currentFullness <= 0)
			{
			}
		}
	}

	private IEnumerator ShowHealthTickAmountNextToHealthTickPie()
	{
		float timer2 = 0f;
		healthTickAmount.text = "+" + healthRegenFromFood;
		Color healthTickTextColor = healthTickAmount.color;
		Color healthTickBackColor = healthTickAmountBack.color;
		for (; timer2 < 1f; timer2 += Time.deltaTime * 2.5f)
		{
			healthTickTextColor.a = Mathf.Lerp(0f, 1f, timer2);
			healthTickAmount.color = healthTickTextColor;
			healthTickBackColor.a = Mathf.Lerp(0f, 1f, timer2);
			healthTickAmountBack.color = healthTickBackColor;
			yield return null;
		}
		healthTickTextColor.a = 1f;
		healthTickAmount.color = healthTickTextColor;
		healthTickBackColor.a = 1f;
		healthTickAmountBack.color = healthTickBackColor;
		for (timer2 = 0f; timer2 < 1f; timer2 += Time.deltaTime * 2f)
		{
			healthTickTextColor.a = Mathf.Lerp(1f, 0f, timer2);
			healthTickAmount.color = healthTickTextColor;
			healthTickBackColor.a = Mathf.Lerp(1f, 0f, timer2);
			healthTickAmountBack.color = healthTickBackColor;
			yield return null;
		}
		healthTickTextColor.a = 0f;
		healthTickAmount.color = healthTickTextColor;
		healthTickBackColor.a = 0f;
		healthTickAmountBack.color = healthTickBackColor;
	}

	public void HandleHealthRegen()
	{
		if (connectedDamge.health < connectedDamge.maxHealth && connectedDamge.health != 0)
		{
			if (healthRegenFromFood > 0)
			{
				if (healthRegenTimer > 20f)
				{
					NetworkMapSharer.Instance.localChar.CmdGiveHealthBack(healthRegenFromFood);
					StartCoroutine(ShowHealthTickAmountNextToHealthTickPie());
					healthRegenTimer = 0f;
					healthTickIcon.fillAmount = 1f;
					return;
				}
				healthRegenTimer += Time.deltaTime + Time.deltaTime / 2f * (float)getBuffLevel(BuffType.healthTickSpeedIncrease);
				if (NetworkMapSharer.Instance.localChar.myPickUp.sitting)
				{
					addBuff(BuffType.sitting, 2, 1);
					healthRegenTimer += Time.deltaTime * 4f;
				}
				healthTickIcon.fillAmount = healthRegenTimer / 20f;
			}
			else
			{
				healthRegenTimer = 0f;
				healthTickIcon.fillAmount = 0f;
			}
		}
		else
		{
			healthRegenTimer = 0f;
			healthTickIcon.fillAmount = 0f;
		}
	}

	public void HandleStaminaRegen()
	{
		if (stamina == 0f && staminaDrainCounter <= 0)
		{
			staminaPunishment = true;
		}
		float num = 1f + 1f * (1f - stamina / staminaMax);
		float num2 = staminaRegenFromFood + 0.5f;
		if (staminaPunishment && RealWorldTimeLight.time.currentHour == 0 && getBuffLevel(BuffType.sleepless) == 0)
		{
			num2 = 2f;
		}
		else if (staminaPunishment)
		{
			num2 /= 1.5f;
		}
		else if (getBuffLevel(BuffType.sickness) != 0)
		{
			num2 = 0.1f;
		}
		float num3 = Time.deltaTime * (num2 * num);
		if (staminaPunishment && stamina >= Mathf.Clamp(staminaMax, 0f, 50f))
		{
			staminaPunishment = false;
			changeStamina(0f);
		}
		if (stopStaminaRegenTimer > 0f)
		{
			stopStaminaRegenTimer -= Time.deltaTime;
		}
		else
		{
			if (staminaPunishment)
			{
				num3 /= 2f;
			}
			if (stamina < staminaMax)
			{
				if (RealWorldTimeLight.time.currentHour == 0 && getBuffLevel(BuffType.sleepless) == 0)
				{
					num3 = ((!staminaPunishment) ? (num3 / 2f) : (num3 / 5f));
				}
				changeStamina(num3 * 2f);
			}
		}
		if (getBuffLevel(BuffType.staminaRegen) != 0 && stamina < staminaMax)
		{
			changeStamina(Time.deltaTime * (0.5f * (float)getBuffLevel(BuffType.staminaRegen)));
		}
	}

	private void die()
	{
		InputMaster.input.doRumble(0.85f);
		Inventory.Instance.pressActiveBackButton();
		Inventory.Instance.pressActiveBackButton();
		takeDamageUIChanges(100);
		MusicManager.manage.stopMusic();
		SoundManager.Instance.play2DSound(faintSound);
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.Faint);
		dead = true;
		NetworkMapSharer.Instance.localChar.myEquip.equipNewItem(-1);
		NetworkMapSharer.Instance.localChar.myPickUp.dropItemOnPassOut();
		NetworkMapSharer.Instance.localChar.myPickUp.fallOffVehicleOnPassOut();
		stamina = 0f;
		NetworkMapSharer.Instance.localChar.CmdCharFaints();
		reviveWindow.gameObject.SetActive(value: true);
		Inventory.Instance.checkIfWindowIsNeeded();
		ClearFoodAndBuffs(playSound: false);
		ClearWellRested();
	}

	public void changeStamina(float takeOrPlus)
	{
		if (takeOrPlus < 0f)
		{
			if (stamina + takeOrPlus <= 0f)
			{
				staminaDrainCounter--;
			}
			else
			{
				staminaDrainCounter = 2;
			}
			if (staminaPunishment)
			{
				stopStaminaRegenTimer = Mathf.Abs(takeOrPlus) / 150f + stopStaminaRegenTimerMax * 2f;
			}
			else
			{
				stopStaminaRegenTimer = Mathf.Abs(takeOrPlus) / 150f + stopStaminaRegenTimerMax;
			}
		}
		else if (stamina == staminaMax)
		{
			staminaDrainCounter = 2;
		}
		if (staminaSignBounceTimer >= 0.5f && stamina != Mathf.Clamp(stamina + takeOrPlus, 0f, staminaMax))
		{
			statusBubbleBounce.UpdateSlotContents();
			staminaSignBounceTimer = 0f;
		}
		stamina = Mathf.Clamp(stamina + takeOrPlus, 0f, staminaMax);
		if (Mathf.Floor(stamina) != (float)connectedDamge.myChar.stamina)
		{
			if (staminaPunishment)
			{
				if ((float)connectedDamge.myChar.stamina != 0f)
				{
					connectedDamge.myChar.CmdSetNewStamina(0);
				}
			}
			else
			{
				connectedDamge.myChar.CmdSetNewStamina((int)Mathf.Floor(stamina) + 1);
			}
		}
		if (!dead)
		{
			if (!(stamina <= 0f) && ((stamina < 10f && takeOrPlus < 0f) || stamina == 0f))
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.sweatParticles, connectedDamge.transform.root.position + Vector3.up * 1.5f, UnityEngine.Random.Range(5, 10));
			}
			if (staminaPunishment)
			{
				tired = true;
			}
			else
			{
				tired = false;
			}
		}
	}

	public void sweatParticlesNotLocal(Vector3 pos)
	{
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.sweatParticles, pos + Vector3.up * 1.5f, UnityEngine.Random.Range(5, 10));
	}

	public void addTempPoints(int tempHealthDif, float tempStaminaDif)
	{
		if (RealWorldTimeLight.time.currentHour != 0)
		{
			staminaMax = Mathf.Clamp(staminaMax + tempStaminaDif, 50f, 100f);
		}
		else if (NetworkMapSharer.Instance.nextDayIsReady)
		{
			if (getBuffLevel(BuffType.sleepless) != 0)
			{
				staminaMax = 30f;
				if (stamina > staminaMax)
				{
					stamina = staminaMax;
				}
			}
			else
			{
				staminaMax = 10f;
				if (stamina > staminaMax)
				{
					stamina = staminaMax;
				}
			}
		}
		if (tempHealthDif != 0)
		{
			int num = Mathf.Clamp(connectedDamge.maxHealth + tempHealthDif, 50, 100);
			connectedDamge.maxHealth = num;
			connectedDamge.CmdChangeMaxHealth(num);
		}
		if (stamina > staminaMax)
		{
			changeStatus(0, stamina - staminaMax);
		}
		if (connectedDamge.health > connectedDamge.maxHealth)
		{
			changeStatus(connectedDamge.health - connectedDamge.maxHealth, 0f);
		}
		if (fillExtraBarRoutine != null)
		{
			StopCoroutine(fillExtraBarRoutine);
		}
		fillExtraBarRoutine = StartCoroutine(fillExtraBar());
	}

	public void loadNewMaxStaminaAndHealth(float newMaxStam, int newMaxHealth)
	{
		connectedDamge.maxHealth = newMaxHealth;
		connectedDamge.CmdChangeMaxHealth(newMaxHealth);
		staminaMax = newMaxStam;
		if (fillExtraBarRoutine != null)
		{
			StopCoroutine(fillExtraBarRoutine);
		}
		fillExtraBarRoutine = StartCoroutine(fillExtraBar());
	}

	public void changeStatus(int healthChange, float staminaChange)
	{
		changeStamina(staminaChange);
		if (healthChange != 0)
		{
			connectedDamge.CmdChangeHealth(healthChange);
		}
	}

	public void changeHealthTo(int newHealth)
	{
		connectedDamge.CmdChangeHealthTo(newHealth);
	}

	public void nextDayReset()
	{
		staminaMax = 50f;
		connectedDamge.CmdChangeMaxHealth(Mathf.Clamp(50, 50, 100));
		connectedDamge.maxHealth = Mathf.Clamp(50, 50, 100);
		changeStamina(50f);
		changeHealthTo(50);
		healthBarRect.sizeDelta = Vector2.Lerp(healthBarRect.sizeDelta, new Vector2(20 + connectedDamge.maxHealth * 2, 18f), 1f);
		staminaBarRect.sizeDelta = Vector2.Lerp(staminaBarRect.sizeDelta, new Vector2(20f + staminaMax * 2f, 18f), 1f);
		NetworkMapSharer.Instance.localChar.followedBy = -1;
		ClearFoodAndBuffs(playSound: false);
		ClearWellRested();
		if (NetworkMapSharer.Instance.localChar.myInteract.IsInsidePlayerHouse)
		{
			addBuff(BuffType.wellrested, 600, 1);
		}
	}

	public void connectPlayer(Damageable mainPlayerDamage)
	{
		connectedDamge = mainPlayerDamage;
		StartCoroutine(lowHealthCheck());
		statusWindow.gameObject.SetActive(value: true);
		playerAnim = mainPlayerDamage.gameObject.GetComponent<Animator>();
	}

	public void revive()
	{
		StartCoroutine(reviveSelfButton());
	}

	public IEnumerator reviveSelfButton()
	{
		reviveWindow.gameObject.SetActive(value: false);
		NetworkMapSharer.Instance.canUseMineControls = true;
		NetworkMapSharer.Instance.localChar.CmdReviveSelf();
		MusicManager.manage.startMusic();
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
		yield return StartCoroutine(reviveDelay());
		if ((WorldManager.Instance.year != 1 || WorldManager.Instance.month != 1 || WorldManager.Instance.week != 1 || WorldManager.Instance.day != 1) && NetworkNavMesh.nav.getPlayerCount() <= 1 && !RealWorldTimeLight.time.underGround && !RealWorldTimeLight.time.offIsland)
		{
			WorldManager.Instance.nextDay();
		}
	}

	public void getRevivedByOtherChar()
	{
		dead = false;
		playerAnim.SetBool("Fainted", value: false);
		MusicManager.manage.startMusic();
		reviveWindow.gameObject.SetActive(value: false);
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
		NetworkMapSharer.Instance.localChar.GetComponent<Rigidbody>().isKinematic = false;
	}

	private IEnumerator reviveDelay()
	{
		connectedDamge.GetComponent<Rigidbody>().isKinematic = true;
		while (connectedDamge.health <= 0)
		{
			yield return null;
		}
		if (!RealWorldTimeLight.time.underGround && !RealWorldTimeLight.time.offIsland && TownManager.manage.lastSleptPos != Vector3.zero)
		{
			if (TownManager.manage.sleepInsideHouse != null)
			{
				NetworkMapSharer.Instance.localChar.myInteract.ChangeInsideOut(isEntry: true, TownManager.manage.sleepInsideHouse);
				TownManager.manage.savedInside[0] = TownManager.manage.sleepInsideHouse.xPos;
				TownManager.manage.savedInside[1] = TownManager.manage.sleepInsideHouse.yPos;
				WeatherManager.Instance.ChangeToInsideEnvironment(MusicManager.indoorMusic.Default);
				RealWorldTimeLight.time.goInside();
				MusicManager.manage.ChangeCharacterInsideOrOutside(newInside: true, MusicManager.indoorMusic.Default);
				NetworkMapSharer.Instance.localChar.myEquip.setInsideOrOutside(insideOrOut: true, playersHouse: true);
			}
			else
			{
				NetworkMapSharer.Instance.localChar.myInteract.ChangeInsideOut(isEntry: false);
				WeatherManager.Instance.ChangeToOutsideEnvironment();
				RealWorldTimeLight.time.goOutside();
			}
			NetworkMapSharer.Instance.localChar.transform.position = TownManager.manage.lastSleptPos + Vector3.up;
		}
		else if (!RealWorldTimeLight.time.underGround && !RealWorldTimeLight.time.offIsland && NetworkMapSharer.Instance.personalSpawnPoint != Vector3.zero)
		{
			NetworkMapSharer.Instance.localChar.transform.position = NetworkMapSharer.Instance.personalSpawnPoint;
			if (WorldManager.Instance.spawnPos.position.y <= -12f)
			{
				NewChunkLoader.loader.inside = false;
				WeatherManager.Instance.ChangeToOutsideEnvironment();
				RealWorldTimeLight.time.goOutside();
				NetworkMapSharer.Instance.localChar.myEquip.setInsideOrOutside(insideOrOut: false, playersHouse: false);
				MusicManager.manage.ChangeCharacterInsideOrOutside(newInside: true, MusicManager.indoorMusic.Default);
			}
		}
		else
		{
			NetworkMapSharer.Instance.localChar.transform.position = WorldManager.Instance.spawnPos.position;
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
			{
				WeatherManager.Instance.ChangeToInsideEnvironment(MusicManager.indoorMusic.Default);
				RealWorldTimeLight.time.goInside();
				MusicManager.manage.ChangeCharacterInsideOrOutside(newInside: true, MusicManager.indoorMusic.Default);
				NetworkMapSharer.Instance.localChar.myEquip.setInsideOrOutside(insideOrOut: true, playersHouse: false);
			}
		}
		CameraController.control.transform.position = NetworkMapSharer.Instance.localChar.transform.position;
		NewChunkLoader.loader.forceInstantUpdateAtPos();
		if (WorldManager.Instance.year == 1 && WorldManager.Instance.month == 1 && WorldManager.Instance.week == 1 && WorldManager.Instance.day == 1)
		{
			NetworkNavMesh.nav.InstantNavMeshRefresh();
			NetworkMapSharer.Instance.fadeToBlack.setBlack();
			while (!NetworkNavMesh.nav.doesPositionHaveNavChunk(Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.transform.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.Instance.localChar.transform.position.z / 2f)))
			{
				yield return null;
			}
			NPCManager.manage.moveNpcToPlayerAndStartTalking(6, turnplayer: false, faintedOnFirstDayConvoSO);
			if (WorldManager.Instance.spawnPos.position.y <= -12f)
			{
				NPCManager.manage.setNPCInSideBuilding(6, NPCSchedual.Locations.Post_Office);
			}
		}
		if (stamina == 0f)
		{
			stamina = 10f;
			tired = false;
		}
		playerAnim.SetBool("Tired", value: false);
		takeMoneyOnRevive();
		Inventory.Instance.damageAllTools();
		dead = false;
		Inventory.Instance.equipNewSelectedSlot();
		Inventory.Instance.checkIfWindowIsNeeded();
		playerAnim.SetBool("Fainted", value: false);
		yield return null;
		connectedDamge.GetComponent<Rigidbody>().isKinematic = false;
	}

	public void takeMoneyOnRevive()
	{
		Inventory.Instance.changeWallet(-(Inventory.Instance.wallet / 100 * 20));
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			if (Inventory.Instance.invSlots[i].itemNo == Inventory.Instance.moneyItem.getItemId())
			{
				Inventory.Instance.invSlots[i].updateSlotContentsAndRefresh(Inventory.Instance.invSlots[i].itemNo, Inventory.Instance.invSlots[i].stack - Inventory.Instance.invSlots[i].stack / 100 * 20);
			}
		}
	}

	public bool IsStaminaAbove(float aboveThis)
	{
		return stamina > aboveThis;
	}

	public float getStamina()
	{
		return stamina;
	}

	public bool CanSwingWithStamina()
	{
		if (staminaPunishment)
		{
			return !(stopStaminaRegenTimer > 0f);
		}
		return true;
	}

	public float getStaminaMax()
	{
		return staminaMax;
	}

	public void loadStatus(int loadHealth, int loadHealthMax, float loadStamina, float loadStaminaMax)
	{
	}

	public IEnumerator loadStaminaAndHealth(int loadHealth, int loadHealthMax, float loadStamina, float loadStaminaMax)
	{
		while (connectedDamge == null)
		{
			yield return null;
		}
		loadNewMaxStaminaAndHealth(loadStaminaMax, loadHealthMax);
		stamina = loadStamina;
		connectedDamge.CmdChangeHealthTo(loadHealth);
	}

	public void staminaAndHealthBarOn(bool isOn)
	{
		healthBarToHide.SetActive(isOn);
		staminaBarToHide.SetActive(isOn);
		fullnessIconRect.gameObject.SetActive(isOn);
		healthTickIconRect.gameObject.SetActive(isOn);
		if (isOn)
		{
			lateTiredStatusIcon.SetActive(isOn && RealWorldTimeLight.time.currentHour == 0);
		}
		else
		{
			lateTiredStatusIcon.SetActive(value: false);
		}
		QuestTracker.track.pinnedMissionTextOn = isOn;
		QuestTracker.track.updatePinnedTask();
	}

	private IEnumerator lowHealthCheck()
	{
		while (true)
		{
			if ((float)connectedDamge.health <= 10f && !dead)
			{
				float noiseTimer = 2f;
				while ((float)connectedDamge.health <= 10f && !dead)
				{
					noiseTimer += Time.deltaTime;
					if (noiseTimer >= 2f)
					{
						SoundManager.Instance.play2DSound(lowHealthSound);
						healthBubbleBounce.UpdateSlotContents();
						noiseTimer = 0f;
					}
					yield return null;
				}
			}
			yield return null;
		}
	}

	private IEnumerator setTiredColours()
	{
		while (true)
		{
			if (stamina < 10f)
			{
				if (!worldColorSettings.basics.saturation.Equals(stamina / 10f))
				{
					float timer = 0f;
					while (!worldColorSettings.basics.saturation.Equals(stamina / 10f) && stamina < 10f)
					{
						worldColorSettings.basics.saturation = Mathf.Lerp(worldColorSettings.basics.saturation, stamina / 10f, timer);
						worldColor.colorGrading = worldColorSettings;
						timer += Time.deltaTime / 2f;
						yield return null;
					}
				}
			}
			else if (stamina >= 10f && !worldColorSettings.basics.saturation.Equals(1f))
			{
				float timer = 0f;
				while (!worldColorSettings.basics.saturation.Equals(1f) && stamina >= 10f)
				{
					worldColorSettings.basics.saturation = Mathf.Lerp(worldColorSettings.basics.saturation, 1f, timer);
					worldColor.colorGrading = worldColorSettings;
					timer += Time.deltaTime / 4f;
					yield return null;
				}
			}
			yield return null;
		}
	}

	private IEnumerator fillExtraBar()
	{
		float timer = 0f;
		while (timer < 1f)
		{
			timer += Time.deltaTime * 2f;
			healthBarRect.sizeDelta = Vector2.Lerp(healthBarRect.sizeDelta, new Vector2(20 + connectedDamge.maxHealth * 2, 18f), timer);
			staminaBarRect.sizeDelta = Vector2.Lerp(staminaBarRect.sizeDelta, new Vector2(20f + staminaMax * 2f, 18f), timer);
			yield return null;
		}
		healthBarRect.sizeDelta = Vector2.Lerp(healthBarRect.sizeDelta, new Vector2(20 + connectedDamge.maxHealth * 2, 18f), 1f);
		staminaBarRect.sizeDelta = Vector2.Lerp(staminaBarRect.sizeDelta, new Vector2(20f + staminaMax * 2f, 18f), 1f);
		fillExtraBarRoutine = null;
	}

	public void addBuff(BuffType typeToAdd, int time, int level)
	{
		if (currentBuffs[(int)typeToAdd] == null)
		{
			currentBuffs[(int)typeToAdd] = new Buff(time, level);
			showBuffLevel(typeToAdd);
			StartCoroutine(countDownBuff((int)typeToAdd, currentBuffs[(int)typeToAdd]));
		}
		else
		{
			currentBuffs[(int)typeToAdd].stackBuff(time, level, typeToAdd == BuffType.fullBuff);
			showBuffLevel(typeToAdd);
		}
		checkIfBuffNeedsCommand(typeToAdd, level, time);
	}

	private IEnumerator countDownBuff(int buffId, Buff myBuff)
	{
		buffIcons[buffId].gameObject.SetActive(value: true);
		while (currentBuffs[buffId] != null)
		{
			buffIcons[buffId].SetBuffTimeText(myBuff.getTimeRemaining());
			yield return sec;
			if (myBuff.takeTick())
			{
				checkIfBuffNeedsCommand((BuffType)buffId, 0, 0);
				currentBuffs[buffId] = null;
				break;
			}
		}
		CalculateActiveFoodToRegen();
		buffIcons[buffId].gameObject.SetActive(value: false);
	}

	public int getBuffLevel(BuffType buffType)
	{
		if (currentBuffs[(int)buffType] == null)
		{
			return 0;
		}
		return currentBuffs[(int)buffType].getLevel();
	}

	private void showBuffLevel(BuffType buffType)
	{
		buffIcons[(int)buffType].SetBuffLevel(getBuffLevel(buffType));
	}

	public void checkIfBuffNeedsCommand(BuffType buffType, int level, int timer)
	{
		switch (buffType)
		{
		case BuffType.defenceBuff:
			NetworkMapSharer.Instance.localChar.CmdSetDefenceBuff(1f + 0.25f * (float)level);
			return;
		case BuffType.healthRegen:
			if (level > 0)
			{
				NetworkMapSharer.Instance.localChar.CmdSetHealthRegen(timer, level);
				return;
			}
			break;
		}
		if (buffType == BuffType.fireResistance && level > 0)
		{
			NetworkMapSharer.Instance.localChar.CmdSetFireResistance(level);
			return;
		}
		switch (buffType)
		{
		case BuffType.speedBuff:
			NetworkMapSharer.Instance.localChar.setSpeedDif((float)level / 2f);
			break;
		case BuffType.swimBuff:
			NetworkMapSharer.Instance.localChar.setSwimBuff((float)level / 2f);
			break;
		case BuffType.wellrested:
			CalculateActiveFoodToRegen();
			break;
		}
	}

	public void SetTemeratureColourForWeatherEvent(float newTemp)
	{
		worldColorSettings.basics.temperatureShift = newTemp;
		worldColor.colorGrading = worldColorSettings;
	}

	public float GetCurrentColourTemperature()
	{
		return worldColorSettings.basics.temperatureShift;
	}

	public void EatFoodAndAddStatus(InventoryItem foodToEat)
	{
		for (int i = 0; i < eatenFoods.Length; i++)
		{
			if (eatenFoods[i].CurrentlyEmpty())
			{
				eatenFoods[i].AddFood(foodToEat);
				break;
			}
		}
	}

	public void AdjustExtraStaminaAndHealthToCurrentFood()
	{
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < eatenFoods.Length; i++)
		{
			if (!eatenFoods[i].CurrentlyEmpty())
			{
				num += eatenFoods[i].GetTotalExtraHealthGivenFromThisFood();
				num2 += eatenFoods[i].GetTotalExtraStaminaGivenFromThisFood();
			}
		}
		float num3 = Mathf.Clamp(staminaMax - (num2 + 50f), 0f, 50f);
		int num4 = Mathf.Clamp(connectedDamge.maxHealth - (num + 50), 0, 50);
		addTempPoints(-num4, 0f - num3);
	}

	public void StartCountDownFoodTimer(StoredFoodType countMeDown)
	{
		StartCoroutine(CountDownFoodTimer(countMeDown));
	}

	private IEnumerator CountDownFoodTimer(StoredFoodType countMeDown)
	{
		while (!countMeDown.CurrentlyEmpty())
		{
			yield return tick;
			countMeDown.Tick();
		}
	}

	public void CalculateActiveFoodToRegen()
	{
		staminaRegenFromFood = 0f;
		healthRegenFromFood = 0;
		for (int i = 0; i < eatenFoods.Length; i++)
		{
			staminaRegenFromFood += eatenFoods[i].GetCurrentStaminaTick();
			healthRegenFromFood += eatenFoods[i].GetCurrentHealthTick();
		}
		if (getBuffLevel(BuffType.wellrested) != 0)
		{
			staminaRegenFromFood += 2f;
		}
		if (healthRegenFromFood != 0)
		{
			foodTickIcon.gameObject.SetActive(value: true);
			foodTickIcon.secondsRemaining.text = healthRegenFromFood + "<b><size=8>/t</size>";
		}
		else
		{
			foodTickIcon.gameObject.SetActive(value: false);
		}
		if (staminaRegenFromFood != 0f)
		{
			staminaTickIcon.gameObject.SetActive(value: true);
			staminaTickIcon.secondsRemaining.text = staminaRegenFromFood + "<b><size=8>/s</size>";
		}
		else
		{
			staminaTickIcon.gameObject.SetActive(value: false);
		}
	}

	public void ClearFoodAndBuffs(bool playSound = true)
	{
		bool flag = false;
		for (int i = 0; i < eatenFoods.Length; i++)
		{
			if (!eatenFoods[i].CurrentlyEmpty())
			{
				flag = true;
				eatenFoods[i].ClearFood();
			}
		}
		for (int j = 0; j < currentBuffs.Length; j++)
		{
			if (currentBuffs[j] != null && j != 15)
			{
				checkIfBuffNeedsCommand((BuffType)j, 0, 0);
				currentBuffs[j] = null;
				buffIcons[j].gameObject.SetActive(value: false);
			}
		}
		if (playSound && flag)
		{
			Invoke("PlayFoodGoneSound", 1f);
		}
	}

	public void ClearWellRested()
	{
		if (currentBuffs[15] != null)
		{
			checkIfBuffNeedsCommand(BuffType.wellrested, 0, 0);
			currentBuffs[15] = null;
			buffIcons[15].gameObject.SetActive(value: false);
		}
	}

	public void PlayFoodGoneSound()
	{
		SoundManager.Instance.play2DSound(SoundManager.Instance.clearFoodAndHunger);
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.UseTheDunny);
	}

	public void BuffIconButtonsOn(bool isOn)
	{
		for (int i = 0; i < buffIcons.Length; i++)
		{
			buffIcons[i].GetComponent<InvButton>().enabled = isOn;
		}
		for (int j = 0; j < eatenFoods.Length; j++)
		{
			eatenFoods[j].GetComponent<InvButton>().enabled = isOn;
		}
		staminaTickIcon.GetComponent<InvButton>().enabled = isOn;
		foodTickIcon.GetComponent<InvButton>().enabled = isOn;
	}
}
