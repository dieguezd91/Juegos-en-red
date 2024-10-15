using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDiedState<T> : PlayerStateBase<T>
{

    public override void Awake()
    {
        base.Awake();
        Debug.Log("DiedENTER");
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
