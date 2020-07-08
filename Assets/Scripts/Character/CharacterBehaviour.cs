using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterBehaviour : MonoBehaviour {
    private Animator _animator = null;
    private InputController _input = null;
    private CharacterController _characterController = null;
    [SerializeField] private FootIKController _footIK = null;
    [SerializeField] private WalkController _walk = null;
    [SerializeField] private CameraBehaviour _camera = null;

    protected virtual void Awake() {
        this._animator = this.GetComponent<Animator>();
        this._characterController = this.GetComponent<CharacterController>();
        this._walk.Configure(this._animator, this._camera.transform);
        this._walk.isMovingSetter = this._footIK.SetIsMoving;

        this._footIK.Configure(this._animator);
        this._footIK.isEnabled = true;

        this._input = new InputController();
        this._input.AddListener(this._walk);
        this._input.AddListener(this._camera);
        this._input.Enable();

        this._walk.Configure(this._input);
    }

    protected virtual void Update() {
        this._walk.Update();
        this._footIK.Update();
    }

    private void OnAnimatorMove() {
        this._characterController.Move(this._animator.velocity * Time.deltaTime);
        if (this._animator.deltaRotation == Quaternion.identity) { return; }
        this.transform.rotation *= this._animator.deltaRotation;
    }

    protected virtual void OnAnimatorIK(int layerIndex) {
        this._footIK.OnAnimatorIK();
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        this._footIK.OnDrawGizmos();
        this._walk.OnDrawGizmos();
    }
#endif
}