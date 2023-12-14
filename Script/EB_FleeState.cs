using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class EB_FleeState : EB_BaseState
{
    private EB_SmartTank _myTank;
    private float t;

    public EB_FleeState(EB_SmartTank MyTank)
    {
        _myTank = MyTank;
    }
    public override void StateEnter()
    {
        _myTank.facts[_myTank.FLEESTATE] = true;
        t = 0;
    }

    public override void StateExit()
    {
        _myTank.facts[_myTank.FLEESTATE] = false;
    }

    public override Type StateUpdate()
    {
        t += Time.deltaTime;
        if (t < 5)
        {
            _myTank.Fleeing();
        }
        else if (_myTank.checkForEnemy())
        {

            return typeof(EB_RoamState);
        }
        else
        {
            t = 0;
        }
        return null;
    }
}
