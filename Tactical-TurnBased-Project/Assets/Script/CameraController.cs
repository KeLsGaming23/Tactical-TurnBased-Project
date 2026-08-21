using Unity.Cinemachine;
using UnityEngine;

namespace kelsgaming.site
{
    public class CameraController : MonoBehaviour
    {
        private const float MIN_FOLLOW_ZOOM = 5f;
        private const float MAX_FOLLOW_ZOOM = -3f;
        [SerializeField] private CinemachinePositionComposer virtualCamera;
        [SerializeField] private float followSpeed = 8f;

        private Vector3 targetFollowOffset;

        private void Start()
        {
            if (virtualCamera != null)
            {
                targetFollowOffset = virtualCamera.TargetOffset;
            }
        }

        private void Update()
        {
            // Follow the selected grid cell smoothly
            if (GridCursor.Instance != null && LevelGrid.Instance != null)
            {
                Vector3 targetPosition = LevelGrid.Instance.GetWorldPosition(GridCursor.Instance.GetSelectedGridPosition());
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            }

            // Camera Rotation Logic (Q / E)
            Vector3 rotationVector = Vector3.zero;
            if (Input.GetKey(KeyCode.Q))
            {
                rotationVector.y = +1f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                rotationVector.y = -1f;
            }
            float rotationSpeed = 100f;
            transform.eulerAngles += rotationVector * rotationSpeed * Time.deltaTime;

            // Camera Zoom Logic (Mouse Scroll Wheel)
            if (virtualCamera != null)
            {
                float zoomAmount = -1f;
                float zoomSpeed = 5f;
                if (Input.mouseScrollDelta.y > 0)
                {
                    targetFollowOffset.z -= zoomAmount;
                }
                if (Input.mouseScrollDelta.y < 0)
                {
                    targetFollowOffset.z += zoomAmount;
                }
                targetFollowOffset.z = Mathf.Clamp(targetFollowOffset.z, MAX_FOLLOW_ZOOM, MIN_FOLLOW_ZOOM);
                virtualCamera.TargetOffset = Vector3.Lerp(virtualCamera.TargetOffset, targetFollowOffset, Time.deltaTime * zoomSpeed);
            }
        }
    }
}