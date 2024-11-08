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
        _playerController.view.Animator.SetBool("IsMoving", true);
    }

    public override void Execute()
    {
        base.Execute();
        if (_playerController.pv.IsMine)
        {
            _playerController.HandleInput();
        }

        if (_playerController.model.IsSprinting)
        {
            _playerController.view.Animator.SetBool("IsSprinting", true);
            _playerController.view.Animator.SetBool("IsMoving", false);
        }
        else
        {
            _playerController.view.Animator.SetBool("IsSprinting", false);
            _playerController.view.Animator.SetBool("IsMoving", true);
        }

        if (CheckState()) _root.Execute();
    }

    public override void Sleep()
    {
        base.Sleep();
        _playerController.view.Animator.SetBool("IsMoving", false);
        _playerController.view.Animator.SetBool("IsSprinting", false);
    }

    bool CheckState()
    {
        return _playerController.InputMovement == Vector2.zero || _playerController.model.IsDashing || _playerController.LifeController.currentHp <= 0f;
    }
}