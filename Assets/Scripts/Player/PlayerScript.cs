using System;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] float velocidadmax = 10f;
    [SerializeField, Range(0f, 10f)] float altitudSalto = 2f;
    [SerializeField, Range(0f, 5)] int saltoAireMax = 0;

    [SerializeField]
    Transform playerInputSpace = default;

    [SerializeField, Range(0f, 90f)]
    float maxAnguloSuelo= 25f;

    [SerializeField, Min(0f)] float aceleracionMax = 10f;
    [SerializeField, Min(0f)] float aceleracionAireMax = 5f;
    public PlayerVida PlayerVida;
    public PlayerCollect PlayerCollect;


    float minGroundDotProduct;

    private Vector3 contactNormal;

    private int saltoFase;
    private float horizontal;
    private float vertical;
    private Rigidbody rigbody;
    private Vector3 objetivoVelocity, velocidad;
    private bool objetivoSaltar;

    private int pisaSueloContador;
    private bool pisaSuelo => pisaSueloContador > 0;

    [SerializeField]private SystemRespawn SystemRespawn;

    
    [SerializeField, Range(0f, 1f)]
    float rotationSmoothness = 0.2f; // Controla la suavidad de la rotaci�n

    void Awake()
    {
        rigbody = GetComponent<Rigidbody>();
        OnValidate();
        // Aumentar la gravedad global
        Physics.gravity = new Vector3(0f, -20f, 0f);

        PlayerVida = new PlayerVida();
        PlayerVida.MeMuero += PlayerVida_MeMuero;

        PlayerCollect = new PlayerCollect();

    }

    private void PlayerVida_MeMuero()
    {
        SystemRespawn.Respawn();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
 
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal"); // Valor entre -1 (izq) y 1 (der)
        vertical = Input.GetAxis("Vertical"); // Valor entre -1 (atr�s) y 1 (adelante)
        objetivoSaltar |= Input.GetButtonDown("Jump") ;

        if (playerInputSpace)
        {
            Vector3 forward = playerInputSpace.forward;// Direcci�n "adelante" de la c�mara
            forward.y = 0f;                             // Ignorar inclinaci�n vertical
            forward.Normalize();                        // Normalizar para mantener velocidad constante
            Vector3 right = playerInputSpace.right;     // Direcci�n "derecha" de la c�mara
            right.y = 0f;
            right.Normalize();
            objetivoVelocity = (forward * vertical + right  * horizontal) * velocidadmax;//objetivo de la velocidad
        }
        else
        {
            objetivoVelocity = new Vector3(horizontal, 0f, vertical) * velocidadmax;
        }
        
        
        //Vector3 desplazamiento = velocidad * Time.deltaTime;

        //Vector3 nuevodesplazamiento = transform.localPosition += desplazamiento;

        //transform.localPosition = nuevodesplazamiento;

    }

    private void FixedUpdate()// metodo por si va mal el jeugo te aseguras que cada x frames se ejecuta
    {
        UpdateState();// Actualiza contacto con suelo
        velocidadAjustada();// Ajusta la velocidad

        // Rotacion basada en movimiento
        if (velocidad.magnitude > 0.1f) // Solo rotar si nos estamos moviendo
        {
            // Ignorar componente Y para la rotaci�n
            Vector3 horizontalVelocity = new Vector3(velocidad.x, 0f, velocidad.z);

            // Calcular la rotaci�n objetivo
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);

            // Aplicar rotaci�n suavizada
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothness
            );
        }
        //maneja el salto
        if (objetivoSaltar)
        {
            objetivoSaltar = false;
            Jump();
        }
        //Aplica velocidad 
        rigbody.linearVelocity = velocidad;

        ClearState();  // Prepara el siguiente frame
    }

    private void UpdateState()
    {
        velocidad = rigbody.linearVelocity;//utilizamos el rigbody para movernso ahora
        if (pisaSuelo)
        {
            saltoFase = 0;//Reinicia Fase de salto no esta en el aire
            if (pisaSueloContador > 1)
            {
                contactNormal.Normalize();//Normaliza el vector de contacto para mantener magnitud unitaria
            }
        }
        else//objeto en el aire
        {
            contactNormal = Vector3.up;//normal hacia arriba
        }
    }

    private void Jump()
    {
        if (pisaSuelo || saltoFase < saltoAireMax)
        {
            saltoFase += 1;

            float velocidadSalto = Mathf.Sqrt(-2f * Physics.gravity.y * altitudSalto);

            // Cancelar cualquier velocidad vertical antes de saltar
            velocidad.y = 0f;

            velocidad += contactNormal * velocidadSalto;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxAnguloSuelo * Mathf.Deg2Rad);//calcula el angulo maximo en el que el personaje considera suelo,pendiente
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                pisaSueloContador += 1;
                contactNormal += normal;
            }
        }
    }


    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    private void velocidadAjustada()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        // velocidad ya est� alineada con los ejes proyectados.
        float currentX = Vector3.Dot(velocidad, xAxis);
        float currentZ = Vector3.Dot(velocidad, zAxis);

        // Elegimos la aceleraci�n adecuada seg�n si estamos en el suelo
        float aceleracionActual = pisaSuelo ? aceleracionMax : aceleracionAireMax;
        float maxCambioVelocidad = aceleracionActual * Time.deltaTime;

        // Ajustamos suavemente la velocidad horizontal
        float newX = Mathf.MoveTowards(velocidad.x, objetivoVelocity.x, maxCambioVelocidad);
        float newZ = Mathf.MoveTowards(velocidad.z, objetivoVelocity.z, maxCambioVelocidad);


        velocidad += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private void ClearState()
    {
        pisaSueloContador = 0;
        contactNormal = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PuntoControl"))
        {
           SystemRespawn.posPuntoControl = other.gameObject.transform.position; 
        }
        if (other.gameObject.CompareTag("Cacahuete"))
        {
            AudioController.instancia.SonidoCacahuete();//singelton
            PlayerCollect.Recoger(1, "Cacahuete");
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("LLaves"))
        {
            PlayerCollect.Recoger(1, "LLaves");
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("Muerte"))
        {
            PlayerVida.PerderVida();
        }
    }

    [ContextMenu("TestVidas")]
    public void TestVidas()
    {
        PlayerVida.PerderVida();
    }

    [ContextMenu("TestCorazones")]
    public void TestCorazones()
    {
        PlayerVida.RecibirDa�o(1);
    }

    [ContextMenu("TestCurar")]
    public void TestCurar()
    {
        PlayerVida.Curar(1);
    }

   
}
