using System;
using UnityEngine;

[Serializable]
public class FootIKController {
    private Animator _animator;
    private CharacterController _player;

    private Vector3 _leftFootIKPosition = Vector3.zero;
    private Vector3 _leftFootPosition = Vector3.zero;
    private Quaternion _leftFootIKRotation = Quaternion.identity;
    private Quaternion _leftFootRotation = Quaternion.identity;

    private Vector3 _rightFootIKPosition = Vector3.zero;
    private Vector3 _rightFootPosition = Vector3.zero;
    private Quaternion _rightFootIKRotation = Quaternion.identity;
    private Quaternion _rightFootRotation = Quaternion.identity;

    private AcceleratedVector3 _currentNormal = AcceleratedVector3.zero;
    private AcceleratedValue _rightFootWeight = AcceleratedValue.zero;
    private AcceleratedValue _leftFootWeight = AcceleratedValue.zero;
    
    [SerializeField] private float _footRaycastMaxDistance = 1F;
    [SerializeField] private LayerMask _raycastLayerMask = 0;
    [SerializeField] private float _footCorrectionAcceleration = 1F;
#if UNITY_EDITOR
    [SerializeField] private bool _drawGizmos = true;
#endif

    public bool isEnabled { get; set; } = true;

    public Action<Vector3> rightFootPositionSetter { get; set; }
    public Action<Vector3> leftFootPositionSetter { get; set; }

    public void Configure(Animator animator, CharacterController player) {
        this._player = player;
        this._animator = animator;

        this._rightFootWeight.acceleration = this._footCorrectionAcceleration;
        this._leftFootWeight.acceleration = this._footCorrectionAcceleration;
        this._currentNormal.acceleration = this._footCorrectionAcceleration;
    }

    public void FixedUpdate() {
        if (!this.isEnabled) { return; }

        this._currentNormal.target = this.RaycastFromPosition(this._player.transform.position)?.normal ?? Vector3.up;

        this.CalculateFoot(
            this._rightFootIKPosition,
            this._rightFootIKRotation,
            out this._rightFootPosition,
            out this._rightFootRotation,
            ref this._rightFootWeight
        );
        this.CalculateFoot(
            this._leftFootIKPosition,
            this._leftFootIKRotation,
            out this._leftFootPosition,
            out this._leftFootRotation,
            ref this._leftFootWeight
        );

        this._rightFootWeight.FixedUpdate();
        this._leftFootWeight.FixedUpdate();
        this._currentNormal.FixedUpdate();

        this.leftFootPositionSetter?.Invoke(this._leftFootPosition);
        this.rightFootPositionSetter?.Invoke(this._rightFootPosition);
    }

    public void OnAnimatorIK() {
        if (!this.isEnabled) { return; }

        this._rightFootIKPosition = this._animator.GetIKPosition(AvatarIKGoal.RightFoot);
        this._rightFootIKRotation = this._animator.GetIKRotation(AvatarIKGoal.RightFoot);
        this._leftFootIKPosition = this._animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        this._leftFootIKRotation = this._animator.GetIKRotation(AvatarIKGoal.LeftFoot);

        this.MoveFootToPosition(this._rightFootIKPosition, this._rightFootPosition, this._rightFootRotation, AvatarIKGoal.RightFoot, this._rightFootWeight.current);
        this.MoveFootToPosition(this._leftFootIKPosition, this._leftFootPosition, this._leftFootRotation, AvatarIKGoal.LeftFoot, this._leftFootWeight.current);
    }

#if UNITY_EDITOR
    public void OnDrawGizmos() {

    }
#endif

    private void CalculateFoot(Vector3 footIKPosition, Quaternion footIKRotation, out Vector3 footPosition, out Quaternion footRotation, ref AcceleratedValue footWeight) {
        var hit = this.RaycastFromPosition(footIKPosition);
        if (hit.HasValue) {
            this.UpdateValues(hit.Value.point, hit.Value.normal, footIKRotation, out footPosition, out footRotation);
            footWeight.target = 1F;
        } else {
            footPosition = footIKPosition;
            footRotation = footIKRotation;

            footWeight.target = 0F;
        }
    }

    private void MoveFootToPosition(Vector3 fromPosition, Vector3 position, Quaternion rotation, AvatarIKGoal foot, float weight) {
        var normalRotation = Quaternion.FromToRotation(Vector3.up, this._currentNormal.current);
        var center = this._player.transform.position;
        var rotatedPosition = this.RotatePointAroundPivot(fromPosition, center, normalRotation.eulerAngles);

        var time = 1F - Mathf.Min(1F, this._player.velocity.magnitude);
        var targetPosition = Vector3.Lerp(rotatedPosition, position, time);

        this._animator.SetIKPosition(foot, targetPosition);
        this._animator.SetIKPositionWeight(foot, weight);

        this._animator.SetIKRotation(foot, rotation);
        this._animator.SetIKRotationWeight(foot, weight);
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        point = dir + pivot;
        return point;
    }

    private void UpdateValues(Vector3 point, Vector3 normal, Quaternion rotation, out Vector3 outPosition, out Quaternion outRotation) {
        outRotation = Quaternion.FromToRotation(Vector3.up, normal) * rotation;
        outPosition = point + rotation * Vector3.up * 0.15F;
    }

    private RaycastHit? RaycastFromPosition(Vector3 raycastPosition) {
        var upDisplacement = Vector3.up;
#if UNITY_EDITOR
        if (this._drawGizmos) {
            Debug.DrawLine(raycastPosition + upDisplacement, raycastPosition + upDisplacement + Vector3.down * this._footRaycastMaxDistance, Color.yellow);
        }
#endif
        Physics.Raycast(raycastPosition + upDisplacement, Vector3.down, out RaycastHit hit, this._footRaycastMaxDistance, this._raycastLayerMask);
        return hit;
    }
}
