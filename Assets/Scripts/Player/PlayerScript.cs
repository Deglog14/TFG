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

    void Awake()
    {
        rigbody = GetComponent<Rigidbody>();
        OnValidate();
        // Aumentar la gravedad global
        Physics.gravity = new Vector3(0f, -20f, 0f);
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
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        objetivoSaltar |= Input.GetButtonDown("Jump") ;

        if (playerInputSpace)
        {
            Vector3 forward = playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = playerInputSpace.right;
            right.y = 0f;
            right.Normalize();
            objetivoVelocity = (forward * vertical + right  * horizontal) * velocidadmax;
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
        UpdateState();
        velocidadAjustada();
        

        //JUMP

        if (objetivoSaltar)
        {
            objetivoSaltar = false;
            Jump();
        }
        rigbody.linearVelocity = velocidad;

        ClearState();
    }

    private void UpdateState()
    {
        velocidad = rigbody.linearVelocity;//utilizamos el rigbody para movernso ahora
        if (pisaSuelo)
        {
            saltoFase = 0;
            if (pisaSueloContador > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
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
        minGroundDotProduct = Mathf.Cos(maxAnguloSuelo * Mathf.Deg2Rad);//calcula el angulo maximo en el que el personaje considera suelo
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

        float currentX = Vector3.Dot(velocidad, xAxis);
        float currentZ = Vector3.Dot(velocidad, zAxis);

        // Elegimos la aceleración adecuada según si estamos en el suelo
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
}
