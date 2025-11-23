using System;
using UnityEngine;

namespace kelsgaming.site
{
    public class GridSystem
    {
        private int height;
        private int width;
        private float cellSize;
        public GridSystem(int width, int height, float cellSize)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z) + Vector3.right * .2f, Color.white, 1000);
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
    }
}