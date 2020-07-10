using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterBehaviour : MonoBehaviour {
    private InputController _input = null;

    [SerializeField] private FootIKController _footIK = null;
    [SerializeField] private MovementController _movement = null;
    [SerializeField] private CameraBehaviour _camera = null;

    protected virtual void Awake() {
        var animator = this.GetComponent<Animator>();
        var charController = this.GetComponent<CharacterController>();
        this._movement.Configure(animator, this._camera, charController);
        this._movement.isMovingSetter = this._footIK.SetIsMoving;

        this._footIK.Configure(animator);
        this._footIK.isEnabled = true;

        this._input = new InputController();
        this._input.AddListener(this._movement);
        this._input.AddListener(this._camera);
        this._input.Enable();

        this._movement.Configure(this._input);
    }

    protected virtual void FixedUpdate() {
        this._movement.FixedUpdate();
    }

    protected virtual void Update() {
        this._movement.Update();
        this._footIK.Update();
    }

    protected virtual void OnAnimatorMove() => this._movement.OnAnimatorMove();
    protected virtual void OnAnimatorIK(int layerIndex) => this._footIK.OnAnimatorIK();

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        this._footIK.OnDrawGizmos();
        this._movement.OnDrawGizmos();
    }
#endif
}