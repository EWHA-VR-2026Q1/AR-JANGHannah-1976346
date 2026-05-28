using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomItem : MonoBehaviour
{
    public TMP_Text roomNameText;
    private string roomName;

    public void SetRoomInfo(string name, int currentPlayers, int maxPlayers)
    {
        roomName = name;
        roomNameText.text = $"{name} ({currentPlayers}/{maxPlayers})";
    }

    //아이템 자체를 터치했을 때도 안전하게 데이터를 다 정제하도록 수정
    public void OnClickRoom()
    {
        // 만약 씬에 LobbyManager가 있다면 그 안전한 함수를 대신 호출해 줍니다.
        LobbyManager lobbyMgr = FindObjectOfType<LobbyManager>();
        if (lobbyMgr != null)
        {
            lobbyMgr.JoinSelectedRoom(roomName);
        }
        else
        {
            // 만약 매니저가 없을 때를 대비한 2차 백업 방어선
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4, CleanupCacheOnLeave = true };
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }
    }
}