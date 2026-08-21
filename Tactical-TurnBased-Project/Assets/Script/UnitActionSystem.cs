using System;
using UnityEngine;

namespace kelsgaming.site
{
    public class UnitActionSystem : MonoBehaviour
    {
        public enum ActionFlowState
        {
            GridNavigation,
            ActionMenuSelection,
            TargetGridSelection,
            ActionExecuting,
        }

        public static UnitActionSystem Instance { get; private set; }

        public event EventHandler OnSelectedUnitChanged;
        public event EventHandler OnSelectedActionChanged;
        public event EventHandler<ActionFlowState> OnActionFlowStateChanged;
        public event EventHandler<bool> OnBusyChanged;

        [SerializeField] private Unit selectedUnit;
        [SerializeField] private LayerMask unitLayerMask;

        private BaseAction selectedAction;
        private ActionFlowState currentFlowState = ActionFlowState.GridNavigation;
        private int selectedMenuActionIndex = 0;
        private bool isBusy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _ = UnitActionSystemUI.Instance;
            _ = TurnSystem.Instance;
            _ = EnemyAI.Instance;
        }

        private void Update()
        {
            if (isBusy) return;

            // Handle Menu Selection State (for player units only)
            if (currentFlowState == ActionFlowState.ActionMenuSelection)
            {
                HandleMenuSelectionInput();
                return;
            }

            // Cancel / Return to Grid Navigation with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentFlowState == ActionFlowState.TargetGridSelection)
                {
                    CancelTargetSelection();
                }
                else if (currentFlowState == ActionFlowState.ActionMenuSelection)
                {
                    CloseActionMenu();
                }
                return;
            }

