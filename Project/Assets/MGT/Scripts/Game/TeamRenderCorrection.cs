﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamRenderCorrection : MonoBehaviour
{

	CharacterController Me;
	[SerializeField] Material DefaultMaterial;
	[SerializeField] Material PlayerTeamMaterial;
	[SerializeField] Material EnemyTeamMaterial;
	CharacterController[] characters;
	private void Start()
	{
		Me = transform.parent.GetComponent<CharacterController>();

	}

	private void OnPreRender()
	{
		characters = FindObjectsOfType<CharacterController>();

		Team myTeam = Me.playerManager.myTeam;

		if (myTeam == Team.NONE)
		{
			foreach (var item in characters)
			{
				var renderer = item.GetComponentInChildren<CharacterBody>().GetComponent<MeshRenderer>();
				renderer.material = DefaultMaterial;
			}
			return;
		}

		foreach (var item in characters)
		{
			var renderer = item.GetComponentInChildren<CharacterBody>().GetComponent<MeshRenderer>();
			if (item.playerManager.myTeam == myTeam)
			{
				renderer.material = PlayerTeamMaterial;
			}
			else if (item.playerManager.myTeam == Team.NONE)
			{
				renderer.material = DefaultMaterial;
			}
			else
			{
				renderer.material = EnemyTeamMaterial;
			}
		}
	}
}
