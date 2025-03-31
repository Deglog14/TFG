using System;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] float velocidadmax = 10f;
    [SerializeField, Range(0f, 10f)] float altitudSalto = 2f;
    [SerializeField, Range(0f, 5)] int saltoAireMax = 0;

    [SerializeField]
    Transform playerInputSpace = default;

    private int saltoFase;
    private float horizontal;
    private float vertical;
    private Rigidbody rigbody;
    private Vector3 objetivoVelocity, velocidad;
    private bool objetivoSaltar;
    private bool pisaSuelo;

    void Awake()
    {
        rigbody = GetComponent<Rigidbody>();
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
        float maxVelocidadCambia = velocidadmax * Time.deltaTime;
        velocidad.x = Mathf.MoveTowards(velocidad.x, objetivoVelocity.x, maxVelocidadCambia);
        velocidad.z = Mathf.MoveTowards(velocidad.z, objetivoVelocity.z, maxVelocidadCambia);


        //JUMP

        if (objetivoSaltar)
        {
            objetivoSaltar = false;
            Jump();
        }
        rigbody.linearVelocity = velocidad;
        pisaSuelo = false;
    }

    private void UpdateState()
    {
        velocidad = rigbody.linearVelocity;//utilizamos el rigbody para movernso ahora
        if (pisaSuelo)
        {
            saltoFase = 0;
        }
    }

    private void Jump()
    {
        if (pisaSuelo|| saltoFase<saltoAireMax)//si pisa el suelo salta
        {
            saltoFase += 1;
            float velocidadSalto = (float)Math.Sqrt(-2f * Physics.gravity.y * altitudSalto);
            if (velocidad.y > 0f)
            {
                velocidadSalto = velocidadSalto - velocidad.y;
            }
            velocidad.y += velocidadSalto;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
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
            pisaSuelo |= normal.y >= 0.9f;
        }
    }
}
