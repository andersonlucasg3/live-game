using System;
using UnityEngine;

[Serializable]
public class ElevationController {
    private Animator _animator = null;
    private CharacterController _player = null;
    private bool _inStairs = false;
    private float _directionY = 0F;
    private AcceleratedValue _playerHeight = new AcceleratedValue { target = 1.8F };
    private AcceleratedVector3 _playerCapsuleCenter = new AcceleratedVector3 { target = Vector3.up * 0.9F };

    private Vector3? _firstHitPoint;
    private Vector3? _secondHitPoint;

    [SerializeField] private LayerMask _stairsLayer = 0;
    [SerializeField] private LayerMask _raycastLayerMask = 0;
    [SerializeField] private float _raycastMaxDistance = 1F;
    [SerializeField] private Vector3 _firstRayDisplacement = Vector3.zero;
    [SerializeField] private Vector3 _secondRayDisplacement = Vector3.zero;
    [SerializeField] private float _flatGroundHeight = 1.8F;
    [SerializeField] private Vector3 _flatGroundCenter = Vector3.up * 0.9F;
    [SerializeField] private float _slopeGroundHeight = 1.6F;
    [SerializeField] private Vector3 _slopeGroundCenter = Vector3.up * 0.8F;
#if UNITY_EDITOR
    [SerializeField] private bool _drawGizmos = true;
#endif

    public void Configure(Animator animator, CharacterController player) {
        this._animator = animator;
        this._player = player;

        this._playerHeight.acceleration = 4F; // TODO: property
        this._playerCapsuleCenter.acceleration = 4F;
    }

    public void FixedUpdate() {
        if (Physics.Raycast(this.GetRay(this._firstRayDisplacement), out RaycastHit firstHitInfo, this._raycastMaxDistance, this._raycastLayerMask) &&
            Physics.Raycast(this.GetRay(this._secondRayDisplacement), out RaycastHit secondHitInfo, this._raycastMaxDistance, this._raycastLayerMask)) {
            this._firstHitPoint = firstHitInfo.point;
            this._secondHitPoint = secondHitInfo.point;

            var firstLayer = firstHitInfo.transform.gameObject.layer;
            var secondLayer = secondHitInfo.transform.gameObject.layer;
            this._inStairs = this._stairsLayer.Contains(firstLayer) && this._stairsLayer.Contains(secondLayer);
            this._directionY = (this._animator.transform.rotation * (secondHitInfo.point - firstHitInfo.point)).y;

            if (this._inStairs && this._directionY > 0F) {
                this.SetSlopeTargets();
            } else {
                this.SetFlatGroundTargets();
            }
        } else {
            this._inStairs = false;
            this._directionY = 0F;

            this.SetFlatGroundTargets();
        }

        this._playerHeight.FixedUpdate();
        this._playerCapsuleCenter.FixedUpdate();
    }

    public void Update() {
        this._player.height = this._playerHeight.current;
        this._player.center = this._playerCapsuleCenter.current;

        this._animator.SetBool(AnimationKeys.inStairsProperty, this._inStairs);
        this._animator.SetFloat(AnimationKeys.directionYProperty, this._directionY);
    }

    private Ray GetRay(Vector3 displacement) {
        return new Ray(this._animator.transform.position + this._animator.transform.rotation * displacement, Vector3.down);
    }

    private void SetSlopeTargets() {
        this._playerHeight.target = this._slopeGroundHeight;
        this._playerCapsuleCenter.target = this._slopeGroundCenter;
    }

    private void SetFlatGroundTargets() {
        this._playerHeight.target = this._flatGroundHeight;
        this._playerCapsuleCenter.target = this._flatGroundCenter;
    }

#if UNITY_EDITOR
    public void OnDrawGizmos() {
        if (!this._drawGizmos) { return; }
        if (this._animator == null) { return; }

        var ray = this.GetRay(this._firstRayDisplacement);
        Gizmos.DrawLine(ray.origin, ray.GetPoint(this._raycastMaxDistance));
        ray = this.GetRay(this._secondRayDisplacement);
        Gizmos.DrawLine(ray.origin, ray.GetPoint(this._raycastMaxDistance));

        if (this._firstHitPoint.HasValue) { Gizmos.DrawSphere(this._firstHitPoint.Value, 0.1F); }
        if (this._secondHitPoint.HasValue) { Gizmos.DrawSphere(this._secondHitPoint.Value, 0.1F); }
    }
#endif

    private struct AnimationKeys {
        public static readonly int directionYProperty = Animator.StringToHash("directionY");
        public static readonly int inStairsProperty = Animator.StringToHash("inStairs");
    }
}
