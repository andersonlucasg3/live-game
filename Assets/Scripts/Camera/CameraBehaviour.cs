using UnityEngine;

public class CameraBehaviour : MonoBehaviour, InputController.ICameraListener {
    [SerializeField] private Transform _target = null;
    [SerializeField] private float _followSpeed = 10F;
    [SerializeField] private Vector3 _positionDisplacement = Vector3.zero;
    [SerializeField] private Vector3 _lookAtDisplacement = Vector3.zero;
    [SerializeField] private float _cameraMinRotation = 90F;
    [SerializeField] private float _cameraMaxRotation = 180F;

    private Quaternion _rotation = Quaternion.identity;
    private Vector3 _currentVelocity = Vector3.zero;

    protected virtual void LateUpdate() {
        var step = this._followSpeed * Time.deltaTime;
        var targetPosition = this._target.position + this._rotation * this._positionDisplacement;
        //var position = Vector3.SmoothDamp(this.transform.position, targetPosition, ref this._currentVelocity, step);
        this.transform.position = targetPosition;//position;

        var targetLookAt = this._target.position + this.transform.rotation * this._lookAtDisplacement;
        this.transform.LookAt(targetLookAt, Vector3.up);
    }

    void InputController.ICameraListener.Rotate(Vector2 mouseDelta) {
        var euler = this._rotation.eulerAngles;
        euler.y += mouseDelta.x;
        euler.x -= mouseDelta.y;
        //euler.x = Mathf.Clamp(euler.x, this._cameraMinRotation, this._cameraMaxRotation);
        this._rotation.eulerAngles = euler;
    }
}
