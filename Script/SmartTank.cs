﻿using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using System.Data;

public class SmartTank : AITank
{
    //store ALL currently visible 
    public Dictionary<GameObject, float> enemyTanksFound = new Dictionary<GameObject, float>();
    public Dictionary<GameObject, float> consumablesFound = new Dictionary<GameObject, float>();
    public Dictionary<GameObject, float> enemyBasesFound = new Dictionary<GameObject, float>();

    public KeyValuePair<GameObject, float> closestTarget = new KeyValuePair<GameObject, float>();
    public Dictionary<GameObject, Vector3> targetLastFrame;
    private KeyValuePair<GameObject, float> closestConsumable;

    public GameObject attackPoint;
    public GameObject lastSeenPos;

    //store ONE from ALL currently visible
    public GameObject enemyTankPosition;
    public GameObject consumablePosition;
    public GameObject enemyBasePosition;

    public float predictedEnemyHealth;

    //timer
    float t;


    private StateMachine _stateMachine;
    private Rules _rules = new();
    public Dictionary<string, bool> facts = new();

    public string ROAMSTATE => "RoamState";
    public string CAMPSTATE => "CampState";
    public string COMBATSTATE => "CombatState";
    public string CLOSECOMBATSTATE => "CloseCombatState";
    public string LONGCOMBATSTATE => "LongCombatState";
    public string REFILLSTATE => "RefillState";
    public string CHASESTATE => "ChaseState";

    public string HEALTHFOUND => "HealthFound";

    public string AMMOFOUND => "AmmoFound";
    //True when health lower than 75.
    public string NOTFULLHEALTH => "NotFullHealth";
    public string NOAMMO => "NoAmmo";
    public string GOODAMMO => "GoodAmmo";
    public string ENEMYTANKFOUND => "EnemyTankFound";
    public string BASEFOUND => "BaseFound";
    public string CANSEEENEMY => "CanSeeEnemy";
    public string ENEMYCANSEEUS => "EnemyCanSeeUs";
    public string ENEMYCANNOTSEEUS => "EnemyCanNotSeeUs";
    public string FUELFOUND => "FuelFound";
    public string BADFUEL => "BadFuel";
    public string FLEESTATE => "FleeState";
    public string GOODHEALTH => "GoodHealth";
    public string ENEMYHEALTHHIGHER => "EnemyHealthHiger";
    public string ENEMYHEALTHLOWER => "EnemyHealthLower";
    public string TARGETINRANGE => "TargetInRange";
    public string NOTARGETINRANGE => "NoTargetInRange";
    public string REACHEDLASTPOS => "ReachedLastPos";

