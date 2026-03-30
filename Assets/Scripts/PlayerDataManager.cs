using Fusion;
using UnityEngine;

// struct quản lý thông tin
public struct PlayerMetaData : INetworkStruct
{
    public NetworkString<_16> Name;
    public CharacterClass Class;
}

public class PlayerDataManager : NetworkBehaviour
{
    // biến này của Fusion sẽ tự động đồng bộ giữa các client và host,
    // khi có thay đổi sẽ tự động cập nhật ở tất cả các bên
    [Networked]
    public NetworkDictionary<PlayerRef, PlayerMetaData> Players => default;
    
    // RPC: phương thức này sẽ được gọi từ client hoặc
    // host để cập nhật thông tin player, sau đó sẽ được gửi
    // đến state authority (host) để xử lý và đồng bộ lại cho tất cả các client
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UpdatePlayerMetaData(PlayerRef playerRef, PlayerMetaData metaData)
    {
        Players.Set(playerRef, metaData);
    }
    
    public bool TryGetPlayerMetaData(PlayerRef playerRef, out PlayerMetaData metaData)
    {
        return Players.TryGet(playerRef, out metaData);
    }
}