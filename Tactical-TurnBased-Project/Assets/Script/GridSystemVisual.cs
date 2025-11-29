using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class GridSystemVisual : MonoBehaviour
    {
        public static GridSystemVisual Instance { get; private set; }
        [SerializeField] private Transform gridSystemVisualSinglePrefab;
        private GridSystemVisualSingle [,] gridSystemVisualSinglesArray;
        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            gridSystemVisualSinglesArray = new GridSystemVisualSingle[
                LevelGrid.Instance.GetWidth(),
                LevelGrid.Instance.GetHeight()
            ];
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    GridPosition gridPosition = new GridPosition(x,z);
                    Transform gridSystemVisualTransform = 
                        Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
                    gridSystemVisualSinglesArray[x,z] = gridSystemVisualTransform.GetComponent<GridSystemVisualSingle>();
                }
            }
        }
        private void Update()
        {
            UpdateGridVisual();
        }
        public void HideAllGridPosition()
        {
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    gridSystemVisualSinglesArray[x,z].Hide();
                }
            }
        }
        public void ShowGridPositionList(List<GridPosition> gridPositionsList)
        {
            foreach (GridPosition gridPosition in gridPositionsList)
            {
                gridSystemVisualSinglesArray[gridPosition.x,gridPosition.z].Show();
            }
        }
        private void UpdateGridVisual()
        {
            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            HideAllGridPosition();
            ShowGridPositionList(
                selectedUnit.GetMoveAction().GetValidActionGridPositionList());
        }
    }
}