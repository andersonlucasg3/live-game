using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
class WalkController : InputController.IMovementListener {
    private Transform _player;
    private Transform _camera;
    private Animator _animator;
    private InputController _input;
    private AcceleratedVector2 _movementVector;
    private AcceleratedVector3 _playerDirectionVector;
    private AnimatorStateInfo _stateInfo;
    private float _speedMultiplier = 1F;

    [SerializeField] private float _movementAcceleration = 5F;
    [SerializeField] private float _directionAcceleration = 2.5F;
    [SerializeField] private float _movementDampSpeed = 0.05F;

    public Action<bool> isMovingSetter { private get; set; }

    public void Configure(Animator animator, Transform camera) {
        this._animator = animator;
        this._player = animator.transform;
        this._camera = camera;

        this._movementVector.acceleration = this._movementAcceleration;
        this._playerDirectionVector.acceleration = this._directionAcceleration;
    }

    public void Configure(InputController input) {
        this._input = input;
    }

    public void FixedUpdate() {
        this._playerDirectionVector.target = this.GetDirectionFromCamera();

        this._movementVector.FixedUpdate();
        this._playerDirectionVector.FixedUpdate();

        this._stateInfo = this._animator.GetCurrentAnimatorStateInfo(0);

        bool isMoving = this._movementVector.target != Vector2.zero;
        var isPivoting = this.IsPivoting();
        this.isMovingSetter(isMoving || isPivoting);
        this._animator.SetFloat(AnimationKeys.directionProperty, this.DistanceFromDirection(), this._movementDampSpeed, Time.fixedDeltaTime);

        var hasMovement = this._movementVector.target.magnitude > 0F;
        this._animator.SetBool(AnimationKeys.hasMovementProperty, hasMovement);
        this._animator.SetFloat(AnimationKeys.speedProperty, this._movementVector.current.magnitude);

        if (hasMovement) {
            this._animator.SetFloat(AnimationKeys.angleProperty, this.CalculateAngle());
        } else {
            this._animator.SetFloat(AnimationKeys.angleProperty, 0F);
        }
    }

    private Vector3 GetDirectionFromCamera() {
        if (this._movementVector.target == Vector2.zero) {
            return this._camera.forward;
        }

        var moveDirection = this._camera.rotation * this._movementVector.current.ToDirectionVector();
        moveDirection.y = 0F;
        return moveDirection;
    }

    private float CalculateAngle() {
        var cameraDirection = this._playerDirectionVector.target;
        var playerDirection = this._player.forward;
        var angle = Vector3.Angle(cameraDirection, playerDirection);
        var cross = Vector3.Cross(cameraDirection, playerDirection);
        if (cross.y > 0) { angle *= -1; }
        return angle;
    }

    private float DistanceFromDirection() {
        var inputDirection = this._player.forward;
        var playerDirection = this._playerDirectionVector.current;
        return Vector3.Cross(inputDirection, playerDirection).y * 2F;
    }

    private bool IsPivoting() {
        return (AnimationKeys.walkTurnStates.Contains(this._stateInfo.fullPathHash) ||
            AnimationKeys.idleTurnStates.Contains(this._stateInfo.fullPathHash)) &&
            1F - this._stateInfo.normalizedTime >= 0.1F;
    }

    private void UpdateMovementVector(Vector2 direction) {
        this._movementVector.target = Vector2.ClampMagnitude(direction, 1) * this._speedMultiplier;
    }

#if UNITY_EDITOR
    public void OnDrawGizmos() {
        if (!EditorApplication.isPlaying) { return; }

        var ray = new Ray(this._player.position + Vector3.up, Vector3.zero);

        void DrawLine(Color color) {
            Gizmos.color = color;
            Gizmos.DrawLine(ray.origin, ray.GetPoint(2F));
        }

        ray.direction = this._player.forward;
        DrawLine(Color.blue);

        ray.direction = this._playerDirectionVector.target;
        DrawLine(Color.yellow);
    }
#endif

    #region Inputs

    void InputController.IMovementListener.Move(Vector2 inputDirection) {
        this.UpdateMovementVector(inputDirection);
    }

    void InputController.IMovementListener.Run(bool running) {
        this._speedMultiplier = running ? 2F : 1F;
        this.UpdateMovementVector(this._input.directionVector);
    }

    #endregion

    private struct AnimationKeys {
        public static readonly int speedProperty = Animator.StringToHash("speed");
        public static readonly int directionProperty = Animator.StringToHash("direction");
        public static readonly int angleProperty = Animator.StringToHash("angle");
        public static readonly int hasMovementProperty = Animator.StringToHash("hasMovement");

        public static readonly int idleTurnLeft45State = Animator.StringToHash("MovementLayer.Idle Turn Left 45");
        public static readonly int idleTurnLeft90State = Animator.StringToHash("MovementLayer.Idle Turn Left 90");
        public static readonly int idleTurnLeft180State = Animator.StringToHash("MovementLayer.Idle Turn Left 180");
        public static readonly int idleTurnRight45State = Animator.StringToHash("MovementLayer.Idle Turn Right 45");
        public static readonly int idleTurnRight90State = Animator.StringToHash("MovementLayer.Idle Turn Right 90");
        public static readonly int idleTurnRight180State = Animator.StringToHash("MovementLayer.Idle Turn Right 180");

        public static readonly int walkTurnLeft90State = Animator.StringToHash("MovementLayer.Walk Turn Left 90");
        public static readonly int walkTurnLeft180State = Animator.StringToHash("MovementLayer.Walk Turn Left 180");
        public static readonly int walkTurnRight90State = Animator.StringToHash("MovementLayer.Walk Turn Right 90");
        public static readonly int walkTurnRight180State = Animator.StringToHash("MovementLayer.Walk Turn Right 180");

        public static readonly int[] idleTurnStates = new int[] {
            idleTurnLeft45State,
            idleTurnLeft90State,
            idleTurnLeft180State,
            idleTurnRight45State,
            idleTurnRight90State,
            idleTurnRight180State
        };

        public static readonly int[] walkTurnStates = new int[] {
            walkTurnLeft90State,
            walkTurnLeft180State,
            walkTurnRight90State,
            walkTurnRight180State
        };
    }
}