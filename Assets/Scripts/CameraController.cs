using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    #region Fields

    #region public

    public Camera MainCamera;

    [Space]
    [Header("Camera Movement")]
    public float SmoothMovementSpeed = 0.2f;
    public float MovementSpeed = 2.5f;

    [Space]
    [Header("Camera Rotation")]
    public float SmoothRotationSpeed = 0.1f;
    public float RotationSpeed = 1f;
    public float MouseRotationSpeed = 30f;

    public float SpeedMultiplier = 2f;

    public Transform Target;

    [Header("Mouse Settings")]
    public float StepSize;
    public float ZoomDampening;
    public float MinHeight;
    public float MaxHeight;
    public float ZoomSpeed;

    #endregion

    private CameraInput _cameraInput;

    private Vector2 _currentMovementInput;
    private Vector2 _smoothMovementInputVelocity;

    private Vector2 _currentRotationInput;
    private Vector2 _smoothRotationInputVelocity;

    private Quaternion _rotationQuaternion;

    private Transform _target;

    private float _multiplier = 1f;

    private float _zoomHeight;
    private Transform _cameraTransform;
    private Vector3 _startDrag;


    #endregion

    #region Mono

    private void Awake()
    {
        _cameraTransform = this.GetComponentInChildren<Camera>().transform;
        _cameraInput = new CameraInput();
        SetTargetTransform(Target);
    }

    public void OnEnable()
    {
        _zoomHeight = _cameraTransform.localPosition.y;
        _cameraTransform.LookAt(this.transform);

        _cameraInput.Camera.RotateCameraMouse.performed += RotateCamera;
        _cameraInput.Camera.ZoomCameraMouse.performed += ZoomCamera;
        _cameraInput.Enable();
    }

    public void OnDisable()
    {
        _cameraInput.Camera.RotateCameraMouse.performed -= RotateCamera;
        _cameraInput.Camera.ZoomCameraMouse.performed -= ZoomCamera;
        _cameraInput.Disable();
    }

    public void Update()
    {
        HandleCameraMovement();
        HandleCameraRotation();
        HandleSpeedMultiplier();
        HandleZoomCamera();

        HandleCameraDrag();
    }


    #endregion

    #region Handlers

    private void HandleSpeedMultiplier()
    {
        var input = _cameraInput.Camera.CameraSpeedMultiplier.ReadValue<float>();
        _multiplier = input > 0 ? SpeedMultiplier : 1f;
    }

    public void SetTargetTransform(Transform target)
    {
        _target = target;
        _rotationQuaternion = _target.rotation;
    }

    private void HandleCameraRotation()
    {
        var input = _cameraInput.Camera.RotateCameraKeyboard.ReadValue<float>();
        var inputVector2 = new Vector2(input, 0);

        _currentRotationInput = Vector2.SmoothDamp(_currentRotationInput, inputVector2,
            ref _smoothRotationInputVelocity, SmoothRotationSpeed);

        _rotationQuaternion *= Quaternion.Euler(Vector3.up * _currentRotationInput.x * RotationSpeed * Time.deltaTime);
        _target.rotation = _rotationQuaternion;
    }

    private void HandleCameraMovement()
    {
        var input = _cameraInput.Camera.Move.ReadValue<Vector2>();
        _currentMovementInput = Vector2.SmoothDamp(_currentMovementInput, input, ref _smoothMovementInputVelocity,
            SmoothMovementSpeed);

        var dir = Camera.main.transform.rotation * new Vector3(_currentMovementInput.x, 0, _currentMovementInput.y);
        dir.y = 0;

        _target.position += dir * Time.deltaTime * MovementSpeed * _multiplier;
    }

    private void RotateCamera(InputAction.CallbackContext obj)
    {
        if (!Mouse.current.rightButton.isPressed)
            return;

        var inputVector2 = obj.ReadValue<Vector2>();

        //_currentRotationInput = Vector2.SmoothDamp(_currentRotationInput, inputVector2, ref _smoothRotationInputVelocity, SmoothRotationSpeed);

        _rotationQuaternion *= Quaternion.Euler(Vector3.up * inputVector2.x * MouseRotationSpeed * Time.deltaTime);
        _target.rotation = _rotationQuaternion;
    }

    private void HandleCameraDrag()
    {
        if (!Mouse.current.leftButton.isPressed)
            return;


        var plane = new Plane(Vector3.up, Vector3.zero);
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                _startDrag = ray.GetPoint(distance);
            else
            {
                var dir = _startDrag - ray.GetPoint(distance);
                _target.position += dir * Time.deltaTime * MovementSpeed;
            }
        }
    }

    private void ZoomCamera(InputAction.CallbackContext obj)
    {
        var inputValue = -obj.ReadValue<Vector2>().y / 100f;

        if (Mathf.Abs(inputValue) > 0.1f)
        {
            _zoomHeight = _cameraTransform.localPosition.y + inputValue * StepSize;

            if (_zoomHeight < MinHeight)
                _zoomHeight = MinHeight;
            else if (_zoomHeight > MaxHeight)
                _zoomHeight = MaxHeight;
        }
    }

    private void HandleZoomCamera()
    {
        var zoomTarget = new Vector3(_cameraTransform.localPosition.x, _zoomHeight, _cameraTransform.localPosition.z);
        zoomTarget -= ZoomSpeed * (_zoomHeight - _cameraTransform.localPosition.y) * Vector3.forward;

        _cameraTransform.localPosition = Vector3.Lerp(_cameraTransform.localPosition, zoomTarget, Time.deltaTime * ZoomDampening);
        _cameraTransform.LookAt(this.transform);
    }
    #endregion
}