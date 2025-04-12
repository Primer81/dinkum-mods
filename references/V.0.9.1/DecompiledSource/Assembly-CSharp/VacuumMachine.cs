using System.Collections;
using UnityEngine;

public class VacuumMachine : MonoBehaviour
{
	public LayerMask dropLayer;

	private CharMovement myChar;

	public Transform vacPoint;

	public ASound suckUpItemSound;

	public ParticleSystem particleSystem1;

	public ParticleSystem particleSystem2;

	public Transform targetTransform;

	private ParticleSystem.Particle[] particles;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			StartCoroutine(VacuumItems());
		}
	}

	private IEnumerator VacuumItems()
	{
		WaitForSeconds vacWait = new WaitForSeconds(0.15f);
		while (true)
		{
			yield return null;
			if (myChar.localUsing)
			{
				yield return vacWait;
				yield return vacWait;
				while (myChar.localUsing)
				{
					yield return vacWait;
					CollectDrops();
				}
			}
		}
	}

	public void CollectDrops()
	{
		if (!WorldManager.Instance.isPositionOnMap(vacPoint.position))
		{
			return;
		}
		bool flag = false;
		Vector3 position = new Vector3(vacPoint.position.x, WorldManager.Instance.heightMap[Mathf.RoundToInt(vacPoint.position.x / 2f), Mathf.RoundToInt(vacPoint.position.z / 2f)], vacPoint.position.z);
		if (!Physics.CheckSphere(position, 1.8f, dropLayer))
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(position, 1.8f, dropLayer);
		for (int i = 0; i < array.Length; i++)
		{
			DroppedItem component = array[i].GetComponent<DroppedItem>();
			if (!component)
			{
				continue;
			}
			if (Inventory.Instance.checkIfItemCanFit(component.myItemId, component.stackAmount))
			{
				if (myChar.myEquip.myPermissions.CheckIfCanPickUp())
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.pickUpItem);
					component.pickUpLocal();
					NetworkMapSharer.Instance.localChar.myPickUp.CmdPickUp(component.netId);
					Inventory.Instance.useItemWithFuel();
					if (!flag)
					{
						flag = false;
						SoundManager.Instance.playASoundAtPoint(suckUpItemSound, base.transform.position);
					}
				}
			}
			else
			{
				NotificationManager.manage.turnOnPocketsFullNotification(holdingButton: true);
			}
		}
	}

	private void LateUpdate()
	{
		if (targetTransform != null)
		{
			AttractParticles(particleSystem1);
			AttractParticles(particleSystem2);
		}
	}

	private void AttractParticles(ParticleSystem ps)
	{
		if (!(ps == null))
		{
			int maxParticles = ps.main.maxParticles;
			if (particles == null || particles.Length < maxParticles)
			{
				particles = new ParticleSystem.Particle[maxParticles];
			}
			int num = ps.GetParticles(particles);
			for (int i = 0; i < num; i++)
			{
				Vector3 normalized = (targetTransform.position - particles[i].position).normalized;
				float num2 = Vector3.Distance(targetTransform.position, particles[i].position) / particles[i].remainingLifetime;
				particles[i].velocity = normalized * num2;
			}
			ps.SetParticles(particles, num);
		}
	}
}
