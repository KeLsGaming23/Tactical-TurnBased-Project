using UnityEngine;

namespace kelsgaming.site
{
    public class TestRandomUnitToInstantiateOnGridCell : MonoBehaviour
    {
        [SerializeField] private Transform unitPrefab;
        [SerializeField] private int numberOfUnits = 5;

        private LevelGrid levelGrid;

        private void Start()
        {
            levelGrid = LevelGrid.Instance;

            if (levelGrid == null)
            {
                Debug.LogError("LevelGrid Instance is NULL! Add LevelGrid to scene.");
                return;
            }

            SpawnUnitsOnFreeCells(numberOfUnits);
        }

        private void SpawnUnitsOnFreeCells(int count)
        {
            int gridWidth = 4;  
            int gridHeight = 4;

            for (int i = 0; i < count; i++)
            {
                GridPosition spawnPos;

                // Try to find an empty grid cell
                do
                {
                    int randomX = Random.Range(0, gridWidth);
                    int randomZ = Random.Range(0, gridHeight);

                    spawnPos = new GridPosition(randomX, randomZ);

                } while (levelGrid.GetUnitListAtGridPosition(spawnPos).Count > 0);
                // Keep searching until cell is empty

                // Convert to world
                Vector3 worldPos = levelGrid.GetWorldPosition(spawnPos);

                // Instantiate unit
                Transform unitTransform = Instantiate(unitPrefab, worldPos, Quaternion.identity);

                Unit unit = unitTransform.GetComponent<Unit>();

                // Register in grid system
                levelGrid.AddUnitAtGridPosition(spawnPos, unit);

                Debug.Log($"Spawned Unit at EMPTY cell {spawnPos}");
            }
        }
    }
}