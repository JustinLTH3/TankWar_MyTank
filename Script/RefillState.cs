﻿using System;
using UnityEngine;

public class RefillState : BaseState
{
    private MyTank _myTank;
    private float t;

    public RefillState(MyTank myTank)
    {
        _myTank = myTank;
    }

    public override void StateEnter()
    {
        _myTank.facts[_myTank.REFILLSTATE] = true;
        t = 0;
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