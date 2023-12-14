using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using System.Data;

public class EB_SmartTank : AITank
{
    //store ALL currently visible 
    public Dictionary<GameObject, float> enemyTanksFound = new Dictionary<GameObject, float>();
    public Dictionary<GameObject, float> consumablesFound = new Dictionary<GameObject, float>();
    public Dictionary<GameObject, float> enemyBasesFound = new Dictionary<GameObject, float>();

    public GameObject attackPoint;
    public GameObject lastSeenPos;

    //store ONE from ALL currently visible
    public GameObject enemyTankPosition;
    public GameObject enemyBasePosition;

    private EB_StateMachine _stateMachine;
    private EB_Rules _rules = new();
    public Dictionary<string, bool> facts = new();

    public string ROAMSTATE => "RoamState";
    public string COMBATSTATE => "CombatState";
    public string REFILLSTATE => "RefillState";
    public string CHASESTATE => "ChaseState";
    public string FLEESTATE => "FleeState";

    public string HEALTHFOUND => "HealthFound";
    public string AMMOFOUND => "AmmoFound";
    public string FUELFOUND => "FuelFound";

    //Health < 75.
    public string NOTFULLHEALTH => "NotFullHealth";
    //Health <= 25
    public string LOWHEALTH => "LowHealth";
    public string NOAMMO => "NoAmmo";
    //When enemy tank is visible and inside the attack range
    public string ENEMYTANKFOUND => "EnemyTankFound";
    //When enemy base is visible and inside the attack range
    public string BASEFOUND => "BaseFound";
    public string CANSEEENEMY => "CanSeeEnemy";


    //Fuel < 30
    public string BADFUEL => "BadFuel";
    //at least 1 enemy tank or base is visible
    public string TARGETINRANGE => "TargetInRange";
    public string NOTARGETINRANGE => "NoTargetInRange";
    public string REACHEDLASTPOS => "ReachedLastPos";

