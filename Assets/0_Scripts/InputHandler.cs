using System;
using CoolTools.Actors;
using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Game
{
	public class InputHandler : MonoBehaviour
	{
		[Header("Camera")]
		[SerializeField] private bool _getFromMainCamera;
		[SerializeField] private Transform _cameraTransform;
		
		[Header("Character Input Values")]
		[SerializeField] private Vector2 _move;
		[SerializeField] private Vector2 _look;
		[SerializeField] private bool _jump;
		[SerializeField] private bool _sprint;
		
		[FormerlySerializedAs("cursorLocked")] 
		[Header("Mouse Cursor Settings")]
		public bool _cursorLocked = true;
		[FormerlySerializedAs("cursorInputForLook")] 
		public bool _cursorInputForLook = true;

		private MovementBehaviour _movementBehaviour;
		private bool _hasCamera;

		private void Awake()
		{
			_movementBehaviour = GetComponent<MovementBehaviour>();
		}

		private void Start()
		{
			if (_cameraTransform == null && _getFromMainCamera)
			{
				var mainCamera = Camera.main;
				if (mainCamera != null)
				{
					_cameraTransform = mainCamera.transform;
					_hasCamera = true;
				}
				else
				{
					Debug.LogError("No main camera found in the scene");
				}
			}
			else
			{
				_hasCamera = _cameraTransform != null;
			}
		}

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(_cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif
		public void MoveInput(Vector2 newMoveDirection)
		{
			_move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			_look = newLookDirection;
			// _movementBehaviour.LookInput = new Vector3(look.x, 0f, look.y);
		}

		public void JumpInput(bool newJumpState)
		{
			_jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			_sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(_cursorLocked);
		}

		private void Update()
		{
			if (!_hasCamera) return;
			
			if (_move != Vector2.zero)
			{
				var transformedMove = _cameraTransform.TransformDirection(new Vector3(_move.x, 0f, _move.y));
				var transformedMove2D = new Vector2(transformedMove.x, transformedMove.z);

				_movementBehaviour.MovementInput = transformedMove2D;
				_movementBehaviour.LookInput = transformedMove.normalized;
			}
			else
			{
				_movementBehaviour.MovementInput = Vector2.zero;
			}
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}