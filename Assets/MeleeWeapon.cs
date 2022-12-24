using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public float damage = 20f;
    new PolygonCollider2D collider;

    private void Awake() {
        collider = GetComponent<PolygonCollider2D>();
    }

    private void OnDrawGizmos() {
        if (!collider) return;
        Gizmos.color = collider.enabled ? Color.yellow : Color.gray;
        DrawHitbox();
    }

    private void DrawHitbox() {
        Vector3[] points = collider.points.Select(point => transform.TransformPoint(point)).ToArray();
        for (int i = 0; i < points.Length; i++) {
            Vector3 a = points[i];
            Vector3 b = points[i + 1 < points.Length ? i + 1 : 0];
            Gizmos.DrawLine(a, b);
        }
    }
}
