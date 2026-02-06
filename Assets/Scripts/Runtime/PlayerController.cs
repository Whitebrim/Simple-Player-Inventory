using Game.Services;
using UnityEngine;

namespace Game.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private const float GroundedPullDown = -2f;

        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _lookSensitivity = 0.15f;
        [SerializeField] private float _cameraPitchMin = -30f;
        [SerializeField] private float _cameraPitchMax = 60f;
        [SerializeField] private float _cameraDistance = 5f;

        private CharacterController _characterController;
        private InputService _input;
        private float _yaw;
        private float _pitch;
        private float _verticalVelocity;

        public void Init(InputService input)
        {
            _input = input;
            _characterController = GetComponent<CharacterController>();
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (_input == null)
                return;

            UpdateMovement();
        }

        private void LateUpdate()
        {
            if (_input == null)
                return;

            UpdateCamera();
        }

        private void UpdateMovement()
        {
            Vector2 moveInput = _input.MoveDirection;

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            Vector3 horizontal = (forward * moveInput.y + right * moveInput.x) * _moveSpeed;

            if (_characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = GroundedPullDown;
            else
                _verticalVelocity += Physics.gravity.y * Time.deltaTime;

            Vector3 move = horizontal;
            move.y = _verticalVelocity;

            _characterController.Move(move * Time.deltaTime);
        }

        private void UpdateCamera()
        {
            Vector2 look = _input.LookDelta;

            _yaw += look.x * _lookSensitivity;
            _pitch -= look.y * _lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, _cameraPitchMin, _cameraPitchMax);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

            Quaternion cameraRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 offset = cameraRotation * new Vector3(0f, 0f, -_cameraDistance);

            _cameraTransform.position = _cameraTarget.position + offset;
            _cameraTransform.rotation = cameraRotation;
        }
    }
}