    public override void AITankStart()
    {
        attackPoint = new GameObject();
        lastSeenPos = new GameObject();

        _stateMachine = gameObject.AddComponent<StateMachine>();

        facts.Add(ROAMSTATE, false);
        facts.Add(CAMPSTATE, false);
        facts.Add(REFILLSTATE, false);
        facts.Add(COMBATSTATE, false);
        facts.Add(CLOSECOMBATSTATE, false);
        facts.Add(LONGCOMBATSTATE, false);
        facts.Add(FLEESTATE, false);
        facts.Add(CHASESTATE, false);

        _stateMachine.AddState(typeof(RoamState), new RoamState(this));
        _stateMachine.AddState(typeof(CampState), new CampState(this));
        _stateMachine.AddState(typeof(CombatState), new CombatState(this));
        _stateMachine.AddState(typeof(RefillState), new RefillState(this));
        _stateMachine.AddState(typeof(FleeState), new FleeState(this));
        _stateMachine.AddState(typeof(CloseCombatState), new CloseCombatState(this));
        _stateMachine.AddState(typeof(LongCombatState), new LongCombatState(this));
        _stateMachine.AddState(typeof(ChaseState), new ChaseState(this));

        facts.Add(ENEMYTANKFOUND, false);
        facts.Add(HEALTHFOUND, false);
        facts.Add(FUELFOUND, false);
        facts.Add(AMMOFOUND, false);
        facts.Add(BASEFOUND, false);
        facts.Add(NOTFULLHEALTH, false);
        facts.Add(BADFUEL, false);
        facts.Add(CANSEEENEMY, false);
        facts.Add(ENEMYCANSEEUS, false);
        facts.Add(ENEMYHEALTHHIGHER, false);
        facts.Add(ENEMYHEALTHLOWER, false);
        facts.Add(GOODAMMO, false);
        facts.Add(TARGETINRANGE, false);
        facts.Add(NOTARGETINRANGE, false);

        _rules.AddRule(new Rule(ROAMSTATE, TARGETINRANGE, typeof(ChaseState), Rule.Predicate.And));
        _rules.AddRule(new Rule(CHASESTATE, ENEMYTANKFOUND, typeof(CombatState), Rule.Predicate.And));
        //_rules.AddRule(new Rule(ROAMSTATE, HEALTHFOUND, typeof(CampState), Rule.Predicate.And));
        _rules.AddRule(new Rule(ROAMSTATE, HEALTHFOUND, typeof(RefillState), Rule.Predicate.And));
        _rules.AddRule(new Rule(ROAMSTATE, FUELFOUND, typeof(RefillState), Rule.Predicate.And));
        _rules.AddRule(new Rule(ROAMSTATE, AMMOFOUND, typeof(RefillState), Rule.Predicate.And));
        _rules.AddRule(new Rule(CHASESTATE, BASEFOUND, typeof(CombatState), Rule.Predicate.And));
        _rules.AddRule(new Rule(CAMPSTATE, NOTFULLHEALTH, typeof(RefillState), Rule.Predicate.And));
        _rules.AddRule(new Rule(COMBATSTATE, ENEMYCANSEEUS, typeof(LongCombatState), Rule.Predicate.And));
        _rules.AddRule(new Rule(COMBATSTATE, ENEMYHEALTHLOWER, typeof(CloseCombatState), Rule.Predicate.And));
        _rules.AddRule(new Rule(COMBATSTATE, NOAMMO, typeof(FleeState), Rule.Predicate.And));
        _rules.AddRule(new Rule(COMBATSTATE, GOODAMMO, typeof(LongCombatState), Rule.Predicate.And));
        _rules.AddRule(new Rule(CLOSECOMBATSTATE, NOTARGETINRANGE, typeof(RoamState), Rule.Predicate.And));
        _rules.AddRule(new Rule(LONGCOMBATSTATE, NOTARGETINRANGE, typeof(RoamState), Rule.Predicate.And));
        _rules.AddRule(new Rule(COMBATSTATE, NOTARGETINRANGE, typeof(RoamState), Rule.Predicate.And));
        _rules.AddRule(new Rule(CHASESTATE, REACHEDLASTPOS, typeof(RoamState), Rule.Predicate.And));
    }

