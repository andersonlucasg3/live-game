using UnityEngine;

public class CharacterBehaviour : MonoBehaviour {
    private InputController _input = null;
    [SerializeField] private FootIKController _footIK = null;
    [SerializeField] private WalkController _walk = null;
    [SerializeField] private CameraBehaviour _camera = null;

    protected virtual void Awake() {
        var animator = this.GetComponent<Animator>();
        this._walk.Configure(animator);
        this._footIK.Configure(animator);
        this._walk.isMovingSetter = this._footIK.SetIsMoving;

        this._input = new InputController {
            movementListener = this._walk,
            cameraListener = this._camera
        };
        this._input.Enable();
    }

    protected virtual void Update() {
        this._walk.Update();
        this._footIK.Update();
    }

    protected virtual void OnAnimatorIK(int layerIndex) {
        this._footIK.OnAnimatorIK();
    }

    public void EnableRightFootIK() {
        this._footIK.EnableRightFoot();
    }

    public void EnableLeftFootIK() {
        this._footIK.EnableLeftFoot();
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        this._footIK.OnDrawGizmos();
    }
#endif
}