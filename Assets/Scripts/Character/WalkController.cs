using System;
using UnityEngine;

[Serializable]
class WalkController : InputController.IMovementListener {
    private Transform _player;
    private Transform _camera;
    private Animator _animator;
    private AcceleratedVector2 _movementVector;
    private AcceleratedVector3 _playerDirectionVector;
    private AnimatorStateInfo _stateInfo;

    [SerializeField] private float _movementAcceleration = 5F;

    public Action<bool> isMovingSetter { private get; set; }

    public void Configure(Animator animator, Transform camera) {
        this._animator = animator;
        this._player = animator.transform;
        this._camera = camera;

        this._movementVector.acceleration = this._movementAcceleration;
        this._playerDirectionVector.acceleration = this._movementAcceleration;
    }

    public void Update() {
        this._movementVector.Update();
        this._playerDirectionVector.Update();

        this._stateInfo = this._animator.GetCurrentAnimatorStateInfo(0);

        bool isMoving = this._movementVector.target != Vector2.zero;
        this.isMovingSetter(isMoving);
        this._animator.SetFloat(AnimationKeys.angleProperty, this.CalculateAngle());
        this._animator.SetFloat(AnimationKeys.speedProperty, this._movementVector.current.magnitude);
        this._animator.SetFloat(AnimationKeys.directionProperty, this.DistanceFromDirection());

        this._playerDirectionVector.target = this.GetDirectionFromCamera();
    }

    private Vector3 GetDirectionFromCamera() {
        var moveDirection = this._camera.rotation * this._movementVector.current.ToDirectionVector();
        moveDirection.y = 0F;
        return moveDirection;
    }

    private float CalculateAngle() {
        var cameraDirection = this.GetDirectionFromCamera();
        var playerDirection = this._player.forward;
        var angle = Vector3.Angle(cameraDirection, playerDirection);
        var cross = Vector3.Cross(cameraDirection, playerDirection);
        if (cross.y > 0) { angle *= -1; }
        return angle;
    }

    private float DistanceFromDirection() {
        var inputDirection = this._player.forward;
        var playerDirection = this._playerDirectionVector.current;
        return Vector3.Cross(inputDirection, playerDirection).y;
    }

    private bool IsInMovementState() => this._stateInfo.fullPathHash == AnimationKeys.walkMovementTreeState;

    #region Inputs

    void InputController.IMovementListener.Move(Vector2 inputDirection) {
        this._movementVector.target = inputDirection;
    }

    #endregion

    private struct AnimationKeys {
        public static readonly int speedProperty = Animator.StringToHash("speed");
        public static readonly int directionProperty = Animator.StringToHash("direction");
        public static readonly int angleProperty = Animator.StringToHash("angle");

        public static readonly int walkMovementTreeState = Animator.StringToHash("MovementLayer.Walk Movement Tree");
    }
}