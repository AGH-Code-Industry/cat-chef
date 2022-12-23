using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public float damage = 20f;

    private void OnDrawGizmos() {
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        Vector3[] points = collider.points.Select(point => transform.TransformPoint(point)).ToArray();
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Length; i++) {
            Vector3 a = points[i];
            Vector3 b = points[i + 1 < points.Length ? i + 1 : 0];
            Gizmos.DrawLine(a, b);
        }
    }
}
