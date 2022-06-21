using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class CharacterController : MonoBehaviour
{	
	public PlayerManager playerManager;
	public MatchManager matchManager;
	public CharacterControllerProxy proxy;
	
	public float MovementRewardMultplier = 1.0f;

	public int  health = 100;
	public new Camera camera;

	public float punishRayLenght = 1.0f;
	int punishCount = 0;

	float currentCameraAngle = 0.0f;
	const float maxCameraAngle = 85.0f;
	const float movementSpeed = 7.0f;
	const int gunDamage = 10;


	Vector3 previousPos;

	#region APIBindings
	public delegate bool GotHitDelegate(int damage);
	public event GotHitDelegate GotHitEvent;
	public event Action<PlayerManager, PlayerManager> GotKilledEvent;
	#endregion

	#region PublicAPI
	bool GotHit(int damage)
	{
		return GotHitEvent.Invoke(damage);
	}
	void GotKilled(PlayerManager killer, PlayerManager player)
	{
		GotKilledEvent.Invoke(killer, player);
	}
	#endregion

	#region LocalImpl
	bool GotHitImpl(int damage)
	{
		health -= damage;
		if (health <= 0)
		{
			return true;
		}
		return false;
	}
	void GotKilledImpl(PlayerManager killer, PlayerManager player)
	{
		// assuming player is this player. local impl does not need to communicate to all players who has died
		Debug.Assert(player == this.playerManager);

		playerManager.DespawnPlayer();
		playerManager.rewardManager.AddReward(-10.0f);

	}
	#endregion

	#region Control
	public void LookHorizontal(float degrees)
	{
		transform.Rotate(Vector3.up, degrees);
	}

	public void LookVertical(float degrees)
	{
		currentCameraAngle -= degrees;
		currentCameraAngle = Mathf.Clamp(currentCameraAngle, -maxCameraAngle, maxCameraAngle);
		camera.transform.localEulerAngles = new Vector3(currentCameraAngle, 0.0f, 0.0f);
	}
	public void Move(Vector2 direction)
	{
		var new_pos = transform.position + (transform.rotation * new Vector3(direction.x, 0.0f, direction.y) * movementSpeed);
		GetComponent<Rigidbody>().MovePosition(new_pos);
	
	}
	public void Shoot(bool inTraining)
	{
		CharacterController target = null;

		RaycastHit hit;
		if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, Mathf.Infinity))
		{
			target = hit.transform.GetComponent<CharacterController>();
		}

		if (inTraining)
		{
			if (target != null)
			{
				if (target.playerManager.myTeam != playerManager.myTeam)
				{
					if (target.GotHit(gunDamage))
					{
						target.GotKilled(playerManager, target.playerManager);
					}
					playerManager.rewardManager.AddReward(8.0f);
				}
			}
		}
		else
		{
			if (target != null)
			{
				if (target.GotHit(gunDamage))
				{
					target.GotKilled(playerManager, target.playerManager);
					playerManager.rewardManager.AddReward(5.0f);
					PunishForTeamKilling(target, -100.0f);
				}
				else
				{
					PunishForTeamKilling(target, -10.0f);
					playerManager.rewardManager.AddReward(0.5f);
				}
			}
			//BroadcastShoot();
		}
	}

	private void PunishForTeamKilling(CharacterController target, float reward)
	{
		if (target.playerManager.myTeam == playerManager.myTeam)
		{
			playerManager.rewardManager.AddReward(reward);
		}
	}
	#endregion

	private void Start()
	{
		matchManager = FindObjectOfType<MatchManager>();
		proxy = GetComponent<CharacterControllerProxy>();

		currentCameraAngle = camera.transform.eulerAngles.x % 360.0f;
		currentCameraAngle = currentCameraAngle > 180.0f ? currentCameraAngle - 360.0f : currentCameraAngle;

		previousPos = transform.position;
		
		if (proxy != null)
		{
			if (proxy.photonView.AmOwner)
			{
				StartCoroutine(RewardOnDistance());
			}
			else
			{
				Destroy(camera.gameObject);
			}
		}
		else
		{
			StartCoroutine(RewardOnDistance());

			GotKilledEvent += GotKilledImpl;
			GotHitEvent += GotHitImpl;
		}
	}

	private void OnDestroy()
	{
		StopCoroutine(RewardOnDistance());
	}

	IEnumerator RewardOnDistance()
	{
		while (true)
		{
			yield return new WaitForSeconds(1.0f);
			var temp = transform.position;
			var reward = (temp - previousPos).magnitude / movementSpeed;
			playerManager.rewardManager.AddReward(reward * MovementRewardMultplier);
			previousPos = temp;
		}
	}
	
	private void FixedUpdate()
	{
		RaycastHit hit;
		if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, punishRayLenght))
		{
			punishCount++;
		}
		else
		{
			punishCount = 0;
		}
		if (punishCount > 5)
		{
			playerManager.rewardManager.AddReward(-1.0f);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(health);
		}
		else
		{
			health = (int)stream.ReceiveNext();
		}
	}
	
	/* GetHitBy
	public void GetHitBy(int otherPlayerViewID, int damage)
	{
		if (proxy.photonView.IsMine)
		{
			health -= damage;
			if (health <= 0)
			{
				player.photonView.RPC("DespawnPlayer", RpcTarget.All);
				matchManager.photonView.RPC("APlayerKilled", RpcTarget.All);
			} 
		}
	}
	*/

	/* BroadcastShoot
	private void BroadcastShoot()
	{
		var audio = GetComponent<AudioSource>();
		audio.Play();
	}
	*/
}
