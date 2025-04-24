using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;

namespace Game.ChatchThiefFart
{
    public class PlayerCharacter : MonoBehaviour
    {
        public string PlayerId;
        public string PlayerName;
        [SerializeField] private CharacterController[] _characterControllers;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _thiefFartAudioClip;
        [SerializeField] private AudioClip[] _normalFartAudioClips;
        [SerializeField] private TMP_Text _numberText;
        private int _currentCharacterIndex = 0;
        private bool _isThief = false;

        //테스트 코드드
        private Coroutine _fartCoroutine;
        private bool _isFartingEnabled = false;
        public bool IsThief;

        private void Awake()
        {
            SetCharacter("test", "test", IsThief);

            StartRandomFarting();
        }

        public void StartRandomFarting()
        {
            if (!_isFartingEnabled)
            {
                _isFartingEnabled = true;
                _fartCoroutine = StartCoroutine(RandomFartCoroutine());
            }
        }

        public void StopRandomFarting()
        {
            if (_isFartingEnabled)
            {
                _isFartingEnabled = false;
                if (_fartCoroutine != null)
                {
                    StopCoroutine(_fartCoroutine);
                    _fartCoroutine = null;
                }
            }
        }

        private IEnumerator RandomFartCoroutine()
        {
            while (_isFartingEnabled)
            {
                // 0~5초 사이의 무작위 시간 대기
                float randomWaitTime = Random.Range(0f, 5f);
                yield return new WaitForSeconds(randomWaitTime);
                
                // Fart 함수 실행
                Fart();
                
                // 남은 시간 대기하여 총 5초를 채움
                yield return new WaitForSeconds(5f - randomWaitTime);
            }
        }
        /////////////////////////////

        public void SetCharacter(string playerId, string playerName, bool isThief)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            _isThief = isThief;

            _currentCharacterIndex = Random.Range(0, _characterControllers.Length);
            _characterControllers[_currentCharacterIndex].gameObject.SetActive(true);

            if (_isThief)
            {
                _audioSource.clip = _thiefFartAudioClip;
            }
            else
            {
                _audioSource.clip = _normalFartAudioClips[Random.Range(0, _normalFartAudioClips.Length)];
            }

            _numberText.text = (transform.GetSiblingIndex() + 1).ToString();
        }

        public void Fart()
        {
            _characterControllers[_currentCharacterIndex].PlayFart();
        }
    }
}