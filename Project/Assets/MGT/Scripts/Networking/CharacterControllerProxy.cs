using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerProxy : MonoBehaviourPun, IPunObservable
{
	CharacterController controller;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		throw new System.NotImplementedException();
	}

	[PunRPC]
	public void GetHitBy(int otherPlayerViewID, int damage)
	{
		if (photonView.IsMine)
		{
			controller.health -= damage;
			if (controller.health <= 0)
			{
				controller.playerManager.proxy.photonView.RPC("DespawnPlayer", RpcTarget.All);
				controller.matchManager.photonView.RPC("APlayerKilled", RpcTarget.All);
			}
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
