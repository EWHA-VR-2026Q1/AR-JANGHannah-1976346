using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    public GameObject connectionStatusPanel;
    public GameObject lobbyPanel;

    [Header("Room List UI")]
    public Transform roomListContent;
    public GameObject roomEntryPrefab;

    [Header("Input Field")]
    public TMP_InputField roomInputField;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private void Awake()
    {
        roomEntryPrefab = Resources.Load<GameObject>("RoomItem");
        if (roomEntryPrefab == null)
        {
            Debug.LogError("[경고] Resources 폴더 안에서 'RoomItem' 프리팹 파일을 찾을 수 없습니다!");
        }
    }

    void Start()
    {
        //방장이 씬 전환 시 모든 유저가 안정적으로 화면을 동기화하도록 미리 선언
        PhotonNetwork.AutomaticallySyncScene = true;

        if (roomEntryPrefab == null)
        {
            roomEntryPrefab = Resources.Load<GameObject>("RoomItem");
        }

        PhotonNetwork.ConnectUsingSettings();
        connectionStatusPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 접속 완료. 로비 진입 시도...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 진입 완료.");
        connectionStatusPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        cachedRoomList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
        RenderRoomList();
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList || !info.IsOpen || !info.IsVisible)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
    }

    private void RenderRoomList()
    {
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(roomEntryPrefab, roomListContent);
            string roomName = info.Name;

            RoomItem roomItem = entry.GetComponent<RoomItem>();
            if (roomItem != null)
            {
                roomItem.SetRoomInfo(info.Name, info.PlayerCount, info.MaxPlayers);
            }
            else
            {
                TMP_Text tmpText = entry.GetComponentInChildren<TMP_Text>();
                if (tmpText != null)
                {
                    tmpText.text = $"{info.Name} ({info.PlayerCount}/{info.MaxPlayers})";
                }
            }

            Button btn = entry.GetComponent<Button>();
            if (btn == null)
            {
                btn = entry.GetComponentInChildren<Button>();
            }

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => JoinSelectedRoom(roomName));
            }
        }
    }

    //사용자가 기존 생성된 방 버튼을 눌렀을 때 실행될 함수
    public void JoinSelectedRoom(string roomName)
    {
        Debug.Log($"[디버그] 방 목록 버튼 클릭됨! 입장 혹은 재생성 시도 방 이름: {roomName}");

        // 그냥 JoinRoom 대신 안전하게 옵션을 채운 JoinOrCreateRoom을 사용합니다.
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true,
            CleanupCacheOnLeave = true // 방 탈퇴 시 서버에 남은 데이터 완전 삭제 옵션!
        };

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[디버그] 방 입장 실패! 이유: {message} (코드: {returnCode})");
    }

    // 새로운 룸을 직접 타이핑해서 생성하는 함수
    public void CreateNewRoom()
    {
        string roomName = roomInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("방 이름을 입력해주세요!");
            return;
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true,
            CleanupCacheOnLeave = true // [추가] 생성할 때도 청소 옵션 켜주기
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"룸 입장 완료: {PhotonNetwork.CurrentRoom.Name}");

        if (PhotonNetwork.IsMasterClient)
        {
            // 포톤 전용 씬 로드로 안정적인 동기화 이동
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
}