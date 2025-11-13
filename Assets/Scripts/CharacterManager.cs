using System.Collections.Generic;
using UnityEngine;

public class CharacterManager: MonoBehaviour
{
    private List<Character> _characterList = new();
    [SerializeField] private Sprite sprite1, sprite2, sprite3, sprite4, sprite5;
    private Sprite[] Sprites;
    [SerializeField] private Animator a1, a2, a3, a4, a5;
    private Animator[] Animators;

    public void Awake()
    {
        Animators = new Animator[] { a1, a2, a3, a4, a5 };
        Sprites = new Sprite[] { sprite1, sprite2, sprite3, sprite4, sprite5 };

        for (int i = 0; i < 5; i++)
        {
            _characterList.Add(new(i+1, Sprites[i], Animators[i]));
        }
    }

    public Sprite GetSprite(int id) => Sprites[id-1];

    public Animator GetAnimator(int id) => Animators[id-1];

    public int GetFirstAvailableCharacterId()
    {
        foreach (var character in _characterList)
        {
            if (character.GetAvailable()) return character.GetId();
        }

        return 0;
    }

    public bool GetCharacterAvailable(int id) => _characterList[id-1].GetAvailable();

    public Character GetCharacter(int id) => _characterList[id-1];

    public void SetCharacterAvailable(int id, bool available)
    {
        Character character = _characterList[id-1];
        if (character == null) return;

        character.SetAvailable(available);
    }
}
