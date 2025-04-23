using System;
using System.Collections.Generic;

[Serializable]
public class Player
{
    public string id;
    public string name;
    public bool isReady;
    public int score;
    public Dictionary<string, string> gameData; // "게임 커스텀 데이터"

    public Player(string id, string name)
    {
        this.id = id;
        this.name = name;
        this.isReady = false;
        this.score = 0;
        this.gameData = new Dictionary<string, string>();
    }
}