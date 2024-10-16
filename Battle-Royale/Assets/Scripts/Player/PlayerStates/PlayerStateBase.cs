using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateBase<T> : State<T>
{
    protected PlayerController _playerController;
    protected PlayerView _view;
    protected PlayerModel _model;
    protected FSM<T> _fsm;

    public void InitializedState(PlayerController _controller, FSM<T> fsm)
    {
        _playerController = _controller;
        _fsm = fsm;
    }
}
