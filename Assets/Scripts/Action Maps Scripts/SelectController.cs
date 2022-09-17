using UnityEngine;
using UnityEngine.InputSystem;

public class SelectController : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    private SelectInput _selectInput;

    private void Awake()
    {
        _selectInput = new SelectInput();
    }

    public void OnEnable()
    {
        _selectInput.Select.LeftClick.performed += LeftClick_performed;
        _selectInput.Enable();
    }

    private void LeftClick_performed(InputAction.CallbackContext obj)
    {
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var hit))
        {
            Debug.LogWarning(hit.collider.gameObject.name);
            var pos = hit.collider.gameObject.transform.position;
            var location = MapBuilder.Instance.GetTileLocationByPosition(new Vector2(pos.x, pos.z));
            //MapBuilder.Instance.DestroyHex(location);
        }
    }

    public void OnDisable()
    {
        _selectInput.Select.LeftClick.performed -= LeftClick_performed;
        _selectInput.Disable();
    }
}
