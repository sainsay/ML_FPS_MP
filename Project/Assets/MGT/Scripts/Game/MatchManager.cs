using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public enum Team
{
	A,
	B,
	NONE
}

public struct MatchScore
{
	public int ScoreTeamA;
	public int ScoreTeamB;
}

[Serializable]
public class MatchRules
{
	[SerializeField] private int minScoreToWin = 15;
	[SerializeField] private int minScoreDiff = 2;

	public int MinScoreToWin { get => minScoreToWin; }
	public int MinScoreDiff { get => minScoreDiff; }
	public (bool isVictory, Team team) CheckVictory(MatchScore score)
	{
		if (score.ScoreTeamA < MinScoreToWin && score.ScoreTeamB < MinScoreToWin)
		{
			return (false, Team.NONE);
		}

		if (score.ScoreTeamA >= MinScoreToWin && (score.ScoreTeamA - score.ScoreTeamB) >= MinScoreDiff)
		{
			return (true, Team.A);
		}
		else if (score.ScoreTeamB >= MinScoreToWin && (score.ScoreTeamB - score.ScoreTeamA) >= MinScoreDiff)
		{
			return (true, Team.B);
		}
		return (false, Team.NONE);
	}
}

[RequireComponent(typeof(PhotonView))]
public class MatchManager : MonoBehaviourPun, IPunObservable
{
	[SerializeField] private float maxRoundTime = 180.0f;
	[SerializeField] private MatchRules rules;

	private float currentRoundTime = 0.0f;
	private bool roundRunning = false;

	private Team defendingTeam = Team.A;
	private MatchScore score = new MatchScore();

	private float timeLeft = 0.0f;
	private bool firstRound = true;
	private bool inMatch = true;

	private PlayerManager[] playersTeamA = new PlayerManager[3];
	private PlayerManager[] playersTeamB = new PlayerManager[3];

	private Spawnpoint[] spawnpointsDefenders;
	private Spawnpoint[] spawnpointsAttackers;

	#region propperties
	public float MaxRoundTime { get => maxRoundTime; }
	public float CurrentRoundTime { get => currentRoundTime; set => currentRoundTime = value; }
	public bool RoundRunning { get => roundRunning; }
	public MatchScore Score { get => score; }
	public Team DefendingTeam { get => defendingTeam; set => defendingTeam = value; } 
	#endregion

	

	private void Start()
	{
		var me = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player Manager"), Vector3.zero, Quaternion.identity);
		if (photonView.IsMine) // only master has ownership. prevents costly api interaction to request who is room master
		{
			StartCoroutine(M_MatchFlow());
		}
		var points = FindObjectsOfType<Spawnpoint>();
		List<Spawnpoint> att = new List<Spawnpoint>();
		List<Spawnpoint> def = new List<Spawnpoint>();
		foreach (var item in points)
		{
			if (item.transform.parent.name == "Att")
			{
				att.Add(item);
			}
			else
			{
				def.Add(item);
			}
		}
		spawnpointsAttackers = att.ToArray();
		spawnpointsDefenders = def.ToArray();
	}
	float getTimeLeft()
	{
		return Mathf.CeilToInt(this.timeLeft);
	}

	MatchScore getScore()
	{
		return score;
	}

	[PunRPC]
	private void showSyncPlayerScreen()
	{
		var menuManager = FindObjectOfType<MenuManager>();
		if (menuManager != null)
		{
			menuManager.OpenMenu("SyncMenu");
		}

	}


	[PunRPC]
	private void initRoundCountDown()
	{
		var menuManager = FindObjectOfType<MenuManager>();
		if (menuManager != null)
		{
			var menu = (CountDownMenu)menuManager.OpenMenu("RoundCountDownMenu");
			menu.getTimeEvent += getTimeLeft;
		}
	}

	[PunRPC]
	private void releasePlayers()
	{
		var menuManager = FindObjectOfType<MenuManager>();
		if (menuManager != null)
		{
			var menu = (GameplayMenu)menuManager.OpenMenu("InGameMenu");
			menu.getScoreValue += getScore;
			var players = FindObjectsOfType<PlayerManager>();
			PlayerManager me = null; ;
			foreach (var item in players)
			{
				if (item.proxy.photonView.IsMine)
				{
					me = item;
				}
			}
			if (me != null)
			{
				menu.myTeam = me.myTeam;
			}
		}
		if (photonView.IsMine)  // only master has ownership. prevents costly api interaction to request who is room master
		{
			foreach (var item in playersTeamA)
			{
				if (item == null)
				{
					continue;
				}
				item.proxy.photonView.RPC("GiveControl", RpcTarget.All);
			}
			foreach (var item in playersTeamB)
			{
				if (item == null)
				{
					continue;
				}
				item.proxy.photonView.RPC("GiveControl", RpcTarget.All);
			} 
		}
	}

