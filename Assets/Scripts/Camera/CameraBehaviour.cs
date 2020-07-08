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
    private Vector3 _shoulderPosition = Vector3.zero;
    private AcceleratedVector3 _shoulderDirection = AcceleratedVector3.zero;
    private AcceleratedVector3 _position = AcceleratedVector3.zero;

    protected virtual void Awake() {
        this._position.acceleration = this._followSpeed;
        this._shoulderDirection.acceleration = this._followSpeed * 2;
    }

    protected virtual void Update() {
        this._shoulderPosition = this.GetShoulderPosition();
        this._shoulderDirection.target = this._shoulderPosition - this.transform.position;
        this._position.target = this.GetCameraPosition(this._shoulderPosition);

        this._position.Update();
        this._shoulderDirection.Update();
    }

    protected virtual void LateUpdate() {
        this.transform.position = this._position.current;
        this.transform.rotation = Quaternion.LookRotation(this._shoulderDirection.current);
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

        var shoulderPosition = this._shoulderPosition;
        var cameraPosition = this._position.target;
        var cubeSize = Vector3.one * 0.1F;

        Gizmos.color = Color.green;
        Gizmos.DrawCube(shoulderPosition, cubeSize);
        Gizmos.DrawSphere(cameraPosition, 0.1F);
    }
#endif

    void InputController.ICameraListener.Rotate(Vector2 mouseDelta) {
        this._rotation.y += mouseDelta.x;
        this._rotation.x -= mouseDelta.y;
        this._rotation.x = Mathf.Clamp(this._rotation.x, this._cameraMinRotation, this._cameraMaxRotation);
    }
}