            // Quick shortcut: Press Space in Grid Navigation to refocus active player unit and open menu
            if (currentFlowState == ActionFlowState.GridNavigation && selectedUnit != null && !selectedUnit.IsEnemy())
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (GridCursor.Instance != null)
                    {
                        GridCursor.Instance.SetSelectedGridPosition(selectedUnit.GetGridPosition());
                    }
                    OpenActionMenu();
                }
            }
        }

        private void HandleMenuSelectionInput()
        {
            if (selectedUnit == null || selectedUnit.IsEnemy())
            {
                SetFlowState(ActionFlowState.GridNavigation);
                return;
            }

            BaseAction[] actions = selectedUnit.GetBaseActionArray();
            if (actions == null || actions.Length == 0) return;

            // W / Up Arrow: Move selection UP
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedMenuActionIndex = (selectedMenuActionIndex - 1 + actions.Length) % actions.Length;
            }

            // S / Down Arrow: Move selection DOWN
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedMenuActionIndex = (selectedMenuActionIndex + 1) % actions.Length;
            }

            // Number shortcut keys (1 for Move, 2 for Spin)
            if (Input.GetKeyDown(KeyCode.Alpha1) && actions.Length > 0)
            {
                selectedMenuActionIndex = 0;
                ConfirmMenuSelection();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && actions.Length > 1)
            {
                selectedMenuActionIndex = 1;
                ConfirmMenuSelection();
                return;
            }

            // Enter: Confirm current choice
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ConfirmMenuSelection();
            }

            // Escape: Close action menu and return to grid navigation
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseActionMenu();
            }
        }

        public void OpenActionMenu()
        {
            if (selectedUnit == null || selectedUnit.IsEnemy() || isBusy) return;
            selectedMenuActionIndex = 0;
            SetFlowState(ActionFlowState.ActionMenuSelection);
            Debug.Log($"[UnitActionSystem] Action Menu opened for {selectedUnit.name} (AP: {selectedUnit.GetActionPoints()}/{selectedUnit.GetMaxActionPoints()}).");
        }

        public void CloseActionMenu()
        {
            selectedAction = null;
            SetFlowState(ActionFlowState.GridNavigation);
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
            Debug.Log($"[UnitActionSystem] Action Menu closed. Exploring grid with WASD.");
        }

        public void CancelTargetSelection()
        {
            selectedAction = null;
            SetFlowState(ActionFlowState.GridNavigation);
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
            Debug.Log($"[UnitActionSystem] Target selection cancelled. Returned to Grid Navigation.");
        }

        public void ConfirmMenuSelection()
        {
            if (selectedUnit == null || selectedUnit.IsEnemy()) return;
            BaseAction[] actions = selectedUnit.GetBaseActionArray();
            if (actions == null || selectedMenuActionIndex < 0 || selectedMenuActionIndex >= actions.Length) return;

            BaseAction chosenAction = actions[selectedMenuActionIndex];
            ExecuteActionChoice(chosenAction);
        }

        public void ExecuteActionChoice(BaseAction chosenAction)
        {
            if (selectedUnit == null || chosenAction == null) return;

            // Check if unit has enough Action Points
            if (!selectedUnit.CanSpendActionPointsToTakeAction(chosenAction))
            {
                Debug.Log($"[UnitActionSystem] Not enough Action Points to execute {chosenAction.GetActionName()} (Costs {chosenAction.GetActionPointsCost()}, Available: {selectedUnit.GetActionPoints()}).");
                return;
            }

            if (chosenAction is MoveAction moveAction)
            {
                SetSelectedAction(moveAction);
                SetFlowState(ActionFlowState.TargetGridSelection);
                Debug.Log($"[Action Menu] Selected MOVE. WASD re-enabled to choose destination tile. Press Enter to move.");
            }
            else if (chosenAction is SpinAction spinAction)
            {
                SetSelectedAction(spinAction);
                SetFlowState(ActionFlowState.TargetGridSelection);
                Debug.Log($"[Action Menu] Selected SPIN ATTACK. WASD to select cardinal direction (Up/Down/Left/Right), Press Enter to strike.");
            }
            else
            {
                SetSelectedAction(chosenAction);
                SetFlowState(ActionFlowState.TargetGridSelection);
            }
        }

        public void ExecuteGridTargetAction(GridPosition targetGridPosition)
        {
            if (selectedUnit == null || selectedAction == null) return;

            if (!selectedUnit.CanSpendActionPointsToTakeAction(selectedAction))
            {
                Debug.Log($"[UnitActionSystem] Not enough Action Points for {selectedAction.GetActionName()}.");
                return;
            }

            if (selectedAction.IsValidActionGridPosition(targetGridPosition))
            {
                SetFlowState(ActionFlowState.ActionExecuting);
                SetBusy();

                selectedUnit.TrySpendActionPointsToTakeAction(selectedAction);

                selectedAction.TakeAction(targetGridPosition, () =>
                {
                    ClearBusy();
                    OnActionCompleted();
                });
            }
            else
            {
                Debug.Log($"[UnitActionSystem] Cell {targetGridPosition} is out of range for {selectedAction.GetActionName()}.");
            }
        }

        private void OnActionCompleted()
        {
            selectedAction = null;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);

            if (selectedUnit != null && selectedUnit.GetActionPoints() <= 0)
            {
                Debug.Log($"[UnitActionSystem] {selectedUnit.name} has used all Action Points. Ending turn automatically...");
                TurnSystem.Instance.EndCurrentUnitTurn();
            }
            else
            {
                SetFlowState(ActionFlowState.GridNavigation);
                Debug.Log($"[UnitActionSystem] Action completed. {selectedUnit.name} has {selectedUnit.GetActionPoints()} AP remaining.");
            }
        }

        public void SetActiveTurnUnit(Unit unit)
        {
            selectedUnit = unit;
            selectedAction = null;
            selectedMenuActionIndex = 0;
            SetFlowState(ActionFlowState.GridNavigation);

            if (unit != null && GridCursor.Instance != null)
            {
                GridCursor.Instance.SetSelectedGridPosition(unit.GetGridPosition());
            }

            OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetSelectedUnit(Unit unit)
        {
            Unit activeTurnUnit = TurnSystem.Instance != null ? TurnSystem.Instance.GetCurrentTurnUnit() : null;
            if (activeTurnUnit != null && unit != null && unit != activeTurnUnit)
            {
                Debug.Log($"[UnitActionSystem] Cannot select {unit.name}. It is {activeTurnUnit.name}'s turn!");
                return;
            }

            SetActiveTurnUnit(unit);
        }

        public void SetSelectedAction(BaseAction baseAction)
        {
            selectedAction = baseAction;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetFlowState(ActionFlowState newState)
        {
            currentFlowState = newState;
            OnActionFlowStateChanged?.Invoke(this, currentFlowState);
        }

        public ActionFlowState GetFlowState() => currentFlowState;
        public int GetSelectedMenuActionIndex() => selectedMenuActionIndex;
        public void SetSelectedMenuActionIndex(int index) => selectedMenuActionIndex = index;
        public Unit GetSelectedUnit() => selectedUnit;
        public BaseAction GetSelectedAction() => selectedAction;
        public bool IsBusy() => isBusy;

        public void SetBusy()
        {
            isBusy = true;
            OnBusyChanged?.Invoke(this, true);
        }

        public void ClearBusy()
        {
            isBusy = false;
            OnBusyChanged?.Invoke(this, false);
        }
    }
}