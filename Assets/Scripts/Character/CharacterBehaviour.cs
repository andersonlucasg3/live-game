using UnityEngine;

public class CharacterBehaviour : MonoBehaviour {
    private InputController _input = null;
    [SerializeField] private FootIKController _footIK = null;
    [SerializeField] private WalkController _walk = null;
    [SerializeField] private CameraBehaviour _camera = null;

    protected virtual void Awake() {
        var animator = this.GetComponent<Animator>();
        this._walk.Configure(animator, this._camera.transform);
        this._footIK.Configure(animator);
        this._walk.isMovingSetter = this._footIK.SetIsMoving;

        this._input = new InputController();
        this._input.AddListener(this._walk);
        this._input.AddListener(this._camera);
        this._input.Enable();
    }

    protected virtual void Update() {
        this._walk.Update();
        this._footIK.Update();
    }

    protected virtual void OnAnimatorIK(int layerIndex) {
        this._footIK.OnAnimatorIK();
    }

    #region Animation Events

    public void EnableRightFootIK() => this._footIK.EnableRightFoot();
    public void EnableLeftFootIK() => this._footIK.EnableLeftFoot();

    #endregion

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        this._footIK.OnDrawGizmos();
    }
#endif
}