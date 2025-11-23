using UnityEngine;

namespace kelsgaming.site
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private Transform gridDebugObjectPrefab;
        private GridSystem gridSystem;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
           gridSystem = new GridSystem(10, 10, 2f); 
           gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
           Debug.Log(new GridPosition(5, 7));
        }

        private void Update()
        {
            Debug.Log(gridSystem.GridPosition(MouseWorld.GetPosition()));
        }
    }
}