using System;
using UnityEngine;

[Serializable]
class WalkController : InputController.IMovementListener {
    private Animator _animator;
    private AcceleratedVector2 _movementVector;

    [SerializeField] private float _movementAcceleration = 5F;

    public Action<bool> isMovingSetter { private get; set; }

    public void Configure(Animator animator) {
        this._animator = animator;

        this._movementVector.acceleration = this._movementAcceleration;
    }

    public void Update() {
        this._movementVector.Update();

        bool isMoving = this._movementVector.target != Vector2.zero;

        this.isMovingSetter(isMoving);
        this._animator.SetBool(AnimationKeys.isMoving, isMoving);
        this._animator.SetFloat(AnimationKeys.verticalProperty, this._movementVector.current.y);
        this._animator.SetFloat(AnimationKeys.horizontalProperty, this._movementVector.current.x);
    }

    #region Inputs

    void InputController.IMovementListener.Move(Vector2 inputDirection)
        => this._movementVector.target = inputDirection;

    #endregion

    private struct AnimationKeys {
        public static readonly int verticalProperty = Animator.StringToHash("vertical");
        public static readonly int horizontalProperty = Animator.StringToHash("horizontal");
        public static readonly int isMoving = Animator.StringToHash("isMoving");
    }
}