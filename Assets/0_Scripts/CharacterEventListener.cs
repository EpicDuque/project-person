using Game.Data;
using UnityEngine;
using Game.Extensions;
using UnityEngine.Serialization;

namespace Game
{
    /// <summary>
    /// Has some common animations events for a character
    /// </summary>
    public class CharacterEventListener : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        
        [Space(10f)]
        [FormerlySerializedAs("_audioEvent")] 
        [SerializeField] private AudioEvent _audioFootStep;
        [SerializeField] private AudioEvent _audioJump;
        [SerializeField] private AudioEvent _audioLand;
        
        private void OnFootstep()
        {
            _audioFootStep.Play(_source);
        }
        
        private void OnLand()
        {
            _audioLand.Play(_source);
        }
        
        private void OnJump()
        {
            _audioJump.Play(_source);
        }
    }
}