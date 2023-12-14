using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EB_CombatState : EB_BaseState
{
    private EB_SmartTank _myTank;

    public EB_CombatState(EB_SmartTank myTank)
    {
        _myTank = myTank;
    }

    public override void StateEnter()
    {
        Debug.Log("Combat State");
        _myTank.facts[_myTank.COMBATSTATE] = true;
    }

    public override Type StateUpdate()
    {
        if (_myTank.facts[_myTank.ENEMYTANKFOUND] || _myTank.facts[_myTank.BASEFOUND]) { _myTank.Attack(); }
        else return typeof(EB_RoamState);
        return null;
    }

    public override void StateExit()
    {
        _myTank.facts[_myTank.COMBATSTATE] = false;
    }
}