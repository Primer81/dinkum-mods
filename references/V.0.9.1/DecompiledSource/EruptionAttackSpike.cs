using System.Collections;
using UnityEngine;

public class EruptionAttackSpike : MonoBehaviour
{
	public ItemHitBox myHitBox;

	public ASound popUpSound;

	public Collider noHurtCollider;

	public MeshRenderer myRen;

	private bool connectedToPlayer;

	public bool playParticles = true;

	public void setShotByAnimal(AnimalAI shotBy)
	{
		myHitBox.setIsAnimal(shotBy);
		base.transform.localScale = Vector3.zero;
	}

	public void setShotByPlayer(CharMovement player)
	{
		myHitBox.canDamageFriendly = false;
		if (player.isLocalPlayer)
		{
			myHitBox.setShotByLocalPlayer();
		}
		connectedToPlayer = true;
		base.transform.localScale = Vector3.zero;
	}

	public IEnumerator popUpAtPos(Vector3 newPos)
	{
		base.transform.localPosition = newPos;
		if (WorldManager.Instance.isPositionOnMap(base.transform.position))
		{
			base.transform.position = new Vector3(base.transform.position.x, WorldManager.Instance.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)], base.transform.position.z);
		}
		base.transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		setMaterial();
		float timer = 0f;
		yield return null;
		myHitBox.startAttack();
		if (playParticles)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[33], base.transform.position);
		}
		while (timer < 0.25f)
		{
			yield return null;
			timer += Time.deltaTime;
			if (connectedToPlayer)
			{
				timer += Time.deltaTime;
			}
			base.transform.localScale = new Vector3(timer * 4f, timer * 4f, timer * 4f);
		}
		StartCoroutine(destroyMe());
	}

	public void setMaterial()
	{
		if (WorldManager.Instance.isPositionOnMap(base.transform.position))
		{
			Material[] materials = myRen.materials;
			materials[0] = WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)]].myTileMaterial;
			myRen.materials = materials;
		}
	}

	private IEnumerator destroyMe()
	{
		yield return new WaitForSeconds(0.5f);
		myHitBox.endAttack();
		noHurtCollider.enabled = true;
		yield return new WaitForSeconds(0.5f);
		float timer = 1f;
		while (timer > 0f)
		{
			yield return null;
			timer -= Time.deltaTime;
			base.transform.localScale = new Vector3(timer, timer, timer);
		}
		Object.Destroy(base.gameObject);
	}
}
