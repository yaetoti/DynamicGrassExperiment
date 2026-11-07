// TODO Warning: slop

using UnityEngine;

public class FlyingCamera : MonoBehaviour {
  [Header("Movement Settings")]
  [SerializeField]
  private float moveSpeed = 15f;
  [SerializeField]
  private float sprintMultiplier = 2.5f;

  [Header("Rotation Settings")]
  [SerializeField]
  private float mouseSensitivity = 2f;
  [SerializeField]
  private float rollSpeed = 60f;

  [Header("Smoothing Settings")]
  [SerializeField]
  private float movementSmoothness = 10f;
  [SerializeField]
  private float rotationSmoothness = 10f;

  [Header("Speed Adjustment")]
  [SerializeField]
  private float minSpeed = 1f;
  [SerializeField]
  private float maxSpeed = 100f;
  [SerializeField]
  private float scrollSensitivity = 2f;

  private bool isControlActive = false;
  private Vector3 targetPosition;
  private Quaternion targetRotation;

  private void Start() {
    targetPosition = transform.position;
    targetRotation = transform.rotation;
  }

  private void Update() {
    HandleRotation();
    HandleMovement();
    HandleSpeedAdjustment();
  }

  private void LateUpdate() {
    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSmoothness);
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
  }

  private void HandleRotation() {
    if (Input.GetMouseButtonDown(1)) {
      isControlActive = !isControlActive;
    }

    if (isControlActive) {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;

      float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
      float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

      targetRotation *= Quaternion.Euler(-mouseY, mouseX, 0f);
    }
    else {
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }

    if (Input.GetKey(KeyCode.Q)) {
      targetRotation *= Quaternion.Euler(0f, 0f, rollSpeed * Time.deltaTime);
    }

    if (Input.GetKey(KeyCode.E)) {
      targetRotation *= Quaternion.Euler(0f, 0f, -rollSpeed * Time.deltaTime);
    }
  }

  private void HandleMovement() {
    float currentSpeed = moveSpeed;
    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
      currentSpeed *= sprintMultiplier;
    }

    Vector3 moveDirection = Vector3.zero;

    if (Input.GetKey(KeyCode.W)) moveDirection += targetRotation * Vector3.forward;
    if (Input.GetKey(KeyCode.S)) moveDirection -= targetRotation * Vector3.forward;
    if (Input.GetKey(KeyCode.D)) moveDirection += targetRotation * Vector3.right;
    if (Input.GetKey(KeyCode.A)) moveDirection -= targetRotation * Vector3.right;

    if (Input.GetKey(KeyCode.Space)) moveDirection += targetRotation * Vector3.up;
    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) moveDirection -= targetRotation * Vector3.up;

    if (moveDirection.sqrMagnitude > 1f) {
      moveDirection.Normalize();
    }

    targetPosition += moveDirection * (currentSpeed * Time.deltaTime);
  }

  private void HandleSpeedAdjustment() {
    float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
    if (scrollDelta != 0f) {
      moveSpeed += scrollDelta * scrollSensitivity;
      moveSpeed = Mathf.Clamp(moveSpeed, minSpeed, maxSpeed);
    }
  }
}
