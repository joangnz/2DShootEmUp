using Fusion;
public struct PlayerState : INetworkStruct
{
    public PlayerState(int _characterId, float _health)
    {
        CharacterId = _characterId;
        Health = _health;
    }

    private int CharacterId;
    private float Health;

    public void SetCharacterId(int id) => CharacterId = id;

    public readonly int GetCharacterId() => CharacterId;

    public void SetHealth(float health) => Health = health;

    public readonly float GetHealth() => Health;
}