using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{

    private LineRenderer lineRenderer;
    private List<RopeSegment> segments = new List<RopeSegment>();
    private float segmentLength = 0.25f;
    private int segmentCount = 35;
    private float lineWidth = 0.1f;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    void Start() {
        lineRenderer = GetComponent<LineRenderer>();

        Vector3 startPointPosition = startPoint.position;
        for (int i = 0; i < segmentCount; i++) {
            segments.Add(new RopeSegment(startPointPosition));
            startPointPosition.y -= segmentLength;
        }
    }

    void Update() {
        DrawRope();
    }

    private void FixedUpdate() {
        Simulate();
    }

    private void Simulate() {
        // SIMULATION
        Vector2 forceGravity = new Vector2(0f, -1.5f);

        for (int i = 1; i < segmentCount; i++) {
            RopeSegment firstSegment = segments[i];
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            segments[i] = firstSegment;
        }

        //CONSTRAINTS
        for (int i = 0; i < 50; i++) {
            ApplyConstraint();
        }
    }

    private void ApplyConstraint() {
        //Constrant to First Point 
        RopeSegment firstSegment = segments[0];
        firstSegment.posNow = startPoint.position;
        segments[0] = firstSegment;


        //Constrant to Second Point 
        RopeSegment endSegment = segments[segments.Count - 1];
        endSegment.posNow = endPoint.position;
        segments[segments.Count - 1] = endSegment;

        for (int i = 0; i < segmentCount - 1; i++) {
            RopeSegment firstSeg = segments[i];
            RopeSegment secondSeg = segments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - segmentLength);
            Vector2 changeDir = Vector2.zero;

            if (dist > segmentLength) {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            } else if (dist < segmentLength) {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0) {
                firstSeg.posNow -= changeAmount * 0.5f;
                segments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                segments[i + 1] = secondSeg;
            } else {
                secondSeg.posNow += changeAmount;
                segments[i + 1] = secondSeg;
            }
        }
    }

    private void DrawRope() {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[segmentCount];
        for (int i = 0; i < segmentCount; i++) {
            ropePositions[i] = segments[i].posNow;
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos) {
            posNow = pos;
            posOld = pos;
        }
    }
}
