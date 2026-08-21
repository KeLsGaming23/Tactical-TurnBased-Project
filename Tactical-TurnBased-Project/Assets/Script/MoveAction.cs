using System;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class MoveAction : BaseAction
    {
        [SerializeField] private Animator unitAnimator;
        [SerializeField] private int maxMoveDistance = 4;
        private Vector3 targetPosition;

        protected override void Awake()
        {
            base.Awake();
            targetPosition = transform.position;
            if (unitAnimator == null)
            {
                unitAnimator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

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
                if (unitAnimator != null) unitAnimator.SetBool("IsWalking", false);
                isActive = false;
                onActionComplete?.Invoke();
            }

            float rotateSpeed = 10f;
            transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
        }

        public override string GetActionName() => "Move";

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
        {
            this.onActionComplete = onActionComplete;
            Move(gridPosition);
        }

        public void Move(GridPosition gridPosition)
        {
            this.targetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
            isActive = true;
        }

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            List<GridPosition> validGridPosition = new List<GridPosition>();
            GridPosition unitGridPosition = unit.GetGridPosition();
            for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
            {
                for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z);
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                    {
                        continue;
                    }
                    if (unitGridPosition == testGridPosition)
                    {
                        continue;
                    }
                    if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                    {
                        continue;
                    }
                    validGridPosition.Add(testGridPosition);
                }
            }
            return validGridPosition;
        }
    }
}