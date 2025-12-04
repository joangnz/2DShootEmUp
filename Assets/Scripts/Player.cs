using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkObject _networkObject;
    [Networked] private TickTimer Delay { get; set; }
    [Networked] private ref PlayerState PlayerStateRef => ref MakeRef<PlayerState>();

    [SerializeField] private Attack1 _prefabAttack1;
    [SerializeField] private Attack2 _prefabAttack2;

    private CharacterManager CharacterManager;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator an;
    private TextMeshPro tmp;

    private Vector2 _forward = Vector2.right;

    [Networked] private string Username { get; set; }

    public PlayerState GetPlayerState() => PlayerStateRef;
    public void UpdatePlayerState(PlayerState state) => PlayerStateRef = state;
    
    public void SetSprite(Sprite sprite) => sr.sprite = sprite;
    public Sprite GetSprite() => sr.sprite;

    public void SetAnimator(Animator animator) => an.runtimeAnimatorController = animator.runtimeAnimatorController;
    public Animator GetAnimator() => an;

    private void Awake()
    {
        CharacterManager = FindFirstObjectByType<CharacterManager>();

        rb = GetComponent<Rigidbody2D>();
        an = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        tmp = GetComponentInChildren<TextMeshPro>();
    }

    public override void Spawned()
    {
        _networkObject = GetComponent<NetworkObject>();
        if (Object.HasInputAuthority)
        {
            Username = PlayerPrefs.GetString("username", "Player " + Object.InputAuthority.PlayerId);
            RPC_SetUsername(Username);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();

            transform.position += 3*Runner.DeltaTime*(Vector3)data.direction;
            rb.position = transform.position;

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            an.SetBool("north", data.north);
            an.SetBool("south", data.south);
            an.SetBool("east", data.east);
            an.SetBool("west", data.west);

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
    public void RPC_UpdatePlayerInfo(PlayerRef _)
    {
        int id = PlayerStateRef.GetCharacterId();
        SetSprite(CharacterManager.GetSprite(id));
        SetAnimator(CharacterManager.GetAnimator(id));
        tmp.text = Username;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetUsername(string username)
    {
        tmp.text = username;
    }

    public string GetUsername() => Username;
}