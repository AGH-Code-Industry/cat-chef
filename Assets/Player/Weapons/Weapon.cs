using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Weapon : MonoBehaviour
{
    public float damage = 20f;
    private new PolygonCollider2D collider;
    private Transform inner;

    private void Awake() {
        collider = GetComponentInChildren<PolygonCollider2D>();
        inner = transform.GetChild(0);
    }

    private void OnDrawGizmos() {
        if (!collider) return;
        Gizmos.color = collider.enabled ? Color.yellow : Color.gray;
        DrawHitbox();
    }

    private void DrawHitbox() {
        Vector3[] points = collider.points.Select(point => inner.TransformPoint(point)).ToArray();
        for (int i = 0; i < points.Length; i++) {
            Vector3 a = points[i];
            Vector3 b = points[i + 1 < points.Length ? i + 1 : 0];
            Gizmos.DrawLine(a, b);
        }
    }

    private void OnAttackEnd() {
        transform.parent.SendMessage("OnAttackEnd");
    }
}
