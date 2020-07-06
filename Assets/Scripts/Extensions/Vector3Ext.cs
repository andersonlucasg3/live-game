using UnityEngine;

static class Vector3Ext {
    public static Vector3 ToDirectionVector(this Vector2 vec) {
        Vector3 vec3 = Vector3.zero;
        vec3.x = vec.x;
        vec3.z = vec.y;
        return vec3;
    }
}
