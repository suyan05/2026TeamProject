using System.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public bool isRotating { get; private set; } = false;   // 회전 중인지에 대한 여부
    public bool isMoving { get; private set; } = false;     // 움직이고 있는 중인지에 대한 여부
    public bool isTrackingRotation { get; private set; } = false;   // 회전 추적 중인지에 대한 여부
    public bool isTrackingPosition { get; private set; } = false;   // 위치 추적 중인지에 대한 여부

    public static CameraMovement Instance { get; private set; } // 싱글톤용 인스턴스 변수
    public static float Threshold = 0.0005f;  // 목표에 가까워질 수 있는 한계점
    public static float cameraTrackingSpeed = 10f;   // 카메라 추적 속도 (회전 포함)
    public static float yTrackingDampening = 2.25f; // Y축 추적에 나눠질 수 (Y축은 더 낮은 추적성을 보임)
    public static bool normalizeRotation = true;    // 회전을 정규화할지 여부


    private float currentZ;     // 현재 Z값
    private float shakeRotationOffset = 0f;     // 회전 흔들림에 대한 오프셋
    private float mainFOV = 60f;    // FOV 값
    private float ExplodingFovOffset = 0;   // FOV 흔들림에 대한 오프셋
    private float trackingVelocityX = 0f;   // SmoothDamp용 속도 변수
    private float trackingVelocityY = 0f;
    private Vector3 mainPosition;   // 위치
    private Vector2 shakePositionOffset = Vector2.zero; // 위치 흔들림에 대한 오프셋
    private Vector3 mainRotation;   // 회전값
    private Transform positionTrackingTarget;   // 위치 추적 대상
    private Vector3 lastTrackingTargetPos;
    private Transform rotationTrackingTarget;   // 회전 추적 대상
    // 코루틴 저장 변수
    private Coroutine panCoroutine;
    private Coroutine zoomCoroutine;
    private Coroutine rotationCoroutine;
    private Coroutine positionShakingCoroutine;
    private Coroutine rotationShakingCoroutine;
    private Coroutine fovCoroutine;
    private Coroutine explodingFovCoroutine;

    // 컴포넌트 참조
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam != null)
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                enabled = false;
                return;
            }
            Instance = this;
            mainPosition = transform.position;
            currentZ = transform.position.z;
        }
        else
        {
            Debug.LogWarning("이 오브젝트는 카메라가 아님 : " + this.gameObject.name);
            enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (normalizeRotation && !isRotating) mainRotation = NormalizeAngles(mainRotation);
        if (positionTrackingTarget != null) lastTrackingTargetPos = positionTrackingTarget.position;
    }

    private void LateUpdate()
    {
        transform.position = mainPosition + new Vector3(shakePositionOffset.x, shakePositionOffset.y, 0f);
        transform.rotation = Quaternion.Euler(new Vector3(mainRotation.x, mainRotation.y, mainRotation.z + shakeRotationOffset));
        cam.fieldOfView = mainFOV + ExplodingFovOffset;
    }

    private Vector3 NormalizeAngles(Vector3 angles)
    {
        float x = angles.x % 360f;
        float y = angles.y % 360f;
        float z = angles.z % 360f;

        if (x < 0) x += 360f;
        if (y < 0) y += 360f;
        if (z < 0) z += 360f;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 인수 : 위치 - 기간
    /// </summary>
    public static void DollyTo(Vector2 targetPosition, float duration)
    {
        if (Instance.panCoroutine != null)
        {
            Instance.StopCoroutine(Instance.panCoroutine);
        }
        Instance.isMoving = true;
        Instance.isTrackingPosition = false;
        Instance.panCoroutine = Instance.StartCoroutine(Instance.DollyCoroutine(targetPosition, duration));
    }

    private IEnumerator DollyCoroutine(Vector2 targetPosition, float duration)
    {
        if (duration > 0)
        {
            Vector2 startPosition = mainPosition;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                Vector2 LerpPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
                mainPosition = new Vector3(LerpPosition.x, LerpPosition.y, currentZ);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        mainPosition = new Vector3(targetPosition.x, targetPosition.y, currentZ);
        isMoving = false;
        panCoroutine = null;
    }

    /// <summary>
    /// 인수 : 타겟 - 오프셋
    /// </summary>
    public static void TargetTracking(Transform targetPosition, Vector3 offset)
    {
        if (Instance.panCoroutine != null)
        {
            Instance.StopCoroutine(Instance.panCoroutine);
        }

        Instance.positionTrackingTarget = targetPosition;
        Instance.isMoving = true;
        Instance.isTrackingPosition = true;
        Instance.panCoroutine = Instance.StartCoroutine(Instance.TrackTargetCoroutine(offset));
    }

    private IEnumerator TrackTargetCoroutine(Vector3 offset)
    {
        while (true)
        {
            Vector2 targetPosition = positionTrackingTarget == null ? lastTrackingTargetPos : (Vector2)positionTrackingTarget.position + (Vector2)offset;

            float smoothTime = 1f / cameraTrackingSpeed;
            float posX = Mathf.SmoothDamp(mainPosition.x, targetPosition.x, ref trackingVelocityX, smoothTime);
            float posY = Mathf.SmoothDamp(mainPosition.y, targetPosition.y, ref trackingVelocityY, smoothTime * yTrackingDampening);

            mainPosition = new Vector3(posX, posY, currentZ);
            yield return null;
        }
    }

    /// <summary>
    /// 인수 : 각도 - 지속시간
    /// </summary>
    public static void RotateTo(Vector3 targetRotation, float duration)
    {
        if (Instance.rotationCoroutine != null)
        {
            Instance.StopCoroutine(Instance.rotationCoroutine);
        }
        Instance.isRotating = true;
        Instance.isTrackingRotation = false;
        Instance.rotationCoroutine = Instance.StartCoroutine(Instance.RotateToCoroutine(targetRotation, duration));
    }

    private IEnumerator RotateToCoroutine(Vector3 targetRotation, float duration)
    {
        if (duration > 0)
        {
            Vector3 startRotation = mainRotation;
            float elapsedTime = 0f;

            if (normalizeRotation)
            {
                while (elapsedTime < duration)
                {
                    float t = elapsedTime / duration;

                    float x = Mathf.LerpAngle(startRotation.x, targetRotation.x, t);
                    float y = Mathf.LerpAngle(startRotation.y, targetRotation.y, t);
                    float z = Mathf.LerpAngle(startRotation.z, targetRotation.z, t);

                    mainRotation = new Vector3(x, y, z);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                while (elapsedTime < duration)
                {
                    mainRotation = Vector3.Lerp(startRotation, targetRotation, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
        mainRotation = targetRotation;
        isRotating = false;
        rotationCoroutine = null;
    }

    /// <summary>
    /// 인수 : 타겟 - 오프셋
    /// </summary>
    public static void RotationTracking(Transform target, Vector3 offset)
    {
        if (Instance.rotationCoroutine != null)
        {
            Instance.StopCoroutine(Instance.rotationCoroutine);
        }
        Instance.rotationTrackingTarget = target;
        Instance.isRotating = true;
        Instance.isTrackingRotation = true;
        Instance.rotationCoroutine = Instance.StartCoroutine(Instance.RotationTrackCoroutine(offset));
    }

    private IEnumerator RotationTrackCoroutine(Vector3 offset)
    {
        while (true)
        {
            Vector3 targetDirection = rotationTrackingTarget.position - mainPosition;

            if (targetDirection == Vector3.zero)
            {
                yield return null;
                continue;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(targetDirection);
            Vector3 finalTargetEulerAngles = desiredRotation.eulerAngles + offset;

            float t = 1f - Mathf.Exp(-cameraTrackingSpeed * Time.deltaTime);

            float x = Mathf.LerpAngle(mainRotation.x, finalTargetEulerAngles.x, t);
            float y = Mathf.LerpAngle(mainRotation.y, finalTargetEulerAngles.y, t);
            float z = Mathf.LerpAngle(mainRotation.z, finalTargetEulerAngles.z, t);

            mainRotation = new Vector3(x, y, z);
            yield return null;
        }
    }

    /// <summary>
    /// 인수 : 위치
    /// </summary>
    public static void LookAtPositionTracking(Vector3 targetPosition)
    {
        if (Instance.rotationCoroutine != null)
        {
            Instance.StopCoroutine(Instance.rotationCoroutine);
        }

        Instance.isRotating = true;
        Instance.isTrackingRotation = true;
        Instance.rotationCoroutine = Instance.StartCoroutine(Instance.LookAtPositionTrackingCoroutine(targetPosition));
    }

    private IEnumerator LookAtPositionTrackingCoroutine(Vector3 targetPosition)
    {
        while (true)
        {
            Vector3 targetDirection = targetPosition - mainPosition;

            if (targetDirection == Vector3.zero)
            {   
                yield return null;
                continue;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(targetDirection);
            Vector3 finalTargetEulerAngles = desiredRotation.eulerAngles;

            float t = 1f - Mathf.Exp(-cameraTrackingSpeed * Time.deltaTime);

            float x = Mathf.LerpAngle(mainRotation.x, finalTargetEulerAngles.x, t);
            float y = Mathf.LerpAngle(mainRotation.y, finalTargetEulerAngles.y, t);
            float z = Mathf.LerpAngle(mainRotation.z, finalTargetEulerAngles.z, t);

            mainRotation = new Vector3(x, y, z);
            yield return null;
        }
    }

    /// <summary>
    /// 인수 : Z좌표 - 지속시간
    /// </summary>
    public static void PositionZoom(float targetZ, float duration)
    {
        if (Instance.zoomCoroutine != null)
        {
            Instance.StopCoroutine(Instance.zoomCoroutine);
        }

        Instance.zoomCoroutine = Instance.StartCoroutine(Instance.CameraZoomCoroutine(-targetZ, duration));
    }

    private IEnumerator CameraZoomCoroutine(float targetZ, float duration)
    {
        if (duration > 0)
        {
            float startZ = currentZ;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                mainPosition.z = currentZ = Mathf.Lerp(startZ, targetZ, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        mainPosition.z = currentZ = targetZ;
        zoomCoroutine = null;
    }

    /// <summary>
    /// 인수 : 최대 거리 - 주기 - 지속시간
    /// </summary>
    public static void PositionShaking(float intensity, float period, float duration)
    {
        if (Instance.positionShakingCoroutine != null)
        {
            Instance.StopCoroutine(Instance.positionShakingCoroutine);
        }

        Instance.positionShakingCoroutine = Instance.StartCoroutine(Instance.PositionShakingCoroutine(intensity, period, duration));
    }

    IEnumerator PositionShakingCoroutine(float intensity, float period, float duration)
    {
        float elapsedTime = 0f; // 총 경과 시간
        float periodTimer = 0f; // 주기 내 시간

        Vector2 startPoint = Vector2.zero;
        Vector2 targetPoint = Vector2.zero;

        while (elapsedTime < duration)
        {
            if (periodTimer >= period || elapsedTime == 0f)
            {
                periodTimer = 0f;
                startPoint = shakePositionOffset;

                float currentIntensity = intensity * (1 - (elapsedTime / duration));

                // Random.insideUnitCircle.normalized : 길이가 1인 랜덤 방향 벡터
                targetPoint = Random.insideUnitCircle.normalized * currentIntensity;
            }

            elapsedTime += Time.deltaTime;
            periodTimer += Time.deltaTime;

            shakePositionOffset = Vector2.Lerp(startPoint, targetPoint, periodTimer / period);

            yield return null;
        }

        shakePositionOffset = Vector2.zero;
        positionShakingCoroutine = null;
    }

    /// <summary>
    /// 인수 : 최대 각도 - 주기 - 지속시간
    /// </summary>
    public static void RotationShaking(float intensity, float period, float duration)
    {
        if (Instance.rotationShakingCoroutine != null)
        {
            Instance.StopCoroutine(Instance.rotationShakingCoroutine);
        }

        Instance.rotationShakingCoroutine = Instance.StartCoroutine(Instance.RotationShakingCoroutine(intensity, period, duration));
    }
    IEnumerator RotationShakingCoroutine(float intensity, float period, float duration)
    {
        float elapsedTime = 0f; // 총 경과 시간
        float periodTimer = 0f; // 주기 내 시간

        float startRotation = shakeRotationOffset;
        float targetRotation = 0f;
        int sign = 1;

        while (elapsedTime < duration)
        {
            if (periodTimer >= period || elapsedTime == 0f)
            {
                periodTimer = 0f;

                startRotation = shakeRotationOffset;
                targetRotation = intensity * (1 - (elapsedTime / duration));
                targetRotation *= sign;
                sign = -sign;
            }

            elapsedTime += Time.deltaTime;
            periodTimer += Time.deltaTime;

            shakeRotationOffset = Mathf.LerpAngle(startRotation, targetRotation, periodTimer / period);

            yield return null;
        }

        float lastRotation = shakeRotationOffset;
        float returnTimer = 0f;

        while (returnTimer < period)
        {
            returnTimer += Time.deltaTime;
            shakeRotationOffset = Mathf.LerpAngle(lastRotation, 0f, returnTimer / (period / 2));

            yield return null;
        }

        shakeRotationOffset = 0f;
        rotationShakingCoroutine = null;
    }

    /// <summary>
    /// 인수 : 목표 FOV - 지속시간
    /// </summary>
    public static void SetFOV(float targetFOV, float duration)
    {
        if (Instance.fovCoroutine != null)
        {
            Instance.StopCoroutine(Instance.fovCoroutine);
        }
        Instance.fovCoroutine = Instance.StartCoroutine(Instance.SetFovCoroutine(targetFOV, duration));
    }

    private IEnumerator SetFovCoroutine(float targetFOV, float duration)
    {
        if (duration > 0)
        {
            float elapsedTime = 0f;
            float startFOV = cam.fieldOfView;
            while (elapsedTime < duration)
            {
                mainFOV = Mathf.Lerp(startFOV, targetFOV, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        mainFOV = targetFOV;
        fovCoroutine = null;
    }

    /// <summary>
    /// 인수 : 목표 FOV 오프셋 - 확장 지속시간 - 축소 지속시간
    /// </summary>
    public static void ExplodingFOV(float targetFOV, float durationIn, float durationOut)
    {
        if (Instance.explodingFovCoroutine != null)
        {
            Instance.StopCoroutine(Instance.explodingFovCoroutine);
        }
        Instance.explodingFovCoroutine = Instance.StartCoroutine(Instance.ExplodingFOVCoroutine(targetFOV, durationIn, durationOut));
    }

    private IEnumerator ExplodingFOVCoroutine(float targetFovOffset, float durationIn, float durationOut)
    {
        float elapsedTime = 0f;

        if (ExplodingFovOffset != 0.0f)
        {
            float startFOV = ExplodingFovOffset;
            float BackupTime = durationIn * 0.1f;
            durationIn -= BackupTime;

            while (elapsedTime < BackupTime)
            {
                ExplodingFovOffset = Mathf.Lerp(startFOV, 0.0f, elapsedTime / BackupTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ExplodingFovOffset = elapsedTime = 0.0f;
        }

        if (durationIn > 0)
        {
            float startFOV = ExplodingFovOffset;
            while (elapsedTime < durationIn)
            {
                ExplodingFovOffset = Mathf.Lerp(startFOV, targetFovOffset, elapsedTime / durationIn);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            elapsedTime = 0.0f;
        }
        ExplodingFovOffset = targetFovOffset;

        if (durationOut > 0)
        {
            float startFOV = ExplodingFovOffset;
            while (elapsedTime < durationOut)
            {
                ExplodingFovOffset = Mathf.Lerp(startFOV, 0.0f, elapsedTime / durationOut);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        ExplodingFovOffset = 0.0f;
        explodingFovCoroutine = null;
    }

    public static void AllStop()
    {
        if (Instance == null) return;

        Instance.StopAllCoroutines();

        // 코루틴 참조
        Instance.panCoroutine = null;
        Instance.zoomCoroutine = null;
        Instance.rotationCoroutine = null;
        Instance.positionShakingCoroutine = null;
        Instance.rotationShakingCoroutine = null;
        Instance.fovCoroutine = null;
        Instance.explodingFovCoroutine = null;

        // 상태 변수
        Instance.isRotating = false;
        Instance.isMoving = false;
        Instance.isTrackingRotation = false;
        Instance.isTrackingPosition = false;
    }
}