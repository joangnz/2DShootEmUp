using UnityEngine;
using Fusion;

public class Attack1 : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }
    [Networked] private int Damage { get; set; } = 100;

    private AttackManager AttackManager;

    private Vector2 _direction;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    public void Awake()
    {
        AttackManager = FindFirstObjectByType<AttackManager>();

        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
    }

    public void Init(Vector2 direction, int characterId)
    {
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        _direction = direction.normalized;

        RpcInit(characterId);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcInit(int characterId)
    {
        _spriteRenderer.sprite = AttackManager.GetA1Sprite(characterId);
        _animator.runtimeAnimatorController = AttackManager.GetA1Animator(characterId).runtimeAnimatorController;
    }

    public override void FixedUpdateNetwork()
    {
        if (Life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += (Vector3) (5 * Runner.DeltaTime * _direction);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if Mob and reduce HP if so
        if (collision.gameObject.TryGetComponent<Mob>( out Mob mob))
        {
            Debug.Log("TestBall");
            mob.SetHealth(mob.GetHealth() - Damage);
            if (mob.GetHealth() <= 0) Runner.Despawn(mob.Object);
            Runner.Despawn(Object);
        }
    }
}