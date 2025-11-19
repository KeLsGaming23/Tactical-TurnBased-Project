using UnityEngine;

namespace kelsgaming.site
{
    public class MouseWorld : MonoBehaviour
    {
        private void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.Log(Physics.Raycast(ray));
        }
    }
}