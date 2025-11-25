using Unity.Cinemachine;
using UnityEngine;

namespace kelsgaming.site
{
    public class CameraController : MonoBehaviour
    {
        private const float MIN_FOLLOW_ZOOM = 5f;
        private const float MAX_FOLLOW_ZOOM = -3f;
        [SerializeField] private CinemachinePositionComposer virtualCamera;

        private Vector3 targetFollowOffset;
        private void Start()
        {
            targetFollowOffset = virtualCamera.TargetOffset;
        }
        private void Update()
        {
            Vector3 inputMoveDir = new Vector3(0, 0, 0);
            if (Input.GetKey(KeyCode.W))
            {
                inputMoveDir.z = +1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                inputMoveDir.z = -1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                inputMoveDir.x = -1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                inputMoveDir.x = +1f;
            }
            float moveSpeed = 5f;
            // Take forward/right but remove vertical component so movement stays flat
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            Vector3 moveVector = forward * inputMoveDir.z + right * inputMoveDir.x;

            transform.position += moveVector * moveSpeed * Time.deltaTime;

            Vector3 rotationVector = new Vector3(0, 0, 0);
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

            //Zoom Logic
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