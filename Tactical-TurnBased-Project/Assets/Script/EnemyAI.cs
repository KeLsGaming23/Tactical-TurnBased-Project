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

                Unit activeUnit = TurnSystem.Instance.GetCurrentTurnUnit();
                if (activeUnit != null && activeUnit.IsEnemy() && !isRunningAI)
                {
                    StartEnemyTurn(activeUnit);
                }
            }
        }

        private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
        {
            Unit activeUnit = TurnSystem.Instance.GetCurrentTurnUnit();
            if (activeUnit != null && activeUnit.IsEnemy() && !isRunningAI)
            {
                StartEnemyTurn(activeUnit);
            }
        }

        public void StartEnemyTurn(Unit enemyUnit)
        {
            if (enemyUnit == null || !enemyUnit.IsEnemy() || enemyUnit.IsDead()) return;

            StopAllCoroutines();
            StartCoroutine(RunEnemyTurnRoutine(enemyUnit));
        }

        private IEnumerator RunEnemyTurnRoutine(Unit enemyUnit)
        {
            isRunningAI = true;
            Debug.Log($"[EnemyAI] >>> ENEMY TURN ACTIVE: {enemyUnit.name} (HP: {enemyUnit.GetHealth()}/{enemyUnit.GetMaxHealth()}, Speed: {enemyUnit.GetSpeed()}, AP: {enemyUnit.GetActionPoints()}) <<<");

            // Focus camera on the active enemy
            if (GridCursor.Instance != null)
            {
                GridCursor.Instance.SetSelectedGridPosition(enemyUnit.GetGridPosition());
            }

            // Brief pause before first action for smooth pacing
            yield return new WaitForSeconds(0.6f);

            while (enemyUnit != null && !enemyUnit.IsDead() && enemyUnit.GetActionPoints() > 0)
            {
                bool actionTaken = false;
                bool actionCompleted = false;

                // Priority 1: Check if enemy has a player in cardinal range (Up/Down/Left/Right 1 cell) -> SPIN Attack!
                GridPosition targetCardinalPos;
                Unit cardinalPlayer = GetCardinalPlayerUnit(enemyUnit, out targetCardinalPos);

                if (cardinalPlayer != null && enemyUnit.CanSpendActionPointsToTakeAction(enemyUnit.GetSpinAction()))
                {
                    Debug.Log($"[EnemyAI] {enemyUnit.name} attacking {cardinalPlayer.name} with Cardinal Spin Attack at {targetCardinalPos}!");

                    if (UnitActionSystem.Instance != null)
                    {
                        UnitActionSystem.Instance.SetFlowState(UnitActionSystem.ActionFlowState.ActionExecuting);
                        UnitActionSystem.Instance.SetBusy();
                    }

                    enemyUnit.TrySpendActionPointsToTakeAction(enemyUnit.GetSpinAction());
                    enemyUnit.GetSpinAction().TakeAction(targetCardinalPos, () =>
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
                        else
                        {
                            Debug.Log($"[EnemyAI] {enemyUnit.name} already at best reachable position or blocked.");
                        }
                    }
                }

                if (!actionTaken)
                {
                    // No valid actions possible (e.g. trapped, reached destination, or insufficient AP)
                    break;
                }

                // Wait for the action to finish
                while (!actionCompleted)
                {
                    yield return null;
                }

                // Pause slightly between actions for readability
                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log($"[EnemyAI] {enemyUnit.name} finished actions (Remaining AP: {enemyUnit?.GetActionPoints()}). Ending turn...");
            yield return new WaitForSeconds(0.4f);

            isRunningAI = false;
            if (TurnSystem.Instance != null)
            {
                TurnSystem.Instance.EndCurrentUnitTurn();
            }
        }

        private Unit GetCardinalPlayerUnit(Unit enemyUnit, out GridPosition targetGridPosition)
        {
            targetGridPosition = new GridPosition(0, 0);
            GridPosition enemyPos = enemyUnit.GetGridPosition();

            GridPosition[] cardinalOffsets = new GridPosition[]
            {
                new GridPosition(0, +1),  // UP
                new GridPosition(0, -1),  // DOWN
                new GridPosition(-1, 0),  // LEFT
                new GridPosition(+1, 0),  // RIGHT
            };

            foreach (GridPosition offset in cardinalOffsets)
            {
                GridPosition testPos = enemyPos + offset;
                if (LevelGrid.Instance != null && LevelGrid.Instance.IsValidGridPosition(testPos))
                {
                    Unit unitOnTile = LevelGrid.Instance.GetUnitAtGridPosition(testPos);
                    if (unitOnTile != null && !unitOnTile.IsEnemy() && !unitOnTile.IsDead())
                    {
                        targetGridPosition = testPos;
                        return unitOnTile;
                    }
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
                if (unit == null || unit.IsEnemy() || unit.IsDead()) continue;

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

            if (validMovePositions == null || validMovePositions.Count == 0)
            {
                return currentPos;
            }

            GridPosition bestPosition = currentPos;
            int bestDistance = Mathf.Abs(currentPos.x - targetPlayerPos.x) + Mathf.Abs(currentPos.z - targetPlayerPos.z);

            foreach (GridPosition movePos in validMovePositions)
            {
                int distance = Mathf.Abs(movePos.x - targetPlayerPos.x) + Mathf.Abs(movePos.z - targetPlayerPos.z);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPosition = movePos;
                }
            }

            return bestPosition;
        }

        public bool IsRunningAI() => isRunningAI;
    }
}
