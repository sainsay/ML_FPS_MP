using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Policies;

public class AimTrainer : MonoBehaviour
{
    public GameObject playerManagerPrefab;
    public int MaxPlayers = 6;
    public float episodeLength = 10.0f;
    public Spawnpoint[] spawnpoints;
    public PlayerManager[] players;

    public bool IsRecording = false;
    public GameObject recordingCam;

    public bool IsObserving = false;
    public int ObservePlayerId = 0;

    float timeStartEpisode = 0.0f;

    private bool mouseLocked = false;

    // Start is called before the first frame update
    void Start()
    {
        spawnpoints = FindObjectsOfType<Spawnpoint>();
        List<PlayerManager> temp = new List<PlayerManager>();

        int managersToSpawn = Mathf.Min(MaxPlayers, spawnpoints.Length);

        for (int i = 0; i < managersToSpawn; ++i)
        {
            temp.Add(Instantiate(playerManagerPrefab).GetComponent<PlayerManager>());
        }
        players = temp.ToArray();
        foreach (var item in players)
        {
            item.myTeam = Team.NONE;
        }

        StartCoroutine(Training());
    }

    IEnumerator RoundStillGoing()
	{

        bool isRunning = true;

		while (isRunning)
		{
            yield return new WaitForFixedUpdate();

            int teamCountA = 0;
            int teamCountB = 0;
			foreach (var item in players)
			{
				if (item.playerCharacter != null)
				{
					switch (item.myTeam)
					{
						case Team.A:
							teamCountA++;
							break;
						case Team.B:
							teamCountB++;
							break;
						case Team.NONE:
							break;
						default:
							break;
					} 
				}
			}
			if (teamCountA == 0 || teamCountB == 0)
			{
                isRunning = false;
			}

			if (Time.time - timeStartEpisode >= episodeLength)
			{
                isRunning = false;
			} 
		}
	}

    IEnumerator Training()
    {
        var pointOfIntrest = FindObjectsOfType<CapturePoint>();

        //skip the first frame
        yield return null;

        


        while (true)
        {
            SpawnPlayers();
            foreach (var item in players)
            {
                item.agentInput.Phase = CharacterAgent.TrainingPhase.Phase2;
            }
            if (IsRecording)
            {
                foreach (var item in players)
                {
                    item.agentInput.Phase = CharacterAgent.TrainingPhase.Phase1;
                    var behaviour = item.agentInput.GetComponent<BehaviorParameters>();
                    Debug.Assert(behaviour != null);
                    behaviour.BehaviorType = BehaviorType.InferenceOnly;
                    behaviour.TeamId = 0;
                }
                players[0].agentInput.Phase = CharacterAgent.TrainingPhase.Phase2;
                var p0behaviour = players[0].agentInput.GetComponent<BehaviorParameters>();
                Debug.Assert(p0behaviour != null);
                p0behaviour.BehaviorType = BehaviorType.HeuristicOnly;
                p0behaviour.TeamId = 0;
            }
            timeStartEpisode = Time.time;

            yield return StartCoroutine(RoundStillGoing());
            
            // finishing the episode
            foreach (var item in players)
            {
				if (item.playerCharacter != null)
				{
					var pos = item.playerCharacter.transform.position;
					float dist = float.MaxValue;

					foreach (var point in pointOfIntrest)
					{
						var temp = (pos - point.transform.position).magnitude;
						dist = temp < dist ? temp : dist;
					}
					if (dist < 10.0f)
					{
						item.rewardManager.AddReward((10.0f - dist));
					} 
				}
                item.agentInput.EndEpisode();
            }
        }
    }

    void SpawnPlayers()
    {
        List<Spawnpoint> temp = new List<Spawnpoint>(spawnpoints);
        foreach (var item in players)
        {
            int rnd = Random.Range(0, temp.Count - 1);
            Spawnpoint point = temp[rnd];
            temp.RemoveAt(rnd);
			switch (point.transform.parent.name)
			{
                case "Def":
                    item.myTeam = Team.A;
                    break;
                case "Att":
                    item.myTeam = Team.B;
                    break;
				default:
					break;
			}

			item.SpawnPlayer(point.transform.position, point.transform.rotation, true);

        }
        if (IsRecording)
        {
            players[0].GiveControl();
            //players[0].playerCharacter.camera.depth = 1.0f;
            Instantiate(recordingCam, players[0].playerCharacter.camera.gameObject.transform, false);
            //players[0].EnableDemonstrationRecording();
		}
		if (IsObserving)
		{
            Instantiate(recordingCam, players[ObservePlayerId].playerCharacter.camera.gameObject.transform, false);
        }
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (mouseLocked)
			{
                Cursor.lockState = CursorLockMode.None;
			}
			else
			{
                Cursor.lockState = CursorLockMode.Locked;
            }
            mouseLocked = !mouseLocked; 
		}
    }
}
