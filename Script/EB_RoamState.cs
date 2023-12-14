using System;
using System.Collections.Generic;

public class EB_RoamState : EB_BaseState
{
    private EB_SmartTank _myTank;

    List<string> doCombat = new List<string>();
    List<string> doRefill = new List<string>();
    List<string> doCamping = new List<string>();
    List<string> doCloseCombat = new List<string>();

    

    public EB_RoamState(EB_SmartTank myTank)
    {
        _myTank = myTank;

        doCombat.Add(_myTank.ENEMYTANKFOUND);

        doRefill.Add(_myTank.FUELFOUND);
        doRefill.Add(_myTank.AMMOFOUND);

        doCamping.Add(_myTank.HEALTHFOUND);

        doCloseCombat.Add(_myTank.BASEFOUND);
    }

    public override void StateEnter()
    {
        _myTank.facts[_myTank.ROAMSTATE] = true;
    }

    public override Type StateUpdate()
    {
        _myTank.Search();
        if (Selection(doCombat, _myTank.facts)) { return typeof(EB_CombatState); }
        if (Selection(doRefill,_myTank.facts)) { return typeof(EB_RefillState); }
        return null;
    }

    public override void StateExit()
    {
        _myTank.facts[_myTank.ROAMSTATE] = false;
    }
}