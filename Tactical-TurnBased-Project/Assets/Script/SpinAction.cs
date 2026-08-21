using System;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class SpinAction : BaseAction
    {
        private float totalSpinAmount;
        private GridPosition targetGridPosition;
        private bool hasDealtDamage;

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            float spinSpeed = 720f;
            float spinAddAmount = spinSpeed * Time.deltaTime;
            transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
            totalSpinAmount += spinAddAmount;

            // Deal damage halfway through the spin
            if (!hasDealtDamage && totalSpinAmount >= 180f)
            {
                hasDealtDamage = true;
                ApplyAttackDamage(targetGridPosition);
            }

            if (totalSpinAmount >= 360f)
            {
                isActive = false;
                onActionComplete?.Invoke();
            }
        }

        private void ApplyAttackDamage(GridPosition targetPos)
        {
            if (LevelGrid.Instance == null) return;

            Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(targetPos);
            if (targetUnit != null && targetUnit != unit)
            {
                // Prevent friendly fire
                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    Debug.Log($"[Combat Attack] ⛔ You cannot attack allies ({targetUnit.name})!");
                    return;
                }

                int attackerStrength = unit.GetStrength();
                int targetDefense = targetUnit.GetDefense();
                int damage = Mathf.Max(1, attackerStrength - targetDefense);

                Debug.Log($"[Combat Attack] ⚔️ {unit.name} (STR: {attackerStrength}) struck {targetUnit.name} (DEF: {targetDefense}) for {damage} damage!");
                targetUnit.Damage(damage);
            }
            else
            {
                Debug.Log($"[Combat Attack] ⚔️ {unit.name} swung attack at {targetPos} (Empty).");
            }
        }

        public override string GetActionName() => "Spin Attack";

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
        {
            this.onActionComplete = onActionComplete;
            this.targetGridPosition = gridPosition;
            this.hasDealtDamage = false;
            this.totalSpinAmount = 0f;
            this.isActive = true;
        }

        public override bool IsValidActionGridPosition(GridPosition gridPosition)
        {
            if (!base.IsValidActionGridPosition(gridPosition)) return false;

            if (LevelGrid.Instance != null)
            {
                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
                if (targetUnit != null)
                {
                    if (targetUnit.IsEnemy() == unit.IsEnemy())
                    {
                        Debug.Log($"[Combat Attack] ⛔ You cannot attack allies ({targetUnit.name})! Action cancelled (no Action Points spent).");
                        return false;
                    }
                }
            }

            return true;
        }

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            List<GridPosition> validGridPositionList = new List<GridPosition>();
            GridPosition unitGridPosition = unit.GetGridPosition();

            // Cardinal 4-directional attack range (Up, Down, Left, Right: 1 cell away)
            GridPosition[] cardinalOffsets = new GridPosition[]
            {
                new GridPosition(0, +1),  // UP
                new GridPosition(0, -1),  // DOWN
                new GridPosition(-1, 0),  // LEFT
                new GridPosition(+1, 0),  // RIGHT
            };

            foreach (GridPosition offset in cardinalOffsets)
            {
                GridPosition testGridPosition = unitGridPosition + offset;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;

                // Exclude allied units so friendly fire is impossible
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                    if (targetUnit != null && targetUnit.IsEnemy() == unit.IsEnemy())
                    {
                        continue; // Skip ally cell!
                    }
                }

                validGridPositionList.Add(testGridPosition);
            }

            return validGridPositionList;
        }
    }
}