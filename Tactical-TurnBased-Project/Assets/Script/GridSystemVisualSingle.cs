using UnityEngine;

namespace kelsgaming.site
{
    public class GridSystemVisualSingle : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        private Vector3 baseLocalPosition;
        private Vector3 targetLocalPosition;
        private Material defaultMaterial;

        private void Awake()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }

            if (meshRenderer != null)
            {
                defaultMaterial = meshRenderer.sharedMaterial;
                baseLocalPosition = meshRenderer.transform.localPosition;
                targetLocalPosition = baseLocalPosition;
            }
        }

        private void Update()
        {
            if (meshRenderer != null)
            {
                meshRenderer.transform.localPosition = Vector3.Lerp(
                    meshRenderer.transform.localPosition,
                    targetLocalPosition,
                    Time.deltaTime * 12f
                );
            }
        }

        public void Show(Material material = null)
        {
            if (meshRenderer == null) return;
            meshRenderer.enabled = true;
            meshRenderer.material = material != null ? material : defaultMaterial;
            targetLocalPosition = baseLocalPosition;
        }

        public void ShowSelected(Material selectedMaterial = null, float elevationOffset = 0.25f)
        {
            if (meshRenderer == null) return;
            meshRenderer.enabled = true;
            if (selectedMaterial != null)
            {
                meshRenderer.material = selectedMaterial;
            }
            targetLocalPosition = baseLocalPosition + new Vector3(0, elevationOffset, 0);
        }

        public void Hide()
        {
            if (meshRenderer == null) return;
            meshRenderer.enabled = false;
            targetLocalPosition = baseLocalPosition;
            if (defaultMaterial != null)
            {
                meshRenderer.material = defaultMaterial;
            }
        }
    }
}