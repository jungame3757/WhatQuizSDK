using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SessionData
{
    public string hostId;
    public long createdAt;
    public long expiresAt;
    public GameSettingData gameSetting;
    public List<Player> players;
    public string sessionStatus; // "waiting", "playing", "finished"
    public string gameStatus; // "커스텀으로 설정"

    public SessionData(string hostId, string gameStatus)
    {
        this.hostId = hostId;
        createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        expiresAt = createdAt + (3 * 60 * 60 * 1000); // 3시간 후 만료
        gameSetting = new GameSettingData("normal", 0, 0);
        players = new List<Player> {};
        sessionStatus = "waiting";
        this.gameStatus = gameStatus;
    }
}