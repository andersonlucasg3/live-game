using System;
using UnityEngine;

[Serializable]
public class FootIKController {
    private Animator _animator;
    private Transform _leftFoot;
    private Transform _rightFoot;
    private bool _isRightFootIKEnabled = false;
    private bool _isLeftFootIKEnabled = false;
    private Vector3 _leftFootIKPosition = Vector3.zero;
    private Vector3 _rightFootIKPosition = Vector3.zero;
    private Vector3 _leftFootPosition = Vector3.zero;
    private Vector3 _rightFootPosition = Vector3.zero;
    private AcceleratedValue _leftFootWeight = AcceleratedValue.zero;
    private AcceleratedValue _rightFootWeight = AcceleratedValue.zero;
    private bool _isMoving;

    [SerializeField] private float _footRaycastDistance = 1F;
    [SerializeField] private float _footCorrectionAcceleration = 5F;
    [SerializeField] private float _footHeightCorrection = 0.1F;
    [SerializeField] private LayerMask _footIgnoreCollisionLayerMask = 0;

    public bool isEnabled { get; set; } = true;

    public void Configure(Animator animator) {
        this._animator = animator;
        this._leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        this._rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        this._leftFootWeight.acceleration = this._footCorrectionAcceleration;
        this._rightFootWeight.acceleration = this._footCorrectionAcceleration;
    }

    public void Update() {
        if (!this.isEnabled) { return; }

        if (!this._isMoving || this._isLeftFootIKEnabled) {
            this._leftFootWeight.target = 1;
            this._leftFootPosition = this.CalculateFootTargetPosition(this._leftFootIKPosition) ?? this._leftFootIKPosition;
        } else {
            this._leftFootWeight.target = 0;
        }

        if (!this._isMoving || this._isRightFootIKEnabled) {
            this._rightFootWeight.target = 1;
            this._rightFootPosition = this.CalculateFootTargetPosition(this._rightFootIKPosition) ?? this._rightFootIKPosition;
        } else {
            this._rightFootWeight.target = 0;
        }

        this._leftFootWeight.Update();
        this._rightFootWeight.Update();
    }

    public void OnAnimatorIK() {
        if (!this.isEnabled) { return; }

        this._rightFootIKPosition = this._animator.GetIKPosition(AvatarIKGoal.RightFoot);
        this._leftFootIKPosition = this._animator.GetIKPosition(AvatarIKGoal.LeftFoot);

        if (!this._isMoving || this._isLeftFootIKEnabled) {
            this._animator.SetIKPosition(AvatarIKGoal.LeftFoot, this._leftFootPosition);
            this._animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, this._leftFootWeight.current);
        }

        if (!this._isMoving || this._isRightFootIKEnabled) {
            this._animator.SetIKPosition(AvatarIKGoal.RightFoot, this._rightFootPosition);
            this._animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, this._rightFootWeight.current);
        }
    }

    public void EnableRightFoot() {
        this._isRightFootIKEnabled = true;
        this._isLeftFootIKEnabled = false;
    }

    public void EnableLeftFoot() {
        this._isLeftFootIKEnabled = true;
        this._isRightFootIKEnabled = false;
    }

    public void SetIsMoving(bool isMoving) => this._isMoving = isMoving;

#if UNITY_EDITOR
    public void OnDrawGizmos() {
        if (this._leftFoot == null || this._rightFoot == null) { return; }

        var leftFootRay = this.CreateFootRay(this._leftFoot.position);
        var rightFootRay = this.CreateFootRay(this._rightFoot.position);
        Gizmos.DrawLine(leftFootRay.origin, leftFootRay.GetPoint(this._footRaycastDistance));
        Gizmos.DrawLine(rightFootRay.origin, rightFootRay.GetPoint(this._footRaycastDistance));

        void DrawSpheres(Vector3 rPos, Vector3 lPos, Color color, float radius = 0.05F) {
            Gizmos.color = color;
            Gizmos.DrawSphere(lPos, radius);
            Gizmos.DrawSphere(rPos, radius);
        }

        DrawSpheres(this._rightFootIKPosition, this._leftFootIKPosition, Color.yellow, 0.04F);
        DrawSpheres(this._rightFootPosition, this._leftFootPosition, Color.blue, 0.03F);
        DrawSpheres(this._rightFoot.position, this._leftFoot.position, Color.red, 0.02F);        
    }
#endif

    private Vector3? CalculateFootTargetPosition(Vector3 position) {
        var ray = this.CreateFootRay(position);
        if (!Physics.Raycast(ray, out RaycastHit hit, this._footRaycastDistance, this._footIgnoreCollisionLayerMask)) { return null; }
        return hit.point + Vector3.up * this._footHeightCorrection;
    }

    private Ray CreateFootRay(Vector3 pos) {
        return new Ray(pos + Vector3.up * 0.5F, Vector3.down);
    }
}
