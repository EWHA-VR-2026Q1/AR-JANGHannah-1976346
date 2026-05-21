using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class AR_UI_Touch : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 손가락 터치(또는 마우스 클릭) 순간 감지
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Pointer.current.position.ReadValue();

            // 화면 터치 좌표에서 3D 공간으로 레이저(Ray)를 발사
            Ray ray = mainCamera.ScreenPointToRay(touchPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 터치한 물체에 버튼(Button) 컴포넌트가 있는지 확인
                Button btn = hit.transform.GetComponent<Button>();
                if (btn != null)
                {
                    // 버튼이 존재하면 클릭 이벤트(On Click)를 강제로 실행!
                    btn.onClick.Invoke();
                    Debug.Log($"{hit.transform.name} 버튼 강제 클릭 성공!");
                }
            }
        }
    }
}