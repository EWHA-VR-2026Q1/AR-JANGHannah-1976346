using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab; // Resources 폴더 안의 프리팹
    public Transform[] spawnPoints;  // 플레이어가 스폰될 위치들

    private bool hasSpawned = false; // 중복 생성을 방지하기 위한 안전장치 변수

    void Start()
    {
        // [호스트 방어] 만약 씬이 시작하는 시점에 이미 방에 들어와 있는 상태라면 즉시 스폰 프로세스 시작
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            TriggerSpawnProcess();
        }
    }

    // [게스트 구원 콜백] ★나중에 방에 참가 완료한 게스트들은 이 포톤 이벤트 함수를 타고 들어옵니다!
    public override void OnJoinedRoom()
    {
        Debug.Log("[시스템] 포톤 룸에 성공적으로 입장했습니다. 스폰 프로세스를 검토합니다.");
        TriggerSpawnProcess();
    }

    // 중복 소환을 막고 코루틴을 안전하게 실행해주는 함수
    void TriggerSpawnProcess()
    {
        if (hasSpawned) return; // 이미 내 캐릭터를 뽑았다면 통과

        hasSpawned = true;
        StartCoroutine(CleanUpAndSpawnProcess());
    }

    void Update()
    {
        // XR 디바이스 시뮬레이터 실시간 파괴 방어 로직
        GameObject simulator = GameObject.Find("XR Device Simulator");
        if (simulator != null)
        {
            Debug.Log("[시스템 방어] 기습 생성된 XR Device Simulator를 실시간으로 제거했습니다.");
            Destroy(simulator);
        }
    }

    // 씬 청소 및 안전 스폰을 위한 코루틴 함수
    IEnumerator CleanUpAndSpawnProcess()
    {
        GameObject simulator = GameObject.Find("XR Device Simulator");
        if (simulator != null)
        {
            Debug.Log("[디버그] 방해 요소(XR Device Simulator)를 발견하여 삭제를 시작합니다.");
            Destroy(simulator);
            yield return null; // 1프레임 대기
        }

        // 시뮬레이터가 사라진 깨끗한 상태에서 플레이어를 안전하게 소환합니다.
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // 포톤 고유 액터 번호를 기반으로 스폰 포인트 분배
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Transform spawnPoint = spawnPoints[(actorNumber - 1) % spawnPoints.Length];

            if (spawnPoint != null)
            {
                spawnPos = spawnPoint.position;
                spawnRot = spawnPoint.rotation;
            }
        }
        else
        {
            Debug.LogWarning("[경고] SpawnPoints 배열이 비어있어 기본 위치에 생성합니다.");
            spawnPos = new Vector3(0, 1f, 0);
        }

        // 포톤을 통해 내 캐릭터 진짜 서버에 생성!
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, spawnRot);
        Debug.Log($"[디버그] 플레이어 스폰이 정상 완료되었습니다. 프리팹 이름: {playerPrefab.name}");

        // 캐릭터가 무사히 소환되었으니, 관전자용이었던 씬의 기본 카메라는 꺼줍니다.
        GameObject sceneMainCamera = GameObject.FindWithTag("MainCamera");
        if (sceneMainCamera != null && sceneMainCamera.transform.parent == null)
        {
            sceneMainCamera.SetActive(false);
            Debug.Log("[디버그] 플레이어가 생성되어 기본 관전자 카메라는 비활성화되었습니다.");
        }
    }

    // 로비로 돌아가기 버튼을 누를 때 호출할 함수
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("HW25_MultiPlayer_Lobby");
        }
    }

    // 포톤 룸에서 완전히 성공적으로 나갔을 때 자동으로 실행되는 콜백
    public override void OnLeftRoom()
    {
        Debug.Log("포톤 서버룸 퇴장 완료. 로비 씬으로 복귀합니다.");
        SceneManager.LoadScene("HW25_MultiPlayer_Lobby");
    }
}