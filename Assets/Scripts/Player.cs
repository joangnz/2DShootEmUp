using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkObject _networkObject;
    [Networked] private TickTimer Delay { get; set; }
    [Networked] private ref PlayerState PlayerStateRef => ref MakeRef<PlayerState>();

    [SerializeField] private Attack1 _prefabAttack1;
    [SerializeField] private Attack2 _prefabAttack2;

    private CharacterManager CharacterManager;

    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private Vector2 _forward = Vector2.right;

    public PlayerState GetPlayerState() => PlayerStateRef;
    public void UpdatePlayerState(PlayerState state) => PlayerStateRef = state;
    
    public void SetSprite(Sprite sprite) => _spriteRenderer.sprite = sprite;
    public Sprite GetSprite() => _spriteRenderer.sprite;

    public void SetAnimator(Animator animator) => _animator.runtimeAnimatorController = animator.runtimeAnimatorController;
    public Animator GetAnimator() => _animator;

    private void Awake()
    {
        CharacterManager = FindFirstObjectByType<CharacterManager>();

        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public override void Spawned()
    {
        _networkObject = GetComponent<NetworkObject>();

        // Create logic for availability
        //int characterId = CharacterManager.GetFirstAvailableCharacterId();
        //if (characterId < 1 || characterId > 5) return;

        //CharacterManager.SetCharacterAvailable(characterId, false);
        //PlayerStateRef = new PlayerState(characterId, 100);
        //SetAnimator(CharacterManager.GetAnimator(characterId));
        //SetSprite(CharacterManager.GetSprite(characterId));
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();

            transform.position += 3*Runner.DeltaTime*(Vector3)data.direction;
            _rigidbody2D.position = transform.position;

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
                    if (data.buttons.IsSet(i) && Runner.IsServer)
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
                        o.GetComponent<Attack1>().Init(_forward, PlayerStateRef.GetCharacterId());
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
                        o.GetComponent<Attack2>().Init(_networkObject, PlayerStateRef.GetCharacterId());
                    });
                }
            }
        }
    }

    public void ChangeCharacter(int id)
    {
        if (!CharacterManager.GetCharacterAvailable(id)) return;
        RPC_ChangeCharacter(id);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ChangeCharacter(int id)
    {
        SetSprite(CharacterManager.GetSprite(id));
        SetAnimator(CharacterManager.GetAnimator(id));

        CharacterManager.SetCharacterAvailable(PlayerStateRef.GetCharacterId(), true);
        CharacterManager.SetCharacterAvailable(id, false);

        PlayerStateRef.SetCharacterId(id);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UpdatePlayerInfo(PlayerRef targetPlayer)
    {
        int id = PlayerStateRef.GetCharacterId();
        SetSprite(CharacterManager.GetSprite(id));
        SetAnimator(CharacterManager.GetAnimator(id));
    }
}