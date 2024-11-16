﻿using UnityEngine;

namespace CoolTools.Actors
{
    // Do Not make this abstract, in case it needs to be used on Visual Scripting
    public class EffectBase : ScriptableObject
    {
        [SerializeField] private string _effectName;
        [SerializeField, TextArea] private string _effectDescription;
        
        public string EffectName => _effectName;

        public string EffectDescription => _effectDescription;

        public virtual void Execute(Actor source, IDetectable target)
        {
        }

        public virtual void Execute(Actor source, Actor target)
        {
        }

        public virtual void Execute(Actor source, Vector3 target)
        {
        }
        
        public virtual void Execute(Actor source)
        {
        }

        public virtual void ResetEffect(Actor target)
        {
        }
    }
}