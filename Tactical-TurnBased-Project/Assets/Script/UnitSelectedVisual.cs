using System;
using UnityEngine;

namespace kelsgaming.site
{
    public class UnitSelectedVisual : MonoBehaviour
    {
        [SerializeField] private Unit unit;
        private MeshRenderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (unit == null)
            {
                unit = GetComponentInParent<Unit>();
            }
        }

        private void Start()
        {
            if (UnitActionSystem.Instance != null)
            {
                UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
            }
            UpateVisual();
        }

        private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs empty)
        {
            UpateVisual();
        }

        private void UpateVisual()
        {
            if (meshRenderer == null) return;

            if (UnitActionSystem.Instance != null && UnitActionSystem.Instance.GetSelectedUnit() == unit)
            {
                meshRenderer.enabled = true;
            }
            else
            {
                meshRenderer.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (UnitActionSystem.Instance != null)
            {
                UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
            }
        }
    }
}