using System;

[Serializable]
public class GameSettingData
{
    public string gameMode; // "time", "score"
    public int timeLimit;
    public int scoreLimit;

    public GameSettingData(string gameMode, int timeLimit, int scoreLimit)
    {
        this.gameMode = gameMode;
        this.timeLimit = timeLimit;
        this.scoreLimit = scoreLimit;
    }
}