using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked] private TickTimer Delay { get; set; }
    [Networked] private ref PlayerState PlayerStateRef => ref MakeRef<PlayerState>();

    [SerializeField] private Attack1 _prefabAttack1;
    [SerializeField] private Attack2 _prefabAttack2;

    private CharacterManager CharacterManager;

    private Rigidbody2D _rb;
    private NetworkTransform _nt;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private Vector2 _forward = Vector2.right;

    public PlayerState GetPlayerState() => PlayerStateRef;
    public void UpdatePlayerState(PlayerState state) => PlayerStateRef = state;

    private void Awake()
    {
        CharacterManager = FindFirstObjectByType<CharacterManager>();

        _rb = GetComponent<Rigidbody2D>();
        _nt = GetComponent<NetworkTransform>();

        _animator = transform.GetComponentInChildren<Animator>();
        _spriteRenderer = transform.GetComponentInChildren<SpriteRenderer>();
    }

    public override void Spawned()
    {
        // Create logic for availability
        int characterId = CharacterManager.GetFirstAvailableCharacterId();
        if (characterId < 1 || characterId > 5) return;

        PlayerStateRef = new PlayerState(characterId, 100);
        CharacterManager.SetCharacterAvailable(characterId, false);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();

            _rb.position += 3*Runner.DeltaTime*data.direction;
            transform.position = _rb.position;

            //_rb.transform.position += 3*Runner.DeltaTime*(Vector3)data.direction;
            //_nt.transform.position = _rb.transform.position;

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            _animator.SetBool("north", data.north);
            _animator.SetBool("south", data.south);
            _animator.SetBool("east", data.east);
            _animator.SetBool("west", data.west);

            if (HasStateAuthority && Delay.ExpiredOrNotRunning(Runner))
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (data.buttons.IsSet(i))
                    {
                        ChangeCharacter(i);
                    }
                }

                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    float angle = Mathf.Atan2(_forward.y, _forward.x) * Mathf.Rad2Deg;
                    Quaternion spawnRot = Quaternion.Euler(0f, 0f, angle);

                    Runner.Spawn(_prefabAttack1,
                    // transform.position + (Vector3)_forward,
                    transform.position,
                    spawnRot,
                    Object.InputAuthority, (runner, o) =>
                    {
                        // Initialize the Ball before synchronizing it
                        o.GetComponent<Attack1>().Init(_forward);
                    });
                }

                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    float angle = Mathf.Atan2(_forward.y, _forward.x) * Mathf.Rad2Deg - 180;
                    Debug.Log(angle);
                    Quaternion spawnRot = Quaternion.Euler(0f, 0f, angle);

                    Runner.Spawn(_prefabAttack2,
                    transform.position,
                    spawnRot,
                    Object.InputAuthority, (runner, o) =>
                    {
                        o.GetComponent<Attack2>().Init(_forward);
                    });
                }
            }
        }
    }

    public void ChangeCharacter(int id)
    {
        _spriteRenderer.sprite = CharacterManager.GetSprite(id);
        _animator.runtimeAnimatorController = CharacterManager.GetAnimator(id).runtimeAnimatorController;
        
        CharacterManager.SetCharacterAvailable(PlayerStateRef.GetCharacterId(), true);
        CharacterManager.SetCharacterAvailable(id, false);
        
        PlayerStateRef.SetCharacterId(id);
    }
}