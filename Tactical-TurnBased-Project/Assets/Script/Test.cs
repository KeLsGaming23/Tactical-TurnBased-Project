using UnityEngine;

namespace kelsgaming.site
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private Unit unit;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                unit.GetMoveAction().GetValidActionGridPositionList();
            }

        }
    }
}