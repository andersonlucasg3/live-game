using UnityEngine;

struct AcceleratedValue {
    public float acceleration { get; set; }
    public float target { get; set; }
    public float current { get; private set; }

    public static readonly AcceleratedValue zero = new AcceleratedValue();

    public void Update() {
        this.current = Accelerate(Time.deltaTime, this.target, this.current, this.acceleration);
    }

    public void FixedUpdate() {
        this.current = Accelerate(Time.fixedDeltaTime, this.target, this.current, this.acceleration);
    }

    internal static float Accelerate(float deltaTime, float target, float current, float acceleration) {
        var delta = target - current;
        var step = Mathf.Abs(delta) * acceleration * deltaTime;
        if (target < current) { step = -step; }

        if (Mathf.Abs(step) > Mathf.Abs(delta)) { return target; }
        return current + step;
    }
}

struct AcceleratedVector2 {
    public float acceleration { get; set; }
    public Vector2 target { get; set; }
    public Vector2 current { get; private set; }

    public static readonly AcceleratedVector2 zero = new AcceleratedVector2();

    public void Update() {
        this.Accelerate(Time.deltaTime);
    }

    public void FixedUpdate() {
        this.Accelerate(Time.fixedDeltaTime);
    }

    private void Accelerate(float time) {
        var current = this.current;
        current.x = AcceleratedValue.Accelerate(time, this.target.x, current.x, this.acceleration);
        current.y = AcceleratedValue.Accelerate(time, this.target.y, current.y, this.acceleration);
        this.current = current;
    }
}

struct AcceleratedVector3 {
    public float acceleration { get; set; }
    public Vector3 target { get; set; }
    public Vector3 current { get; private set; }

    public static readonly AcceleratedVector3 zero = new AcceleratedVector3();

    public void Update() {
        this.Accelerate(Time.deltaTime);
    }

    public void FixedUpdate() {
        this.Accelerate(Time.fixedDeltaTime);
    }

    private void Accelerate(float time) {
        var current = this.current;
        current.x = AcceleratedValue.Accelerate(time, this.target.x, current.x, this.acceleration);
        current.y = AcceleratedValue.Accelerate(time, this.target.y, current.y, this.acceleration);
        current.z = AcceleratedValue.Accelerate(time, this.target.z, current.z, this.acceleration);
        this.current = current;
    }
}