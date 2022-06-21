using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.IO;

public class NetworkedGameManager : MonoBehaviourPunCallbacks
{
	const string playerNamePrefKey = "PlayerName";
	const int maxPlayers = 6;

	bool timerRunning = false;
	float timeLeft = 0.0f;

	[SerializeField] private TMP_Text playerCountText;
	[SerializeField] private TMP_Text countDownText;
	[SerializeField] GameObject startGameButton;



	private void Start()
	{
		Debug.Log("Connecting to main server");
		PhotonNetwork.ConnectUsingSettings();
		DontDestroyOnLoad(this);
	}

	private void Update()
	{
		var room = PhotonNetwork.CurrentRoom;

		if (room != null)
		{
			playerCountText.SetText( "Players: " + room.PlayerCount + "/" + maxPlayers);
			//countDownText.SetText("Match Starts in: " + timeLeft + " seconds");
		}
		
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to main server");

		Debug.Log("Connecting to game server");
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true;

	}

	public override void OnJoinedLobby()
	{
		Debug.Log("Connected to game server");


		Debug.Log("current user id: " + PhotonNetwork.LocalPlayer.UserId);
		var menuManager = FindObjectOfType<MenuManager>();
		if (menuManager != null)
		{
			menuManager.OpenMenu("MainMenu");
		}

	}

	private void CreateRoom()
	{
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = maxPlayers;
		PhotonNetwork.CreateRoom(null, roomOptions, null);
	}

	public void JoinRoom()
	{
		Debug.Log("Joining random room");
		PhotonNetwork.JoinRandomRoom();
	}

	private IEnumerator timerCoroutine()
	{
		while (timeLeft > 0.0f)
		{
			timeLeft -= Time.deltaTime;
			yield return null;
		}
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.LoadLevel(1);
			while (PhotonNetwork.LevelLoadingProgress < 1.0f)
			{
				yield return null;
			}
			PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Match Manager"), Vector3.zero, Quaternion.identity);
		}
		var menuManager = FindObjectOfType<MenuManager>();
		if (menuManager != null)
		{
			menuManager.CloseMenu("RoomMenu");
		}
	}

	[PunRPC]
	public void startTimer()
	{
		timeLeft = 5.0f;
		StartCoroutine(timerCoroutine());
	}

	public void StartMatch()
	{
		GetComponent<PhotonView>().RPC("startTimer", RpcTarget.All);
	}

	float getTimeValue()
	{
		return this.timeLeft;
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Connected to room");

		var menuManager = FindObjectOfType<MenuManager>();
		if (menuManager != null)
		{
			var menu = (CountDownMenu)menuManager.OpenMenu("RoomMenu");
			menu.getTimeEvent += getTimeValue;
		}

		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug.Log("No random room availible");
		Debug.Log("Creating new room");
		CreateRoom();
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		base.OnJoinRoomFailed(returnCode, message);
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		base.OnMasterClientSwitched(newMasterClient);
		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{

		base.OnPlayerEnteredRoom(newPlayer);
	}

}
