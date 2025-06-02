using UnityEngine;

[RequireComponent(typeof(Camera))] // Obliga a que el GameObject tenga un componente Camera
public class OrbitCamera : MonoBehaviour
{
    Camera regularCamera; // Referencia al componente Camera

    [SerializeField]
    Transform focus = default; // Objeto que la cámara seguirá (normalmente el jugador)

    [SerializeField, Range(1f, 20f)]
    float distance = 5f; // Distancia entre la cámara y el focus

    [SerializeField, Min(0f)]
    float focusRadius = 1f; // Radio donde comienza el seguimiento suave

    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f; // Factor de suavizado al recentrar

    Vector3 focusPoint, previousFocusPoint; // Puntos de seguimiento actual y anterior
    [SerializeField]
    Vector2 orbitAngles = new Vector2(10f, 5f); // Ángulos iniciales de rotación (X,Y)

    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 90f; // Velocidad de rotación manual/automática

    //controlar los angulos
    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -30f, maxVerticalAngle = 60f; // Límites verticales de la cámara

    [SerializeField, Min(0f)]
    float alignDelay = 5f; // Tiempo antes de activar rotación automática

    float lastManualRotationTime; // Último momento de rotación manual

    //controlar la suavidad del cambio camara
    [SerializeField, Range(0f, 90f)]
    float alignSmoothRange = 45f; // Rango para suavizar rotación automática

    [SerializeField]
    LayerMask obstructionMask = -1; // Capas que obstruyen la vista de la cámara

    // : Awake se ejecuta al crear el objeto, antes de Start
    void Awake()
    {
        regularCamera = GetComponent<Camera>(); // Obtiene referencia al componente Camera
        focusPoint = focus.position; // Inicializa el punto de enfoque
        transform.localRotation = Quaternion.Euler(orbitAngles); // Establece rotación inicial
    }

    //  FixedUpdate se usa para física y movimientos suaves
    void FixedUpdate()
    {
        UpdateFocusPoint(); // Actualiza la posición de seguimiento
        Quaternion lookRotation;

        // Decide si usar rotación manual o automática
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles(); // Limita los ángulos a valores permitidos
            lookRotation = Quaternion.Euler(orbitAngles); // Crea rotación desde ángulos
        }
        else
        {
            lookRotation = transform.localRotation; // Mantiene la rotación actual
        }

        Vector3 lookDirection = lookRotation * Vector3.forward; // Dirección hacia adelante
        Vector3 lookPosition = focusPoint - lookDirection * distance; // Posición deseada

        //  Cálculos para evitar obstrucciones
        Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        //PARA LOS CHOQUES DE LA CAMARA que se acerque
        if (Physics.BoxCast(
            castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
            lookRotation, castDistance, obstructionMask
        ))
        {
            rectPosition = castFrom + castDirection * hit.distance; // Ajusta por colisión
            lookPosition = rectPosition - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation); // Aplica posición/rotación
    }

    // Calcula la mitad del tamaño del rectángulo de la cámara para la detección de colisiones
    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y =
                regularCamera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    //  Actualiza el punto de enfoque con suavizado
    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint; // Guarda posición anterior
        Vector3 targetPoint = focus.position; // Obtiene posición actual del focus

        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.deltaTime);
            }
            if (distance > focusRadius)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t); // Interpolación suave
        }
        else
        {
            focusPoint = targetPoint; // Seguimiento directo sin suavizado
        }
    }

    //para la configuracion y que el max nunca puede estar por debajo
    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle; // Garantiza ángulos válidos
        }
    }

    //limita angulo vertical 
    void ConstrainAngles()
    {
        orbitAngles.x =
            Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle); // Limita vertical

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f; // Normaliza ángulo horizontal
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }

    //solo funciona con raton
    bool ManualRotation()
    {
        Vector2 input = new Vector2(
            Input.GetAxis("Vertical Camera"),
            Input.GetAxis("Horizontal Camera")
        );
        const float e = 0.001f; // Umbral mínimo de entrada
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input; // Aplica rotación
            lastManualRotationTime = Time.unscaledDeltaTime; // Registra tiempo de rotación
            return true;
        }
        return false;
    }

    //Controla la rotación automática cuando no hay input
    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false; // Espera antes de activar rotación automática
        }

        Vector2 movement = new Vector2(
            focusPoint.x - previousFocusPoint.x,
            focusPoint.z - previousFocusPoint.z
        );
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.0001f)
        {
            return false; // No rotar si no hay movimiento significativo
        }

        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        //smooth
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange; // Suaviza rotación cercana
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange; // Suaviza rotación opuesta
        }
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);

        return true;
    }

    // Convierte dirección 2D a ángulo en grados
    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle; // Ajusta según dirección X
    }
}
