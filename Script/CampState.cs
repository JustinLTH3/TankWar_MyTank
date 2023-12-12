using System;
using UnityEngine;

public class CampState : BaseState
{
    private MyTank _myTank;

    public CampState(MyTank myTank)
    {
        _myTank = myTank;
    }

    public override void StateEnter()
    {
        //_myTank.facts[_myTank.ATTACKSTATE] = true;
    }

    public override Type StateUpdate()
    {
        //_myTank.Attack();
        return null;
    }

    public override void StateExit()
    {
        //_myTank.facts[_myTank.ATTACKSTATE] = false;
    }
}