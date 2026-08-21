using System;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class GridCursor : MonoBehaviour
    {
        private static GridCursor instance;
        public static GridCursor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GridCursor>();
                    if (instance == null)
                    {
                        GameObject gridCursorGameObject = new GameObject("GridCursor");
                        instance = gridCursorGameObject.AddComponent<GridCursor>();
                    }
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        public event EventHandler OnSelectedGridPositionChanged;

        private GridPosition selectedGridPosition;

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
            // Default to selected unit position if available, or (0,0)
            if (UnitActionSystem.Instance != null && UnitActionSystem.Instance.GetSelectedUnit() != null)
            {
                selectedGridPosition = UnitActionSystem.Instance.GetSelectedUnit().GetGridPosition();
            }
            else
            {
                selectedGridPosition = new GridPosition(0, 0);
            }

            OnSelectedGridPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Update()
        {
            if (UnitActionSystem.Instance != null && UnitActionSystem.Instance.IsBusy())
            {
                return;
            }

            HandleMovementInput();
            HandleInteractionInput();
        }

        private void HandleMovementInput()
        {
            int moveX = 0;
            int moveZ = 0;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                moveZ = +1;
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                moveZ = -1;
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                moveX = -1;
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                moveX = +1;
            }

            if (moveX != 0 || moveZ != 0)
            {
                GridPosition testGridPosition = new GridPosition(selectedGridPosition.x + moveX, selectedGridPosition.z + moveZ);

                if (LevelGrid.Instance != null && LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    selectedGridPosition = testGridPosition;
                    OnSelectedGridPositionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                HandleCellAction(selectedGridPosition);
            }
        }

        public void HandleCellAction(GridPosition gridPosition)
        {
            if (LevelGrid.Instance == null) return;

            // 1. If the cell has a unit on it, select that unit
            if (LevelGrid.Instance.HasAnyUnitOnGridPosition(gridPosition))
            {
                List<Unit> unitList = LevelGrid.Instance.GetUnitListAtGridPosition(gridPosition);
                if (unitList != null && unitList.Count > 0)
                {
                    Unit unitToSelect = unitList[0];
                    if (UnitActionSystem.Instance != null)
                    {
                        UnitActionSystem.Instance.SetSelectedUnit(unitToSelect);
                    }
                    Debug.Log($"[GridCursor] Selected unit '{unitToSelect.name}' at {gridPosition}.");
                }
                return;
            }

            // 2. If the cell is empty and we currently have a selected unit and action
            Unit selectedUnit = UnitActionSystem.Instance != null ? UnitActionSystem.Instance.GetSelectedUnit() : null;
            BaseAction selectedAction = UnitActionSystem.Instance != null ? UnitActionSystem.Instance.GetSelectedAction() : null;

            if (selectedUnit != null && selectedAction != null)
            {
                if (selectedAction.IsValidActionGridPosition(gridPosition))
                {
                    Debug.Log($"[GridCursor] Executing {selectedAction.GetActionName()} on '{selectedUnit.name}' to {gridPosition}.");
                    UnitActionSystem.Instance.SetBusy();
                    selectedAction.TakeAction(gridPosition, () =>
                    {
                        if (UnitActionSystem.Instance != null)
                        {
                            UnitActionSystem.Instance.ClearBusy();
                        }
                    });
                }
                else
                {
                    Debug.Log($"[GridCursor] Cell {gridPosition} is out of range for {selectedAction.GetActionName()}.");
                }
            }
            else
            {
                // Cell is empty and no unit is selected -> do nothing
                Debug.Log($"[GridCursor] Cell ({gridPosition.x}, {gridPosition.z}): Empty.");
            }
        }

        public GridPosition GetSelectedGridPosition()
        {
            return selectedGridPosition;
        }

        public void SetSelectedGridPosition(GridPosition newGridPosition)
        {
            if (LevelGrid.Instance != null && LevelGrid.Instance.IsValidGridPosition(newGridPosition))
            {
                selectedGridPosition = newGridPosition;
                OnSelectedGridPositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
