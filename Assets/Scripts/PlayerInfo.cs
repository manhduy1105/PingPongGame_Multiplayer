using Fusion;
using TMPro;
using UnityEngine;

public enum CharacterClass
{
    Blue,
    Red
}

public struct PlayerProfile
{
    public string Name;
    public CharacterClass Class;
}

public class PlayerInfo : NetworkBehaviour
{
    [Networked] public string playerName { get; set; }

    public PlayerDataManager playerDataManager;
    public TextMeshProUGUI nameText;
    
    public GameObject[] characterIcons; // mảng chứa icon tương ứng với từng class, có thể gán trong inspector

    // sau khi game object được tạo ra trên mạng,
    // sẽ gọi phương thức này để khởi tạo thông tin player
    public override void Spawned()
    {
        playerDataManager = FindFirstObjectByType<PlayerDataManager>(); // tìm PlayerDataManager trong scene
    }

    // phương thức này sẽ được gọi mỗi frame để cập nhật thông tin hiển thị của player
    public override void Render()
    {
        if (playerDataManager == null) return;
        if (playerDataManager.TryGetPlayerMetaData(Object.InputAuthority, out var metadata))
        {
            var name = metadata.Name;
            var charClass = metadata.Class;
            
            nameText.text = $"{name} ({charClass})";

            for (var i = 0; i < characterIcons.Length; i++)
            {
                characterIcons[i].SetActive(i == (int)charClass); // hiển thị icon tương ứng với class của player
            }
        }
    }
}