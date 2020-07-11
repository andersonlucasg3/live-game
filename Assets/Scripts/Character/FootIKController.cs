using System;
using UnityEngine;

[Serializable]
public class FootIKController {
    private Animator _animator;
    private CharacterController _player;

    private Vector3 _leftFootIKPosition = Vector3.zero,
                    _leftFootPosition = Vector3.zero;
    private Quaternion _leftFootIKRotation = Quaternion.identity,
                       _leftFootRotation = Quaternion.identity;

    private Vector3 _rightFootIKPosition = Vector3.zero,
                    _rightFootPosition = Vector3.zero;
    private Quaternion _rightFootIKRotation = Quaternion.identity,
                       _rightFootRotation = Quaternion.identity;

    private Vector3 _currentNormal;
    
    [SerializeField] private float _footRaycastMaxDistance = 1F;
    [SerializeField] private LayerMask _raycastLayerMask = 0;

    public bool isEnabled { get; set; } = true;

    public void Configure(Animator animator, CharacterController player) {
        this._player = player;
        this._animator = animator;
    }

    public void FixedUpdate() {
        if (!this.isEnabled) { return; }

        this._currentNormal = this.RaycastFromPosition(this._player.transform.position)?.normal ?? Vector3.up;

        var rightHit = this.RaycastFromPosition(this._rightFootIKPosition);
        if (rightHit.HasValue) {
            rightHit = this.AdjustRaycastIfNeeded(rightHit.Value, this._player.transform.position);
            this.UpdateValues(rightHit.Value.point, rightHit.Value.normal, this._rightFootIKRotation,
                        out this._rightFootPosition, out this._rightFootRotation);
        } else {
            this._rightFootPosition = this._rightFootIKPosition;
            this._rightFootRotation = this._rightFootIKRotation;
        }
        var leftHit = this.RaycastFromPosition(this._leftFootIKPosition);
        if (leftHit.HasValue) {
            leftHit = this.AdjustRaycastIfNeeded(leftHit.Value, this._player.transform.position);
            UpdateValues(leftHit.Value.point, leftHit.Value.normal, this._leftFootIKRotation,
                        out this._leftFootPosition, out this._leftFootRotation);
        } else {
            this._leftFootPosition = this._leftFootIKPosition;
            this._leftFootRotation = this._leftFootIKRotation;
        }

    }

    public void OnAnimatorIK() {
        if (!this.isEnabled) { return; }

        this._rightFootIKPosition = this._animator.GetIKPosition(AvatarIKGoal.RightFoot);
        this._rightFootIKRotation = this._animator.GetIKRotation(AvatarIKGoal.RightFoot);
        this._leftFootIKPosition = this._animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        this._leftFootIKRotation = this._animator.GetIKRotation(AvatarIKGoal.LeftFoot);

        this.MoveFootToPosition(this._rightFootIKPosition, this._rightFootPosition, this._rightFootIKRotation, this._rightFootRotation, AvatarIKGoal.RightFoot);
        this.MoveFootToPosition(this._leftFootIKPosition, this._leftFootPosition, this._leftFootIKRotation, this._leftFootRotation, AvatarIKGoal.LeftFoot);
    }

    private void MoveFootToPosition(Vector3 fromPosition, Vector3 position, Quaternion fromRotation, Quaternion rotation, AvatarIKGoal foot) {
        var velocityMagnitude = this._player.velocity.normalized.magnitude;
        var time = 1 - this.Distance(fromPosition.normalized, position.normalized);

        if (velocityMagnitude == 0) {
            var weight = 1 - velocityMagnitude;

            var targetPosition = Vector3.Slerp(fromPosition, position, time);

            this._animator.SetIKPosition(foot, targetPosition);
            this._animator.SetIKPositionWeight(foot, weight);

            var targetRotation = Quaternion.Slerp(fromRotation, rotation, time);

            this._animator.SetIKRotation(foot, targetRotation);
            this._animator.SetIKRotationWeight(foot, weight);
        } else {
            var normalDirection = Quaternion.FromToRotation(Vector3.up, this._currentNormal);
            var center = this._player.transform.position;
            var targetPosition = this.RotatePointAroundPivot(fromPosition, center, normalDirection.eulerAngles);

            this._animator.SetIKPosition(foot, targetPosition);
            this._animator.SetIKPositionWeight(foot, 1F);

            var targetRotation = Quaternion.Slerp(fromRotation, rotation, time);

            this._animator.SetIKRotation(foot, targetRotation);
            this._animator.SetIKRotationWeight(foot, 1F);
        }
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

    private float Distance(Vector3 a, Vector3 b) {
        return Vector3.Distance(a, b);
    }

    private RaycastHit? RaycastFromPosition(Vector3 raycastPosition) {
        var upDisplacement = Vector3.up * 0.5F;
#if UNITY_EDITOR
        Debug.DrawLine(raycastPosition + upDisplacement, raycastPosition + upDisplacement + Vector3.down * this._footRaycastMaxDistance, Color.yellow);
#endif
        Physics.Raycast(raycastPosition + upDisplacement, Vector3.down, out RaycastHit hit, this._footRaycastMaxDistance, this._raycastLayerMask);
        return hit;
    }

    private RaycastHit AdjustRaycastIfNeeded(RaycastHit hit, Vector3 originalPosition) {
        if (Mathf.Abs(hit.point.y - originalPosition.y) > 0.2F) {
            return this.RaycastFromPosition(Vector3.Lerp(hit.point, originalPosition, 0.5F)) ?? hit;
        }
        return hit;
    }
}
