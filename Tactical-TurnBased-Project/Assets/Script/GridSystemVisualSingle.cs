using UnityEngine;

namespace kelsgaming.site
{
    public class GridSystemVisualSingle : MonoBehaviour
    {
        [SerializeField]private MeshRenderer meshRenderer;

        public void Show()
        {
            meshRenderer.enabled = true;
        }
        public void Hide()
        {
            meshRenderer.enabled = false;
        }
    }
}