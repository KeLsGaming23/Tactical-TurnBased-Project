using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class EnemyAI : MonoBehaviour
    {
        private static EnemyAI instance;
        public static EnemyAI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<EnemyAI>();
                    if (instance == null)
                    {
                        GameObject enemyAIGameObject = new GameObject("EnemyAI");
                        instance = enemyAIGameObject.AddComponent<EnemyAI>();
                    }
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        private bool isRunningAI;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Start()
        {
            if (TurnSystem.Instance != null)
            {
                TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
            }
        }

        private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
        {
            Unit activeUnit = TurnSystem.Instance.GetCurrentTurnUnit();
            if (activeUnit != null && activeUnit.IsEnemy())
            {
                StartCoroutine(RunEnemyTurnRoutine(activeUnit));
            }
        }

        private IEnumerator RunEnemyTurnRoutine(Unit enemyUnit)
        {
            isRunningAI = true;
            Debug.Log($"[EnemyAI] >>> ENEMY TURN START: {enemyUnit.name} (Speed: {enemyUnit.GetSpeed()}, AP: {enemyUnit.GetActionPoints()}) <<<");

            // Focus camera on the active enemy
            if (GridCursor.Instance != null)
            {
                GridCursor.Instance.SetSelectedGridPosition(enemyUnit.GetGridPosition());
            }

            // Brief pause before first action for smooth pacing
            yield return new WaitForSeconds(0.6f);

            while (enemyUnit != null && enemyUnit.GetActionPoints() > 0)
            {
                bool actionTaken = false;
                bool actionCompleted = false;

                // Priority 1: Check if enemy is adjacent to any Player unit -> SPIN Attack!
                Unit adjacentPlayer = GetAdjacentPlayerUnit(enemyUnit);
                if (adjacentPlayer != null && enemyUnit.CanSpendActionPointsToTakeAction(enemyUnit.GetSpinAction()))
                {
                    Debug.Log($"[EnemyAI] {enemyUnit.name} is adjacent to {adjacentPlayer.name}! Executing SPIN Attack!");

                    if (UnitActionSystem.Instance != null)
                    {
                        UnitActionSystem.Instance.SetFlowState(UnitActionSystem.ActionFlowState.ActionExecuting);
                        UnitActionSystem.Instance.SetBusy();
                    }

                    enemyUnit.TrySpendActionPointsToTakeAction(enemyUnit.GetSpinAction());
                    enemyUnit.GetSpinAction().TakeAction(enemyUnit.GetGridPosition(), () =>
                    {
                        actionCompleted = true;
                        if (UnitActionSystem.Instance != null)
                        {
                            UnitActionSystem.Instance.ClearBusy();
                        }
                    });

                    actionTaken = true;
                }
                else
                {
                    // Priority 2: Chase down the closest player unit
                    Unit closestPlayer = GetClosestPlayerUnit(enemyUnit);
                    if (closestPlayer != null && enemyUnit.CanSpendActionPointsToTakeAction(enemyUnit.GetMoveAction()))
                    {
                        GridPosition bestMovePosition = GetBestMovePositionTowardsPlayer(enemyUnit, closestPlayer);

                        if (bestMovePosition != enemyUnit.GetGridPosition())
                        {
                            Debug.Log($"[EnemyAI] {enemyUnit.name} chasing {closestPlayer.name} towards {bestMovePosition}.");

                            if (UnitActionSystem.Instance != null)
                            {
                                UnitActionSystem.Instance.SetFlowState(UnitActionSystem.ActionFlowState.ActionExecuting);
                                UnitActionSystem.Instance.SetBusy();
                            }

                            enemyUnit.TrySpendActionPointsToTakeAction(enemyUnit.GetMoveAction());
                            enemyUnit.GetMoveAction().TakeAction(bestMovePosition, () =>
                            {
                                actionCompleted = true;
                                if (UnitActionSystem.Instance != null)
                                {
                                    UnitActionSystem.Instance.ClearBusy();
                                }
                            });

                            actionTaken = true;
                        }
                    }
                }

                if (!actionTaken)
                {
                    // No valid actions possible (e.g. trapped or insufficient AP)
                    break;
                }

                // Wait for the action to finish
                while (!actionCompleted)
                {
                    yield return null;
                }

                // Pause slightly between actions
                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log($"[EnemyAI] {enemyUnit.name} finished all actions. Ending turn...");
            yield return new WaitForSeconds(0.4f);

            isRunningAI = false;
            if (TurnSystem.Instance != null)
            {
                TurnSystem.Instance.EndCurrentUnitTurn();
            }
        }

        private Unit GetAdjacentPlayerUnit(Unit enemyUnit)
        {
            GridPosition enemyPos = enemyUnit.GetGridPosition();
            Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

            foreach (Unit unit in allUnits)
            {
                if (unit.IsEnemy()) continue;

                GridPosition playerPos = unit.GetGridPosition();
                int dx = Mathf.Abs(enemyPos.x - playerPos.x);
                int dz = Mathf.Abs(enemyPos.z - playerPos.z);

                if (dx <= 1 && dz <= 1 && (dx != 0 || dz != 0))
                {
                    return unit;
                }
            }

            return null;
        }

        private Unit GetClosestPlayerUnit(Unit enemyUnit)
        {
            GridPosition enemyPos = enemyUnit.GetGridPosition();
            Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            Unit closestPlayer = null;
            int shortestDistance = int.MaxValue;

            foreach (Unit unit in allUnits)
            {
                if (unit.IsEnemy()) continue;

                GridPosition playerPos = unit.GetGridPosition();
                int distance = Mathf.Abs(enemyPos.x - playerPos.x) + Mathf.Abs(enemyPos.z - playerPos.z);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPlayer = unit;
                }
            }

            return closestPlayer;
        }

        private GridPosition GetBestMovePositionTowardsPlayer(Unit enemyUnit, Unit targetPlayer)
        {
            List<GridPosition> validMovePositions = enemyUnit.GetMoveAction().GetValidActionGridPositionList();
            GridPosition currentPos = enemyUnit.GetGridPosition();
            GridPosition targetPlayerPos = targetPlayer.GetGridPosition();

            GridPosition bestPosition = currentPos;
            int bestPathCost = int.MaxValue;

            foreach (GridPosition movePos in validMovePositions)
            {
                int pathCost = int.MaxValue;
                if (Pathfinding.Instance != null)
                {
                    pathCost = Pathfinding.Instance.GetPathLength(movePos, targetPlayerPos);
                }
                else
                {
                    pathCost = Mathf.Abs(movePos.x - targetPlayerPos.x) + Mathf.Abs(movePos.z - targetPlayerPos.z);
                }

                if (pathCost < bestPathCost)
                {
                    bestPathCost = pathCost;
                    bestPosition = movePos;
                }
            }

            return bestPosition;
        }

        public bool IsRunningAI() => isRunningAI;
    }
}
