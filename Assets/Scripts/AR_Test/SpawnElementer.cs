using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// 1) Waits for AR to find a plane and spawns the Elementer there (pops in).
/// 2) Flies it in an arc to the middle of the student's screen so it can't be missed.
/// 3) Keeps it floating in front of the student: when they look away it glides back to the centre.
/// </summary>
public class SpawnElementer : MonoBehaviour
{
    [Header("References")]
    public ARPlaneManager planeManager;
    public GameObject elementer;

    [Header("Placement in front of the student")]
    [Tooltip("Meters in front of the camera.")]
    public float distanceFromCamera = 0.8f;
    [Tooltip("Meters above (+) or below (-) the exact centre of the screen.")]
    public float heightOffset = 0f;

    [Header("Fly-in animation")]
    public float flyInDuration = 1.4f;
    [Tooltip("How high the flight arcs, in meters.")]
    public float arcHeight = 0.3f;
    public bool spinWhileFlying = true;

    [Header("Following")]
    public bool followCamera = true;
    [Tooltip("Degrees away from the screen centre before it glides back. 0 = always stays dead centre.")]
    public float deadZoneAngle = 25f;
    [Tooltip("Lower = snappier, higher = floatier.")]
    public float followSmoothTime = 0.35f;
    public float rotationSpeed = 6f;
    public float bobAmplitude = 0.015f;
    public float bobSpeed = 2f;

    [Header("Model orientation")]
    [Tooltip("Your old code used (0, 0, -90). If you see the back of the Elementer, try (0, 180, -90).")]
    public Vector3 rotationOffset = new Vector3(0f, 0f, -90f);

    private enum State { WaitingForPlane, FlyingIn, Following, Stopped }
    private State state = State.WaitingForPlane;

    private Transform spawned;
    private Vector3 baseScale;
    private Vector3 startPos;
    private Quaternion startRot;
    private float flyTimer;

    private Vector3 followPos;
    private Vector3 followVel;
    private bool recentering;

    private Camera arCam;

    private Camera GetCam()
    {
        if (arCam == null)
        {
            XROrigin origin = FindAnyObjectByType<XROrigin>();
            arCam = (origin != null && origin.Camera != null) ? origin.Camera : Camera.main;
        }
        return arCam;
    }

    void Update()
    {
        if (state == State.WaitingForPlane) TrySpawn();
    }

    // LateUpdate so we move AFTER the AR camera has updated this frame (no jitter)
    void LateUpdate()
    {
        if (state == State.FlyingIn) UpdateFlyIn();
        else if (state == State.Following) UpdateFollow();
    }

    // ---------- spawn ----------

    private void TrySpawn()
    {
        if (planeManager == null || planeManager.trackables.count == 0) return;
        if (GetCam() == null) return;

        foreach (var plane in planeManager.trackables)
        {
            GameObject go = Instantiate(elementer, plane.transform.position, Quaternion.Euler(rotationOffset));
            SceneManager.MoveGameObjectToScene(go, gameObject.scene);

            spawned = go.transform;
            baseScale = spawned.localScale;
            startPos = spawned.position;
            startRot = spawned.rotation;
            spawned.localScale = Vector3.zero; // pops in during the flight

            flyTimer = 0f;
            state = State.FlyingIn;
            break;
        }
    }

    // ---------- fly in ----------

    private void UpdateFlyIn()
    {
        if (spawned == null) { state = State.Stopped; return; }
        Camera cam = GetCam();

        flyTimer += Time.deltaTime;
        float t = Mathf.Clamp01(flyTimer / flyInDuration);
        float eased = Mathf.SmoothStep(0f, 1f, t);

        // the target is recalculated every frame, so it still lands in front of you if you turn
        Vector3 target = GetFrontPosition(cam);

        Vector3 pos = Vector3.Lerp(startPos, target, eased);
        pos += Vector3.up * (Mathf.Sin(t * Mathf.PI) * arcHeight);

        Quaternion rot = Quaternion.Slerp(startRot, GetFacingRotation(cam, target), eased);
        if (spinWhileFlying) rot = Quaternion.AngleAxis(360f * eased, Vector3.up) * rot; // ends on a full turn

        spawned.SetPositionAndRotation(pos, rot);
        spawned.localScale = baseScale * EaseOutBack(Mathf.Clamp01(t / 0.35f));

        if (t >= 1f)
        {
            spawned.localScale = baseScale;
            followPos = target;
            followVel = Vector3.zero;
            recentering = false;
            state = followCamera ? State.Following : State.Stopped;
        }
    }

    // ---------- follow ----------

    private void UpdateFollow()
    {
        if (spawned == null) { state = State.Stopped; return; }
        Camera cam = GetCam();
        Transform camT = cam.transform;

        Vector3 target = GetFrontPosition(cam);

        if (deadZoneAngle <= 0f)
        {
            recentering = true; // always glued to the screen centre
        }
        else
        {
            float angle = Vector3.Angle(camT.forward, followPos - camT.position);
            float distError = Mathf.Abs(Vector3.Distance(followPos, camT.position) - distanceFromCamera);

            if (angle > deadZoneAngle || distError > 0.5f) recentering = true;
            else if (Vector3.Distance(followPos, target) < 0.03f) recentering = false;
        }

        Vector3 goal = recentering ? target : followPos; // not recentering = ease to a stop
        followPos = Vector3.SmoothDamp(followPos, goal, ref followVel, followSmoothTime);

        Vector3 pos = followPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
        Quaternion face = GetFacingRotation(cam, followPos);
        Quaternion rot = Quaternion.Slerp(spawned.rotation, face, 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime));

        spawned.SetPositionAndRotation(pos, rot);
    }

    // call these from your own code if needed (e.g. when the student presses the Elementer)
    public void StopFollowing() { if (state == State.Following) state = State.Stopped; }

    public void ResumeFollowing()
    {
        if (spawned != null && state == State.Stopped)
        {
            followPos = spawned.position;
            followVel = Vector3.zero;
            state = State.Following;
        }
    }

    // ---------- helpers ----------

    private Vector3 GetFrontPosition(Camera cam)
    {
        Transform c = cam.transform;
        return c.position + c.forward * distanceFromCamera + c.up * heightOffset;
    }

    private Quaternion GetFacingRotation(Camera cam, Vector3 pos)
    {
        Vector3 toCam = cam.transform.position - pos;
        if (toCam.sqrMagnitude < 0.0001f) toCam = -cam.transform.forward;
        toCam.Normalize();

        // keep it upright, unless the camera looks almost straight up/down
        Vector3 up = Mathf.Abs(Vector3.Dot(toCam, Vector3.up)) > 0.95f ? cam.transform.up : Vector3.up;

        return Quaternion.LookRotation(toCam, up) * Quaternion.Euler(rotationOffset);
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}