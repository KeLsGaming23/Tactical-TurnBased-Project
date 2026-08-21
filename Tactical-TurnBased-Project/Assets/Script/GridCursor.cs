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
            // Default to active unit position if available, or (0,0)
            Unit activeUnit = TurnSystem.Instance != null ? TurnSystem.Instance.GetCurrentTurnUnit() : null;
            if (activeUnit != null)
            {
                selectedGridPosition = activeUnit.GetGridPosition();
            }
            else
            {
                selectedGridPosition = new GridPosition(0, 0);
            }

            OnSelectedGridPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Update()
        {
            if (UnitActionSystem.Instance == null) return;

            // When in Action Menu Selection or Action Executing, WASD grid navigation is disabled
            UnitActionSystem.ActionFlowState flowState = UnitActionSystem.Instance.GetFlowState();
            if (flowState == UnitActionSystem.ActionFlowState.ActionMenuSelection ||
                flowState == UnitActionSystem.ActionFlowState.ActionExecuting ||
                UnitActionSystem.Instance.IsBusy())
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
            if (LevelGrid.Instance == null || UnitActionSystem.Instance == null) return;

            UnitActionSystem.ActionFlowState flowState = UnitActionSystem.Instance.GetFlowState();
            Unit activeUnit = TurnSystem.Instance != null ? TurnSystem.Instance.GetCurrentTurnUnit() : UnitActionSystem.Instance.GetSelectedUnit();

            // 1. In Grid Navigation Mode:
            if (flowState == UnitActionSystem.ActionFlowState.GridNavigation)
            {
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(gridPosition))
                {
                    List<Unit> unitList = LevelGrid.Instance.GetUnitListAtGridPosition(gridPosition);
                    if (unitList != null && unitList.Count > 0)
                    {
                        Unit clickedUnit = unitList[0];
                        if (clickedUnit == activeUnit)
                        {
                            // Active turn unit clicked -> Open action menu
                            UnitActionSystem.Instance.OpenActionMenu();
                        }
                        else
                        {
                            // Other unit clicked -> Lock interaction to active unit only!
                            string activeName = activeUnit != null ? activeUnit.name : "None";
                            int activeSpeed = activeUnit != null ? activeUnit.GetSpeed() : 0;
                            Debug.Log($"[Turn System] Cannot select '{clickedUnit.name}' (Speed: {clickedUnit.GetSpeed()}). It is currently {activeName}'s turn (Speed: {activeSpeed})!");
                        }
                    }
                }
                else
                {
                    // Empty cell pressed while exploring in Grid Navigation -> Just log cell info
                    Debug.Log($"[GridCursor] Cell ({gridPosition.x}, {gridPosition.z}): Empty.");
                }
                return;
            }

            // 2. In Target Grid Selection Mode (e.g. Move destination):
            if (flowState == UnitActionSystem.ActionFlowState.TargetGridSelection)
            {
                UnitActionSystem.Instance.ExecuteGridTargetAction(gridPosition);
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
