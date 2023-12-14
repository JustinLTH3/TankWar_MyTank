using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EB_StateMachine : MonoBehaviour
{
    public EB_BaseState CurrentState { get; private set; }
    Dictionary<Type, EB_BaseState> states = new Dictionary<Type, EB_BaseState>();

    public void AddState(Type tState, EB_BaseState state)
    {
        states.Add(tState, state);
        if (CurrentState != null) return;
        CurrentState = state;
        CurrentState.StateEnter();
    }

    private void Update()
    {
        if (CurrentState == null)
        {
            CurrentState = states.First().Value;
            CurrentState.StateEnter();
        }

        Type state = CurrentState.StateUpdate();
        if (state != null) SwitchState(states[state]);
    }

    public void SwitchState(Type state)
    {
        SwitchState(states[state]);
    }

    void SwitchState(EB_BaseState state)
    {
        CurrentState.StateExit();
        CurrentState = state;
        CurrentState.StateEnter();
    }
}