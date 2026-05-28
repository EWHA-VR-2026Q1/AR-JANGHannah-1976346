using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;

public class PlayerSetup : MonoBehaviourPun
{
    [Header("PC 시점 조작 세팅")]
    public float mouseSensitivity = 0.1f;
    private Transform cameraTransform;
    private float xRotation = 0f;
    private float yRotation = 0f;

    [Header("PC 이동 세팅")]
    public float moveSpeed = 5f;
    private CharacterController characterController;

    void Start()
    {
        // -------------------------------------------------------------
        // [1] 다른 사람의 캐릭터일 때 처리 (상대방의 컴포넌트 끄기)
        // -------------------------------------------------------------
        if (!photonView.IsMine)
        {
            Camera otherCamera = GetComponentInChildren<Camera>();
            if (otherCamera != null) otherCamera.enabled = false;

            AudioListener otherListener = GetComponentInChildren<AudioListener>();
            if (otherListener != null) otherListener.enabled = false;

            XROrigin otherOrigin = GetComponent<XROrigin>() ?? GetComponentInChildren<XROrigin>();
            if (otherOrigin != null) otherOrigin.enabled = false;

            var inputManager = GetComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>()
                               ?? GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>();
            if (inputManager != null) inputManager.enabled = false;

            // ★ 중요: 다른 사람의 캐릭터일 때는 몸통(Capsule)을 절대 끄면 안 됩니다!
            // 여기서 return을 해주어 아래에 있는 내 몸통 끄기 로직을 타지 않게 방어합니다.
            return;
        }

        // -------------------------------------------------------------
        // [2] 오직 "진짜 내 캐릭터(Mine)"일 때만 실행되는 로직
        // -------------------------------------------------------------
        // 1. 내 화면에서 "내 몸통"이 카메라 시야를 가리지 않게 내 몸뚱이만 숨기기
        // (하이어라키에 생성해 둔 자식 오브젝트 이름인 "Capsule"로 찾습니다)
        Transform myBodyMesh = transform.Find("BodyMesh");
        if (myBodyMesh != null)
        {
            myBodyMesh.gameObject.SetActive(false);
            Debug.Log($"[내 캐릭터 세팅] 내 시야 확보를 위해 내 프리팹의 BodyMesh를 숨겼습니다.");
        }

        // 2. 내 캐릭터의 카메라 및 물리 컨트롤러 찾기
        Camera myCam = GetComponentInChildren<Camera>();
        if (myCam != null)
        {
            cameraTransform = myCam.transform;
            Vector3 currentRotation = cameraTransform.localRotation.eulerAngles;
            xRotation = currentRotation.x;
            yRotation = currentRotation.y;
        }

        characterController = GetComponent<CharacterController>() ?? GetComponentInChildren<CharacterController>();

        // 커서 자유 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // 내 캐릭터가 아니라면 마우스/키보드 조작을 처리하지 않음
        if (!photonView.IsMine) return;

        // 1. 마우스 우클릭 시점 회전
        if (Mouse.current != null && Mouse.current.rightButton.isPressed && cameraTransform != null)
        {
            Cursor.visible = false;
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            xRotation -= mouseDelta.y * mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            yRotation += mouseDelta.x * mouseSensitivity;

            cameraTransform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
        else
        {
            Cursor.visible = true;
        }

        // 2. WASD 키보드 캐릭터 이동
        if (Keyboard.current != null && characterController != null && cameraTransform != null)
        {
            float moveX = 0f;
            float moveZ = 0f;

            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * moveZ) + (right * moveX);
            moveDirection.Normalize();
            moveDirection.y = -9.81f * Time.deltaTime; // 중력

            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }
}