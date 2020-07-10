using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterBehaviour : MonoBehaviour {
    private InputController _input = null;

    [SerializeField] private FootIKController _footIK = null;
    [SerializeField] private WalkController _walk = null;
    [SerializeField] private CameraBehaviour _camera = null;

    protected virtual void Awake() {
        var animator = this.GetComponent<Animator>();
        var charController = this.GetComponent<CharacterController>();
        this._walk.Configure(animator, this._camera, charController);
        this._walk.isMovingSetter = this._footIK.SetIsMoving;

        this._footIK.Configure(animator);
        this._footIK.isEnabled = true;

        this._input = new InputController();
        this._input.AddListener(this._walk);
        this._input.AddListener(this._camera);
        this._input.Enable();

        this._walk.Configure(this._input);
    }

    protected virtual void FixedUpdate() {
        this._walk.FixedUpdate();
    }

    protected virtual void Update() {
        this._walk.Update();
        this._footIK.Update();
    }

    protected virtual void OnAnimatorMove() => this._walk.OnAnimatorMove();
    protected virtual void OnAnimatorIK(int layerIndex) => this._footIK.OnAnimatorIK();

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        this._footIK.OnDrawGizmos();
        this._walk.OnDrawGizmos();
    }
#endif
}