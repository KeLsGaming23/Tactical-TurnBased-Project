using System;
using System.Collections.Generic;
using UnityEngine;

namespace kelsgaming.site
{
    public class SpinAction : BaseAction
    {
        private float totalSpinAmount;

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            float spinAddAmount = 360f * Time.deltaTime;
            transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
            totalSpinAmount += spinAddAmount;
            if (totalSpinAmount >= 360f)
            {
                isActive = false;
                onActionComplete?.Invoke();
            }
        }

        public override string GetActionName() => "Spin";

        public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
        {
            this.onActionComplete = onActionComplete;
            Spin();
        }

        public void Spin()
        {
            isActive = true;
            totalSpinAmount = 0f;
        }

        public override List<GridPosition> GetValidActionGridPositionList()
        {
            GridPosition unitGridPosition = unit.GetGridPosition();
            return new List<GridPosition> { unitGridPosition };
        }
    }
}