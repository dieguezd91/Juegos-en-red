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
        //Debug.Log("HealingENTER");
    }

    public override void Execute()
    {
        base.Execute();
        //Debug.Log(_playerController.LifeController._isHealing);
        if (CheckState()) _root.Execute();

    }

    public override void Sleep()
    {
        base.Sleep();
        //Debug.Log("HealingEXIT");
    }
    bool CheckState()
    {
        return !_playerController.LifeController._isHealing || _playerController.LifeController.currentHp < 0;
    }
    public void InterruptHealing()
    {
        if (_playerController.LifeController._isHealing)
        {
            _playerController.StopAllCoroutines(); // Detener la corutina de curación
            Debug.Log("Healing interrupted!");

            // Cambiar el estado a Moving, dependiendo de la situación
            _playerController.LifeController._isHealing = false;
            _root.Execute();
        }
    }
}
