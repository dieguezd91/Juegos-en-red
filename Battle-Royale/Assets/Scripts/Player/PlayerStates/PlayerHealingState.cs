using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealingState<T> : PlayerStateBase<T>
{
    private ITreeNode _root;
    public PlayerHealingState(ITreeNode root)
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
    public void InterruptHealing()
    {
        if (_playerController.LifeController._isHealing)
        {
            _playerController.StopAllCoroutines(); // Detener la corutina de curaci�n
            Debug.Log("Healing interrupted!");

            // Cambiar el estado a Moving, dependiendo de la situaci�n
            _playerController.LifeController._isHealing = false;
            _root.Execute();
        }
    }
}