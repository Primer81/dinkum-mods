using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class AnimalAI : NetworkBehaviour
{
	public int animalId;

	public string animalName = "Animal";

	public int dangerValue = 1000;

	public bool nocturnal;

	public bool isAnimal = true;

	public NavMeshAgent myAgent;

	private float baseSpeed;

	private FarmAnimal isFarmAnimal;

	private AnimalAILookForFood looksForFood;

	public float minWaitTime = 1f;

	public float maxWaitTime = 2f;

	public float animalRunAwayAtHealth = 100f;

	public float runCheckDistance = 10f;

	private AnimalAI_Attack attacks;

	public LayerMask myEnemies;

	private Damageable myDamageable;

	public bool isSkiddish = true;

	private Transform currentlyRunningFrom;

	public bool waterOnly;

	private AnimateAnimalAI myAnimation;

	private AnimalAI_Sleep doesSleep;

	private AnimalPrefersArea prefersArea;

	private AnimalAI_Pet isPet;

	public AnimalVariation hasVariation;

	public bool flyingAnimal;

	public float waterSpeedMultiplier = 1f;

	[Header("Can be fenced off")]
	public bool saveFencedOffAnimalsEvent;

	public bool photoRequestable = true;

	public SaveAlphaAnimal saveAsAlpha;

	private Transform _myTransform;

	private Transform _agentTransform;

	public WaitForSeconds checkTimer = new WaitForSeconds(0.1f);

	public static NavMeshPathStatus cantComplete = NavMeshPathStatus.PathPartial;

	public static NavMeshPathStatus completedPath = NavMeshPathStatus.PathComplete;

	private Transform lastAttackedBy;

	private bool rangeExtended;

	private bool inKnockback;

	private bool beingCalled;

	public void setUp()
	{
		attacks = GetComponent<AnimalAI_Attack>();
		isFarmAnimal = GetComponent<FarmAnimal>();
		isPet = GetComponent<AnimalAI_Pet>();
		looksForFood = GetComponent<AnimalAILookForFood>();
		myAnimation = GetComponent<AnimateAnimalAI>();
		myDamageable = GetComponent<Damageable>();
		doesSleep = GetComponent<AnimalAI_Sleep>();
		prefersArea = GetComponent<AnimalPrefersArea>();
		myAgent.autoTraverseOffMeshLink = false;
	}

	public override void OnStartClient()
	{
		_myTransform = base.transform;
		_agentTransform = myAgent.transform;
		if (!base.isServer)
		{
			baseSpeed = myAgent.speed;
		}
		setUp();
	}

	public override void OnStartServer()
	{
		lastAttackedBy = null;
		currentlyRunningFrom = null;
		baseSpeed = myAgent.speed;
		attacks = GetComponent<AnimalAI_Attack>();
		myAgent.avoidancePriority = Random.Range(55, 70);
		if (saveFencedOffAnimalsEvent || (bool)saveAsAlpha)
		{
			AnimalManager.manage.saveFencedOffAnimalsEvent.AddListener(saveAsFencedOff);
		}
		NetworkMapSharer.Instance.returnAgents.AddListener(returnOnLevelChange);
		WorldManager.Instance.changeDayEvent.AddListener(startNewDay);
		StartCoroutine(setUpDelay());
	}

	private void OnDisable()
	{
		if (base.isServer)
		{
			NetworkMapSharer.Instance.returnAgents.RemoveListener(returnOnLevelChange);
			WorldManager.Instance.changeDayEvent.RemoveListener(startNewDay);
			if (saveFencedOffAnimalsEvent || (bool)saveAsAlpha)
			{
				AnimalManager.manage.saveFencedOffAnimalsEvent.RemoveListener(saveAsFencedOff);
			}
			NetworkNavMesh.nav.checkNavMeshEvent.RemoveListener(checkOnNavmeshRebuild);
		}
	}

	private void saveAsFencedOff()
	{
		if ((bool)saveAsAlpha)
		{
			int id = animalId * 10 + getVariationNo();
			if (saveAsAlpha.daysRemaining != 0)
			{
				AnimalManager.manage.alphaAnimals.Add(new FencedOffAnimal(id, Mathf.RoundToInt(_myTransform.position.x / 2f), Mathf.RoundToInt(_myTransform.position.z / 2f), saveAsAlpha.daysRemaining));
			}
			else
			{
				AnimalManager.manage.returnAnimalAndDoNotSaveToMap(this);
			}
		}
		else if (WorldManager.Instance.isPositionOnMap(Mathf.RoundToInt(_myTransform.position.x / 2f), Mathf.RoundToInt(_myTransform.position.z / 2f)) && WorldManager.Instance.fencedOffMap[Mathf.RoundToInt(_myTransform.position.x / 2f), Mathf.RoundToInt(_myTransform.position.z / 2f)] >= 1)
		{
			int id2 = animalId * 10 + getVariationNo();
			AnimalManager.manage.fencedOffAnimals.Add(new FencedOffAnimal(id2, Mathf.RoundToInt(_myTransform.position.x / 2f), Mathf.RoundToInt(_myTransform.position.z / 2f)));
		}
	}

	public override void OnStopClient()
	{
		myAgent.speed = baseSpeed;
		clearRangeExtention();
	}

	private void returnOnLevelChange()
	{
		if (!isFarmAnimal)
		{
			NetworkNavMesh.nav.UnSpawnAnAnimal(this);
		}
	}

	private void startNewDay()
	{
		if ((bool)looksForFood)
		{
			looksForFood.isHungry = true;
		}
		if ((bool)doesSleep)
		{
			doesSleep.sendAnimalToSleep();
		}
		if ((bool)attacks && AnimalManager.manage.WasDifficultyChangedOverNight())
		{
			SetDifficultyLayer(AnimalManager.manage.GetCurrentDifficultyLayer(animalId));
		}
	}

	public void forceSetUp()
	{
		StartCoroutine(setUpDelay());
	}

	private IEnumerator setUpDelay()
	{
		_myTransform = base.transform;
		_agentTransform = myAgent.transform;
		myAgent.transform.parent = null;
		myAgent.transform.position = base.transform.position;
		myAgent.enabled = false;
		yield return new WaitForSeconds(0.1f);
		if (!isFarmAnimal)
		{
			StartCoroutine(checkDistanceAndEnableAgent());
		}
		while (!NetworkNavMesh.nav.doesPositionHaveNavChunk(Mathf.RoundToInt(_myTransform.position.x / 2f), Mathf.RoundToInt(_myTransform.position.z / 2f)))
		{
			yield return null;
		}
		float checkDistance = 2f;
		Vector3 groundPos = _agentTransform.position;
		groundPos.y = WorldManager.Instance.heightMap[(int)_agentTransform.position.x / 2, (int)_agentTransform.position.z / 2];
		if (groundPos.y < 0f && WorldManager.Instance.waterMap[(int)_agentTransform.position.x / 2, (int)_agentTransform.position.z / 2])
		{
			groundPos.y = -0.2f;
		}
		NavMeshHit hit;
		while (!NavMesh.SamplePosition(_agentTransform.position, out hit, checkDistance, myAgent.areaMask) && !NavMesh.SamplePosition(groundPos, out hit, checkDistance, myAgent.areaMask))
		{
			yield return null;
		}
		_agentTransform.position = hit.position;
		checkOnNavmeshRebuild();
		NetworkNavMesh.nav.checkNavMeshEvent.AddListener(checkOnNavmeshRebuild);
		while (!myAgent.isOnNavMesh)
		{
			yield return null;
		}
		myAgent.isStopped = false;
		myAgent.updateRotation = true;
		if ((bool)isFarmAnimal)
		{
			StartCoroutine(checkDistanceAndEnableAgent());
		}
		StartCoroutine(behave());
	}

	private IEnumerator checkDistanceAndEnableAgent()
	{
		yield return checkTimer;
		while (true)
		{
			checkDistanceToPlayerAndReturn();
			yield return checkTimer;
		}
	}

	public bool checkIfShouldContinue()
	{
		if (((bool)myDamageable && myDamageable.isStunned()) || !myAgent.enabled)
		{
			return false;
		}
		return true;
	}

	public bool isStunned()
	{
		if ((bool)myDamageable && myDamageable.isStunned())
		{
			return true;
		}
		return false;
	}

	private IEnumerator behave()
	{
		float waitTime = Random.Range(minWaitTime, maxWaitTime);
		WaitForSeconds myAnimalDelay = new WaitForSeconds(Random.Range(0.05f, 0.07f));
		float stillTimer = waitTime;
		while (true)
		{
			yield return myAnimalDelay;
			if (!myAgent.enabled)
			{
				continue;
			}
			if ((bool)myDamageable && myDamageable.isStunned())
			{
				while (myDamageable.isStunned())
				{
					yield return null;
				}
			}
			if ((bool)doesSleep)
			{
				yield return StartCoroutine(doesSleep.checkIfNeedsSleep());
			}
			if (((bool)doesSleep && doesSleep.checkIfSleeping()) || !myAgent.isActiveAndEnabled || !base.gameObject.activeInHierarchy)
			{
				continue;
			}
			if ((checkIfShouldContinue() && isSkiddish) || (checkIfShouldContinue() && isInjuredAndNeedsToRunAway()))
			{
				if ((bool)attacks && attacks.lookForPrey && !isSkiddish)
				{
					LayerMask myEnemiesTemp = myEnemies;
					myEnemies = attacks.myPrey;
					yield return StartCoroutine(checkForEnemiesAndRunIfFound());
					myEnemies = myEnemiesTemp;
				}
				else
				{
					yield return StartCoroutine(checkForEnemiesAndRunIfFound());
				}
			}
			if (checkIfShouldContinue() && (attacks?.lookForPrey ?? false) && !isInjuredAndNeedsToRunAway())
			{
				if (attacks.huntsWhenHungry)
				{
					if (looksForFood.isHungry)
					{
						yield return StartCoroutine(attacks.lookForClosetPreyAndChase());
					}
				}
				else
				{
					yield return StartCoroutine(attacks.lookForClosetPreyAndChase());
				}
			}
			if (checkIfShouldContinue() && !isInjuredAndNeedsToRunAway() && (attacks?.attackOnlyOnAttack ?? false) && attacks.hasTarget())
			{
				yield return StartCoroutine(attacks.lookForClosetPreyAndChase());
			}
			if (checkIfShouldContinue() && (looksForFood?.isHungry ?? false))
			{
				yield return StartCoroutine(looksForFood.searchForFoodNearby());
			}
			if (checkIfShouldContinue() && (bool)prefersArea)
			{
				yield return StartCoroutine(prefersArea.checkForPreferedArea());
			}
			if (checkIfShouldContinue() && checkIfHasArrivedAtDestination())
			{
				if (stillTimer < waitTime)
				{
					stillTimer += Time.deltaTime;
				}
				else if (myAgent.isActiveAndEnabled)
				{
					Vector3 sureRandomPos = getSureRandomPos();
					if (sureRandomPos != _myTransform.position)
					{
						myAgent.SetDestination(sureRandomPos);
						stillTimer = 0f;
						waitTime = Random.Range(minWaitTime, maxWaitTime);
					}
				}
			}
			else if (flyingAnimal)
			{
				if (myAgent.isActiveAndEnabled && myAgent.remainingDistance > 4f)
				{
					myAgent.speed = getBaseSpeed() * 6f;
				}
				else
				{
					myAgent.speed = getBaseSpeed();
				}
			}
		}
	}

	private IEnumerator checkForEnemiesAndRunIfFound()
	{
		float closestEnemyCheck = 0f;
		Transform transform = returnClosestEnemy();
		if (transform != null)
		{
			if (transform != currentlyRunningFrom)
			{
				currentlyRunningFrom = transform;
				cancleCurrentDestination();
			}
			if (!flyingAnimal)
			{
				myAgent.speed = getBaseSpeed() * 2f;
			}
			else
			{
				myAgent.speed = getBaseSpeed() * 6f;
			}
			myAgent.SetDestination(getSureRunPos(currentlyRunningFrom));
		}
		while ((bool)currentlyRunningFrom)
		{
			yield return null;
			if (myAgent.isActiveAndEnabled)
			{
				if (closestEnemyCheck > 5f)
				{
					Transform transform2 = returnClosestEnemy();
					if (transform2 != null || transform2 == currentlyRunningFrom)
					{
						currentlyRunningFrom = transform2;
						cancleCurrentDestination();
						if (!flyingAnimal)
						{
							myAgent.speed = getBaseSpeed() * 2f;
						}
						else
						{
							myAgent.speed = getBaseSpeed() * 6f;
						}
						myAgent.SetDestination(getSureRunPos(currentlyRunningFrom));
					}
					closestEnemyCheck = 0f;
				}
				else
				{
					closestEnemyCheck += Time.deltaTime;
				}
				if ((bool)currentlyRunningFrom && checkIfHasArrivedAtDestination())
				{
					if (Vector3.Distance(currentlyRunningFrom.position, _myTransform.position) > runCheckDistance * 1.3f)
					{
						currentlyRunningFrom = null;
					}
					else
					{
						myAgent.SetDestination(getSureRunPos(currentlyRunningFrom));
					}
				}
			}
			if (!checkIfShouldContinue())
			{
				break;
			}
		}
		if (!isPet && !flyingAnimal)
		{
			myAgent.speed = getBaseSpeed();
		}
	}

	public bool checkIfHasArrivedAtDestination()
	{
		if (myAgent.isOnNavMesh && myAgent.remainingDistance - 1f <= myAgent.stoppingDistance && myAgent.path.status != cantComplete)
		{
			return true;
		}
		if (myAgent.pathStatus != completedPath)
		{
			return true;
		}
		return false;
	}

	public bool checkIfCanGetToTarget(Transform target)
	{
		Vector3 position = target.position;
		if (WorldManager.Instance.isPositionOnMap(position))
		{
			position.y = WorldManager.Instance.heightMap[Mathf.RoundToInt(position.x / 2f), Mathf.RoundToInt(position.z / 2f)];
			if (waterOnly && WorldManager.Instance.isPositionInWater(position))
			{
				position.y = -0.6f;
			}
			if (NavMesh.SamplePosition(position, out var hit, 3f, myAgent.areaMask))
			{
				if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
				{
					NavMeshPath navMeshPath = new NavMeshPath();
					myAgent.CalculatePath(hit.position, navMeshPath);
					if (navMeshPath.status != 0)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public bool checkIfTargetIsAStraightLineAhead(Transform target)
	{
		if ((bool)target && myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			Vector3 normalized = (target.position - _agentTransform.position).normalized;
			myAgent.CalculatePath(target.position - normalized * 1.5f, navMeshPath);
			if (navMeshPath.corners.Length == 0)
			{
				return false;
			}
			if (navMeshPath.corners.Length <= 3)
			{
				return true;
			}
		}
		return false;
	}

	public void cancleCurrentDestination()
	{
		if (myAgent.enabled)
		{
			myAgent.ResetPath();
		}
	}

	public Transform returnClosestEnemy(float multi = 1f)
	{
		if (Physics.CheckSphere(_myTransform.position + _myTransform.forward * runCheckDistance / 4f, runCheckDistance * multi, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(_myTransform.position + _myTransform.forward * runCheckDistance / 4f, runCheckDistance * multi, myEnemies);
			if (array.Length != 0)
			{
				int num = 0;
				float num2 = 2000f;
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (!waterOnly || (waterOnly && array[i].transform.position.y < 0f))
					{
						float num3 = Vector3.Distance(_myTransform.position, array[i].transform.position);
						if (num3 < num2)
						{
							num = i;
							num2 = num3;
							flag = true;
						}
					}
				}
				if (flag)
				{
					return array[num].transform.root;
				}
			}
		}
		return null;
	}

	public Vector3 checkDestination(Vector3 destinationToCheck)
	{
		if (NavMesh.SamplePosition(destinationToCheck, out var hit, 10f, myAgent.areaMask))
		{
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				myAgent.CalculatePath(hit.position, navMeshPath);
				if (navMeshPath.status == cantComplete)
				{
					return _myTransform.position;
				}
			}
			return hit.position;
		}
		return _myTransform.position;
	}

	public bool checkIfWarpIsPossible(Vector3 warpDestination)
	{
		if (NavMesh.SamplePosition(warpDestination, out var hit, 0.4f, myAgent.areaMask))
		{
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				myAgent.CalculatePath(hit.position, navMeshPath);
				if (navMeshPath.status != 0)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public Vector3 checkIfPointIsWalkable(Vector3 warpDestination)
	{
		if (waterOnly)
		{
			warpDestination.y = -0.6f;
		}
		else
		{
			warpDestination.y = WorldManager.Instance.heightMap[Mathf.RoundToInt(warpDestination.x / 2f), Mathf.RoundToInt(warpDestination.z / 2f)];
		}
		if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			myAgent.CalculatePath(warpDestination, navMeshPath);
			if (navMeshPath.status != NavMeshPathStatus.PathInvalid)
			{
				return warpDestination;
			}
		}
		return _agentTransform.position;
	}

	public Vector3 getSureRunPos(Transform runningFrom)
	{
		Vector3 vector = _myTransform.position;
		for (int i = 0; i < 500; i++)
		{
			Vector3 vector2 = new Vector3(runningFrom.position.x, _myTransform.position.y, runningFrom.position.z);
			vector = _myTransform.position + (_myTransform.position - vector2).normalized * runCheckDistance / 2f;
			vector += new Vector3(Random.Range((0f - runCheckDistance) / 2f, runCheckDistance / 2f), 0f, Random.Range((0f - runCheckDistance) / 2f, runCheckDistance / 2f));
			if (WorldManager.Instance.isPositionOnMap(vector))
			{
				vector.y = WorldManager.Instance.heightMap[Mathf.RoundToInt(vector.x / 2f), Mathf.RoundToInt(vector.z / 2f)];
			}
			vector = checkDestination(vector);
			if (vector != _myTransform.position)
			{
				break;
			}
		}
		if (vector == _myTransform.position)
		{
			Vector3 vector3 = new Vector3(runningFrom.position.x, _myTransform.position.y, runningFrom.position.z) + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
			vector = _myTransform.position + (_myTransform.position - vector3) * runCheckDistance + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
		}
		return vector;
	}

	public Vector3 getSureRandomPos()
	{
		int num = 0;
		Vector3 vector = _myTransform.position;
		int num2 = 12;
		if (flyingAnimal)
		{
			num2 = 45;
		}
		while (vector != _myTransform.position)
		{
			vector = checkDestination(_myTransform.position + new Vector3(Random.Range(-num2, num2), 0f, Random.Range(-num2, num2)));
			num++;
			if (num >= 50)
			{
				vector = _myTransform.position;
				break;
			}
		}
		if (vector == _myTransform.position)
		{
			return _myTransform.position + new Vector3(Random.Range(-num2, num2), 0f, Random.Range(-num2, num2));
		}
		return vector;
	}

	public Vector3 getSureRandomSpawnPos(Vector3 startPos)
	{
		int num = 0;
		Vector3 vector = startPos;
		int num2 = 24;
		while (vector != startPos)
		{
			vector = checkDestination(startPos + new Vector3(Random.Range(-num2, num2), 0f, Random.Range(-num2, num2)));
			num++;
			if (num >= 50)
			{
				vector = startPos;
				break;
			}
		}
		if (vector == startPos)
		{
			return startPos + new Vector3(Random.Range(-num2, num2), 0f, Random.Range(-num2, num2));
		}
		return vector;
	}

	public float getBaseSpeed()
	{
		return getBaseSpeed(Mathf.RoundToInt(_myTransform.position.x / 2f), Mathf.RoundToInt(_myTransform.position.z / 2f));
	}

	public float getBaseSpeed(int xPos, int yPos)
	{
		if (!WorldManager.Instance)
		{
			return baseSpeed;
		}
		if (WorldManager.Instance.waterMap[xPos, yPos] && WorldManager.Instance.heightMap[xPos, yPos] < 0)
		{
			return baseSpeed * waterSpeedMultiplier;
		}
		return baseSpeed;
	}

	public bool isAttackingOrBeingAttackedBy(Transform check)
	{
		if (base.gameObject.activeSelf && myDamageable.health > 0)
		{
			if (lastAttackedBy == check)
			{
				return true;
			}
			if ((bool)attacks && attacks.currentlyAttacking == check)
			{
				return true;
			}
		}
		return false;
	}

	public void damageTaken(Transform attackedBy)
	{
		lastAttackedBy = attackedBy;
		wakeUpAnimal();
		AnimalAI component = attackedBy.GetComponent<AnimalAI>();
		if ((bool)component && component.animalId == animalId)
		{
			return;
		}
		AnimalAI_Attack animalAI_Attack = attacks;
		if ((object)animalAI_Attack != null && animalAI_Attack.attackOnlyOnAttack)
		{
			if (!attacks.hasTarget() || Random.Range(0, 10) == 5)
			{
				attacks.setNewCurrentlyAttacking(attackedBy.root);
			}
			currentlyRunningFrom = null;
		}
		else
		{
			AnimalAI_Attack animalAI_Attack2 = attacks;
			if ((object)animalAI_Attack2 != null && animalAI_Attack2.lookForPrey)
			{
				if (!attacks.hasTarget() || (attacks.hasTarget() && Random.Range(0, 2) == 1 && Vector3.Distance(attackedBy.root.position, _myTransform.position) <= Vector3.Distance(attacks.currentlyAttacking.position, _myTransform.position)))
				{
					attacks.setNewCurrentlyAttacking(attackedBy.root);
				}
			}
			else if (isSkiddish)
			{
				currentlyRunningFrom = attackedBy;
				myAgent.SetDestination(getSureRunPos(attackedBy.root));
			}
		}
		if (!rangeExtended && Vector3.Distance(_myTransform.position, attackedBy.position) >= runCheckDistance - 2f && !rangeExtended)
		{
			rangeExtended = true;
			StartCoroutine(extendRangeTemp());
		}
	}

	private IEnumerator extendRangeTemp()
	{
		rangeExtended = true;
		runCheckDistance *= 3f;
		if ((bool)attacks)
		{
			attacks.chaseDistance *= 3f;
		}
		yield return new WaitForSeconds(5f);
		runCheckDistance /= 3f;
		if ((bool)attacks)
		{
			attacks.chaseDistance /= 3f;
		}
		rangeExtended = false;
	}

	public void clearRangeExtention()
	{
		if (rangeExtended)
		{
			runCheckDistance /= 3f;
			if ((bool)attacks)
			{
				attacks.chaseDistance /= 3f;
			}
		}
	}

	public bool isSleeping()
	{
		if ((bool)doesSleep && doesSleep.checkIfSleeping())
		{
			return true;
		}
		return false;
	}

	public void wakeUpAnimal()
	{
		AnimalAI_Sleep animalAI_Sleep = doesSleep;
		if ((object)animalAI_Sleep != null && animalAI_Sleep.checkIfSleeping())
		{
			doesSleep.wakeUpNow();
		}
	}

	public void takeHitAndKnockBack(Transform attackedBy, float knockBackAmount)
	{
		if (myAgent.isActiveAndEnabled)
		{
			if (!inKnockback && getBaseSpeed() > 0.25f)
			{
				StartCoroutine(knockBackTimer());
				StartCoroutine(knockBackMove(attackedBy, knockBackAmount));
			}
			damageTaken(attackedBy);
		}
	}

	private IEnumerator knockBackTimer()
	{
		inKnockback = true;
		for (float knockBackTimer = 0f; knockBackTimer < 0.75f; knockBackTimer += Time.deltaTime)
		{
			yield return null;
		}
		inKnockback = false;
	}

	private IEnumerator knockBackMove(Transform hitby, float distance)
	{
		float knockBackDistance = 0f;
		Vector3 startingPos = _agentTransform.position;
		Vector3 pushDir = -(hitby.position - _myTransform.position).normalized;
		pushDir.y = 0f;
		Vector3 travelTowards = checkIfPointIsWalkable(_agentTransform.position + pushDir * 25f * Time.deltaTime);
		if ((bool)attacks && attacks.isWindingUp())
		{
			distance /= 3.5f;
		}
		while (knockBackDistance < distance)
		{
			_agentTransform.position = travelTowards;
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				myAgent.ResetPath();
			}
			travelTowards = checkIfPointIsWalkable(_agentTransform.position + pushDir * 25f * Time.deltaTime);
			knockBackDistance = Vector3.Distance(startingPos, _agentTransform.position);
			if (!(travelTowards == _agentTransform.position))
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public bool isInKnockBack()
	{
		return inKnockback;
	}

	public void onDeath()
	{
		if (base.isServer)
		{
			myAgent.enabled = false;
			if ((bool)attacks)
			{
				attacks.removeAttackingChar();
			}
		}
		if ((bool)isFarmAnimal)
		{
			isFarmAnimal.onFarmAnimalDeath();
		}
		if ((bool)myAnimation)
		{
			myAnimation.playDeathAnimation();
		}
	}

	public bool isDead()
	{
		if (!myDamageable)
		{
			return false;
		}
		return myDamageable.health <= 0;
	}

	public int getHealth()
	{
		return myDamageable.health;
	}

	public int getMaxHealth()
	{
		return myDamageable.maxHealth;
	}

	public bool isInjuredAndNeedsToRunAway()
	{
		return (float?)myDamageable?.health <= animalRunAwayAtHealth;
	}

	public void checkDistanceToPlayerAndReturn()
	{
		if (!isFarmAnimal && !NetworkNavMesh.nav.doesPositionHaveNavChunk((int)(_agentTransform.position.x / 2f), (int)(_agentTransform.position.z / 2f)) && !checkIfThereIsStillNavmeshThere())
		{
			NetworkNavMesh.nav.UnSpawnAnAnimal(this);
		}
		else if ((bool)isFarmAnimal)
		{
			checkOnNavmeshRebuild();
		}
	}

	public bool checkIfThereIsStillNavmeshThere()
	{
		if (NavMesh.SamplePosition(_agentTransform.position, out var _, 1f, myAgent.areaMask))
		{
			return true;
		}
		return false;
	}

	public void checkOnNavmeshRebuild()
	{
		if (NetworkNavMesh.nav.isCloseEnoughToNavChunk((int)_myTransform.position.x / 2, (int)_myTransform.position.z / 2))
		{
			if (!myAgent.isActiveAndEnabled && NavMesh.SamplePosition(_agentTransform.position, out var _, 1.5f, myAgent.areaMask) && (!myDamageable || myDamageable.health > 0))
			{
				myAgent.enabled = true;
			}
		}
		else if (myAgent.isActiveAndEnabled)
		{
			myAgent.enabled = false;
		}
	}

	public void setRunningFrom(Transform newRunningFrom)
	{
		currentlyRunningFrom = newRunningFrom;
	}

	public void setBaseSpeed(float newBaseSpeed)
	{
		myAgent.speed = newBaseSpeed;
		baseSpeed = newBaseSpeed;
	}

	public void setAttackType(bool attacksWhenClose)
	{
		if ((bool)attacks)
		{
			attacks.attackAndThenRun = attacksWhenClose;
		}
	}

	public void takeAHitLocal()
	{
		if (!isAttackPlaying() && (bool)myAnimation && myDamageable.health > 0)
		{
			myAnimation.takeAHitLocal();
		}
	}

	public bool isAttackPlaying()
	{
		if ((bool)attacks)
		{
			return attacks.isAttackPlaying();
		}
		return false;
	}

	public void callAnimalToPos(Vector3 callToPos)
	{
		beingCalled = false;
		StartCoroutine(callAnimalToPosRoutine(callToPos));
	}

	private IEnumerator callAnimalToPosRoutine(Vector3 callToPos)
	{
		yield return null;
		AnimalAI_Sleep animalAI_Sleep = doesSleep;
		if ((object)animalAI_Sleep != null && !animalAI_Sleep.checkIfSleeping())
		{
			beingCalled = true;
			float callToTimer = Random.Range(5f, 8f);
			myAgent.SetDestination(callToPos + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f)));
			while ((beingCalled && callToTimer > 0f) || myAgent.remainingDistance <= myAgent.stoppingDistance)
			{
				myAgent.speed = getBaseSpeed() * 2f;
				yield return null;
				callToTimer -= Time.deltaTime;
			}
			beingCalled = false;
			myAgent.speed = getBaseSpeed();
		}
	}

	public int getVariationNo()
	{
		if ((bool)hasVariation)
		{
			return hasVariation.getVaritationNo();
		}
		return 0;
	}

	public void setVariation(int newVariation)
	{
		if ((bool)hasVariation)
		{
			hasVariation.setVariation(newVariation);
		}
	}

	public int getRandomVariationNo()
	{
		if ((bool)hasVariation)
		{
			if (hasVariation.randomVariationLimit != -1)
			{
				return Random.Range(0, hasVariation.randomVariationLimit);
			}
			return Random.Range(0, hasVariation.variations.Length);
		}
		return 0;
	}

	public void setCurrentlyAttacking(Transform newCurrentlyAttacking)
	{
		if ((bool)attacks)
		{
			attacks.setNewCurrentlyAttacking(newCurrentlyAttacking);
			currentlyRunningFrom = null;
		}
	}

	public void SetDifficultyLayer(LayerMask newMask)
	{
		if (isAnimal && (bool)attacks)
		{
			attacks.myPrey = newMask;
			attacks.setNewCurrentlyAttacking(null);
		}
	}

	public Transform currentlyAttacking()
	{
		if (!attacks)
		{
			return null;
		}
		return attacks.currentlyAttacking;
	}

	public bool isInJump()
	{
		if (!attacks)
		{
			return false;
		}
		return attacks.isInJump();
	}

	public AnimalAI_Pet isAPet()
	{
		return isPet;
	}

	public void setSleepPos(Vector3 sleepPos)
	{
		if ((bool)doesSleep && !isFarmAnimal)
		{
			doesSleep.setDesiredSleepPos(sleepPos);
		}
	}

	public Vector3 getSleepPos()
	{
		if ((bool)doesSleep)
		{
			return doesSleep.getSleepPos();
		}
		return Vector3.zero;
	}

	public string GetAnimalName(int count = 1)
	{
		return LocalisationMarkUp.ProcessNameTag(ConversationGenerator.generate.GetAnimalName(animalId), count);
	}

	public LocalisationMarkUp.LanguageGender GetLanguageGender()
	{
		string text = ConversationGenerator.generate.GetAnimalName(animalId);
		if (text == null)
		{
			return LocalisationMarkUp.LanguageGender.neutral;
		}
		if (text.ToString().Contains("{M}"))
		{
			return LocalisationMarkUp.LanguageGender.masculine;
		}
		if (text.ToString().Contains("{F}"))
		{
			return LocalisationMarkUp.LanguageGender.feminine;
		}
		return LocalisationMarkUp.LanguageGender.neutral;
	}

	public string GetAnimalVariationAdjective(int adjectiveId)
	{
		return ConversationGenerator.generate.GetAnimalVariationAdjective(animalId, adjectiveId);
	}

	private void MirrorProcessed()
	{
	}
}
