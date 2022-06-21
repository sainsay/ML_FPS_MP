using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTrainer : MonoBehaviour
{
    public GameObject playerManagerPrefab;
    public int MaxPlayers = 6;
    public float episodeLength = 10.0f;
    public Spawnpoint[] spawnpoints;
    public PlayerManager[] players;

    public bool IsRecording = false;
    public GameObject recordingCam;

    // Start is called before the first frame update
    void Start()
    {
        spawnpoints = FindObjectsOfType<Spawnpoint>();
        List<PlayerManager> temp = new List<PlayerManager>();

        int managersToSpawn = Mathf.Min(MaxPlayers, spawnpoints.Length);

		for(int i = 0; i < managersToSpawn; ++i)
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

    IEnumerator Training()
	{
        var pointOfIntrest = FindObjectsOfType<CapturePoint>();

        //skip the first frame
        yield return null;


        while (true)
		{
			SpawnPlayers();

			yield return new WaitForSeconds(episodeLength);
			foreach (var item in players)
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
            item.SpawnPlayer(point.transform.position, point.transform.rotation, true);
            

			if (IsRecording) {
                item.RemoveControl();
            }
		}
		if (IsRecording)
		{
            players[0].GiveControl();
			//players[0].playerCharacter.camera.depth = 1.0f;
            Instantiate(recordingCam, players[0].playerCharacter.camera.gameObject.transform, false );
            players[0].EnableDemonstrationRecording();
		}
	}


    // Update is called once per frame
    void Update()
    {
        
    }
}
