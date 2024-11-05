using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovingState<T> : PlayerStateBase<T>
{
    private ITreeNode _root;

    public PlayerMovingState(ITreeNode root)
    {
        _root = root;
    }

    public override void Awake()
    {
        base.Awake();
        _playerController.animator.SetBool("IsMoving", true);
    }

    public override void Execute()
    {
        base.Execute();
        if (_playerController.pv.IsMine)
        {
            _playerController.HandleInput();
        }

        if (_playerController.isSprinting)
        {
            _playerController.animator.SetBool("IsSprinting", true);
            _playerController.animator.SetBool("IsMoving", false);
        }
        else
        {
            _playerController.animator.SetBool("IsSprinting", false);
            _playerController.animator.SetBool("IsMoving", true);
        }

        if (CheckState()) _root.Execute();
    }

    public override void Sleep()
    {
        base.Sleep();
        _playerController.animator.SetBool("IsMoving", false);
        _playerController.animator.SetBool("IsSprinting", false);
    }

    bool CheckState()
    {
        return _playerController.InputMovement == Vector2.zero || _playerController.IsDashing || _playerController.LifeController.currentHp <= 0f;
    }
}