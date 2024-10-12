using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodgeState<T> : PlayerStateBase<T>
{
    private ITreeNode _root;
    public PlayerDodgeState(ITreeNode root)
    {
        _root = root;
    }
    public override void Awake()
    {
        base.Awake();
        if (CheckState()) _root.Execute();

    }

    public override void Execute()
    {
        base.Execute();
        if (CheckState()) _root.Execute();
    }

    public override void Sleep()
    {
        base.Sleep();
    }
    bool CheckState()
    {
        return _playerController.InputMovement != Vector2.zero || _playerController.IsDashing || _playerController.LifeController.currentHp > 0;
    }
}
