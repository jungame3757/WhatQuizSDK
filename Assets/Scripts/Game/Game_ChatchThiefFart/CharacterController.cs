using Unity.VisualScripting;
using UnityEngine;

namespace Game.ChatchThiefFart
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _fartParticle;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private Animator _animator;

        public void PlayFart()
        {
            _animator.SetTrigger("Fart");
        }

        public void Fart()
        {
            _fartParticle.Play();
            _audioSource.Play();
        }
    }
}
