using UnityEngine;

[RequireComponent(typeof(Camera))] // Obliga a que el GameObject tenga un componente Camera
public class OrbitCamera : MonoBehaviour
{
    Camera regularCamera; // Referencia al componente Camera

    [SerializeField]
    Transform focus = default; // Objeto que la c�mara seguir� (normalmente el jugador)

    [SerializeField, Range(1f, 20f)]
    float distance = 5f; // Distancia entre la c�mara y el focus

    [SerializeField, Min(0f)]
    float focusRadius = 1f; // Radio donde comienza el seguimiento suave

    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f; // Factor de suavizado al recentrar

    Vector3 focusPoint, previousFocusPoint; // Puntos de seguimiento actual y anterior
    [SerializeField]
    Vector2 orbitAngles = new Vector2(10f, 5f); // �ngulos iniciales de rotaci�n (X,Y)

    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 90f; // Velocidad de rotaci�n manual/autom�tica

    //controlar los angulos
    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -30f, maxVerticalAngle = 60f; // L�mites verticales de la c�mara

    [SerializeField, Min(0f)]
    float alignDelay = 5f; // Tiempo antes de activar rotaci�n autom�tica

    float lastManualRotationTime; // �ltimo momento de rotaci�n manual

    //controlar la suavidad del cambio camara
    [SerializeField, Range(0f, 90f)]
    float alignSmoothRange = 45f; // Rango para suavizar rotaci�n autom�tica

    [SerializeField]
    LayerMask obstructionMask = -1; // Capas que obstruyen la vista de la c�mara

    // : Awake se ejecuta al crear el objeto, antes de Start
    void Awake()
    {
        regularCamera = GetComponent<Camera>(); // Obtiene referencia al componente Camera
        focusPoint = focus.position; // Inicializa el punto de enfoque
        transform.localRotation = Quaternion.Euler(orbitAngles); // Establece rotaci�n inicial
    }

    //  FixedUpdate se usa para f�sica y movimientos suaves
    void FixedUpdate()
    {
        UpdateFocusPoint(); // Actualiza la posici�n de seguimiento
        Quaternion lookRotation;

        // Decide si usar rotaci�n manual o autom�tica
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles(); // Limita los �ngulos a valores permitidos
            lookRotation = Quaternion.Euler(orbitAngles); // Crea rotaci�n desde �ngulos
        }
        else
        {
            lookRotation = transform.localRotation; // Mantiene la rotaci�n actual
        }

        Vector3 lookDirection = lookRotation * Vector3.forward; // Direcci�n hacia adelante
        Vector3 lookPosition = focusPoint - lookDirection * distance; // Posici�n deseada

        //  C�lculos para evitar obstrucciones
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
            rectPosition = castFrom + castDirection * hit.distance; // Ajusta por colisi�n
            lookPosition = rectPosition - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation); // Aplica posici�n/rotaci�n
    }

    // Calcula la mitad del tama�o del rect�ngulo de la c�mara para la detecci�n de colisiones
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
        previousFocusPoint = focusPoint; // Guarda posici�n anterior
        Vector3 targetPoint = focus.position; // Obtiene posici�n actual del focus

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
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t); // Interpolaci�n suave
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
            maxVerticalAngle = minVerticalAngle; // Garantiza �ngulos v�lidos
        }
    }

    //limita angulo vertical 
    void ConstrainAngles()
    {
        orbitAngles.x =
            Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle); // Limita vertical

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f; // Normaliza �ngulo horizontal
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
        const float e = 0.001f; // Umbral m�nimo de entrada
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input; // Aplica rotaci�n
            lastManualRotationTime = Time.unscaledDeltaTime; // Registra tiempo de rotaci�n
            return true;
        }
        return false;
    }

    //Controla la rotaci�n autom�tica cuando no hay input
    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false; // Espera antes de activar rotaci�n autom�tica
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
            rotationChange *= deltaAbs / alignSmoothRange; // Suaviza rotaci�n cercana
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange; // Suaviza rotaci�n opuesta
        }
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);

        return true;
    }

    // Convierte direcci�n 2D a �ngulo en grados
    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle; // Ajusta seg�n direcci�n X
    }
}
