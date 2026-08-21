using System;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class Pathfinding : MonoBehaviour
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        private static Pathfinding instance;
        public static Pathfinding Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<Pathfinding>();
                    if (instance == null)
                    {
                        GameObject pathfindingGameObject = new GameObject("Pathfinding");
                        instance = pathfindingGameObject.AddComponent<Pathfinding>();
                    }
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        [SerializeField] private LayerMask obstaclesLayerMask;

        private int width;
        private int height;
        private float cellSize;
        private PathNode[,] gridNodes;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (obstaclesLayerMask.value == 0)
            {
                obstaclesLayerMask = LayerMask.GetMask("Obstacles");
            }
        }

        private void Start()
        {
            Setup();
        }

        public void Setup()
        {
            if (LevelGrid.Instance == null) return;

            width = LevelGrid.Instance.GetWidth();
            height = LevelGrid.Instance.GetHeight();
            cellSize = 2f;

            gridNodes = new PathNode[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    gridNodes[x, z] = new PathNode(gridPosition);
                }
            }

            ScanObstacles();
        }

        public void ScanObstacles()
        {
            if (gridNodes == null) return;

            float raycastOffsetDistance = 5f;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);

                    // Check for obstacle collision in this cell
                    bool hasObstacle = Physics.Raycast(
                        worldPosition + Vector3.down * raycastOffsetDistance,
                        Vector3.up,
                        raycastOffsetDistance * 2f,
                        obstaclesLayerMask
                    );

                    if (!hasObstacle)
                    {
                        // Also check with small bounding box
                        hasObstacle = Physics.CheckBox(
                            worldPosition + Vector3.up * 1f,
                            new Vector3(0.7f, 0.8f, 0.7f),
                            Quaternion.identity,
                            obstaclesLayerMask
                        );
                    }

                    if (hasObstacle)
                    {
                        GetNode(x, z).SetIsWalkable(false);
                    }
                }
            }
        }

        public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
        {
            pathLength = 0;

            if (!IsValidGridPosition(startGridPosition) || !IsValidGridPosition(endGridPosition))
            {
                return null;
            }

            PathNode startNode = GetNode(startGridPosition.x, startGridPosition.z);
            PathNode endNode = GetNode(endGridPosition.x, endGridPosition.z);

            if (!endNode.IsWalkable())
            {
                return null;
            }

            List<PathNode> openList = new List<PathNode> { startNode };
            List<PathNode> closedList = new List<PathNode>();

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    PathNode pathNode = GetNode(x, z);
                    pathNode.SetGCost(int.MaxValue);
                    pathNode.CalculateFCost();
                    pathNode.ResetCameFromPathNode();
                }
            }

            startNode.SetGCost(0);
            startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
            startNode.CalculateFCost();

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);

                if (currentNode == endNode)
                {
                    // Reached destination!
                    pathLength = endNode.GetFCost();
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
                {
                    if (closedList.Contains(neighbourNode)) continue;
                    if (!neighbourNode.IsWalkable())
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighbourNode.GetGridPosition());

                    if (tentativeGCost < neighbourNode.GetGCost())
                    {
                        neighbourNode.SetCameFromPathNode(currentNode);
                        neighbourNode.SetGCost(tentativeGCost);
                        neighbourNode.SetHCost(CalculateDistance(neighbourNode.GetGridPosition(), endGridPosition));
                        neighbourNode.CalculateFCost();

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

            // No path found
            return null;
        }

        public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
        {
            return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
        }

        public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
        {
            FindPath(startGridPosition, endGridPosition, out int pathLength);
            return pathLength;
        }

        public int CalculateDistance(GridPosition gridPositionA, GridPosition gridPositionB)
        {
            GridPosition gridPositionDistance = gridPositionA - gridPositionB;
            int xDistance = Mathf.Abs(gridPositionDistance.x);
            int zDistance = Mathf.Abs(gridPositionDistance.z);
            int remaining = Mathf.Abs(xDistance - zDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++)
            {
                if (pathNodeList[i].GetFCost() < lowestFCostNode.GetFCost())
                {
                    lowestFCostNode = pathNodeList[i];
                }
            }
            return lowestFCostNode;
        }

        private List<GridPosition> CalculatePath(PathNode endNode)
        {
            List<PathNode> pathNodeList = new List<PathNode> { endNode };
            PathNode currentNode = endNode;

            while (currentNode.GetCameFromPathNode() != null)
            {
                pathNodeList.Add(currentNode.GetCameFromPathNode());
                currentNode = currentNode.GetCameFromPathNode();
            }

            pathNodeList.Reverse();

            List<GridPosition> gridPositionList = new List<GridPosition>();
            foreach (PathNode pathNode in pathNodeList)
            {
                gridPositionList.Add(pathNode.GetGridPosition());
            }

            return gridPositionList;
        }

        private List<PathNode> GetNeighbourList(PathNode currentNode)
        {
            List<PathNode> neighbourList = new List<PathNode>();
            GridPosition gridPosition = currentNode.GetGridPosition();

            // Cardinal neighbors
            if (gridPosition.x - 1 >= 0) neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z));
            if (gridPosition.x + 1 < width) neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z));
            if (gridPosition.z - 1 >= 0) neighbourList.Add(GetNode(gridPosition.x, gridPosition.z - 1));
            if (gridPosition.z + 1 < height) neighbourList.Add(GetNode(gridPosition.x, gridPosition.z + 1));

            // Diagonal neighbors (only if corner isn't completely blocked)
            if (gridPosition.x - 1 >= 0)
            {
                if (gridPosition.z - 1 >= 0) neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1));
                if (gridPosition.z + 1 < height) neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1));
            }
            if (gridPosition.x + 1 < width)
            {
                if (gridPosition.z - 1 >= 0) neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1));
                if (gridPosition.z + 1 < height) neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1));
            }

            return neighbourList;
        }

        public PathNode GetNode(int x, int z)
        {
            if (gridNodes == null || x < 0 || z < 0 || x >= width || z >= height) return null;
            return gridNodes[x, z];
        }

        public bool IsValidGridPosition(GridPosition gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.z >= 0 && gridPosition.x < width && gridPosition.z < height;
        }

        public bool IsWalkableGridPosition(GridPosition gridPosition)
        {
            if (!IsValidGridPosition(gridPosition)) return false;
            PathNode node = GetNode(gridPosition.x, gridPosition.z);
            return node != null && node.IsWalkable();
        }

        public void SetIsWalkableGridPosition(GridPosition gridPosition, bool isWalkable)
        {
            if (IsValidGridPosition(gridPosition))
            {
                PathNode node = GetNode(gridPosition.x, gridPosition.z);
                if (node != null)
                {
                    node.SetIsWalkable(isWalkable);
                }
            }
        }
    }
}
