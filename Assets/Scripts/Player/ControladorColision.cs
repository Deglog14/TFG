using UnityEngine;

[RequireComponent(typeof(PlayerScript))] // Asegura que hay un PlayerScript
public class ControladorColision : MonoBehaviour
{
    [SerializeField] private int volando;
    private void OnCollisionEnter(Collision collision)
    {
        // Solo reaccionamos a objetos con tag "Enemi"
        if (!collision.gameObject.CompareTag("Enemi")) return;

        float diferenciaAltura = transform.position.y - collision.transform.position.y;
        Debug.Log("LA diferencia de altura es "+diferenciaAltura);
        if (diferenciaAltura > 0.5f) // Requiere saltar 0.5 unidades arriba
        {
            Destroy(collision.gameObject);
        }
        else
        {
            // Si no, el jugador recibe daño
            GetComponent<PlayerScript>().PlayerVida.RecibirDaño(1);
            GetComponent<Rigidbody>().AddForce(collision.contacts[0].normal * volando, ForceMode.Impulse);            
        }
    }
}

