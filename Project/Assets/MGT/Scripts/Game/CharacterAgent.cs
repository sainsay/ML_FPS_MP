using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Demonstrations;

public class CharacterAgent : Agent
{

	public CharacterController character;

	[Header("ML Settings")]
	public TrainingPhase Phase;

	public float MovementReward = 0.001f;
	public float OvertimePenaltyMultiplier = 1.0f;

	private float mouseXinput = 0.0f;
	private float mouseYinput = 0.0f;
	private float movementInputX = 0.0f;
	private float movementInputY = 0.0f;

	private Vector3 startingPos;
	private Quaternion startingRotBody;

	[SerializeField] private RenderTexture RenderTextureTemplate;
	public RenderTexture renderTexture;
	[SerializeField] private string recordingDir = "Demonstrations";
	[SerializeField] private string recordingName = "default";


	public enum TrainingPhase
	{
		Phase1,
		Phase2,
		Phase3
	}

	//public event Action OnReset;
	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(0.0f);
		sensor.AddObservation(0.0f);
		sensor.AddObservation(0.0f);
		base.CollectObservations(sensor);
	}

	public override void Heuristic(float[] actionsOut)
	{
		actionsOut[0] =	mouseXinput;
		actionsOut[1] = movementInputX;
		actionsOut[2] = movementInputY;

		mouseXinput = 0.0f;
		mouseYinput = 0.0f;
		movementInputX = 0.0f;
		movementInputY = 0.0f;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (GetComponent<RenderTextureSensorComponent>().RenderTexture == null)
		{
			setupRendertarget();
		}
	}

	public void setupRendertarget()
	{
		GetComponent<RenderTextureSensorComponent>().RenderTexture = renderTexture = new RenderTexture(RenderTextureTemplate);
	}

	private void Reset()
	{

		//OnReset.Invoke();
	}

	private void Update()
	{
		mouseXinput += Input.GetAxis("Mouse X");
		mouseYinput += Input.GetAxis("Mouse Y");

		movementInputX += Input.GetAxis("Horizontal");
		movementInputY += Input.GetAxis("Vertical");

	}

	public override void OnActionReceived(float[] vectorAction)
	{
		switch (Phase)
		{
			case TrainingPhase.Phase1:
				{
					character.LookHorizontal(vectorAction[0]);
					var move = new Vector2(vectorAction[1], vectorAction[2]).normalized * Time.fixedDeltaTime;
					character.Move(move);
				}
				break;
			case TrainingPhase.Phase2:
				{

					character.LookHorizontal(vectorAction[0]);
					var move = new Vector2(vectorAction[1], vectorAction[2]).normalized * Time.fixedDeltaTime;
					character.Move(move);
					character.Shoot(true);
				}
				break;
			case TrainingPhase.Phase3:
				{
					character.LookHorizontal(vectorAction[0]);
					var move = new Vector2(vectorAction[1], vectorAction[2]).normalized * Time.fixedDeltaTime;
					character.Move(move);
					if (Mathf.RoundToInt(vectorAction[3]) == 1)
					{
						character.Shoot(false);
					}
				}
				break;
			default:
				break;
		}
	}

	public void EnableDemonstration()
	{
		var rec = gameObject.AddComponent<DemonstrationRecorder>();
		rec.DemonstrationName = recordingName;
		rec.DemonstrationDirectory = recordingDir;
		rec.Record = true;
	}

	public override void OnEpisodeBegin()
	{
		Reset();
	}
}
