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

  private void Update() {
    HandleRotation();
    HandleMovement();
  }

  private void HandleRotation() {
    if (Input.GetMouseButton(1)) {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;

      float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
      float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

      transform.Rotate(Vector3.up, mouseX, Space.Self);
      transform.Rotate(Vector3.left, mouseY, Space.Self);
    }
    else {
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }

    if (Input.GetKey(KeyCode.Q)) {
      transform.Rotate(Vector3.forward, rollSpeed * Time.deltaTime, Space.Self);
    }

    if (Input.GetKey(KeyCode.E)) {
      transform.Rotate(Vector3.back, rollSpeed * Time.deltaTime, Space.Self);
    }
  }

  private void HandleMovement() {
    float currentSpeed = moveSpeed;
    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
      currentSpeed *= sprintMultiplier;
    }

    Vector3 moveDirection = Vector3.zero;

    if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
    if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
    if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
    if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;

    if (Input.GetKey(KeyCode.Space)) moveDirection += transform.up;
    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) moveDirection -= transform.up;

    if (moveDirection.sqrMagnitude > 1f) {
      moveDirection.Normalize();
    }

    transform.position += moveDirection * (currentSpeed * Time.deltaTime);
  }
}
