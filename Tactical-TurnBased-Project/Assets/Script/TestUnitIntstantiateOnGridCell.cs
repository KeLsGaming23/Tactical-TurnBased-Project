using UnityEngine;

namespace kelsgaming.site
{
    public class TestUnitIntstantiateOnGridCell : MonoBehaviour
    {
        [SerializeField] private Transform unitPrefab;
        private LevelGrid levelGrid;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            levelGrid = LevelGrid.Instance;
            // Example: put a Unit at grid 3,3
            GridPosition spawnPos = new GridPosition(3, 3);

            // Convert grid position → world position
            Vector3 worldPos = levelGrid.GetWorldPosition(spawnPos);

            // Instantiate the Unit at the position
            Transform unitTransform = Instantiate(unitPrefab, worldPos, Quaternion.identity);

            // Get the Unit component
            Unit unit = unitTransform.GetComponent<Unit>();

            // Register the unit inside the grid
            levelGrid.AddUnitAtGridPosition(spawnPos, unit);
        }
    }
}