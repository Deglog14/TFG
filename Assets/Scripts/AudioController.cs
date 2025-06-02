using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instancia;
    public AudioSource sonidoFondo;


    public AudioClip sonidoCacahuete;

    private AudioSource audioSource;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // para no destruir el objeto al cambiar de escena

            audioSource = gameObject.AddComponent<AudioSource>();

            sonidoFondo = gameObject.AddComponent<AudioSource>();
            sonidoFondo.loop = true;
            sonidoFondo.Play();
        }
        else{
            Destroy(gameObject);
        }
    }

    public void SonidoCacahuete()
    {
        if(sonidoCacahuete != null)
        {
            audioSource.PlayOneShot(sonidoCacahuete);
        }
    }
   
}
