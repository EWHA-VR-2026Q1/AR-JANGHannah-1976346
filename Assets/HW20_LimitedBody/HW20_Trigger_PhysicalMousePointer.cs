using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class HW_Trigger_PhysicalMousePointer : MonoBehaviour
{

    private Camera mainCamera;
    private void Awake()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        // 클릭/터치 입력 감지 (New Input System 방식)
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Debug.Log("터치 입력 감지");
            Vector2 screenPosition = Pointer.current.position.ReadValue();
            // UI 클릭 여부 확인
            // IsPointerOverGameObject 는 현재 포인터 아래에 UI(EventSystem 대상)가 있는지 체크
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // UI 를 클릭한 경우이므로 게임 로직은 실행하지 않고 종료
                Debug.Log("UI 클릭됨");
                return;
            }
            // 게임 오브젝트 클릭 처리 (Raycast)
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 여기서 클릭된 오브젝트에 따른 로직 수행
                Debug.Log($"오브젝트 클릭됨: {hit.transform.name}");
            }
        }
    }
}
