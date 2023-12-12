using System;

public class ChaseState : BaseState
{
    private MyTank _myTank;

    public ChaseState(MyTank myTank)
    {
        _myTank = myTank;
    }

    public override void StateEnter()
    {
        _myTank.facts[_myTank.CHASESTATE] = true;
    }

    public override Type StateUpdate()
    {
        _myTank.Chase();
        return null;
    }

    public override void StateExit()
    {
        _myTank.facts[_myTank.CHASESTATE] = false;
    }
}