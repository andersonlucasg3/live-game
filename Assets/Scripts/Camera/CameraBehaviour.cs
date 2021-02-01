using UnityEngine;

public class CameraBehaviour : MonoBehaviour, InputController.ICameraListener {
    [SerializeField] private float _followSpeed = 10F;
    [SerializeField] private Vector3 _positionDisplacement = Vector3.zero;
    [SerializeField] private Vector3 _lookAtDisplacement = Vector3.zero;
    [SerializeField] private float _cameraMinRotation = 90F;
    [SerializeField] private float _cameraMaxRotation = 180F;
    [SerializeField] private float _windowsCameraAccelerationReduction = 0.1F;
#if UNITY_EDITOR
    [SerializeField] private bool _showGizmos = true;
#endif

    private Vector2 _rotation = Vector2.zero;
    private Vector3 _shoulderPosition = Vector3.zero;
    private AcceleratedVector3 _cameraPosition = AcceleratedVector3.zero;
    private AcceleratedVector3 _targetPosition = AcceleratedVector3.zero;

    public Vector3 targetPosition { set => this._targetPosition.target = value; }

    protected virtual void Awake() {
        this._cameraPosition.acceleration = this._followSpeed;
        this._targetPosition.acceleration = this._followSpeed;
    }

    protected virtual void FixedUpdate() {
        this._shoulderPosition = this.GetShoulderPosition();
        this._cameraPosition.target = this.GetCameraPosition(this._shoulderPosition);

        this._cameraPosition.FixedUpdate();
        this._targetPosition.FixedUpdate();

        this.transform.position = this._cameraPosition.current;
        this.transform.rotation = Quaternion.LookRotation(this._shoulderPosition - this.transform.position);
    }

    private Vector3 GetShoulderPosition()
        => this._targetPosition.current + this.GetRotation(onlyY: true) * this._lookAtDisplacement;

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

        var cubeSize = Vector3.one * 0.1F;

        Gizmos.color = Color.green;
        Gizmos.DrawCube(this._shoulderPosition, cubeSize);
        Gizmos.DrawSphere(this._cameraPosition.target, 0.1F);
        Gizmos.DrawCube(this._targetPosition.target, cubeSize);
    }
#endif

    void InputController.ICameraListener.Rotate(Vector2 mouseDelta) {
        if (Application.platform.IsWindowsPlatform()) { mouseDelta *= this._windowsCameraAccelerationReduction; }
        this._rotation.y += mouseDelta.x;
        this._rotation.x -= mouseDelta.y;
        this._rotation.x = Mathf.Clamp(this._rotation.x, this._cameraMinRotation, this._cameraMaxRotation);
    }
}
