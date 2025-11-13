using UnityEngine;

public class Character
{
    private int _id;
    private Sprite _sprite;
    private Animator _animator;
    private bool _available = true;

    public Character(int id, Sprite sprite, Animator animator)
    {
        _id = id;
        _sprite = sprite;
        _animator = animator;
    }

    public int GetId() => _id;

    public Sprite GetSprite() => _sprite;

    public Animator GetAnimator() => _animator;

    public bool GetAvailable() => _available;

    public void SetId(int id) => _id=id;

    public void SetSprite(Sprite sprite) => _sprite=sprite;

    public void SetAvailable(bool available) => _available=available;
}