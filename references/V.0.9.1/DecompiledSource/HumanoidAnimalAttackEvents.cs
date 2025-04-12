using System.Collections;
using UnityEngine;

public class HumanoidAnimalAttackEvents : MonoBehaviour
{
	public ParticleSystem swingParts;

	public ASound swingSound;

	public ParticleSystem castParticles;

	public ASound castSound;

	private AnimalMakeSounds mySounds;

	private AnimalAI_Attack myAttack;

	public GameObject bookOpen;

	public GameObject bookClosed;

	public Light bookLight;

	private void Start()
	{
		mySounds = GetComponent<AnimalMakeSounds>();
	}

	private void OnEnable()
	{
		if (!myAttack)
		{
			myAttack = GetComponent<AnimalAI_Attack>();
		}
		if ((bool)myAttack)
		{
			StartCoroutine(PickUpSomethingNearEnemies());
		}
	}

	public void startCastCharge()
	{
		if ((bool)bookOpen)
		{
			SoundManager.Instance.playASoundAtPoint(swingSound, base.transform.position, 0.5f, 0.45f);
			StopCoroutine("OpenBookEvent");
			StartCoroutine("OpenBookEvent");
		}
	}

	private IEnumerator OpenBookEvent()
	{
		float bookTimer2 = 0f;
		SoundManager.Instance.playASoundAtPoint(mySounds.attackSound, base.transform.position);
		SoundManager.Instance.playASoundAtPoint(swingSound, base.transform.position);
		bookClosed.SetActive(value: false);
		bookOpen.SetActive(value: true);
		bookLight.intensity = 0f;
		bookLight.enabled = true;
		while (bookTimer2 < 1f)
		{
			bookTimer2 += Time.deltaTime;
			bookLight.intensity = Mathf.Lerp(0f, 2f, bookTimer2);
			yield return null;
		}
		bookClosed.SetActive(value: true);
		bookOpen.SetActive(value: false);
		bookTimer2 = 0f;
		while (bookTimer2 < 1f)
		{
			bookTimer2 += Time.deltaTime;
			bookLight.intensity = Mathf.Lerp(2f, 0f, bookTimer2);
			yield return null;
		}
	}

	public void startAttack()
	{
		SoundManager.Instance.playASoundAtPoint(mySounds.attackSound, base.transform.position);
		SoundManager.Instance.playASoundAtPoint(swingSound, base.transform.position);
	}

	public void lookLockFrames(int number)
	{
	}

	public void doDamageNow()
	{
	}

	public void castLine()
	{
		StartCoroutine(PlayCastPartRoutine(10f));
		SoundManager.Instance.playASoundAtPoint(castSound, base.transform.position);
	}

	public void playSwingPartsForFrames(int frames)
	{
		StartCoroutine(PlaySwingPartRoutine(frames));
	}

	private IEnumerator PlaySwingPartRoutine(float frames)
	{
		while (frames > 0f)
		{
			swingParts.Emit(10);
			yield return null;
			frames -= 1f;
		}
	}

	private IEnumerator PlayCastPartRoutine(float frames)
	{
		while (frames > 0f)
		{
			swingParts.Emit(50);
			yield return null;
			frames -= 1f;
		}
	}

	public void setFaceShocked()
	{
	}

	private IEnumerator PickUpSomethingNearEnemies()
	{
		WaitForSeconds waitSec = new WaitForSeconds(0.5f);
		while (true)
		{
			yield return waitSec;
			if (!myAttack.currentlyAttacking)
			{
				continue;
			}
			Vector3 vector = base.transform.position + base.transform.forward * 1.5f;
			int num = Mathf.RoundToInt(vector.x / 2f);
			int num2 = Mathf.RoundToInt(vector.z / 2f);
			int num3 = WorldManager.Instance.onTileMap[num, num2];
			if (num3 > -1 && WorldManager.Instance.allObjectSettings[num3].canBePickedUp && CanBePickedUp(num3, num, num2))
			{
				if (!WorldManager.Instance.allObjectSettings[num3].isMultiTileObject)
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(-1, num, num2);
				}
				else
				{
					NetworkMapSharer.Instance.RpcRemoveMultiTiledObject(-1, num, num2, WorldManager.Instance.rotationMap[num, num2]);
				}
			}
			if (num3 >= -1)
			{
				continue;
			}
			Vector2 vector2 = WorldManager.Instance.findMultiTileObjectPos(num, num2);
			num = (int)vector2.x;
			num2 = (int)vector2.y;
			int num4 = WorldManager.Instance.onTileMap[num, num2];
			if (num4 > -1 && WorldManager.Instance.allObjectSettings[num4].canBePickedUp && CanBePickedUp(num4, num, num2))
			{
				if (!WorldManager.Instance.allObjectSettings[num4].isMultiTileObject)
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(-1, num, num2);
				}
				else
				{
					NetworkMapSharer.Instance.RpcRemoveMultiTiledObject(num4, num, num2, WorldManager.Instance.rotationMap[num, num2]);
				}
			}
		}
	}

	public bool CanBePickedUp(int tileId, int xPos, int yPos)
	{
		if (ItemOnTopManager.manage.hasItemsOnTop(xPos, yPos))
		{
			return false;
		}
		if (WorldManager.Instance.allObjectSettings[tileId].dropsStatusNumberOnDeath)
		{
			return false;
		}
		if ((bool)WorldManager.Instance.allObjects[tileId].tileObjectChest)
		{
			return ContainerManager.manage.checkIfEmpty(xPos, yPos, null);
		}
		return true;
	}
}
