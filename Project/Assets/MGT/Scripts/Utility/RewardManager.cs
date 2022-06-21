using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager
{
	PlayerManager player;

	float currentReward = 0.0f;

	public RewardManager(PlayerManager player)
	{
		this.player = player;
	}

	public void AddReward(float reward)
	{
		currentReward += reward;
		if (player.inputType == PlayerManager.InputType.AGENT)
		{
			player.agentInput.AddReward(reward);
		}
	}
}
