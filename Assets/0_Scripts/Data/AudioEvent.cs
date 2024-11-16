using System;
using UniRx;
using UnityEngine;
using Game.Extensions;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Audio Event")]
    public class AudioEvent : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private HideFlags _hideFlags;
        
        [IntervalVector2(0f, 1f)] 
        [SerializeField] private Vector2 _volume = new Vector2(0.75f, 1f);

        [IntervalVector2(-3f, 3f)] 
        [SerializeField] private Vector2 _pitch = new(0.9f, 1.1f);

        private AudioSource _source;
        
        public void Play(AudioSource source)
        {
            if (_clips.Length == 0) return;
            
            // var instance = new GameObject("Sound")
            // {
            //     hideFlags = _hideFlags,
            //     transform =
            //     {
            //         position = position
            //     }
            // };
            
            var clip = _clips[Random.Range(0, _clips.Length)];
            
            source.clip = clip;
            source.volume = Random.Range(_volume.x, _volume.y);
            source.pitch = Random.Range(_pitch.x, _pitch.y);
            
            source.Play(clip);
            
            // Destroy(instance, clip.length);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(AudioEvent))]
    public class SoundProfileEditor : Editor
    {
        private AudioSource _previewSource;
        
        private void OnEnable()
        {
            _previewSource = EditorUtility.CreateGameObjectWithHideFlags("Preview", HideFlags.None, typeof(AudioSource)).GetComponent<AudioSource>();
        }
        
        private void OnDisable()
        {
            DestroyImmediate(_previewSource.gameObject);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var profile = (AudioEvent) target;
            if (GUILayout.Button("Play"))
            {
                // Find the AudioListener
                // var listener = FindAnyObjectByType<AudioListener>();
                profile.Play(_previewSource);
            }
        }
    }
#endif
}