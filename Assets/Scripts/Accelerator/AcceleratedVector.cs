using System;
using UnityEngine;

struct AcceleratedValue {
    public float acceleration { get; set; }
    public float target { get; set; }
    public float current { get; private set; }

    public static readonly AcceleratedValue zero = new AcceleratedValue();

    public void Update() {
        this.Accelerate(Time.deltaTime);
    }

    public void Update(float deltaTime) {
        this.Accelerate(deltaTime);
    }

    private void Accelerate(float deltaTime) {
        var delta = this.target - this.current;
        var step = this.acceleration * deltaTime;

        this.current += delta * step;

        this.current = ValidateAndFillComponent(desired: this.target, current: this.current, step);
    }

    internal static float ValidateAndFillComponent(float desired, float current, float step) {
        if (desired > 0 && current >= desired) { return desired; }
        if (desired < 0 && current <= desired) { return desired; }
        if (desired == 0 && Mathf.Abs(current) < step) { return desired; }
        return current;
    }
}

struct AcceleratedVector2 {
    public float acceleration { get; set; }
    public Vector2 target { get; set; }
    public Vector2 current { get; private set; }

    public static readonly AcceleratedVector2 zero = new AcceleratedVector2();

    public void Update() {
        this.Accelerate();
    }

    private void Accelerate() {
        var current = this.current;

        var deltaY = this.target.y - current.y;
        var deltaX = this.target.x - current.x;
        var step = this.acceleration * Time.deltaTime;

        current.y += deltaY * step;
        current.x += deltaX * step;

        current.y = AcceleratedValue.ValidateAndFillComponent(desired: this.target.y, current: current.y, step);
        current.x = AcceleratedValue.ValidateAndFillComponent(desired: this.target.x, current: current.x, step);

        this.current = current;
    }
}

struct AcceleratedVector3 {
    public float acceleration { get; set; }
    public Vector3 target { get; set; }
    public Vector3 current { get; private set; }

    public static readonly AcceleratedVector3 zero = new AcceleratedVector3();

    public void Update() {
        this.Accelerate();
    }

    private void Accelerate() {
        var current = this.current;

        var deltaY = this.target.y - current.y;
        var deltaX = this.target.x - current.x;
        var deltaZ = this.target.z - current.z;
        var step = this.acceleration * Time.deltaTime;

        current.y += deltaY * step;
        current.x += deltaX * step;
        current.z += deltaZ * step;

        current.y = AcceleratedValue.ValidateAndFillComponent(desired: this.target.y, current: current.y, step);
        current.x = AcceleratedValue.ValidateAndFillComponent(desired: this.target.x, current: current.x, step);
        current.z = AcceleratedValue.ValidateAndFillComponent(desired: this.target.z, current: current.z, step);

        this.current = current;
    }
}