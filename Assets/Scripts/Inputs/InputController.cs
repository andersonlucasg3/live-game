using UnityEngine;
using UnityEngine.InputSystem;

public class InputController {
    private readonly InputActions _inputs;

    public IMovementListener movementListener { get; set; }
    public ICameraListener cameraListener { get; set; }

    public InputController() {
        this._inputs = new InputActions();
        this._inputs.Player.Walk.performed += this.MovePerformed;
        this._inputs.Player.Rotate.performed += this.RotatePerformed;
        this._inputs.Player.Rotate.canceled += this.RotatePerformed;
    }

    public void Enable() {
        this._inputs.Enable();
    }

    public void Disable() {
        this._inputs.Disable();
    }

    #region Events

    private void MovePerformed(InputAction.CallbackContext context) {
        this.movementListener?.Move(context.ReadValue<Vector2>());
    }

    private void RotatePerformed(InputAction.CallbackContext context) {
        this.cameraListener?.Rotate(context.ReadValue<Vector2>());
    }

    #endregion

    public interface IMovementListener {
        void Move(Vector2 inputDirection);
    }

    public interface ICameraListener {
        void Rotate(Vector2 mouseDelta);
    }
}
