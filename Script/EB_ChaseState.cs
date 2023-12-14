using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EB_ChaseState : EB_BaseState
{
    private EB_SmartTank _myTank;

    public EB_ChaseState(EB_SmartTank myTank)
    {
        _myTank = myTank;
    }

    public override void StateEnter()
    {
        _myTank.facts[_myTank.CHASESTATE] = true;
        Debug.Log("ChaseState");
    }

    public override Type StateUpdate()
    {
        return _myTank.Chase();
    }

    public override void StateExit()
    {
        _myTank.facts[_myTank.CHASESTATE] = false;
    }
}
