using System;
using Unity.Netcode;
using UnityEngine;

namespace MAIN.Scripts
{
    public class Player : NetworkBehaviour
    {
        private enum PlayerState
        {
            Idle,
            Walk,
            Fire,
            Jump,
        }
        
        [SerializeField] private float speed;
        [SerializeField] private float lookSensitivity;
        
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListener;
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private Animator animator;
        
        [SerializeField] private Transform lookPosition;
        [SerializeField] private Transform lookTarget;
        
        [SerializeField] private Transform rightHandPosition;
        [SerializeField] private Transform rightHandTarget;
        
        [SerializeField] private Transform leftHandPosition;
        [SerializeField] private Transform leftHandTarget;
        
        [SerializeField] private NetworkVariable<Vector3> networkPositionDirection = new(Vector3.zero, NetworkVariableReadPermission.Owner);
        [SerializeField] private NetworkVariable<Vector3> networkRotationDirection = new();
        [SerializeField] private NetworkVariable<Vector3> networkCameraRotation = new();
        [SerializeField] private NetworkVariable<PlayerState> networkPlayerState = new(PlayerState.Idle, NetworkVariableReadPermission.Owner);
        
        private Vector3 _oldInputPosition;
        private Vector3 _oldInputRotation;
        private static readonly int X = Animator.StringToHash("X");
        private static readonly int Y = Animator.StringToHash("Y");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly float _cameraRotationLimitDown = 0f;
        private static readonly float _cameraRotationLimitUp = 360;
        private static readonly float _cameraRotationLimitMove= 90f;

        private void Start()
        {
            if (!IsOwner )
            {
                playerCamera.enabled = false;
                audioListener.enabled = false;
                if(!IsServer)
                    this.enabled = false;
            }
            MainModel.Instance.Main.Scrollbar.onValueChanged.AddListener(UpdateSensitivity);
            MainModel.Instance.Main.SetPlayer(this);
        }

        
        // void OnAnimatorIK()
        // {
        //     animator.SetLookAtPosition(target);
        // }
        private void UpdateSensitivity(float value)
        {
            lookSensitivity = value * 10;
            Debug.Log(value);
        }

        private void Update()
        {
            
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                UpdateServer();
            }
            if (IsClient && IsOwner)
            {
                ClientInput();
            }
            ClientVisuals();
        }

        private void UpdateServer()
        {
            Vector3 position = networkPositionDirection.Value * speed;
            Vector3 direction = transform.TransformDirection(position);
            //Vector3 rotation = new Vector3(0, networkRotationDirection.Value.x, 0) * sensitivity;
            //Vector3 cameraRotation = new Vector3(networkRotationDirection.Value.y, 0 , 0) * sensitivity;
            
            if (direction != Vector3.zero)
            {
                rb.MovePosition(rb.position + direction * Time.fixedDeltaTime);
            }
            
            rb.MoveRotation(rb.rotation * Quaternion.Euler(networkRotationDirection.Value));
            playerCamera.transform.Rotate(networkCameraRotation.Value);
            float rotationX;
            // float rotationX = playerCamera.transform.localEulerAngles.x + Mathf.Clamp(networkCameraRotation.Value.x, -_cameraRotationLimitMove, _cameraRotationLimitMove);
            // if ((rotationX > 185 || rotationX < 0) && rotationX < 360)
            // {
            //     rotationX = (360 + rotationX) % 360;
            //     rotationX = Mathf.Clamp(rotationX, _cameraRotationLimitUp - 85f,
            //         _cameraRotationLimitUp);
            // }
            // else
            // {
            //     rotationX = Mathf.Clamp(rotationX, _cameraRotationLimitDown,
            //         _cameraRotationLimitDown + 85f);
            // }
            if (playerCamera.transform.localEulerAngles.x > 185)
            {
                rotationX = Mathf.Clamp(playerCamera.transform.localEulerAngles.x, _cameraRotationLimitUp - 55f,
                    _cameraRotationLimitUp);
            }
            else
            {
                rotationX = Mathf.Clamp(playerCamera.transform.localEulerAngles.x, _cameraRotationLimitDown,
                    _cameraRotationLimitDown + 45f);
            }

            playerCamera.transform.localEulerAngles = new Vector3(rotationX, 0, 0);
            lookTarget.position = lookPosition.position;
            rightHandTarget.position = rightHandPosition.position;
            rightHandTarget.rotation = rightHandPosition.rotation;
            leftHandTarget.position = leftHandPosition.position;
            leftHandTarget.rotation = leftHandPosition.rotation;
        }

        private void ClientInput()
        {
            if(MainModel.Instance.Overlayed)
            {
                UpdateClientPositionAndRotationServerRpc(Vector3.zero, Vector3.zero);
                return;
            }
            
            float xInput = Input.GetAxis("Horizontal");
            float yInput = Input.GetAxis("Vertical");
            float xRot = Input.GetAxisRaw("Mouse X");
            float yRot = Input.GetAxisRaw("Mouse Y");

            bool jump = Input.GetKeyDown(KeyCode.Space);

            Vector3 position = new Vector3(xInput, 0, yInput);
            Vector3 rotation = new Vector3(xRot, yRot, 0);
            
            if (_oldInputPosition != position || _oldInputRotation != rotation)
            {
                _oldInputPosition = position;
                _oldInputRotation = rotation;

                UpdateClientPositionAndRotationServerRpc(position, rotation);
            }

            if (jump)
            {
                UpdatePlayerStateServerRpc(PlayerState.Jump);
                Debug.Log("Jump");
            }
            else if (position != Vector3.zero)
            {
                UpdatePlayerStateServerRpc(PlayerState.Walk);
            } 
            else
            {
                UpdatePlayerStateServerRpc(PlayerState.Idle);
            }
        }
        private void ClientVisuals()
        {
            if (networkPlayerState.Value == PlayerState.Walk)
            {
                animator.SetFloat(Y, networkPositionDirection.Value.z);
                animator.SetFloat(X, networkPositionDirection.Value.x);
            }
            else if (networkPlayerState.Value == PlayerState.Jump)
            {
                animator.SetFloat(Y, networkPositionDirection.Value.z);
                animator.SetFloat(X, networkPositionDirection.Value.x);

                if (!animator.GetBool(Jump))
                {
                    animator.SetBool(Jump, true);
                }
            }
            else
            {
                animator.SetFloat(Y, 0);
                animator.SetFloat(X, 0);
            }
        }
        [ServerRpc]
        private void UpdateClientPositionAndRotationServerRpc(Vector3 networkPosition, Vector3 networkRotation)
        {
            networkPositionDirection.Value = networkPosition;
            networkRotationDirection.Value = new Vector3(0, networkRotation.x, 0) * lookSensitivity;
            networkCameraRotation.Value = new Vector3(networkRotation.y , 0, 0) * lookSensitivity;
        }

        [ServerRpc]
        private void UpdatePlayerStateServerRpc(PlayerState newState)
        {
            networkPlayerState.Value = newState;
        }

        public void JumpEnd()
        {
            animator.SetBool(Jump, false);
        }
    }
}
