using System;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine.Serialization;
using Unity.MLAgents.Sensors;

public class TicTacToeAgent : Agent
{
	[FormerlySerializedAs("m_Area")]
	[Header("Specific to TicTacToe")]
	public TicTacToeArea area;

	int TEAM_ID_O = 0;
	int TEAM_ID_X = 1;
	int teamId;
	int NO_ACTION = 0;
	int TEAM_TURN_O = 0;
	int TEAM_TURN_X = 1;
	BehaviorType behaviorType;
	BehaviorParameters m_BehaviorParameters;

    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;

    public override void Initialize()
    {
		m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
		teamId = m_BehaviorParameters.TeamId;
		behaviorType = m_BehaviorParameters.BehaviorType;
    }

	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(m_BehaviorParameters.TeamId);
		var tempBoard = area.GetObservations();
		for (int i = 0; i < 9; i++)
		{
			sensor.AddObservation(tempBoard[i]);
		}
	}

	// to be implemented by the developer
	public override void OnActionReceived(float[] vectorAction)
	{
		var action = Mathf.FloorToInt(vectorAction[0]);
		if (behaviorType == BehaviorType.HeuristicOnly)
		{
			if (action != NO_ACTION)
			{
				area.TakeAction(action - 1, teamId, false); //add 1 to action in heuristic	
			}
			return;
		}
		else
		{
			//non heuristic mode, agent must take an action
			if (action == NO_ACTION)
			{
				//force random action
				action = UnityEngine.Random.Range(0, 9);
				area.TakeAction(action, teamId, true);
			}
			else
			{
				area.TakeAction(action - 1, teamId, true); //subtract 1 to turn 1 to 9 into 0 to 8, ie board indices
			}
		}

	}

	public override void Heuristic(float[] actionsOut)
    {
		//0 is no action, rest are indices of the gameboard
		if (Input.GetKey(KeyCode.Q))
		{
			actionsOut[0] = 1;
		}
		else if (Input.GetKey(KeyCode.W))
		{
			actionsOut[0] = 2;
		}
		else if (Input.GetKey(KeyCode.E))
		{
			actionsOut[0] = 3;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			actionsOut[0] = 4;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			actionsOut[0] = 5;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			actionsOut[0] = 6;
		}
		else if (Input.GetKey(KeyCode.Z))
		{
			actionsOut[0] = 7;
		}
		else if (Input.GetKey(KeyCode.X))
		{
			actionsOut[0] = 8;
		}
		else if (Input.GetKey(KeyCode.C))
		{
			actionsOut[0] = 9;
		}
	}

	// to be implemented by the developer
	public override void OnEpisodeBegin()
	{
		//area.ResetBoard();
		//don't think it's needed in this example because I'm resetting the board in TicTacToeArea.cs on game being finished
	}

	public void FixedUpdate()
    {
        WaitTimeInference();
    }

    void WaitTimeInference()
    {
        if (Academy.Instance.IsCommunicatorOn)
        {
			if ((area.teamTurn == TEAM_TURN_O && teamId == TEAM_ID_O) || (area.teamTurn == TEAM_TURN_X && teamId == TEAM_ID_X))
			{
				RequestDecision();
			}
		}
        else
        {

			if (m_TimeSinceDecision >= timeBetweenDecisionsAtInference)
			{
				m_TimeSinceDecision = 0f;
				if ( (area.teamTurn == TEAM_TURN_O && teamId == TEAM_ID_O) || (area.teamTurn == TEAM_TURN_X && teamId == TEAM_ID_X) )
				{
					//heuristic mode need break between games to ensure proper action
					if (behaviorType == BehaviorType.HeuristicOnly && area.isHeuristicPause)
					{
						return;
					}
					RequestDecision();
				}
			}
			else
			{
				m_TimeSinceDecision += Time.fixedDeltaTime;
			}
		}
    }

	
}
