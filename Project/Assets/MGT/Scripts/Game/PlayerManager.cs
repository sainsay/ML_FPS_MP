using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;

using Unity.MLAgents.Sensors;

public class PlayerManager : MonoBehaviour
{
	public enum InputType
	{
        PLAYER,
        AGENT
	}

	[SerializeField] GameObject characterPrefab;
    [SerializeField] GameObject inputPrefab;

    public CharacterController playerCharacter;
    public PlayerInput playerInput;
    public CharacterAgent agentInput;
    public InputType inputType = InputType.AGENT;

    public Team myTeam;

	public PlayerManagerProxy proxy;

	public RewardManager rewardManager;

	#region APIBindings
	public event Action<Vector3, Quaternion, bool> SpawnPlayerEvent;
	public event Action<bool> SetupInputEvent;
	public event Action DespawnPlayerEvent;
	public event Action GiveControlEvent;
	public event Action RemoveControlEvent;
	#endregion

	#region PublicAPI
	public void SpawnPlayer(Vector3 location, Quaternion rotation, bool hasControl)
	{
		SpawnPlayerEvent.Invoke(location, rotation, hasControl);
	}
	public void SetupInput(bool hasControl) {
		SetupInputEvent.Invoke(hasControl);
	}
	public void DespawnPlayer() {
		DespawnPlayerEvent.Invoke();
	}
	public void GiveControl() {
		GiveControlEvent.Invoke();
	}
	public void RemoveControl() {
		RemoveControlEvent.Invoke();
	}
	#endregion

	#region LocalImpl
	private void SpawnPlayerImpl(Vector3 location, Quaternion rotation, bool hasControl)
	{
		if (playerCharacter != null)
		{
			DespawnPlayer();
		}
		playerCharacter = Instantiate(characterPrefab, location, rotation).GetComponent<CharacterController>();
		playerCharacter.playerManager = this;
		SetupInput(hasControl);
	}
	private void SetupInputImpl(bool hasControl)
	{
		switch (inputType)
		{
			case InputType.PLAYER:
				{
                    if (playerInput == null)
                    {
                        playerInput = Instantiate(inputPrefab).GetComponent<PlayerInput>();
                    }
                    playerInput.enabled = hasControl;
					playerInput.character = playerCharacter;
                }
				break;
			case InputType.AGENT:
				{
					if (agentInput == null)
					{
						agentInput = Instantiate(inputPrefab).GetComponent<CharacterAgent>();
					}
					agentInput.character = playerCharacter;
					if (agentInput.renderTexture == null)
					{
						agentInput.setupRendertarget();
					}

					agentInput.character.camera.targetTexture = agentInput.renderTexture;
					agentInput.enabled = hasControl;
				}
				break;
			default:
				break;
		}
	}
	private void DespawnPlayerImpl()
	{
		DestroyImmediate(playerCharacter.gameObject);
		playerCharacter = null;
		switch (inputType)
		{
			case InputType.PLAYER:
				playerInput.character = null;
				break;
			case InputType.AGENT:
				agentInput.character = null;
				agentInput.enabled = false;
				break;
			default:
				break;
		}
	}
	private void GiveControlImpl()
    {
		switch (inputType)
		{
			case InputType.PLAYER:
				{
                    playerInput.enabled = true;
                }
				break;
			case InputType.AGENT:
				{
					agentInput.enabled = true;
                }
				break;
			default:
				break;
		}
    }
	private void RemoveControlImpl()
    {
		switch (inputType)
		{
			case InputType.PLAYER:
				{
					playerInput.enabled = false;
				}
				break;
			case InputType.AGENT:
				{
					agentInput.enabled = false;
				}
				break;
			default:
				break;
		}
	}
	#endregion

	public void EnableDemonstrationRecording()
	{
		agentInput.EnableDemonstration();
	}

	// Start is called before the first frame update
	void Start()
	{
		rewardManager = new RewardManager(this);

		proxy = GetComponent<PlayerManagerProxy>();

		if (proxy != null)
		{
			proxy.BindFuntions();
		}
		else
		{
			SpawnPlayerEvent += SpawnPlayerImpl;
			SetupInputEvent += SetupInputImpl;
			DespawnPlayerEvent += DespawnPlayerImpl;
			GiveControlEvent += GiveControlImpl;
			RemoveControlEvent += RemoveControlImpl;
		}

	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
