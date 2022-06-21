using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GameplayMenu : Menu
{
    public delegate MatchScore GetScoreValue();
    public event GetScoreValue getScoreValue;

    public Team myTeam;

    public TMP_Text teamAScore;
    public TMP_Text teamBScore;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (IsOpen)
		{
            var score = getScoreValue.Invoke();
			if (myTeam == Team.A)
			{
                teamAScore.SetText(score.ScoreTeamA.ToString());
                teamBScore.SetText(score.ScoreTeamB.ToString());
			}
			else
			{
                teamAScore.SetText(score.ScoreTeamB.ToString());
                teamBScore.SetText(score.ScoreTeamA.ToString());
            }
		}
    }
}
