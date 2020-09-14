using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;


public class TicTacToeArea : MonoBehaviour
{
	int TEAM_ID_O = 0;
	int TEAM_ID_X = 1;
	public int teamTurn = -1;
	int TEAM_TURN_NONE = -1;

	public bool isHeuristicPause = false;
	float fixedHeuristicPauseTime = 2.0f;
	float currentHeuristicPauseTime = 0.0f;

	int[] gameBoardArray; //empty spot for 0, 1 for O, -1 for X
	int SQUARE_EMPTY = 0;
	int SQUARE_O = 1;
	int SQUARE_X = -1;

	Dictionary<int, int> teamValue; //team O places 1's on the gameBoardArray to signify O's and team X places -1 on the gameboard to signify X's
	public TicTacToeAgent m_AgentO;
	public TicTacToeAgent m_AgentX;

	//game board indicators, alternatively could instantiate prefab on each move and move piece where needed
	public GameObject[] markerOArray;
	public GameObject[] markerXArray;

	int GAME_ONGOING = -1;
	int GAME_DRAW = 0;
	int GAME_WON = 1;
	
	public void Start()
	{
		teamTurn = TEAM_TURN_NONE;
		teamValue = new Dictionary<int, int>();
		teamValue.Add(TEAM_ID_O, SQUARE_O);
		teamValue.Add(TEAM_ID_X, SQUARE_X);
		ResetBoard();

	}

	public void FixedUpdate()
	{
		if (isHeuristicPause)
		{
			if (currentHeuristicPauseTime > fixedHeuristicPauseTime)
			{
				isHeuristicPause = false;
				currentHeuristicPauseTime = 0.0f;
			}
			currentHeuristicPauseTime += Time.fixedDeltaTime;
		}
	}

	public int[] GetObservations()
	{
		return gameBoardArray;
	}

	//updates game board based on the action
	public void TakeAction(int index, int teamId, bool isForceAction)
	{
		//check that input is valid (ie not already something placed there)
		if(isForceAction && gameBoardArray[index] != SQUARE_EMPTY)
		{
			//random action from available squares
			List<int> actionList = new List<int>();
			for(int i = 0; i<9; i++)
			{
				if (gameBoardArray[i] == SQUARE_EMPTY)
					actionList.Add(i);
			}
			if(actionList.Count > 0)
			{
				int randomInt = UnityEngine.Random.Range(0, actionList.Count);
				index = actionList[randomInt];
			}
		}

		if(gameBoardArray[index] == SQUARE_EMPTY)
		{
			UpdateBoard(index, teamId);
			int gameResult = GetGameResult(index);
			if (gameResult == GAME_ONGOING)
			{
				//next player's turn
				teamTurn = (teamTurn + 1) % 2; 
				return;
			}
			else if (gameResult == GAME_DRAW)
			{
				teamTurn = TEAM_TURN_NONE;
				AgentDraws();
			}
			else
			{
				teamTurn = TEAM_TURN_NONE;
				if (teamId == TEAM_ID_O)
					AgentOWins();
				else
					AgentXWins();
			}
		}
		
	}

	//update the display board and the internal gameBoardArray
	void UpdateBoard(int index, int teamId)
	{
		int boardSymbol = teamValue[teamId];
		gameBoardArray[index] = boardSymbol;
		//show the move being placed
		if (teamId == TEAM_ID_O)
			markerOArray[index].SetActive(true);
		else
			markerXArray[index].SetActive(true);
	}