    public override void AITankUpdate()
    {
        //Update all currently visible.
        enemyTanksFound = TanksFound;
        consumablesFound = ConsumablesFound;
        enemyBasesFound = BasesFound;

        //Update facts
        facts[NOTFULLHEALTH] = GetHealthLevel < 75;
        facts[ENEMYTANKFOUND] = enemyTanksFound.Count > 0 && enemyTanksFound.First().Value < 25f;
        facts[BASEFOUND] = enemyBasesFound.Count > 0 && enemyBasesFound.First().Value < 25f;
        facts[CANSEEENEMY] = facts[ENEMYTANKFOUND] || facts[BASEFOUND];
        facts[ENEMYCANSEEUS] = facts[ENEMYTANKFOUND] && Vector3.Dot(enemyTanksFound.Keys.First().transform.forward, transform.forward) < 0;
        facts[NOAMMO] = GetAmmoLevel == 0;
        facts[GOODAMMO] = !facts[NOAMMO];
        facts[AMMOFOUND] = ConsumablesFound.Count > 0 && ConsumablesFound.Where(x => x.Key.CompareTag("Ammo")).Count() > 0;
        facts[HEALTHFOUND] = ConsumablesFound.Count > 0 && ConsumablesFound.Where(x => x.Key.CompareTag("Health")).Count() > 0;
        facts[FUELFOUND] = ConsumablesFound.Count > 0 && ConsumablesFound.Where(x => x.Key.CompareTag("Fuel")).Count() > 0;
        facts[GOODHEALTH] = !facts[NOTFULLHEALTH];
        facts[ENEMYCANNOTSEEUS] = !facts[ENEMYCANSEEUS];
        facts[ENEMYHEALTHHIGHER] = predictedEnemyHealth > GetHealthLevel;
        facts[ENEMYHEALTHLOWER] = !facts[ENEMYHEALTHHIGHER];
        facts[TARGETINRANGE] = enemyTanksFound.Count > 0 || enemyBasesFound.Count > 0;
        facts[NOTARGETINRANGE] = !facts[TARGETINRANGE];
        facts[REACHEDLASTPOS] = Vector3.Distance(transform.position, lastSeenPos.transform.position) < 5f;



        foreach (var rule in _rules.RuleList)
        {
            Type x = rule.CheckRule(facts);
            if (x != null) _stateMachine.SwitchState(x);
        }
    }

    public void Search()
    {
        FollowPathToRandomPoint(facts[BADFUEL] ? 0.5f : 0.7f);
    }

    public Type Chase()
    {
        if (facts[TARGETINRANGE] && enemyTanksFound.Count != 0)
        {
            enemyTankPosition = enemyTanksFound.Keys.First();
            lastSeenPos.transform.position = enemyTankPosition.transform.position;
            FollowPathToPoint(enemyTankPosition, 1f);
            return null;
        }
        else if (facts[TARGETINRANGE] && enemyBasesFound.Count != 0)
        {
            enemyBasePosition = enemyBasesFound.Keys.First();
            FollowPathToPoint(enemyBasePosition, 1f);
            return null;
        }
        else if (enemyTanksFound.Count == 0 && lastSeenPos != null)
        {
            FollowPathToPoint(lastSeenPos, 1f);
            return typeof(RoamState);
        }
        return null;
    }

    public void Attack()
    {

        Debug.Log(enemyTanksFound.Count);
        if (facts[ENEMYTANKFOUND])
        {
            attackPoint.transform.position = enemyTanksFound.Keys.First().transform.position;
            var TankDistance = enemyTanksFound.First().Value;
            Vector3 EnemyDirecion = enemyTanksFound.Keys.First().transform.forward;
            attackPoint.transform.position += EnemyDirecion* enemyTanksFound.Keys.First().GetComponent<Rigidbody>().velocity.magnitude;

        }
        //else if (enemyBasesFound.Count == 2)
        //{
            
        //}
        else if (facts[BASEFOUND])
        {
            if (IsFiring) return;
            attackPoint.transform.position = enemyBasesFound.Keys.First().transform.position;
        }
        
        if (IsFiring) return;



        FireAtPoint(attackPoint);
    }

    public void Refill()
    {
        //FaceTurretToPoint(closestConsumable.Key);
        GameObject target = null;
        if (facts[FUELFOUND]) target = ConsumablesFound.Where(x => x.Key.CompareTag("Fuel")).First().Key;
        else if (facts[HEALTHFOUND]) target = ConsumablesFound.Where(x => x.Key.CompareTag("Health")).First().Key;
        else if (facts[AMMOFOUND]) target = ConsumablesFound.Where(x => x.Key.CompareTag("Ammo")).First().Key;
        if (target == null) return;
        FollowPathToPoint(target, .8f);
    }

    public override void AIOnCollisionEnter(Collision collision)
    {
    }
}