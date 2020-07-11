using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterBehaviour : MonoBehaviour {
    private InputController _input = null;

    [SerializeField] private CameraBehaviour _camera = null;
    [SerializeField] private FootIKController _footIK = null;
    [SerializeField] private MovementController _movement = null;
    [SerializeField] private ElevationController _elevation = null;

    protected virtual void Awake() {
        var animator = this.GetComponent<Animator>();
        var charController = this.GetComponent<CharacterController>();
        this._movement.Configure(animator, this._camera, charController);

        this._footIK.Configure(animator, charController);
        this._footIK.isEnabled = true;

        this._input = new InputController();
        this._input.AddListener(this._movement);
        this._input.AddListener(this._camera);
        this._input.Enable();

        this._movement.Configure(this._input);
        this._elevation.Configure(animator, charController);
    }

    protected virtual void FixedUpdate() {
        this._footIK.FixedUpdate();
        this._movement.FixedUpdate();
        //this._elevation.FixedUpdate();
    }

    protected virtual void Update() {
        this._movement.Update();
        //this._elevation.Update();
    }

    protected virtual void OnAnimatorMove() => this._movement.OnAnimatorMove();
    protected virtual void OnAnimatorIK(int layerIndex) => this._footIK.OnAnimatorIK();

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        this._movement.OnDrawGizmos();
        //this._elevation.OnDrawGizmos();
    }
#endif
}