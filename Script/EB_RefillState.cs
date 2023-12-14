using System;
using UnityEngine;

public class EB_RefillState : EB_BaseState
{
    private EB_SmartTank _myTank;

    public EB_RefillState(EB_SmartTank myTank)
    {
        _myTank = myTank;
    }

    public override void StateEnter()
    {
        _myTank.facts[_myTank.REFILLSTATE] = true;
    }

    public override Type StateUpdate()
    {
        _myTank.Refill();
        return typeof(EB_RoamState);
    }

    public override void StateExit()
    {
        _myTank.facts[_myTank.REFILLSTATE] = false;
    }
}