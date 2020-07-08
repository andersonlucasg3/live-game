using UnityEngine;

public class CharacterBehaviour : MonoBehaviour {
    private InputController _input = null;
    [SerializeField] private FootIKController _footIK = null;
    [SerializeField] private WalkController _walk = null;
    [SerializeField] private CameraBehaviour _camera = null;

    protected virtual void Awake() {
        var animator = this.GetComponent<Animator>();
        this._walk.Configure(animator, this._camera.transform);
        this._walk.isMovingSetter = this._footIK.SetIsMoving;

        this._footIK.Configure(animator);
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