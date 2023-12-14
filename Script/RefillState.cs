using System;
using UnityEngine;

public class RefillState : BaseState
{
    private SmartTank _myTank;

    public RefillState(SmartTank myTank)
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
        return typeof(RoamState);
    }

    public override void StateExit()
    {
        _myTank.facts[_myTank.REFILLSTATE] = false;
    }
}