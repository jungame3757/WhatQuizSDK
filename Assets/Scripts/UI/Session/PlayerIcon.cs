using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerIcon : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private Button _kickButton;
    
    private Player _playerData;
    private string _playerId;
    private string _localPlayerId;
    private Action<string> _onKickPlayer;
    private bool _isLocalPlayerHost;
    
    public void Initialize(Player player, string localPlayerId, bool isLocalPlayerHost, Action<string> onKickPlayer)
    {
        _playerData = player;
        _playerId = player.id;
        _localPlayerId = localPlayerId;
        _isLocalPlayerHost = isLocalPlayerHost;
        _onKickPlayer = onKickPlayer;
        
        // 플레이어 이름 표시
        _playerNameText.text = player.name;
        
        // 킥 버튼 설정
        if (_kickButton != null)
        {
            // 호스트만 다른 플레이어를 킥할 수 있음
            bool canKick = _isLocalPlayerHost && !IsLocalPlayer();
            _kickButton.gameObject.SetActive(canKick);
            
            if (canKick)
            {
                _kickButton.onClick.RemoveAllListeners();
                _kickButton.onClick.AddListener(OnKickButtonClicked);
            }
        }
    }
    
    public void UpdatePlayerData(Player player)
    {
        if (player != null && player.id == _playerId)
        {
            _playerData = player;
            _playerNameText.text = player.name;
        }
    }
    
    public string GetPlayerId()
    {
        return _playerId;
    }
    
    private bool IsLocalPlayer()
    {
        return !string.IsNullOrEmpty(_localPlayerId) && _localPlayerId == _playerId;
    }
    
    private void OnKickButtonClicked()
    {
        if (_onKickPlayer != null && !string.IsNullOrEmpty(_playerId))
        {
            _onKickPlayer.Invoke(_playerId);
        }
    }
}
