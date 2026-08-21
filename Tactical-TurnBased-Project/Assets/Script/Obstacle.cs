using UnityEngine;

namespace kelsgaming.site
{
    public class Obstacle : MonoBehaviour
    {
        private void Start()
        {
            if (LevelGrid.Instance != null && Pathfinding.Instance != null)
            {
                GridPosition gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
                Pathfinding.Instance.SetIsWalkableGridPosition(gridPosition, false);
            }
        }
    }
}
