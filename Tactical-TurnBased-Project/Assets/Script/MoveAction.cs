using System;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class MoveAction : BaseAction
    {
        private const int PATHFINDING_DISTANCE_MULTIPLIER = 10;

        [SerializeField] private Animator unitAnimator;
        [SerializeField] private int maxMoveDistance = 4;

        private List<Vector3> positionList;
        private int currentPositionIndex;

        protected override void Awake()
        {
            base.Awake();
            if (unitAnimator == null)
            {
                unitAnimator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            if (!isActive || positionList == null || positionList.Count == 0)
            {
                return;
            }

            Vector3 targetPosition = positionList[currentPositionIndex];
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            float stoppingDistance = .1f;

            if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
            {
                float moveSpeed = 4f;
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
                if (unitAnimator != null) unitAnimator.SetBool("IsWalking", true);
            }
            else
            {
                currentPositionIndex++;
                if (currentPositionIndex >= positionList.Count)
                {
                    if (unitAnimator != null) unitAnimator.SetBool("IsWalking", false);
                    isActive = false;
                    onActionComplete?.Invoke();
                }
            }

            if (moveDirection != Vector3.zero)
            {
                float rotateSpeed = 10f;
                transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
            }
        }

        public override string GetActionName() => "Move";

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
        {
            this.onActionComplete = onActionComplete;
            Move(gridPosition);
        }

        public void Move(GridPosition gridPosition)
        {
            List<GridPosition> pathGridPositionList = null;
            if (Pathfinding.Instance != null)
            {
                pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out int pathLength);
            }

            currentPositionIndex = 0;
            positionList = new List<Vector3>();

            if (pathGridPositionList != null && pathGridPositionList.Count > 0)
            {
                foreach (GridPosition pathGridPosition in pathGridPositionList)
                {
                    positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
                }
            }
            else
            {
                positionList.Add(LevelGrid.Instance.GetWorldPosition(gridPosition));
            }

            isActive = true;
        }

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            List<GridPosition> validGridPositionList = new List<GridPosition>();
            GridPosition unitGridPosition = unit.GetGridPosition();

            for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
            {
                for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z);
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                    if (unitGridPosition == testGridPosition) continue;
                    if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) continue;

                    // Obstacle and Pathfinding checks
                    if (Pathfinding.Instance != null)
                    {
                        if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition)) continue;
                        if (!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition)) continue;

                        int pathLength = Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition);
                        if (pathLength > maxMoveDistance * PATHFINDING_DISTANCE_MULTIPLIER) continue;
                    }

                    validGridPositionList.Add(testGridPosition);
                }
            }
            return validGridPositionList;
        }
    }
}