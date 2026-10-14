using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class AdvancedFirstPersonController : MonoBehaviour
{
    [Header("Speeds (Скорости)")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2.5f;
    public float proneSpeed = 1.5f;

    [Header("Dynamics (Динамика бега)")]
    [Tooltip("Ускорение (разгон)")]
    public float acceleration = 5f;
    [Tooltip("Замедление (затухание)")]
    public float deceleration = 10f;

    [Header("Slide (Скольжение)")]
    [Tooltip("Начальная скорость скольжения")]
    public float slideInitialSpeed = 15f;
    [Tooltip("Продолжительность скольжения")]
    public float slideDuration = 1f;

    [Header("Jumping & Gravity (Прыжок и Гравитация)")]
    public float jumpForce = 5f;
    public float gravity = 15f;

    [Header("Capsule Dimensions (Размеры капсулы)")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;
    public float proneHeight = 0.5f;
    public float capsuleRadius = 0.5f;

    [Header("Camera Heights (Высота камеры)")]
    public float standingCameraY = 1.6f;
    public float crouchingCameraY = 0.8f;
    public float proneCameraY = 0.3f;

    [Header("Camera & Look (Обзор)")]
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Head Bobbing (Покачивание)")]
    public float bobFrequency = 1.5f;
    public float bobAmplitude = 0.05f;

    // State
    public enum Posture { Standing, Crouching, Prone }
    private Posture currentPosture = Posture.Standing;

    private CharacterController controller;
    private float currentSpeed;
    private float verticalRotation;
    private Vector3 moveDirection;
    private float verticalVelocity;
    
    private float targetCameraY;
    private float currentBaseCameraY;
    private float bobTimer;

    private bool isSprinting;
    private bool isSliding;
    private float slideTimer;
    private Vector3 slideDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        targetCameraY = standingCameraY;
        currentBaseCameraY = standingCameraY;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        UpdateCapsuleDimensions();
    }

    void Update()
    {
        HandleLook();
        HandlePostureChanges();
        HandleMovement();
        HandleHeadBob();
    }

    private void HandleLook()
    {
        if (playerCamera == null || Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        // В новой системе ввода значения delta от мыши больше, чем в старой, 
        // поэтому применяем небольшой коэффициент для сохранения схожей чувствительности.
        float mouseX = mouseDelta.x * mouseSensitivity * 0.1f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.1f;

        transform.Rotate(0, mouseX, 0);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandlePostureChanges()
    {
        if (isSliding || Keyboard.current == null) return; 

        // Приседание (Ctrl)
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            if (currentPosture == Posture.Crouching)
            {
                if (CanChangePosture(Posture.Standing)) SetPosture(Posture.Standing);
            }
            else if (currentPosture == Posture.Standing)
            {
                // Скольжение, если максимальная скорость бега (допуск в 1f)
                if (isSprinting && currentSpeed >= sprintSpeed - 1f)
                {
                    if (CanChangePosture(Posture.Crouching)) StartSlide();
                }
                else
                {
                    if (CanChangePosture(Posture.Crouching)) SetPosture(Posture.Crouching);
                }
            }
            else if (currentPosture == Posture.Prone)
            {
                if (CanChangePosture(Posture.Crouching)) SetPosture(Posture.Crouching);
            }
        }

        // Ползание (Alt)
        if (Keyboard.current.leftAltKey.wasPressedThisFrame)
        {
            if (currentPosture == Posture.Prone)
            {
                if (CanChangePosture(Posture.Standing)) SetPosture(Posture.Standing);
            }
            else
            {
                if (CanChangePosture(Posture.Prone)) SetPosture(Posture.Prone);
            }
        }
    }

    private bool CanChangePosture(Posture targetPosture)
    {
        float tHeight = standingHeight;
        float tCamY = standingCameraY;
        if (targetPosture == Posture.Crouching)
        {
            tHeight = crouchingHeight;
            tCamY = crouchingCameraY;
        }
        else if (targetPosture == Posture.Prone)
        {
            tHeight = proneHeight;
            tCamY = proneCameraY;
        }

        if (controller.isGrounded && tHeight <= controller.height)
        {
            return true; // Если на земле и уменьшаемся, то всегда можно
        }

        Vector3 bottomPos = transform.position;
        if (!controller.isGrounded)
        {
            // В воздухе мы будем компенсировать позицию (подтягивать ноги), поэтому проверяем будущую позицию капсулы
            bottomPos += Vector3.up * (targetCameraY - tCamY);
        }

        float radius = capsuleRadius * 0.9f;
        Vector3 p1 = bottomPos + Vector3.up * (radius + 0.05f);
        Vector3 p2 = bottomPos + Vector3.up * (tHeight - radius - 0.05f);
        
        if (p2.y < p1.y) p2 = p1;

        // Отключаем контроллер на время проверки, чтобы он не обнаруживал сам себя
        controller.enabled = false;
        bool hasOverlap = Physics.CheckCapsule(p1, p2, radius, ~0, QueryTriggerInteraction.Ignore);
        controller.enabled = true;

        return !hasOverlap;
    }

    private void SetPosture(Posture newPosture)
    {
        float previousCameraY = targetCameraY;
        currentPosture = newPosture;
        UpdateCapsuleDimensions();

        if (!controller.isGrounded)
        {
            float camYDiff = previousCameraY - targetCameraY;
            
            // Компенсация высоты в прыжке (crouch-jump)
            // Это удерживает камеру на одном уровне и "подтягивает" нижнюю часть капсулы вверх
            controller.enabled = false;
            transform.position += Vector3.up * camYDiff;
            controller.enabled = true;
            
            // Моментально обновляем базу камеры, чтобы не было визуального рывка
            currentBaseCameraY = targetCameraY; 
        }
    }

    private void UpdateCapsuleDimensions()
    {
        controller.radius = capsuleRadius;
        switch (currentPosture)
        {
            case Posture.Standing:
                controller.height = standingHeight;
                controller.center = new Vector3(0, standingHeight / 2f, 0);
                targetCameraY = standingCameraY;
                break;
            case Posture.Crouching:
                controller.height = crouchingHeight;
                controller.center = new Vector3(0, crouchingHeight / 2f, 0);
                targetCameraY = crouchingCameraY;
                break;
            case Posture.Prone:
                controller.height = proneHeight;
                controller.center = new Vector3(0, proneHeight / 2f, 0);
                targetCameraY = proneCameraY;
                break;
        }
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        SetPosture(Posture.Crouching);
        
        float h = 0f;
        float v = 0f;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) h += 1f;
            if (Keyboard.current.aKey.isPressed) h -= 1f;
            if (Keyboard.current.wKey.isPressed) v += 1f;
            if (Keyboard.current.sKey.isPressed) v -= 1f;
        }

        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        slideDirection = (transform.forward * inputDir.z + transform.right * inputDir.x).normalized;
        
        if (slideDirection == Vector3.zero)
        {
            slideDirection = transform.forward;
        }
        
        currentSpeed = slideInitialSpeed;
    }

    private void HandleMovement()
    {
        float targetSpeed = 0f;
        
        float h = 0f;
        float v = 0f;
        bool sprintPressed = false;
        bool jumpPressedThisFrame = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) h += 1f;
            if (Keyboard.current.aKey.isPressed) h -= 1f;
            if (Keyboard.current.wKey.isPressed) v += 1f;
            if (Keyboard.current.sKey.isPressed) v -= 1f;
            
            sprintPressed = Keyboard.current.leftShiftKey.isPressed;
            jumpPressedThisFrame = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                isSliding = false;
            }
            else
            {
                // Замедление в скольжении
                currentSpeed = Mathf.Lerp(currentSpeed, crouchSpeed, Time.deltaTime * (1f / slideDuration) * 2f);
                moveDirection = slideDirection * currentSpeed;
            }
        }
        else
        {
            Vector3 inputDirection = new Vector3(h, 0, v).normalized;

            bool isMoving = inputDirection.sqrMagnitude > 0;
            
            isSprinting = false;
            // Бег доступен только из положения стоя и при движении вперед
            if (currentPosture == Posture.Standing && sprintPressed && v > 0)
            {
                isSprinting = true;
                targetSpeed = sprintSpeed;
            }
            else if (isMoving)
            {
                switch (currentPosture)
                {
                    case Posture.Standing: targetSpeed = walkSpeed; break;
                    case Posture.Crouching: targetSpeed = crouchSpeed; break;
                    case Posture.Prone: targetSpeed = proneSpeed; break;
                }
            }

            // Плавный разгон и затухание скорости
            if (isMoving || isSprinting)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }

            moveDirection = (transform.forward * inputDirection.z + transform.right * inputDirection.x) * currentSpeed;
        }

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0) verticalVelocity = -2f; 

            // Прыжок только стоя
            if (!isSliding && currentPosture == Posture.Standing && jumpPressedThisFrame)
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 finalMove = moveDirection;
        finalMove.y = verticalVelocity;
        
        controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleHeadBob()
    {
        if (playerCamera == null) return;

        currentBaseCameraY = Mathf.Lerp(currentBaseCameraY, targetCameraY, Time.deltaTime * 10f);

        float speedRatio = currentSpeed / walkSpeed;
        if (speedRatio < 0.1f) speedRatio = 0.1f;
        
        bool hasInput = false;
        if (Keyboard.current != null)
        {
            hasInput = Keyboard.current.wKey.isPressed || 
                       Keyboard.current.aKey.isPressed || 
                       Keyboard.current.sKey.isPressed || 
                       Keyboard.current.dKey.isPressed;
        }

        bool isMovingOnGround = controller.isGrounded && !isSliding && currentSpeed > 0.1f && hasInput;

        if (isMovingOnGround)
        {
            bobTimer += Time.deltaTime * bobFrequency * speedRatio;
            float bobY = Mathf.Sin(bobTimer) * bobAmplitude;
            float bobX = Mathf.Cos(bobTimer / 2f) * bobAmplitude * 0.5f;

            playerCamera.transform.localPosition = new Vector3(bobX, currentBaseCameraY + bobY, playerCamera.transform.localPosition.z);
        }
        else
        {
            bobTimer = 0f;
            playerCamera.transform.localPosition = new Vector3(
                Mathf.Lerp(playerCamera.transform.localPosition.x, 0, Time.deltaTime * 10f),
                currentBaseCameraY,
                playerCamera.transform.localPosition.z
            );
        }
    }
}