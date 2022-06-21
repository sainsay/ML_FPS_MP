using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.IO;
using System;

[RequireComponent(typeof(PlayerManager))]
public class PlayerManagerProxy : MonoBehaviourPun
{
    PlayerManager manager;

    public void BindFuntions()
	{

	}



	private void Awake()
	{
        manager = GetComponent<PlayerManager>();
	}

    [PunRPC]
    public void SpawnPlayer(Vector3 location, Quaternion rotation, bool hasControl)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (manager.playerCharacter != null)
        {
            photonView.RPC("DespawnPlayer", RpcTarget.All);
        }
        manager.playerCharacter = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Character"), location, rotation).GetComponent<CharacterController>();
        manager.playerCharacter.playerManager = manager;
        manager.SetupInput(hasControl);
    }

    public void DestroyPlayer()
    {
        PhotonNetwork.Destroy(  manager.playerCharacter.gameObject.GetComponent<PhotonView>());
    }

    [PunRPC]
    public void DespawnPlayer()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(manager.playerCharacter.proxy.photonView);
            manager.DespawnPlayer();
        }
    }

    [PunRPC]
    public void GiveControl()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        manager.GiveControl();
    }

    [PunRPC]
    public void RemoveControl()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        manager.RemoveControl();
    }

}
