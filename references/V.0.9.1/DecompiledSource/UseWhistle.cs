using System.Collections;
using UnityEngine;

public class UseWhistle : MonoBehaviour
{
	public ASound whistleSound;

	public int[] callAnimalId;

	public float whistleRadius = 10f;

	public LayerMask animalLayer;

	public ParticleSystem whistleParts;

	public bool isPetWhistle;

	public bool petsAllAnimalsThatHear;

	public bool callVehicleAnimal;

	public bool bubbleBlower;

	public bool pinWheel;

	public Animator pinWheelAnim;

	private CharMovement myChar;

	private float spinSpeed;

	private void Start()
	{
		if (pinWheel)
		{
			myChar = GetComponentInParent<CharMovement>();
			if ((bool)myChar)
			{
				StartCoroutine(SpinPinWheel());
			}
		}
	}

	public void useWhistle()
	{
		if ((bool)whistleSound)
		{
			SoundManager.Instance.playASoundAtPoint(whistleSound, base.transform.position);
		}
		Invoke("shootWhisleParticles", 0.1f);
		if (!NetworkMapSharer.Instance.isServer)
		{
			return;
		}
		if (callVehicleAnimal)
		{
			FarmAnimalManager.manage.specialCallFarmAnimalToYou(callAnimalId[0], base.transform.position);
		}
		else
		{
			if (!Physics.CheckSphere(base.transform.root.position + base.transform.root.forward * whistleRadius / 4f, whistleRadius, animalLayer))
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(base.transform.position, whistleRadius, animalLayer);
			for (int i = 0; i < array.Length; i++)
			{
				AnimalAI componentInParent = array[i].GetComponentInParent<AnimalAI>();
				if (!componentInParent)
				{
					continue;
				}
				for (int j = 0; j < callAnimalId.Length; j++)
				{
					if (componentInParent.animalId != callAnimalId[j])
					{
						continue;
					}
					componentInParent.callAnimalToPos(base.transform.root.position);
					if (isPetWhistle && (bool)componentInParent.isAPet())
					{
						componentInParent.GetComponent<AnimalAI_Pet>().setNewFollowTo(base.transform.root.GetComponent<CharMovement>().netId);
					}
					if (petsAllAnimalsThatHear && Random.Range(0, 10) == 2)
					{
						FarmAnimal component = componentInParent.GetComponent<FarmAnimal>();
						if ((bool)component)
						{
							component.RpcPetAnimal();
						}
					}
				}
			}
		}
	}

	private void shootWhisleParticles()
	{
		if ((bool)whistleParts && !pinWheel)
		{
			if (bubbleBlower)
			{
				ParticleManager.manage.bubbleBlowerPart.transform.rotation = whistleParts.transform.rotation;
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.bubbleBlowerPart, whistleParts.transform.position, 15);
			}
			else
			{
				whistleParts.Emit(15);
			}
		}
	}

	private IEnumerator SpinPinWheel()
	{
		Vector3 charLastPos = myChar.transform.position;
		while (true)
		{
			if (!myChar.myEquip.usingItem)
			{
				float num = Vector3.Distance(myChar.transform.position, charLastPos) / Time.fixedDeltaTime / 9f;
				if (!RealWorldTimeLight.time.underGround && myChar.transform.position.y >= -6f)
				{
					num = Mathf.Clamp(num, WeatherManager.Instance.windMgr.CurrentWindSpeed, 2f);
				}
				spinSpeed = Mathf.Lerp(spinSpeed, num, Time.deltaTime);
			}
			else
			{
				spinSpeed = Mathf.Lerp(spinSpeed, 1f, Time.deltaTime);
			}
			charLastPos = myChar.transform.position;
			pinWheelAnim.SetFloat("SpinSpeed", spinSpeed);
			yield return null;
		}
	}
}