	#region GameFinishedCheck
	//based on last move, checks if there's a winner
	int GetGameResult(int index)
	{

		//checks for three in a row in proper gameBoard indices based on last move
		if( index == 0)
		{
			if ( IsThreeInARow(0,1,2) || IsThreeInARow(0, 3, 6) || IsThreeInARow(0, 4, 8)) //IsTopRow() || IsLeftColumn() || IsDiagnolTopLeft()
				return GAME_WON;
		}
		else if(index == 1)
		{
			if (IsThreeInARow(0, 1, 2) || IsThreeInARow(1, 4, 7)) //IsTopRow() || IsMiddleColumn() )
				return GAME_WON;
		}
		else if (index == 2)
		{
			if (IsThreeInARow(0, 1, 2) || IsThreeInARow(2, 5, 8) || IsThreeInARow(2, 4, 6)) //IsTopRow() || IsRightColumn() || IsDiagnolBottomLeft() )
				return GAME_WON;
		}
		else if (index == 3)
		{
			if (IsThreeInARow(3, 4, 5) || IsThreeInARow(0, 3, 6)) //IsMiddleRow() || IsLeftColumn())
				return GAME_WON;
		}
		else if (index == 4)
		{
			if (IsThreeInARow(3, 4, 5) || IsThreeInARow(1, 4, 7) || IsThreeInARow(0, 4, 8) || IsThreeInARow(2, 4, 6)) //(IsMiddleRow() || IsMiddleColumn() || IsDiagnolTopLeft() || IsDiagnolBottomLeft())
				return GAME_WON;
		}
		else if (index == 5)
		{
			if (IsThreeInARow(3, 4, 5) || IsThreeInARow(2, 5, 8)) //(IsMiddleRow() || IsRightColumn())
				return GAME_WON;
		}
		else if (index == 6)
		{
			if (IsThreeInARow(6, 7, 8) || IsThreeInARow(0, 3, 6) || IsThreeInARow(2, 4, 6)) //(IsBottomRow() || IsLeftColumn() || IsDiagnolBottomLeft() )
				return GAME_WON;
		}
		else if (index == 7)
		{
			if (IsThreeInARow(6, 7, 8) || IsThreeInARow(1, 4, 7)) //(IsBottomRow() || IsMiddleColumn())
				return GAME_WON;
		}
		else if (index == 8)
		{
			if (IsThreeInARow(6, 7, 8) || IsThreeInARow(2, 5, 8) || IsThreeInARow(0, 4, 8)) //(IsBottomRow() || IsRightColumn() || IsDiagnolTopLeft())
				return GAME_WON;
		}

		//check for draw
		int boardSum = 0;
		for (int i = 0; i < 9; i++)
		{
			if (gameBoardArray[i] != 0)
				boardSum += 1;
		}
		if (boardSum == 9)
			return GAME_DRAW;

		return GAME_ONGOING;
	}

	//checks that all three indices have X or O
	bool IsThreeInARow(int index1, int index2, int index3)
	{
		int threeSum = gameBoardArray[index1] + gameBoardArray[index2] + gameBoardArray[index3];
		if (threeSum == 3 || threeSum == -3)
			return true;
		return false;
	}

	#endregion

	void AgentOWins()
	{
		Debug.Log("O wins, resetting");
		m_AgentO.SetReward(1);
		m_AgentX.SetReward(-1);
		Reset();
	}

	void AgentXWins()
	{
		Debug.Log("X wins, resetting");
		m_AgentO.SetReward(-1);
		m_AgentX.SetReward(1);
		Reset();
	}

	void AgentDraws()
	{
		Debug.Log("game is a draw, resetting");
		m_AgentO.SetReward(0);
		m_AgentX.SetReward(0);
		Reset();
	}

	private void Reset()
	{
		m_AgentO.EndEpisode();
		m_AgentX.EndEpisode();
		ResetBoard();
	}

	void ResetBoard()
	{
		isHeuristicPause = true;

		//clear gameBoardArray
		gameBoardArray = new int[9] { SQUARE_EMPTY, SQUARE_EMPTY, SQUARE_EMPTY, SQUARE_EMPTY, SQUARE_EMPTY, SQUARE_EMPTY, SQUARE_EMPTY, SQUARE_EMPTY, SQUARE_EMPTY };
		//set game board markers to inactive
		for(int i = 0; i < 9; i++)
		{
			markerOArray[i].SetActive(false);
			markerXArray[i].SetActive(false);
		}

		//determine who goes first next game
		teamTurn = Random.Range(0, 2);
	}

}
