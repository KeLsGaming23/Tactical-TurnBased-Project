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
            HandleCameraFollow();
            HandleCameraRotation();
            HandleCameraZoom();
        }

        private void HandleCameraFollow()
        {
            Vector3 targetPosition = transform.position;
            Unit activeTurnUnit = TurnSystem.Instance != null ? TurnSystem.Instance.GetCurrentTurnUnit() : null;

            // During Enemy Turn: dynamically follow the moving enemy's real-time world transform position
            if (activeTurnUnit != null && activeTurnUnit.IsEnemy())
            {
                targetPosition = activeTurnUnit.transform.position;

                // Sync GridCursor with enemy's current tile
                if (GridCursor.Instance != null && LevelGrid.Instance != null)
                {
                    GridCursor.Instance.SetSelectedGridPosition(activeTurnUnit.GetGridPosition());
                }
            }
            else
            {
                // During Player Turn: follow the GridCursor (WASD tile selection) as usual
                if (GridCursor.Instance != null && LevelGrid.Instance != null)
                {
                    targetPosition = LevelGrid.Instance.GetWorldPosition(GridCursor.Instance.GetSelectedGridPosition());
                }
                else if (activeTurnUnit != null)
                {
                    targetPosition = activeTurnUnit.transform.position;
                }
            }

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        }

        private void HandleCameraRotation()
        {
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
        }

        private void HandleCameraZoom()
        {
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