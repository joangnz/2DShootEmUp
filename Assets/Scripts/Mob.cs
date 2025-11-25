using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mob : NetworkBehaviour
{
    [Networked] private int Health { get; set; } = 100;
    [Networked] private int Damage { get; set; } = 10;
    [Networked] private bool Debounce { get; set; } = false;
    [Networked] private bool AttackedDebounce { get; set; } = false;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters;

    private Rigidbody2D _rb;
    private Vector2 _forward;

    public int GetHealth() => Health;
    public void SetHealth(int health) => Health = health;
    public int GetDamage() => Damage;
    public bool GetDebounce() => Debounce;
    public void SetDebounce(bool debounce) => Debounce = debounce;
    public bool GetAttackedDebounce() => AttackedDebounce;
    public void SetAttackedDebounce(bool attackedDebounce) => AttackedDebounce = attackedDebounce;

    public IEnumerator ActivateDebounce()
    {
        SetDebounce(true);
        yield return new WaitForSeconds(0.25f);
        SetDebounce(false);
    }

    public override void Spawned()
    {
        base.Spawned();
        SetDebounce(false);
    }

    private void Awake()
    {
        _spawnedCharacters = BasicSpawner._spawnedCharacters;
        _rb = GetComponent<Rigidbody2D>();
        _forward = GetClosestPlayerDirection().normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object) return;
        if (!Object.HasStateAuthority) return;

        _spawnedCharacters = BasicSpawner._spawnedCharacters;
        _forward = GetClosestPlayerDirection();

        _rb.position += 2*Runner.DeltaTime*_forward;
        transform.position = _rb.position;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!Object) return;
        if (!Object.HasStateAuthority) return;

        // Check if player and reduce HP if player
        if (collision.gameObject.TryGetComponent<Player>(out Player player))
        {
            PlayerState playerState = player.GetPlayerState();
            if (GetDebounce()) return;
            StartCoroutine(ActivateDebounce());

            playerState.SetHealth(playerState.GetHealth() - Damage);
            if (playerState.GetHealth() < 0) Runner.GetComponent<BasicSpawner>().KillPlayer(player);
            player.UpdatePlayerState(playerState);
        }
    }

    private Vector2 GetClosestPlayerDirection()
    {
        Vector2 myPos = new (transform.position.x, transform.position.y);
        float bestSqrDist = float.MaxValue;
        Vector2 bestDirection = Vector2.zero;

        foreach (NetworkObject playerObject in _spawnedCharacters.Values)
        {
            if (playerObject == null) continue;
            Vector2 playerPos = new(playerObject.transform.position.x, playerObject.transform.position.y);

            Vector2 direction = playerPos - myPos;

            float sqrDist = direction.sqrMagnitude;
            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                bestDirection = direction.normalized;
            }
        }

        return bestDirection;
    }
}