    public override void AITankStart()
    {
        attackPoint = new GameObject();
        lastSeenPos = new GameObject();

        _stateMachine = gameObject.AddComponent<EB_StateMachine>();

        facts.Add(ROAMSTATE, false);
        facts.Add(REFILLSTATE, false);
        facts.Add(COMBATSTATE, false);
        facts.Add(FLEESTATE, false);
        facts.Add(CHASESTATE, false);

        _stateMachine.AddState(typeof(EB_RoamState), new EB_RoamState(this));
        _stateMachine.AddState(typeof(EB_CombatState), new EB_CombatState(this));
        _stateMachine.AddState(typeof(EB_RefillState), new EB_RefillState(this));
        _stateMachine.AddState(typeof(EB_FleeState), new EB_FleeState(this));
        _stateMachine.AddState(typeof(EB_ChaseState), new EB_ChaseState(this));

        facts.Add(ENEMYTANKFOUND, false);
        facts.Add(HEALTHFOUND, false);
        facts.Add(FUELFOUND, false);
        facts.Add(AMMOFOUND, false);
        facts.Add(BASEFOUND, false);
        facts.Add(NOTFULLHEALTH, false);
        facts.Add(BADFUEL, false);
        facts.Add(CANSEEENEMY, false);
        facts.Add(TARGETINRANGE, false);
        facts.Add(NOTARGETINRANGE, false);

        _rules.AddRule(new EB_Rule(ROAMSTATE, TARGETINRANGE, typeof(EB_ChaseState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(ROAMSTATE, HEALTHFOUND, typeof(EB_RefillState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(ROAMSTATE, FUELFOUND, typeof(EB_RefillState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(ROAMSTATE, AMMOFOUND, typeof(EB_RefillState), EB_Rule.Predicate.And));

        _rules.AddRule(new EB_Rule(CHASESTATE, ENEMYTANKFOUND, typeof(EB_CombatState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(CHASESTATE, BASEFOUND, typeof(EB_CombatState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(CHASESTATE, REACHEDLASTPOS, typeof(EB_RoamState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(CHASESTATE, NOAMMO, typeof(EB_RoamState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(CHASESTATE, BADFUEL, typeof(EB_RoamState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(CHASESTATE, LOWHEALTH, typeof(EB_RoamState), EB_Rule.Predicate.And));

        _rules.AddRule(new EB_Rule(COMBATSTATE, NOTARGETINRANGE, typeof(EB_RoamState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(COMBATSTATE, LOWHEALTH, typeof(EB_FleeState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(COMBATSTATE, BADFUEL, typeof(EB_FleeState), EB_Rule.Predicate.And));
        _rules.AddRule(new EB_Rule(COMBATSTATE, NOAMMO, typeof(EB_FleeState), EB_Rule.Predicate.And));
    }

    public override void AITankUpdate()
    {
        //Update facts
        UpdateFacts();

        foreach (var rule in _rules.RuleList)
        {
            Type x = rule.CheckRule(facts);
            if (x != null) _stateMachine.SwitchState(x);
        }
    }
    public void UpdateFacts()
    {
        //Update all currently visible.
        enemyTanksFound = TanksFound;
        consumablesFound = ConsumablesFound;
        enemyBasesFound = BasesFound;

        facts[NOTFULLHEALTH] = GetHealthLevel < 75;
        facts[LOWHEALTH] = GetHealthLevel <= 25;

        facts[ENEMYTANKFOUND] = enemyTanksFound.Count > 0 && enemyTanksFound.First().Value < 25f;
        facts[BASEFOUND] = enemyBasesFound.Count > 0 && enemyBasesFound.First().Value < 25f;
        facts[CANSEEENEMY] = facts[ENEMYTANKFOUND] || facts[BASEFOUND];

        facts[NOAMMO] = GetAmmoLevel == 0;

        facts[BADFUEL] = GetFuelLevel < 30;

        facts[AMMOFOUND] = ConsumablesFound.Count > 0 && ConsumablesFound.Where(x => x.Key.CompareTag("Ammo")).Count() > 0;
        facts[HEALTHFOUND] = ConsumablesFound.Count > 0 && ConsumablesFound.Where(x => x.Key.CompareTag("Health")).Count() > 0;
        facts[FUELFOUND] = ConsumablesFound.Count > 0 && ConsumablesFound.Where(x => x.Key.CompareTag("Fuel")).Count() > 0;

        facts[TARGETINRANGE] = enemyTanksFound.Count > 0 || enemyBasesFound.Count > 0;
        facts[NOTARGETINRANGE] = !facts[TARGETINRANGE];
        facts[REACHEDLASTPOS] = Vector3.Distance(transform.position, lastSeenPos.transform.position) < 5f;

    }
    //Goes to a random point at different speeds depending on the fuel level
    public void Search()
    {
        FollowPathToRandomPoint(facts[BADFUEL] ? 0.5f : 0.7f);
    }

    //Chasing the tank or go to the base
    public Type Chase()
    {
        UpdateFacts();
        if (facts[TARGETINRANGE])
        {
            if (enemyTanksFound.Count != 0)
            {
                enemyTankPosition = enemyTanksFound.Keys.First();
                lastSeenPos.transform.position = enemyTankPosition.transform.position;
                FollowPathToPoint(enemyTankPosition, 1f);
                return null;
            }
            else if (enemyBasesFound.Count != 0)
            {
                enemyBasePosition = enemyBasesFound.Keys.First();
                FollowPathToPoint(enemyBasePosition, 1f);
                return null;
            }
        }
        else if (lastSeenPos != null)//if we can't see any target go to last seen position of the tank
        {
            FollowPathToPoint(lastSeenPos, 1f);
            if (facts[REACHEDLASTPOS])
            {
                return typeof(EB_RoamState);
            }
        }
        return null;
    }

    public void Attack()
    {
        UpdateFacts();
        if (facts[ENEMYTANKFOUND])
        {
            attackPoint.transform.position = enemyTanksFound.Keys.First().transform.position;
            Vector3 EnemyDirecion = enemyTanksFound.Keys.First().transform.forward;
            attackPoint.transform.position += EnemyDirecion * enemyTanksFound.Keys.First().GetComponent<Rigidbody>().velocity.magnitude;
            // /\ Predicting the enemy movement based on where the enemy tank is facing and its speed
        }
        else //If the enemy is a base assigns a point to it but doesnt predict its movement as its a stationary object
            if (enemyBasesFound.Keys.First() != null)
        {
            attackPoint.transform.position = enemyBasesFound.Keys.First().transform.position;
        }
        FireAtPoint(attackPoint);//Fires at attack point 
    }

    //Flees to random point on the map with max speed
    public void Fleeing()
    {
        FollowPathToRandomPoint(1f);
        UpdateFacts();
    }

    //Get resource
    public void Refill()
    {
        GameObject target = null;
        //select target based on priority if there are more than 1: Fuel > Health > Ammo
        if (facts[FUELFOUND]) target = ConsumablesFound.Where(x => x.Key.CompareTag("Fuel")).First().Key;
        else if (facts[HEALTHFOUND]) target = ConsumablesFound.Where(x => x.Key.CompareTag("Health")).First().Key;
        else if (facts[AMMOFOUND]) target = ConsumablesFound.Where(x => x.Key.CompareTag("Ammo")).First().Key;
        if (target == null) return;
        //drive over to the resource.
        FollowPathToPoint(target, .8f);
    }

    public bool checkForEnemy()
    {
        GameObject point = new();
        point.transform.position = ((transform.position - transform.forward));
        FaceTurretToPoint(point);
        UpdateFacts();
        return !facts[ENEMYTANKFOUND];
    }

    public override void AIOnCollisionEnter(Collision collision)
    {
    }
}