using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingTargetManager : MonoBehaviour
{
	public Vector3 SpawnAreaSize;
	public GameObject Target;
	public float MinSpawnDelay = 1.0f;
	public float MaxSpawnDelay = 5.0f;
	public int MaxScore = 15;

	public CharacterAgent agent;

	private List<SimpleShootingTarget> currentTargets = new List<SimpleShootingTarget>();
	private int score = 0;
	

	private void Awake()
	{
		//agent.OnReset += Reset;

		StartCoroutine(nameof(Spawn));
	}

	private IEnumerator Spawn()
	{
		if (currentTargets.Count >= 5)
		{
			agent.AddReward(-1.0f);
			agent.EndEpisode();
		}
		else
		{
			var spawned = Instantiate(Target, new Vector3(	UnityEngine.Random.Range(transform.position.x - (SpawnAreaSize.x / 2.0f), transform.position.x + (SpawnAreaSize.x / 2.0f)), 
															UnityEngine.Random.Range(transform.position.y - (SpawnAreaSize.y / 2.0f), transform.position.y + (SpawnAreaSize.y / 2.0f)), 
															UnityEngine.Random.Range(transform.position.z - (SpawnAreaSize.z / 2.0f), transform.position.z + (SpawnAreaSize.z / 2.0f))
															), transform.rotation, transform).GetComponent<SimpleShootingTarget>();
			currentTargets.Add(spawned);
			spawned.OnDeath += targetCallback;
		}

		yield return new WaitForSeconds(Random.Range(MinSpawnDelay, MaxSpawnDelay));
		StartCoroutine(nameof(Spawn));
	}

	void targetCallback(SimpleShootingTarget target)
	{
		Destroy(target.gameObject);
		currentTargets.Remove(target);

		++score;
		if (score == MaxScore )
		{
			agent.EndEpisode();
		}
	}

	private void Reset()
	{
		for (int i = currentTargets.Count - 1; i >= 0; i--)
		{
			Destroy(currentTargets[i].gameObject);
		}
		currentTargets.Clear();
		score = 0;
	}

}
