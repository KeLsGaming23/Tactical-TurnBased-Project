using System;
using UnityEngine;

namespace kelsgaming.site
{
    public class UnitActionSystem : MonoBehaviour
    {
        public static UnitActionSystem Instance { get; private set; }

        public event EventHandler OnSelectedUnitChanged;
        public event EventHandler OnSelectedActionChanged;
        public event EventHandler<bool> OnBusyChanged;

        [SerializeField] private Unit selectedUnit;
        [SerializeField] private LayerMask unitLayerMask;

        private BaseAction selectedAction;
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

            if (selectedUnit != null)
            {
                SetSelectedUnit(selectedUnit);
            }
        }

        private void Update()
        {
            if (isBusy) return;

            // Optional mouse click selection/action
            if (Input.GetMouseButtonDown(0))
            {
                if (TryHandleUnitSelection()) return;
                HandleSelectedAction();
            }

            // Keyboard shortcut 1 (Move) / 2 (Spin)
            if (selectedUnit != null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (selectedUnit.GetMoveAction() != null)
                    {
                        SetSelectedAction(selectedUnit.GetMoveAction());
                    }
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (selectedUnit.GetSpinAction() != null)
                    {
                        SetSelectedAction(selectedUnit.GetSpinAction());
                        SetBusy();
                        selectedUnit.GetSpinAction().TakeAction(selectedUnit.GetGridPosition(), ClearBusy);
                    }
                }
            }
        }

        private void HandleSelectedAction()
        {
            if (selectedAction == null) return;

            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

            if (selectedAction.IsValidActionGridPosition(mouseGridPosition))
            {
                SetBusy();
                selectedAction.TakeAction(mouseGridPosition, ClearBusy);
            }
        }

        private bool TryHandleUnitSelection()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    if (unit == selectedUnit) return false;
                    SetSelectedUnit(unit);
                    return true;
                }
            }
            return false;
        }

        public void SetSelectedUnit(Unit unit)
        {
            selectedUnit = unit;
            if (unit != null)
            {
                SetSelectedAction(unit.GetMoveAction());
                if (GridCursor.Instance != null)
                {
                    GridCursor.Instance.SetSelectedGridPosition(unit.GetGridPosition());
                }
            }
            else
            {
                SetSelectedAction(null);
            }
            OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetSelectedAction(BaseAction baseAction)
        {
            selectedAction = baseAction;
            OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        }

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