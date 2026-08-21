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
        }

        private void Update()
        {
            if (isBusy) return;

            // Handle Menu Selection State
            if (currentFlowState == ActionFlowState.ActionMenuSelection)
            {
                HandleMenuSelectionInput();
                return;
            }

            // Cancel / Deselect with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSelection();
                return;
            }

            // Mouse click support
            if (Input.GetMouseButtonDown(0))
            {
                if (TryHandleUnitSelection()) return;

                if (currentFlowState == ActionFlowState.TargetGridSelection)
                {
                    HandleMouseTargetAction();
                }
            }
        }

        private void HandleMenuSelectionInput()
        {
            if (selectedUnit == null)
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
                return;
            }

            // Escape: Cancel menu and deselect
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSelection();
            }
        }

        public void ConfirmMenuSelection()
        {
            if (selectedUnit == null) return;
            BaseAction[] actions = selectedUnit.GetBaseActionArray();
            if (actions == null || selectedMenuActionIndex < 0 || selectedMenuActionIndex >= actions.Length) return;

            BaseAction chosenAction = actions[selectedMenuActionIndex];
            ExecuteActionChoice(chosenAction);
        }

        public void ExecuteActionChoice(BaseAction chosenAction)
        {
            if (selectedUnit == null || chosenAction == null) return;

            if (chosenAction is MoveAction moveAction)
            {
                SetSelectedAction(moveAction);
                SetFlowState(ActionFlowState.TargetGridSelection);
                Debug.Log($"[Action Menu] Selected MOVE. WASD re-enabled to choose destination tile. Press Enter to move.");
            }
            else if (chosenAction is SpinAction spinAction)
            {
                SetSelectedAction(spinAction);
                SetFlowState(ActionFlowState.ActionExecuting);
                SetBusy();
                spinAction.TakeAction(selectedUnit.GetGridPosition(), () =>
                {
                    ClearBusy();
                    SetFlowState(ActionFlowState.GridNavigation);
                    SetSelectedUnit(null);
                });
                Debug.Log($"[Action Menu] Selected SPIN. Unit spinning.");
            }
            else
            {
                SetSelectedAction(chosenAction);
                SetFlowState(ActionFlowState.TargetGridSelection);
            }
        }

        private void HandleMouseTargetAction()
        {
            if (selectedAction == null) return;

            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
            if (selectedAction.IsValidActionGridPosition(mouseGridPosition))
            {
                SetFlowState(ActionFlowState.ActionExecuting);
                SetBusy();
                selectedAction.TakeAction(mouseGridPosition, () =>
                {
                    ClearBusy();
                    SetFlowState(ActionFlowState.GridNavigation);
                    SetSelectedUnit(null);
                });
            }
        }

        private bool TryHandleUnitSelection()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    SelectUnit(unit);
                    return true;
                }
            }
            return false;
        }

        public void SelectUnit(Unit unit)
        {
            selectedUnit = unit;
            selectedMenuActionIndex = 0;
            if (unit != null)
            {
                SetFlowState(ActionFlowState.ActionMenuSelection);
                if (GridCursor.Instance != null)
                {
                    GridCursor.Instance.SetSelectedGridPosition(unit.GetGridPosition());
                }
            }
            else
            {
                SetFlowState(ActionFlowState.GridNavigation);
                SetSelectedAction(null);
            }
            OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetSelectedUnit(Unit unit) => SelectUnit(unit);

        public void CancelSelection()
        {
            selectedUnit = null;
            selectedAction = null;
            selectedMenuActionIndex = 0;
            SetFlowState(ActionFlowState.GridNavigation);
            OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
            Debug.Log($"[UnitActionSystem] Selection cancelled.");
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