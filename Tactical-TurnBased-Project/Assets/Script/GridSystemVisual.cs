using System;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class GridSystemVisual : MonoBehaviour
    {
        public static GridSystemVisual Instance { get; private set; }

        [SerializeField] private Transform gridSystemVisualSinglePrefab;
        [SerializeField] private Material defaultGridMaterial;
        [SerializeField] private Material selectedGridMaterial;

        private GridSystemVisualSingle[,] gridSystemVisualSinglesArray;

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
                    GridPosition gridPosition = new GridPosition(x, z);
                    Transform gridSystemVisualTransform =
                        Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
                    gridSystemVisualSinglesArray[x, z] = gridSystemVisualTransform.GetComponent<GridSystemVisualSingle>();
                }
            }

            if (GridCursor.Instance != null)
            {
                GridCursor.Instance.OnSelectedGridPositionChanged += GridCursor_OnSelectedGridPositionChanged;
            }

            if (UnitActionSystem.Instance != null)
            {
                UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
            }

            UpdateGridVisual();
        }

        private void Update()
        {
            UpdateGridVisual();
        }

        private void GridCursor_OnSelectedGridPositionChanged(object sender, EventArgs e)
        {
            UpdateGridVisual();
        }

        private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs e)
        {
            UpdateGridVisual();
        }

        public void HideAllGridPosition()
        {
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    gridSystemVisualSinglesArray[x, z].Hide();
                }
            }
        }

        public void ShowGridPositionList(List<GridPosition> gridPositionsList, Material material = null)
        {
            foreach (GridPosition gridPosition in gridPositionsList)
            {
                if (LevelGrid.Instance.IsValidGridPosition(gridPosition))
                {
                    gridSystemVisualSinglesArray[gridPosition.x, gridPosition.z].Show(material);
                }
            }
        }

        public void UpdateGridVisual()
        {
            HideAllGridPosition();

            // 1. Show valid action positions for the selected unit if available
            if (UnitActionSystem.Instance != null && UnitActionSystem.Instance.GetSelectedUnit() != null)
            {
                Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
                if (selectedUnit.GetMoveAction() != null)
                {
                    ShowGridPositionList(
                        selectedUnit.GetMoveAction().GetValidActionGridPositionList(),
                        defaultGridMaterial
                    );
                }
            }

            // 2. Highlight and elevate currently selected cursor grid position
            if (GridCursor.Instance != null)
            {
                GridPosition selectedGridPosition = GridCursor.Instance.GetSelectedGridPosition();
                if (LevelGrid.Instance.IsValidGridPosition(selectedGridPosition))
                {
                    gridSystemVisualSinglesArray[selectedGridPosition.x, selectedGridPosition.z].ShowSelected(selectedGridMaterial);
                }
            }
        }
    }
}