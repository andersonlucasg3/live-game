using UnityEngine;

public class CameraBehaviour : MonoBehaviour, InputController.ICameraListener {
    [SerializeField] private Transform _target = null;
    [SerializeField] private float _followSpeed = 10F;
    [SerializeField] private Vector3 _positionDisplacement = Vector3.zero;
    [SerializeField] private Vector3 _lookAtDisplacement = Vector3.zero;
    [SerializeField] private float _cameraMinRotation = 90F;
    [SerializeField] private float _cameraMaxRotation = 180F;
#if UNITY_EDITOR
    [SerializeField] private bool _showGizmos = true;
#endif

    private Vector2 _rotation = Vector2.zero;
    private Vector3 _currentVelocity = Vector3.zero;

    protected virtual void LateUpdate() {
        var step = this._followSpeed * Time.deltaTime;
        var targetShoulderPosition = this.GetShoulderPosition();
        var targetPosition = this.GetCameraPosition(targetShoulderPosition);

        this.transform.position = Vector3.SmoothDamp(this.transform.position, targetPosition, ref this._currentVelocity, step);
        this.transform.LookAt(targetShoulderPosition, Vector3.up);
    }

    private Vector3 GetShoulderPosition()
        => this._target.position + this.GetRotation(onlyY: true) * this._lookAtDisplacement;

    private Vector3 GetCameraPosition(Vector3 shoulderPosition)
        => shoulderPosition + this.GetRotation() * this._positionDisplacement;

    private Quaternion GetRotation(bool onlyY = false) {
        var euler = Vector3.zero;
        euler.y = this._rotation.y;
        if (!onlyY) { euler.x = this._rotation.x; }
        var quat = Quaternion.identity;
        quat.eulerAngles = euler;
        return quat;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        if (!this._showGizmos) { return; }
        if (this._target == null) { return; }

        var shoulderPosition = this.GetShoulderPosition();
        var cameraPosition = this.GetCameraPosition(shoulderPosition);
        var cubeSize = Vector3.one * 0.25F;

        Gizmos.DrawCube(shoulderPosition, cubeSize);
        Gizmos.DrawSphere(cameraPosition, 0.25F);
    }
#endif

    void InputController.ICameraListener.Rotate(Vector2 mouseDelta) {
        this._rotation.y += mouseDelta.x;
        this._rotation.x -= mouseDelta.y;
        this._rotation.x = Mathf.Clamp(this._rotation.x, this._cameraMinRotation, this._cameraMaxRotation);
    }
}
