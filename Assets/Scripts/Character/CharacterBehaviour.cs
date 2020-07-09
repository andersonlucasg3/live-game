using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterBehaviour : MonoBehaviour {
    private Animator _animator = null;
    private InputController _input = null;

    [SerializeField] private FootIKController _footIK = null;
    [SerializeField] private WalkController _walk = null;
    [SerializeField] private CameraBehaviour _camera = null;

    protected virtual void Awake() {
        this._animator = this.GetComponent<Animator>();
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

    protected virtual void FixedUpdate() {
        this._walk.FixedUpdate();
        this._camera.targetPosition = this.transform.position;
    }

    protected virtual void Update() {
        this._footIK.Update();
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