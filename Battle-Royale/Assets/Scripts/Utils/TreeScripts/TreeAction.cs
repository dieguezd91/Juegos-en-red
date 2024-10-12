using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAction : ITreeNode
{
    Action _action;
    public TreeAction(Action action)
    {
        _action = action;
    }
    public void Execute()
    {
        if (_action != null)
//            Debug.Log("TREE"+_action);
            _action();
    }
}
