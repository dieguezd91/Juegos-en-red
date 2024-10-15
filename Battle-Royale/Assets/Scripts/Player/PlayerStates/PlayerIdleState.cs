using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState<T> : PlayerStateBase<T>
{
    private ITreeNode _root;
    public PlayerIdleState(ITreeNode root)
    {
        _root = root;
    }
    public override void Awake()
    {
        base.Awake();
        //Debug.Log("IdleENTER");
    }

    public override void Execute()
    {
        base.Execute();
        if (_playerController.pv.IsMine)
        {
            _playerController.HandleInput();
        }
        if (CheckState())
            _root.Execute();

    }

    public override void Sleep()
    {
        base.Sleep();
        //Debug.Log("IdleEXIT");
    }
    bool CheckState()
    {
        return _playerController.InputMovement != Vector2.zero || _playerController.IsDashing || _playerController.LifeController.currentHp < 0f;
    }
}
