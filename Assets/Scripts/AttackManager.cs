using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : NetworkBehaviour
{
    [Header("Attack 1 Sprites")]
    [SerializeField] private Sprite a1sprite1;
    [SerializeField] private Sprite a1sprite2, a1sprite3, a1sprite4, a1sprite5;
    private Sprite[] Attack1Sprites;

    [Header("Attack 2 Sprites")]
    [SerializeField] private Sprite a2sprite1;
    [SerializeField] private Sprite a2sprite2, a2sprite3, a2sprite4, a2sprite5;
    private Sprite[] Attack2Sprites;

    [Header("Attack 1 Animators")]
    [SerializeField] private Animator a1animator1;
    [SerializeField] private Animator a1animator2, a1animator3, a1animator4, a1animator5;
    private Animator[] Attack1Animators;

    //[Header("Attack 2 Animators")]
    //[SerializeField] private Animator a2animator1;
    //[SerializeField] private Animator a2animator2, a2animator3, a2animator4, a2animator5;
    //private Animator[] Attack2Animators;

    void Awake()
    {
        Attack1Sprites = new Sprite[] { a1sprite1, a1sprite2, a1sprite3, a1sprite4, a1sprite5 };
        Attack2Sprites = new Sprite[] { a2sprite1, a2sprite2, a2sprite3, a2sprite4, a2sprite5 };
        Attack1Animators = new Animator[] { a1animator1, a1animator2, a1animator3, a1animator4, a1animator5 };
        //Attack2Animators = new Animator[] { a2animator1, a2animator2, a2animator3, a2animator4, a2animator5 };
    }

    public Sprite GetA1Sprite(int id) => Attack1Sprites[id-1];

    public Sprite GetA2Sprite(int id) => Attack2Sprites[id-1];

    public Animator GetA1Animator(int id) => Attack1Animators[id-1];

    public Animator GetA2Animator(int id) => Attack1Animators[id-1];
}
