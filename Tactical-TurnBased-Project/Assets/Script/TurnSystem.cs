using System;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class TurnSystem : MonoBehaviour
    {
        private static TurnSystem instance;
        public static TurnSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<TurnSystem>();
                    if (instance == null)
                    {
                        GameObject turnSystemGameObject = new GameObject("TurnSystem");
                        instance = turnSystemGameObject.AddComponent<TurnSystem>();
                    }
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        public event EventHandler OnTurnChanged;
        public event EventHandler OnRoundChanged;

        private int roundNumber = 1;
        private Unit currentTurnUnit;
        private bool hasSpeedAdvantage;
        private List<Unit> allUnitsList = new List<Unit>();

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
            _ = Pathfinding.Instance;
            _ = EnemyAI.Instance;

            RefreshAllUnitsList();
            StartNextTurn();
        }

        public void RefreshAllUnitsList()
        {
            allUnitsList.Clear();
            Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (Unit u in units)
            {
                if (u != null && !u.IsDead())
                {
                    allUnitsList.Add(u);
                }
            }

            // Sort all units by speed descending
            allUnitsList.Sort((a, b) => b.GetSpeed().CompareTo(a.GetSpeed()));
        }

        public void StartNextTurn()
        {
            RefreshAllUnitsList();

            if (allUnitsList.Count == 0) return;

            // Find units that have not yet acted this round and are alive
            List<Unit> availableUnits = allUnitsList.FindAll(u => u != null && !u.IsDead() && !u.HasActedThisRound());

            // If all living units have acted, start a new round!
            if (availableUnits.Count == 0)
            {
                roundNumber++;
                foreach (Unit unit in allUnitsList)
                {
                    if (unit != null) unit.SetHasActedThisRound(false);
                }
                availableUnits = new List<Unit>(allUnitsList);
                OnRoundChanged?.Invoke(this, EventArgs.Empty);
                Debug.Log($"[TurnSystem] === ROUND {roundNumber} STARTED ===");
            }

            if (availableUnits.Count == 0) return;

            // Highest speed unit goes next
            currentTurnUnit = availableUnits[0];

            // Check Double Move / Speed Advantage Rule
            if (availableUnits.Count > 1)
            {
                Unit nextUnit = availableUnits[1];
                if (currentTurnUnit.GetSpeed() >= 2 * nextUnit.GetSpeed())
                {
                    hasSpeedAdvantage = true;
                    currentTurnUnit.SetMaxActionPoints(4); // Double Actions (4 AP)
                    Debug.Log($"[TurnSystem] ⚡ SPEED ADVANTAGE: {currentTurnUnit.name} (Speed {currentTurnUnit.GetSpeed()}) has >= double the speed of {nextUnit.name} (Speed {nextUnit.GetSpeed()})! Granted 4 Action Points!");
                }
                else
                {
                    hasSpeedAdvantage = false;
                    currentTurnUnit.SetMaxActionPoints(2);
                }
            }
            else
            {
                hasSpeedAdvantage = false;
                currentTurnUnit.SetMaxActionPoints(2);
            }

            currentTurnUnit.ResetActionPoints();

            // Set active turn unit in UnitActionSystem
            if (UnitActionSystem.Instance != null)
            {
                UnitActionSystem.Instance.SetActiveTurnUnit(currentTurnUnit);
            }

            // Camera focuses on active unit
            if (GridCursor.Instance != null)
            {
                GridCursor.Instance.SetSelectedGridPosition(currentTurnUnit.GetGridPosition());
            }

            OnTurnChanged?.Invoke(this, EventArgs.Empty);

            string faction = currentTurnUnit.IsEnemy() ? "Enemy" : "Player";
            Debug.Log($"[TurnSystem] >>> TURN START: [{faction}] {currentTurnUnit.name} (HP: {currentTurnUnit.GetHealth()}/{currentTurnUnit.GetMaxHealth()}, Speed: {currentTurnUnit.GetSpeed()}, AP: {currentTurnUnit.GetActionPoints()}) <<<");

            // If it's an Enemy unit's turn, trigger Enemy AI immediately
            if (currentTurnUnit.IsEnemy() && EnemyAI.Instance != null)
            {
                EnemyAI.Instance.StartEnemyTurn(currentTurnUnit);
            }
        }

        public void RemoveUnitFromTurnOrder(Unit unit)
        {
            if (allUnitsList.Contains(unit))
            {
                allUnitsList.Remove(unit);
            }

            if (currentTurnUnit == unit)
            {
                currentTurnUnit = null;
                StartNextTurn();
            }
        }

        public void EndCurrentUnitTurn()
        {
            if (currentTurnUnit != null)
            {
                currentTurnUnit.SetHasActedThisRound(true);
                Debug.Log($"[TurnSystem] Turn ended for {currentTurnUnit.name}.");
            }

            StartNextTurn();
        }

        public Unit GetCurrentTurnUnit() => currentTurnUnit;
        public int GetRoundNumber() => roundNumber;
        public bool HasSpeedAdvantage() => hasSpeedAdvantage;
        public List<Unit> GetAllUnitsSortedBySpeed() => allUnitsList;
    }
}
