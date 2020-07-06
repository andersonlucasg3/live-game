using System;
using UnityEngine;

[Serializable]
class WalkController : InputController.IMovementListener {
    private Transform _camera;
    private Animator _animator;
    private AcceleratedVector2 _movementVector;
    private AcceleratedVector3 _directionVector;

    [SerializeField] private float _movementAcceleration = 5F;

    public Action<bool> isMovingSetter { private get; set; }

    public void Configure(Animator animator, Transform camera) {
        this._animator = animator;
        this._camera = camera;

        this._directionVector.acceleration = this._movementAcceleration;
        this._movementVector.acceleration = this._movementAcceleration;
    }

    public void Update() {
        this._movementVector.Update();
        this._directionVector.Update();

        this.LookToCameraDirection();

        bool isMoving = this._directionVector.target != Vector3.zero;
        this.isMovingSetter(isMoving);
        this._animator.SetBool(AnimationKeys.isMoving, isMoving);


    }

    private void LookToCameraDirection() {
        if (this._directionVector.current != Vector3.zero) {
            var direction = this._camera.InverseTransformDirection(this._directionVector.current);
            this._animator.transform.forward = direction;
        }
    }

    #region Inputs

    void InputController.IMovementListener.Move(Vector2 inputDirection) {
        this._directionVector.target = inputDirection.ToDirectionVector();
    }

    #endregion

    private struct AnimationKeys {
        public static readonly int verticalProperty = Animator.StringToHash("vertical");
        public static readonly int horizontalProperty = Animator.StringToHash("horizontal");
        public static readonly int isMoving = Animator.StringToHash("isMoving");
    }
}