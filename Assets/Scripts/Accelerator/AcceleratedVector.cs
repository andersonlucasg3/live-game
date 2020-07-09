using System;
using UnityEngine;

[Serializable]
struct AcceleratedValue {
    [SerializeField] private float _target;
    [SerializeField] private float _current;

    public float acceleration { get; set; }
    public float target { get => this._target; set => this._target = value; }
    public float current => this._current;

    public static readonly AcceleratedValue zero = new AcceleratedValue() {
        acceleration = 1F,
        _target = 0F,
        _current = 0F
    };

    public void Update() {
        this._current = Accelerate(Time.deltaTime, this._target, this._current, this.acceleration);
    }

    public void FixedUpdate() {
        this._current = Accelerate(Time.fixedDeltaTime, this._target, this._current, this.acceleration);
    }

    internal static float Accelerate(float deltaTime, float target, float current, float acceleration) {
        var delta = target - current;
        var step = Mathf.Abs(delta) * acceleration * deltaTime;
        if (target < current) { step = -step; }

        if (Mathf.Abs(step) > Mathf.Abs(delta)) { return target; }
        return current + step;
    }
}

[Serializable]
struct AcceleratedVector2 {
    private float _acceleration;
    private AcceleratedValue _x;
    private AcceleratedValue _y;

    public float acceleration {
        get => this._acceleration;
        set {
            this._acceleration = value;
            this._x.acceleration = value;
            this._y.acceleration = value;
        }
    }

    public float targetX { get => this._x.target; set => this._x.target = value; }
    public float targetY { get => this._y.target; set => this._y.target = value; }

    public Vector2 target {
        get {
            var vec = Vector2.zero;
            vec.Set(this.targetX, this.targetY);
            return vec;
        }
        set {
            this.targetX = value.x;
            this.targetY = value.y;
        }
    }

    public float x => this._x.current;
    public float y => this._y.current;

    public Vector2 current {
        get {
            var vec = Vector2.zero;
            vec.Set(this.x, this.y);
            return vec;
        }
    }

    public static readonly AcceleratedVector2 zero = new AcceleratedVector2() {
        acceleration = 1F,
        targetX = 0F,
        targetY = 0F
    };

    public void Update() {
        this._x.Update();
        this._y.Update();
    }

    public void FixedUpdate() {
        this._x.FixedUpdate();
        this._y.FixedUpdate();
    }
}

struct AcceleratedVector3 {
    private float _acceleration;
    private AcceleratedVector2 _vec2;
    private AcceleratedValue _z;

    public float acceleration {
        get => this._acceleration;
        set {
            this._acceleration = value;
            this._vec2.acceleration = value;
            this._z.acceleration = value;
        }
    }

    public float targetX { get => this._vec2.targetX; set => this._vec2.targetX = value; }
    public float targetY { get => this._vec2.targetY; set => this._vec2.targetY = value; }
    public float targetZ { get => this._z.target; set => this._z.target = value; }

    public Vector3 target {
        get {
            var vec = Vector3.zero;
            vec.Set(this.targetX, this.targetY, this.targetZ);
            return vec;
        }
        set {
            this._vec2.targetX = value.x;
            this._vec2.targetY = value.y;
            this._z.target = value.z;
        }
    }

    public float x => this._vec2.x;
    public float y => this._vec2.y;
    public float z => this._z.current;

    public Vector3 current {
        get {
            var vec = Vector3.zero;
            vec.Set(this.x, this.y, this.z);
            return vec;
        }
    }

    public static readonly AcceleratedVector3 zero = new AcceleratedVector3() {
        acceleration = 1F,
        targetX = 0F,
        targetY = 0F,
        targetZ = 0F
    };

    public void Update() {
        this._vec2.Update();
        this._z.Update();
    }

    public void FixedUpdate() {
        this._vec2.FixedUpdate();
        this._z.Update();
    }
}