using UnityEngine;
using Fusion;

public class Attack2 : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }
    [Networked] private int Damage { get; set; } = 100;

    private Vector2 _direction;

    private SpriteRenderer _spriteRenderer;
    private CircleCollider2D _collider;

    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<CircleCollider2D>();
    }

    public void Init(Vector2 direction)
    {
        Life = TickTimer.CreateFromSeconds(Runner, 0.3f);
        _direction = direction.normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (Life.Expired(Runner))
            Runner.Despawn(Object);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if Mob and reduce HP if so
        if (collision.gameObject.TryGetComponent<Mob>(out Mob mob))
        {
            Debug.Log("TestSlash");
            mob.SetHealth(mob.GetHealth() - Damage);
            if (mob.GetHealth() <= 0) Runner.Despawn(mob.Object);
        }
    }
}