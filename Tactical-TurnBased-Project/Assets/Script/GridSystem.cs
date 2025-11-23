using System;
using UnityEngine;

namespace kelsgaming.site
{
    public class GridSystem
    {
        private int height;
        private int width;
        private float cellSize;
        private GridObject[,] gridObjectArray;
        public GridSystem(int width, int height, float cellSize)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            gridObjectArray = new GridObject[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    gridObjectArray[x, z] = new GridObject(this, gridPosition);
                }
            }
        }
        public Vector3 GetWorldPosition(int x, int z)
        {
            return new Vector3(x, 0, z) * cellSize;
        }
        public GridPosition GridPosition(Vector3 worlPosition)
        {
            return new GridPosition(
                Mathf.RoundToInt(worlPosition. x / cellSize),
                Mathf.RoundToInt(worlPosition. z / cellSize)
            );
        }
        public void CreateDebugObjects(Transform debugPrefab)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {

                    GameObject.Instantiate(debugPrefab, GetWorldPosition(x ,z), Quaternion.identity);
                }
            }
        }
    }
}