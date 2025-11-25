using UnityEngine;
using Fusion;

public class Attack2 : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }
    [Networked] private int Damage { get; set; } = 100;

    private AttackManager AttackManager;

    private NetworkObject _playerObject;
    private Transform _playerTransform;

    private SpriteRenderer _spriteRenderer;

    public void Awake()
    {
        AttackManager = FindFirstObjectByType<AttackManager>();

        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(NetworkObject networkObject, int characterId)
    {
        _playerObject = networkObject;
        _playerTransform =  _playerObject.GetComponent<Transform>();

        Life = TickTimer.CreateFromSeconds(Runner, 0.3f);

        RpcInit(characterId);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcInit(int characterId)
    {
        _spriteRenderer.sprite = AttackManager.GetA2Sprite(characterId);
        // _animator.runtimeAnimatorController = AttackManager.GetA1Animator(characterId).runtimeAnimatorController;
    }

    public override void FixedUpdateNetwork()
    {
        if (!_playerTransform) return;
        transform.position = _playerTransform.position;
        if (Life.Expired(Runner))
            Runner.Despawn(Object);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if Mob and reduce HP if so
        if (collision.gameObject.TryGetComponent(out Mob mob))
        {
            Debug.Log("TestSlash");
            mob.SetHealth(mob.GetHealth() - Damage);
            if (mob.GetHealth() <= 0) Runner.Despawn(mob.Object);
        }
    }
}