using System;
using UnityEngine;

[Serializable]
public class FootIKController {
    private Animator _animator;
    private Transform _leftFoot;
    private Transform _rightFoot;
    private Vector3 _leftFootIKPosition = Vector3.zero;
    private Vector3 _rightFootIKPosition = Vector3.zero;
    private FootHitInfo? _leftFootInfo = null;
    private FootHitInfo? _rightFootInfo = null;
    private AcceleratedValue _leftFootWeight = AcceleratedValue.zero;
    private AcceleratedValue _rightFootWeight = AcceleratedValue.zero;
    private bool _isMoving;
    private bool _inStairs;

    [SerializeField] private float _footRaycastDistance = 1F;
    [SerializeField] private float _footCorrectionAcceleration = 5F;
    [SerializeField] private float _footHeightCorrection = 0.1F;
    [SerializeField] private float _footMinDistanceToIK = 0.1F;
    [SerializeField] private LayerMask _raycastLayerMask = 0;

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

        if (!this._isMoving || this._inStairs) {
            this._leftFootWeight.target = 1;
            this._leftFootInfo = this.CalculateFootTargetPosition(this._leftFootIKPosition);
        } else {
            this._leftFootWeight.target = 0;
        }

        if (!this._isMoving || this._inStairs) {
            this._rightFootWeight.target = 1;
            this._rightFootInfo = this.CalculateFootTargetPosition(this._rightFootIKPosition);
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

        if (this._leftFootInfo.HasValue) {
            this.DoFootIK(AvatarIKGoal.LeftFoot, this._leftFootInfo.Value, this._leftFootWeight, this._leftFoot, this._leftFootIKPosition);
        }

        if (this._rightFootInfo.HasValue) {
            this.DoFootIK(AvatarIKGoal.RightFoot, this._rightFootInfo.Value, this._rightFootWeight, this._rightFoot, this._rightFootIKPosition);
        }
    }

    private void DoFootIK(AvatarIKGoal goal, FootHitInfo info, AcceleratedValue weight, Transform foot, Vector3 footIKPosition) {
        if (this._isMoving && Vector3.Distance(info.point, footIKPosition) > this._footMinDistanceToIK) { return; }

        this._animator.SetIKPosition(goal, info.point);
        this._animator.SetIKPositionWeight(goal, weight.current);

        this._animator.SetIKRotation(goal, this.RotatedFoot(info.normal, foot));
        this._animator.SetIKRotationWeight(goal, weight.current);
    }

    public void SetIsMoving(bool isMoving) => this._isMoving = isMoving;
    public void SetInStairs(bool inStairs) => this._inStairs = inStairs;

#if UNITY_EDITOR
    public void OnDrawGizmos() {
        if (!this.isEnabled) { return; }
        if (this._leftFoot == null || this._rightFoot == null) { return; }
        if (this._leftFootInfo == null || this._rightFootInfo == null) { return; }

        var leftFootRay = this.CreateFootRay(this._leftFoot.position);
        var rightFootRay = this.CreateFootRay(this._rightFoot.position);
        Gizmos.DrawLine(leftFootRay.origin, leftFootRay.GetPoint(this._footRaycastDistance));
        Gizmos.DrawLine(rightFootRay.origin, rightFootRay.GetPoint(this._footRaycastDistance));

        void DrawSpheres(Vector3 rPos, Vector3 lPos, Color color, float radius = 0.05F) {
            Gizmos.color = color;
            Gizmos.DrawSphere(lPos, radius);
            Gizmos.DrawSphere(rPos, radius);
        }

        void DrawLine(Vector3 point, Vector3 direction, Color color, float length = 1F) {
            Gizmos.color = color;
            Gizmos.DrawLine(point, point + direction * length);
        }

        DrawSpheres(this._rightFootIKPosition, this._leftFootIKPosition, Color.yellow, 0.04F);
        DrawSpheres(this._rightFootInfo.Value.point, this._leftFootInfo.Value.point, Color.blue, 0.03F);
        DrawSpheres(this._rightFoot.position, this._leftFoot.position, Color.red, 0.02F);

        DrawLine(this._rightFootInfo.Value.point, this._rightFootInfo.Value.normal, Color.green, 0.25F);
    }
#endif

    private Quaternion RotatedFoot(Vector3 normal, Transform foot) {
        Vector3 slopeCorrected = Vector3.Cross(normal, foot.right);
        return Quaternion.LookRotation(slopeCorrected, normal);
    }

    private FootHitInfo? CalculateFootTargetPosition(Vector3 position) {
        var ray = this.CreateFootRay(position);
        if (!Physics.Raycast(ray, out RaycastHit hit, this._footRaycastDistance, this._raycastLayerMask)) { return null; }
        return FootHitInfo.zero.Set(hit.point + Vector3.up * this._footHeightCorrection, hit.normal);
    }

    private Ray CreateFootRay(Vector3 pos) {
        return new Ray(pos + Vector3.up * 0.5F, Vector3.down);
    }

    struct FootHitInfo {
        public static readonly FootHitInfo zero = new FootHitInfo();

        public Vector3 point;
        public Vector3 normal;

        public FootHitInfo Set(Vector3 point, Vector3 normal) {
            this.point = point;
            this.normal = normal;
            return this;
        }
    }
}
