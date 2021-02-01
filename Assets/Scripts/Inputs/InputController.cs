using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController {
    private readonly InputActions _inputs = new InputActions();

    private readonly List<WeakReference<IMovementListener>> _movementListeners = new List<WeakReference<IMovementListener>>();
    private readonly List<WeakReference<ICameraListener>> _cameraListeners = new List<WeakReference<ICameraListener>>();

    public Vector2 directionVector { get; private set; } = Vector2.zero;
    public Vector2 rotationVector { get; private set; } = Vector2.up;
    public bool runningState { get; private set; } = false;

    public InputController() {
        this._inputs.Player.Walk.performed += this.MovePerformed;
        this._inputs.Player.Rotate.performed += this.RotatePerformed;
        this._inputs.Player.Rotate.canceled += this.RotatePerformed;
        this._inputs.Player.Run.performed += this.RunPerformed;
    }

    public void Enable() {
        this._inputs.Enable();
    }

    public void Disable() {
        this._inputs.Disable();
    }

    public void AddListener(IInputListener listener) {
        this.AddListener(this._movementListeners, listener);
        this.AddListener(this._cameraListeners, listener);
    }

    public void RemoveListener(IInputListener listener) {
        this.RemoveListener(this._movementListeners, listener);
        this.RemoveListener(this._cameraListeners, listener);
    }

    #region Listeners

    private void ForEachListeners<TListener, TValue>(List<WeakReference<TListener>> list, TValue value, Action<TListener, TValue> action) where TListener : class {
        for (int i = 0; i < list.Count; i++) {
            var weak = list[i];
            if (!weak.TryGetTarget(out TListener listener)) {
                list.RemoveAt(i);
                i--;
                continue;
            }
            action.Invoke(listener, value);
        }
    }

    private void AddListener<TListener>(List<WeakReference<TListener>> list, IInputListener listener) where TListener : class {
        if (listener is TListener typeListener) {
            list.Add(new WeakReference<TListener>(typeListener));
        }
    }

    private void RemoveListener<TListener>(List<WeakReference<TListener>> list, IInputListener listener) where TListener : class {
        list.RemoveAll(weak => !weak.TryGetTarget(out TListener target) || target == listener);
    }

    #endregion

    #region Events

    private void MovePerformed(InputAction.CallbackContext context) {
        this.directionVector = context.ReadValue<Vector2>();
        this.ForEachListeners(this._movementListeners, this.directionVector, this.PerformMove);
    }

    private void RotatePerformed(InputAction.CallbackContext context) {
        this.rotationVector = context.ReadValue<Vector2>();
        this.ForEachListeners(this._cameraListeners, this.rotationVector, this.PerformRotate);
    }

    private void RunPerformed(InputAction.CallbackContext context) {
        if (context.ReadValue<float>() == 1F) {
            this.runningState = !this.runningState;
        }

        this.ForEachListeners(this._movementListeners, this.runningState, this.PerformRun);
    }

    private void PerformMove(IMovementListener listener, Vector2 vector) => listener.Move(vector);
    private void PerformRun(IMovementListener listener, bool running) => listener.Run(running);
    private void PerformRotate(ICameraListener listener, Vector2 vector) => listener.Rotate(vector);

    #endregion

    public interface IInputListener { }

    public interface IMovementListener : IInputListener {
        void Move(Vector2 inputDirection);
        void Run(bool running);
    }

    public interface ICameraListener : IInputListener {
        void Rotate(Vector2 mouseDelta);
    }
}