	[PunRPC]
	private void roundEnded()
	{
		if (photonView.IsMine)  // only master has ownership. prevents costly api interaction to request who is room master
		{
			foreach (var item in playersTeamA)
			{
				if (item == null)
				{
					continue;
				}
				item.proxy.photonView.RPC("RemoveControl", RpcTarget.All);
			}
			foreach (var item in playersTeamB)
			{
				if (item == null)
				{
					continue;
				}
				item.proxy.photonView.RPC("RemoveControl", RpcTarget.All);
			}
		}

		// add some delay here.

		roundRunning = false;
	} 	

	[PunRPC]
	private void matchEnded()
	{

	}

	private IEnumerator M_MatchFlow()
	{
		photonView.RPC("showSyncPlayerScreen", RpcTarget.All);
		timeLeft = 2.0f;
		while (timeLeft > 0.0f) // wait for all the players
		{
			timeLeft -= Time.deltaTime;
			yield return null;
		}

		while (inMatch)
		{
			if (!roundRunning)
			{
				if (firstRound)
				{
					var players = FindObjectsOfType<PlayerManager>();
					bool b = true;
					int i = 0;
					foreach (var item in players)
					{
						if (b)
						{
							playersTeamA[i] = item;
							item.myTeam = Team.A;
							b = false;
						}
						else
						{
							playersTeamB[i] = item;
							item.myTeam = Team.B;
							b = true;
							i++;
						}
					}
				}

				M_SetupRound();
				photonView.RPC("initRoundCountDown", RpcTarget.All);
				timeLeft = 5.0f;
				while (timeLeft > 0.0f)
				{
					timeLeft -= Time.deltaTime;
					yield return null;
				}
				M_StartRound();
			}
			else
			{
				



			}
			yield return null;
		}

	}

	[PunRPC]
	public void APlayerKilled()
	{
		if (photonView.IsMine) // only master has ownership. prevents costly api interaction to request who is room master
		{
			bool team_killed = true;
			foreach (var item in playersTeamA)
			{
				if (item ==null)
				{
					continue;
				}
				if (item.playerCharacter != null)
				{
					team_killed = false;
					break;
				}
			}
			if (team_killed)
			{
				M_ScoredPoint(Team.B);
				return;
			}

			team_killed = true;
			foreach (var item in playersTeamB)
			{
				if (item.playerCharacter != null)
				{
					team_killed = false;
					break;
				}
			}
			if (team_killed)
			{
				M_ScoredPoint(Team.A);
				return;
			}
		}
	}

	public void M_SetupRound()
	{
		defendingTeam = defendingTeam == Team.A ? Team.B : Team.A;
		for (int i = 0; i < playersTeamA.Length; i++)
		{
			if (playersTeamA[i] == null)
			{
				continue;
			}
			var point = (defendingTeam == Team.A ? spawnpointsDefenders[i] : spawnpointsAttackers[i]);
			playersTeamA[i].proxy.photonView.RPC("SpawnPlayer", RpcTarget.All, point.transform.position, point.transform.rotation, false );
		}
		for (int i = 0; i < playersTeamB.Length; i++)
		{
			if (playersTeamB[i] == null)
			{
				continue;
			}
			var point = (defendingTeam == Team.B ? spawnpointsDefenders[i] : spawnpointsAttackers[i]);
			playersTeamB[i].proxy.photonView.RPC("SpawnPlayer", RpcTarget.All, point.transform.position, point.transform.rotation, false);
		}

	}

	public void M_StartRound()
	{
		roundRunning = true;
		currentRoundTime = 0.0f;
		photonView.RPC("releasePlayers", RpcTarget.All);
	}

	public void M_ScoredPoint(Team team)
	{
		switch (team)
		{
			case Team.A:
				++score.ScoreTeamA;
				break;
			case Team.B:
				++score.ScoreTeamB;
				break;
			case Team.NONE:
			default:
				break;
		}
		var matchResult = rules.CheckVictory(score);
		if (matchResult.isVictory)
		{
			photonView.RPC("matchEnded", RpcTarget.All);
		}
		else
		{
			photonView.RPC("roundEnded", RpcTarget.All);
		}
	}

#region DataSharing

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(currentRoundTime);
			stream.SendNext(roundRunning);
			stream.SendNext(defendingTeam);
			stream.SendNext(score.ScoreTeamA);
			stream.SendNext(score.ScoreTeamB);
			stream.SendNext(timeLeft);
			stream.SendNext(firstRound);
			stream.SendNext(inMatch);
		}
		else
		{
			currentRoundTime = (float)stream.ReceiveNext();
			roundRunning = (bool)stream.ReceiveNext();
			defendingTeam = (Team)stream.ReceiveNext();
			score.ScoreTeamA = (int)stream.ReceiveNext();
			score.ScoreTeamB = (int)stream.ReceiveNext();
			timeLeft = (float)stream.ReceiveNext();
			firstRound = (bool)stream.ReceiveNext();
			inMatch = (bool)stream.ReceiveNext();
		}
	}

	[PunRPC]
	void SetPlayerLists(PlayerManager[] teamA, PlayerManager[] teamB)
	{
		playersTeamA = teamA;
		playersTeamB = teamB;
	}

#endregion
}