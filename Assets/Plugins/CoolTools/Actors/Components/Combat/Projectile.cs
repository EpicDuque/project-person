﻿using System;
using CoolTools.Attributes;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CoolTools.Actors
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : OwnableBehaviour
    {
        [ColorSpacer("Projectile")] 
        [SerializeField] private GameObject _model;
        
        [Space(10f)]
        [FormerlySerializedAs("_useRaycast")]
        [Tooltip("Enable to detect hits using raycast against last position. Suitable for fast moving projectiles.")]
        [SerializeField] private bool _useShapecast = true;
        [SerializeField] private float _shapeCastRadius = 0.1f;
        // [SerializeField] private ShapeCastType _castType;
        
        [Space(10f)]
        [SerializeField] private float _disposeDelay = 0.5f;
        
        [Space(10f)]
        [SerializeField] private FloatValueConfig _maxSpeed;
        [SerializeField] private IntValueConfig _maxHits;
        
        [Space(10f)]
        [Tooltip("If Cast with target, the projectile will home towards the target.")]
        [SerializeField] private float _homingFactor;
        [SerializeField] private float _acceleration;
        [SerializeField] private bool _rotateTowardsMovement;
        [SerializeField] private LayerMask _destroyAgainst;
        
        [Space(10f)]
        [SerializeField] private ProjectileEvents _events;
        
        [Space(10f)]
        [SerializeField, InspectorDisabled] private HitBox _hitBox;
        [SerializeField, InspectorDisabled] private Rigidbody _rigidbody;
        private bool _hasHitBox;
        public HitBox HitBox => _hitBox;

        private int _hitCount;
        private Vector3 _lastPosition;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[32];
        private PoolableObject _poolableObject;
        private bool _disposing;
        private bool _hasTargetPosition;

        private enum ShapeCastType
        {
            Box,
            Sphere
        }

        [Serializable]
        public struct ProjectileEvents
        {
            public UnityEvent OnLaunched;
            public UnityEvent OnDestroyed;
        }
        
        public IDetectable Target { get; set; }
        public Vector3 TargetPosition { get; set; }

        public float HomingFactor
        {
            get => _homingFactor;
            set => _homingFactor = value;
        }

        public ProjectileEvents Events => _events;

        public Rigidbody RB => _rigidbody;

        public FloatValueConfig MaxSpeed => _maxSpeed;
        
        public ProjectileLauncher OriginLauncher { get; set; }

        public float Acceleration
        {
            get => _acceleration;
            set => _acceleration = value;
        }

        public int HitCount
        {
            get => _hitCount;
            set
            {
                _hitCount = value;

                if (_maxHits.Value > 0 && _hitCount >= _maxHits.Value)
                {
                    DisposeProjectile();
                }
            }
        }

        private void OnValidate()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _hitBox = GetComponent<HitBox>();

            if (Owner != null)
            {
                _maxHits.UpdateValue(Owner);
                _maxSpeed.UpdateValue(Owner);
            }
            else
            {
                _maxHits.UpdateValue();
                _maxSpeed.UpdateValue();
            }
        }

        protected override void OnStatsUpdated()
        {
            base.OnStatsUpdated();

            UpdateValues();
        }

        private void UpdateValues()
        {
            _maxSpeed.UpdateValue(this);
            _maxHits.UpdateValue(this);
        }

        private void OnEnable()
        {
            ProjectileSystem.RegisterProjectile(this);
            _model.SetActive(true);
            HitCount = 0;
            _disposing = false;
        }
        
        private void OnDisable()
        {
            ProjectileSystem.UnregisterProjectile(this);
        }

        protected new void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody>();
            _hitBox = GetComponent<HitBox>();
            
            _hasHitBox = _hitBox != null;
            if (_hasHitBox)
            {
                _hitBox.Events.OnHit.AddListener(OnHitBoxHit);
            }
        }

        private void Start()
        {
            
        }

        public void Initialize(IDetectable target)
        {
            Initialize();
            
            Target = target;
            _hasTargetPosition = false;
        }
        
        public void Initialize(Vector3 targetPosition)
        {
            Initialize();

            Target = null;
            TargetPosition = targetPosition;
            _hasTargetPosition = true;
        }

        public void Initialize(Transform target)
        {
            Initialize();
        }
        
        public void Initialize()
        {
            UpdateValues();
            
            _events.OnLaunched?.Invoke();
            _hitBox.enabled = true;
            _disposing = false;
        }

        private void OnHitBoxHit(IDamageable other)
        {
            if (HitCount == _maxHits.Value - 1)
            {
                if (other is MonoBehaviour mb)
                {
                    // Move the projectile to the same XZ position as the last target hit
                    var otherXZ = new Vector3(mb.transform.position.x, transform.position.y, mb.transform.position.z);
                    transform.position = otherXZ;
                }
            }
            
            HitCount++;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsValidDestroyCollision(other))
            {
                DisposeProjectile();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (IsValidDestroyCollision(other.collider))
            {
                DisposeProjectile();
            }
        }

        private bool IsValidDestroyCollision(Collider other)
        {
            // Check if other is in our _destroyLayerMask
            
            var result = (_destroyAgainst.value & (1 << other.gameObject.layer)) != 0;

            return result;
        }

        private void UpdateShapeCastHit()
        {
            if (!_useShapecast) return;
            if (_lastPosition == Vector3.zero) return;
            if (HitCount >= _maxHits.Value) return;
                
            // Create a RayCast from this position to lastPosition.
            // Use the NonAlloc version
            var position = transform.position;
            var direction = _lastPosition - position;
            
            var hits = Physics.SphereCastNonAlloc(position, _shapeCastRadius, direction, _raycastHits, 
                direction.magnitude);
                
            // Loop through all hits and check if any of them are a IDamageable.
            for (var i = 0; i < hits; i++)
            {
                var hit = _raycastHits[i];
                // Check if the hit is in the destroy against layer mask
                if (IsValidDestroyCollision(hit.collider))
                {
                    DisposeProjectile();
                    break;
                };
                
                if (!hit.collider.TryGetComponent<IDamageable>(out var damageable)) continue;

                if (damageable is IOwnable ownable)
                {
                    if (_hitBox.FactionFilter == FactionOperations.FactionFilterMode.NotOwner)
                    {
                        if(ownable.Owner.Faction == Owner.Faction) 
                            return;
                    }
                    else
                    {
                        if(ownable.Owner.Faction != Owner.Faction) 
                            return;
                    }
                }

                // This will raise OnHitBoxHit
                _hitBox.Hit(damageable, hit.point);
            }
                
            _lastPosition = position;
        }

        private void Update()
        {
            if (Target != null)
            {
                TargetPosition = Target.TargetPoint.position;
            }
            
            if (_rotateTowardsMovement)
            {
                if (_rigidbody.linearVelocity != Vector3.zero)
                {
                    transform.forward = _rigidbody.linearVelocity.normalized;
                }
            }
        }

        private void FixedUpdate()
        {
            UpdateShapeCastHit();
            
            // Apply Acceleration to rigidBody
            if (Target != null)
            {
                var direction = Target.TargetPoint.position - transform.position;
                _rigidbody.AddForce(direction.normalized * (_homingFactor), ForceMode.Force);
            }
            else if (_hasTargetPosition)
            {
                var direction = TargetPosition - transform.position;
                _rigidbody.AddForce(direction.normalized * _acceleration, ForceMode.Force);
            } 
            else
            {
                _rigidbody.AddForce(transform.forward * _acceleration, ForceMode.Force);
            }
            
            // Limit velocity to maxSpeed
            if(!_rigidbody.isKinematic && _maxSpeed.Value > 0f)
                _rigidbody.linearVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, _maxSpeed.Value);
        }

        public void DisposeProjectile()
        {
            if (_disposing) return;

            _hitBox.enabled = false;
            _disposing = true;
            
            StopPhysics();
            _hasTargetPosition = false;
            Target = null;

            _model.SetActive(false);
            _events.OnDestroyed?.Invoke();
            
            Invoke(nameof(ReturnToPool), _disposeDelay);
        }

        private void ReturnToPool()
        {
            if(TryGetComponent<PoolableObject>(out var poolable))
                poolable.ReturnToPool();
            else
                Destroy(gameObject);
        }

        public void StopPhysics()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;
        }
    }
}