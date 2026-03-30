    using System.Collections.Generic;
    using Fusion;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class LobbyManager : MonoBehaviour
    {
        public GameObject lobbyPanel;
        public GameObject characterSelectionPanel;

        public BasicSpawner spawner;

        [Header("Character Selection")] public TMP_InputField playerNameInput;

        public Image[] characterPreviewImages;

        
        public int selectedCharacterIndex = 0;

        [Header("Room List")] public GameObject roomListParent;
        public GameObject roomListItemPrefab;
        public TMP_InputField roomNameInput;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        async void Start()
        {
            lobbyPanel.SetActive(false);
            characterSelectionPanel.SetActive(true);
            OnSelectCharacter(selectedCharacterIndex);

            spawner = FindFirstObjectByType<BasicSpawner>();
            await spawner.StartLobby();
        }

        public void OnSelectCharacter(int index)
        {
            selectedCharacterIndex = index;
            // update preview images
            for (var i = 0; i < characterPreviewImages.Length; i++)
            {
                characterPreviewImages[i].color = (i == index)
                    ? Color.green
                    : Color.white;
            }
        }

        public void OnNextButton()
        {
            var playerName = playerNameInput.text;
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("Player name cannot be empty!");
                return;
            }

            // tạo 1 player profile tạm thời, sau này sẽ gửi lên server để tạo player object
            var profile = new PlayerProfile()
            {
                Name = playerName,
                Class = (CharacterClass)selectedCharacterIndex
            };
            spawner.SetLocalPlayerProfile(profile);
            // đưa lên host để tạo player object, ở đây tạm thời chỉ log ra console
            Debug.Log($"Player Name: {profile.Name}, Class: {profile.Class}");
            // chuyển sang lobby panel
            characterSelectionPanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }

        // hiển thị danh sách phòng
        public void DisplayRoomList(List<SessionInfo> sessions)
        {
            Debug.Log($"Received {sessions.Count} sessions from lobby");
            // clear danh sách cũ
            foreach (Transform child in roomListParent.transform)
            {
                Destroy(child.gameObject);
            }

            if (sessions.Count == 0) return;

            // tạo item mới cho mỗi phòng
            foreach (var session in sessions)
            {
                var item = Instantiate(roomListItemPrefab, roomListParent.transform);
                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                text.text = $"{session.Name} ({session.PlayerCount}/{session.MaxPlayers})";
                var button = item.GetComponentInChildren<Button>();
                button.onClick.AddListener(() => OnJoinRoom(session.Name));
                item.SetActive(true);
            }
        }

        async void OnJoinRoom(string sessionName)
        {
            await spawner.StartClient(sessionName);
        }

        public async void OnCreateRoomButton()
        {
            var roomName = roomNameInput.text;
            if (string.IsNullOrEmpty(roomName))
            {
                Debug.LogWarning("Room name cannot be empty!");
                return;
            }

            // tạo phòng mới với tên đã nhập
            await spawner.StartHost(roomName, SceneRef.FromIndex(1));
        }
    }