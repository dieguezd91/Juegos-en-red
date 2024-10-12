using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatState<T> : PlayerStateBase<T>
{
    private ITreeNode _root;
    public PlayerCombatState(ITreeNode root)
    {
        _root = root;
    }
    public override void Awake()
    {
        base.Awake();

    }

    public override void Execute()
    {
        base.Execute();

    }

    public override void Sleep()
    {
        base.Sleep();
    }
}
