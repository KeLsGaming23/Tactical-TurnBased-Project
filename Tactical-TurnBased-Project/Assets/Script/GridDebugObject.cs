using TMPro;
using UnityEngine;

namespace kelsgaming.site
{
    public class GridDebugObject : MonoBehaviour
    {
        private GridObject gridObject;

        public void SetGridObject(GridObject gridObject)
        {
            this.gridObject = gridObject;
        }
    }
}