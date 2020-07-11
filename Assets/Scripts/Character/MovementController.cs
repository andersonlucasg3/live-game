using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
class MovementController : InputController.IMovementListener {
    private CharacterController _player;
    private CameraBehaviour _camera;
    private Animator _animator;
    private InputController _input;
    private AcceleratedVector2 _movementVector;
    private AcceleratedVector3 _playerDirectionVector;
    private float _speedMultiplier = 1F;

    private bool _hasMovement = false;
    private bool _isPivoting = false;
    private float _distanceFromDirection = 0F;
    private float _currentAngle = 0F;

    [SerializeField] private float _movementAcceleration = 5F;
    [SerializeField] private float _directionAcceleration = 2.5F;
#if UNITY_EDITOR
    [SerializeField] private bool _drawGizmos = true;
#endif

    public void Configure(Animator animator, CameraBehaviour camera, CharacterController player) {
        this._animator = animator;
        this._player = player;
        this._camera = camera;

        this._movementVector.acceleration = this._movementAcceleration;
        this._playerDirectionVector.acceleration = this._directionAcceleration;
    }

    public void Configure(InputController input) {
        this._input = input;
    }

    public void FixedUpdate() {
        this._hasMovement = this._movementVector.target.magnitude > 0F;
        this._isPivoting = this.IsPivoting(this._animator.GetCurrentAnimatorStateInfo(0));

        this._playerDirectionVector.target = this.GetDirectionFromCamera();
        this._currentAngle = this.CalculateAngle();

        if (this._isPivoting) {
            this._distanceFromDirection = 0F;
        } else {
            this._distanceFromDirection = this.DistanceFromDirection();
        }

        this._movementVector.FixedUpdate();
        this._playerDirectionVector.FixedUpdate();

        if (!this._player.isGrounded) {
            this._player.Move(Vector3.down * -Physics.gravity.y * Time.fixedDeltaTime);
        }
    }

    public void Update() {
        this._animator.SetBool(AnimationKeys.hasMovementProperty, this._hasMovement);
        this._animator.SetFloat(AnimationKeys.speedProperty, this._movementVector.current.magnitude);

        if (this._hasMovement) {
            this._animator.SetFloat(AnimationKeys.angleProperty, this._currentAngle);
            this._animator.SetFloat(AnimationKeys.directionXProperty, this._distanceFromDirection);
        } else {
            this._animator.SetFloat(AnimationKeys.angleProperty, 0F);
            this._animator.SetFloat(AnimationKeys.directionXProperty, 0F);
        }
    }

    public void OnAnimatorMove() {
        var deltaPosition = this._animator.deltaPosition;
        deltaPosition.y = 0;
        this._player.Move(deltaPosition);
        var direction = this._player.transform.forward;
        if (this._animator.velocity.magnitude > 0.5F) { direction += this._player.transform.right * this._distanceFromDirection * 0.05F; }
        this._player.transform.forward = this._animator.deltaRotation * direction;

        this._camera.targetPosition = this._player.transform.position;
    }

    private Vector3 GetDirectionFromCamera() {
        if (this._movementVector.target == Vector2.zero) { return this._camera.transform.forward; }
        var moveDirection = this._camera.transform.rotation * this._movementVector.current.ToDirectionVector();
        moveDirection.y = 0F;
        return moveDirection;
    }

    private float CalculateAngle() {
        var cameraDirection = this._playerDirectionVector.target;
        var playerDirection = this._player.transform.forward;
        var angle = Vector3.Angle(cameraDirection, playerDirection);
        var cross = Vector3.Cross(cameraDirection, playerDirection);
        if (cross.y > 0) { angle *= -1; }
        return angle;
    }

    private float DistanceFromDirection() {
        var inputDirection = this._player.transform.forward;
        var playerDirection = this._playerDirectionVector.current;
        return Vector3.Cross(inputDirection, playerDirection).y * 2F;
    }

    private bool IsPivoting(AnimatorStateInfo stateInfo) {
        return (AnimationKeys.walkTurnStates.Contains(stateInfo.fullPathHash) ||
            AnimationKeys.idleTurnStates.Contains(stateInfo.fullPathHash) ||
            AnimationKeys.runTurnStates.Contains(stateInfo.fullPathHash)) &&
            1F - stateInfo.normalizedTime >= 0.1F;
    }

    private void UpdateMovementVector(Vector2 direction)
        => this._movementVector.target = Vector2.ClampMagnitude(direction, 1) * this._speedMultiplier;

#if UNITY_EDITOR
    public void OnDrawGizmos() {
        if (!EditorApplication.isPlaying) { return; }
        if (!this._drawGizmos) { return; }

        var ray = new Ray(this._player.transform.position + Vector3.up, Vector3.zero);

        void DrawLine(Color color) {
            Gizmos.color = color;
            Gizmos.DrawLine(ray.origin, ray.GetPoint(2F));
        }

        ray.direction = this._player.transform.forward;
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
        public static readonly int directionXProperty = Animator.StringToHash("directionX");
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

        public static readonly int runTurnLeft180State = Animator.StringToHash("MovementLayer.Run Turn Left 180");
        public static readonly int runTurnRight180State = Animator.StringToHash("MovementLayer.Run Turn Right 180");

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

        public static readonly int[] runTurnStates = new int[] {
            runTurnLeft180State,
            runTurnRight180State
        };
    }
}