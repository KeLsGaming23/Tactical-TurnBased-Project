using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class LevelGrid : MonoBehaviour
    {
        public static LevelGrid Instance {get; private set;}
        [SerializeField] private Transform gridDebugObjectPrefab;
        private GridSystem gridSystem;
        private void Awake()
        {
            Instance = this;
            gridSystem = new GridSystem(10, 10, 2f); 
            gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
        }
        public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
        {
            Debug.Log($"[Add] Adding unit to cell {gridPosition}");
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            gridObject.AddUnit(unit);
            Debug.Log($"[Add] Now cell {gridPosition} has {gridObject.GetUnit().Count} units.");
        }
        public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            Debug.Log($"[Check] Cell {gridPosition} contains {gridObject.GetUnit().Count} units.");
            return gridObject.GetUnit();
        }
        public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
        {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            gridObject.RemoveUnit(unit);
        }
        public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
        {
            RemoveUnitAtGridPosition(fromGridPosition, unit);
            AddUnitAtGridPosition(toGridPosition, unit);
        }
        public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);
        public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);
    }
}