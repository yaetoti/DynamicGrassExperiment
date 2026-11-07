// TODO Warning: slop 

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class CapsuleController : MonoBehaviour {
  [Header("References")] [SerializeField]
  private Transform playerCamera;

  [Header("Look Settings")] [SerializeField]
  private float mouseSensitivity = 2.0f;

  [SerializeField] private float maxLookAngle = 89f;

  [Header("Movement Settings")] [SerializeField]
  private float walkSpeed = 6f;

  [SerializeField] private float sprintSpeed = 10f;
  [SerializeField] private float crouchSpeed = 3f;
  [SerializeField] private float jumpForce = 7f;
  [SerializeField] private float maxGroundAngle = 45f;

  [Header("Crouch Settings")] [SerializeField]
  private float standingHeight = 2.0f;

  [SerializeField] private float crouchHeight = 1.0f;
  [SerializeField] private float cameraHeightOffset = -0.2f;
  [SerializeField] private float crouchTransitionSpeed = 15f;
  [SerializeField] private LayerMask headroomMask = ~0;

  private Rigidbody rb;
  private CapsuleCollider col;
  private Transform rendererTransform;

  private readonly List<Vector3> groundNormals = new();
  private Vector2 moveInput;
  private bool wantsToSprint;
  private bool wantsToCrouch;
  private bool jumpRequested;
  private bool isGrounded;
  private bool isCrouching;
  private float jumpCooldownTimer;
  private float cameraPitch;
  private float cameraYaw;
  private float currentHeight;

  public bool IsGrounded => isGrounded;
  public bool IsCrouching => isCrouching;
  public bool IsSprinting => wantsToSprint && !isCrouching && moveInput.sqrMagnitude > 0f;

  private void Awake() {
    rb = GetComponent<Rigidbody>();
    col = GetComponent<CapsuleCollider>();

    rb.freezeRotation = true;
    rb.interpolation = RigidbodyInterpolation.Interpolate;
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    cameraYaw = transform.eulerAngles.y;
    currentHeight = standingHeight;
    ApplyCapsuleDimensions(currentHeight);

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  private void Update() {
    GatherInput();
    UpdateCrouchState();
  }

  private void LateUpdate() {
    UpdateCameraTransform();
  }

  private void FixedUpdate() {
    if (jumpCooldownTimer > 0f) {
      jumpCooldownTimer -= Time.fixedDeltaTime;
      isGrounded = false;
    }
    else {
      isGrounded = groundNormals.Count > 0;
    }

    HandleMovement();

    groundNormals.Clear();
  }

  private void GatherInput() {
    float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
    float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

    cameraYaw += mouseX;
    cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -maxLookAngle, maxLookAngle);

    transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);

    moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    wantsToSprint = Input.GetKey(KeyCode.LeftShift);
    wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

    if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
      jumpRequested = true;
    }
  }

  private void UpdateCameraTransform() {
    if (playerCamera == null) return;

    playerCamera.position = transform.position + transform.up * (currentHeight + cameraHeightOffset);
    playerCamera.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
  }

  private void UpdateCrouchState() {
    if (wantsToCrouch) {
      isCrouching = true;
    }
    else if (isCrouching && CanStandUp()) {
      isCrouching = false;
    }

    float targetHeight = isCrouching ? crouchHeight : standingHeight;
    if (!Mathf.Approximately(currentHeight, targetHeight)) {
      currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
      if (Mathf.Abs(currentHeight - targetHeight) < 0.005f)
        currentHeight = targetHeight;

      ApplyCapsuleDimensions(currentHeight);
    }
  }

  private bool CanStandUp() {
    float radius = col.radius * 0.95f;
    Vector3 currentTopHemisphere = transform.position + transform.up * (currentHeight - col.radius);
    float castDistance = standingHeight - currentHeight;

    if (castDistance <= 0f) return true;

    return !Physics.SphereCast(
      currentTopHemisphere,
      radius,
      transform.up,
      out _,
      castDistance,
      headroomMask,
      QueryTriggerInteraction.Ignore
    );
  }

  private void ApplyCapsuleDimensions(float height) {
    col.height = height;
    col.center = new Vector3(0f, height * 0.5f, 0f);

    if (rendererTransform != null) {
      rendererTransform.localPosition = new Vector3(0f, height * 0.5f, 0f);
      rendererTransform.localScale = new Vector3(1f, height * 0.5f, 1f);
    }
  }

  private void HandleMovement() {
    Vector3 wishDir = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
    float currentSpeed = isCrouching ? crouchSpeed : (wantsToSprint ? sprintSpeed : walkSpeed);

    if (isGrounded) {
      if (jumpRequested) {
        jumpRequested = false;
        isGrounded = false;
        jumpCooldownTimer = 0.15f;
        rb.useGravity = true;

        Vector3 horizVel = wishDir * currentSpeed;
        rb.linearVelocity = new Vector3(horizVel.x, jumpForce, horizVel.z);
        return;
      }

      rb.useGravity = false;

      if (wishDir.sqrMagnitude < 0.0001f) {
        rb.linearVelocity = Vector3.zero;
        return;
      }

      Vector3 bestMovementDir = Vector3.zero;
      float highestUpDot = float.NegativeInfinity;

      for (int i = 0; i < groundNormals.Count; i++) {
        Vector3 projectedDir = Vector3.ProjectOnPlane(wishDir, groundNormals[i]).normalized;
        float upDot = Vector3.Dot(projectedDir, transform.up);

        if (upDot > highestUpDot) {
          highestUpDot = upDot;
          bestMovementDir = projectedDir;
        }
      }

      rb.linearVelocity = bestMovementDir * currentSpeed;
    }
    else {
      jumpRequested = false;
      rb.useGravity = true;

      Vector3 horizontalVel = wishDir * currentSpeed;
      rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
    }
  }

  private void OnCollisionStay(Collision collision) {
    float minUpDot = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);

    int contactCount = collision.contactCount;
    for (int i = 0; i < contactCount; i++) {
      Vector3 normal = collision.GetContact(i).normal;

      if (Vector3.Dot(normal, transform.up) >= minUpDot) {
        groundNormals.Add(normal);
      }
    }
  }
}
