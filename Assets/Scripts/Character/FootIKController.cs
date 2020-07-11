using System;
using UnityEngine;

[Serializable]
public class FootIKController {
    private Animator _animator;

    private Transform _leftToes;
    private Transform _leftFoot;
    private Transform _rightToes;
    private Transform _rightFoot;

    private Vector3 _leftFootIKPosition = Vector3.zero;
    private Vector3 _rightFootIKPosition = Vector3.zero;

    private FootHitInfo? _leftFootInfo = null;
    private FootHitInfo? _rightFootInfo = null;

    private AcceleratedValue _leftFootWeight = AcceleratedValue.zero;
    private AcceleratedValue _rightFootWeight = AcceleratedValue.zero;
    
    [SerializeField] private float _footRaycastMaxDistance = 1F;
    [SerializeField] private float _footHeightCorrection = 0.1F;
    [SerializeField] private float _footCorrectionAcceleration = 5F;
    [SerializeField] private LayerMask _raycastLayerMask = 0;
    [SerializeField] private Vector3 _footRaycastDisplacement = Vector3.zero;
#if UNITY_EDITOR
    [SerializeField] private bool _drawGizmos = true;
#endif

    public bool isEnabled { get; set; } = true;

    public void Configure(Animator animator) {
        this._animator = animator;
        this._leftToes = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        this._leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        this._rightToes = animator.GetBoneTransform(HumanBodyBones.RightToes);
        this._rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        this._leftFootWeight.acceleration = this._footCorrectionAcceleration;
        this._rightFootWeight.acceleration = this._footCorrectionAcceleration;
    }

    public void FixedUpdate() {
        if (!this.isEnabled) { return; }

        this._leftFootInfo = this.CalculateFootTargetPosition(this._leftFootIKPosition, this._animator.transform.rotation, Vector3.down);
        this._rightFootInfo = this.CalculateFootTargetPosition(this._rightFootIKPosition, this._animator.transform.rotation, Vector3.down);
        if (this._leftFootInfo.HasValue) { this._leftFootWeight.target = 1F; } else { this._leftFootWeight.target = 0F; }
        if (this._rightFootInfo.HasValue) { this._rightFootWeight.target = 1F; } else { this._rightFootWeight.target = 0F; }

        this._leftFootWeight.FixedUpdate();
        this._rightFootWeight.FixedUpdate();
    }

    public void Update() {
        if (!this.isEnabled) { return; }
    }

    public void OnAnimatorIK() {
        if (!this.isEnabled) { return; }

        this._leftFootIKPosition = this._animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        this._rightFootIKPosition = this._animator.GetIKPosition(AvatarIKGoal.RightFoot);

        this.DoFootIK(AvatarIKGoal.LeftFoot, this._leftFootInfo, this._leftFootWeight, this._leftFoot);
        this.DoFootIK(AvatarIKGoal.RightFoot, this._rightFootInfo, this._rightFootWeight, this._rightFoot);
    }

    private void DoFootIK(AvatarIKGoal goal, FootHitInfo? info, AcceleratedValue weight, Transform foot) {
        if (!info.HasValue) { return; }

        this._animator.SetIKPosition(goal, info.Value.point);
        this._animator.SetIKPositionWeight(goal, weight.current);

        this._animator.SetIKRotation(goal, this.RotatedFoot(info.Value.normal, foot));
        this._animator.SetIKRotationWeight(goal, weight.current);
    }

#if UNITY_EDITOR
    public void OnDrawGizmos() {
        if (!this.isEnabled) { return; }
        if (!this._drawGizmos) { return; }
        if (this._leftFoot == null || this._rightFoot == null) { return; }

        var leftFootRay = this.CreateFootRay(this._leftFootIKPosition, this._animator.transform.rotation, Vector3.down);
        var rightFootRay = this.CreateFootRay(this._rightFootIKPosition, this._animator.transform.rotation, Vector3.down);
        Gizmos.DrawLine(leftFootRay.origin, leftFootRay.GetPoint(this._footRaycastMaxDistance));
        Gizmos.DrawLine(rightFootRay.origin, rightFootRay.GetPoint(this._footRaycastMaxDistance));

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
        DrawSpheres(this._rightFoot.position, this._leftFoot.position, Color.red, 0.02F);

        if (this._leftFootInfo == null || this._rightFootInfo == null) { return; }

        DrawSpheres(this._rightFootInfo.Value.point, this._leftFootInfo.Value.point, Color.blue, 0.03F);
        DrawLine(this._rightFootInfo.Value.point, this._rightFootInfo.Value.normal, Color.green, 0.25F);
    }
#endif

    private Quaternion RotatedFoot(Vector3 normal, Transform foot) {
        Vector3 slopeCorrected = Vector3.Cross(normal, foot.right);
        return Quaternion.LookRotation(slopeCorrected, normal);
    }

    private FootHitInfo? CalculateFootTargetPosition(Vector3 position, Quaternion rotation, Vector3 direction) {
        var ray = this.CreateFootRay(position, rotation, direction);
        if (!Physics.Raycast(ray, out RaycastHit hit, this._footRaycastMaxDistance, this._raycastLayerMask)) { return null; }
        return new FootHitInfo {
            point = hit.point + Vector3.up * this._footHeightCorrection,
            normal = hit.normal
        };
    }

    private Ray CreateFootRay(Vector3 pos, Quaternion rotation, Vector3 direction) {
        return new Ray(pos + rotation * this._footRaycastDisplacement, direction);
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
