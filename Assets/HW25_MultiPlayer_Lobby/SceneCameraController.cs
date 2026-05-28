using UnityEngine;
using UnityEngine.InputSystem; // 뉴 인풋 시스템 사용

public class SceneCameraController : MonoBehaviour
{
    [Header("시점 회전 세팅")]
    public float rotationSensitivity = 0.1f;
    private float xRotation = 0f;
    private float yRotation = 0f;

    [Header("이동 세팅")]
    public float moveSpeed = 5f; // 카메라 이동 속도

    void Start()
    {
        // 시작할 때 현재 카메라의 회전값을 초기화
        Vector3 currentRot = transform.localRotation.eulerAngles;
        xRotation = currentRot.x;
        yRotation = currentRot.y;
    }

    void Update()
    {
        // -------------------------------------------------------------
        // 1. 마우스 우클릭 시점 회전 (기존 로직)
        // -------------------------------------------------------------
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float mouseX = mouseDelta.x * rotationSensitivity;
            float mouseY = mouseDelta.y * rotationSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 목 꺾임 방지
            yRotation += mouseX;

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }

        // -------------------------------------------------------------
        // 2. [★신규 추가] 게스트 카메라를 위한 WASD 키보드 이동 로직
        // -------------------------------------------------------------
        if (Keyboard.current != null)
        {
            float inputX = 0f;
            float inputZ = 0f;

            // 키보드 입력 감지
            if (Keyboard.current.wKey.isPressed) inputZ += 1f;
            if (Keyboard.current.sKey.isPressed) inputZ -= 1f;
            if (Keyboard.current.dKey.isPressed) inputX += 1f;
            if (Keyboard.current.aKey.isPressed) inputX -= 1f;

            // 카메라가 바라보는 방향을 기준으로 이동 벡터 계산
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // 관전자 모드처럼 공중을 자유롭게 날아다니며 구경할 수 있도록 
            // Y축(높이) 제한을 굳이 두지 않고 카메라 방향 그대로 이동하게 만듭니다.
            Vector3 moveDirection = (forward * inputZ) + (right * inputX);
            moveDirection.Normalize();

            // 기본 카메라는 물리 컴포넌트(CharacterController)가 없으므로 
            // transform.Translate를 이용해 부드럽게 좌표를 이동시킵니다.
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}