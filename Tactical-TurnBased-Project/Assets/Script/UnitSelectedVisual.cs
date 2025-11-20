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
        }
        private void Start()
        {
            UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
            UpateVisual();
        }

        private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs empty)
        {
            UpateVisual();
        }
        private void UpateVisual()
        {
            if(UnitActionSystem.Instance.GetSelectedUnit() == unit)
            {
                meshRenderer.enabled = true;
            }
            else
            {
                meshRenderer.enabled = false;
            }
        }
    }
}