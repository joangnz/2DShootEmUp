using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    [SerializeField] private Sprite a1sprite1, a1sprite2, a1sprite3, a1sprite4, a1sprite5;
    private Sprite[] Attack1Sprites;
    [SerializeField] private Sprite a2sprite1, a2sprite2, a2sprite3, a2sprite4, a2sprite5;
    private Sprite[] Attack2Sprites;

    void Awake()
    {
        Attack1Sprites = new Sprite[] { a1sprite1, a1sprite2, a1sprite3, a1sprite4, a1sprite5 };
        Attack2Sprites = new Sprite[] { a2sprite1, a2sprite2, a2sprite3, a2sprite4, a2sprite5 };
    }

    public Sprite GetA1Sprite(int id) => Attack1Sprites[id-1];

    public Sprite GetA2Sprite(int id) => Attack2Sprites[id-1];

    public Sprite GetSprite(int attackId, int id)
    {
        if (attackId == 1) return GetA1Sprite(id);
        else if (attackId == 2) return GetA2Sprite(id);
        else return null;
    }
}
