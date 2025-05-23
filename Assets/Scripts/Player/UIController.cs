using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text textoVidas;
    [SerializeField] private TMP_Text textoCacahuetes;
    [SerializeField] private TMP_Text textoLLaves;


    [SerializeField] private Image[] corazonImagenes; // Arreglo de imágenes de corazones
    [SerializeField] private Sprite corazonLLenos;
    [SerializeField] private Sprite corazonVacio;
    
    [Header("Referencia")]
    [SerializeField] PlayerScript playerScript;
    private PlayerVida playerVida=>playerScript.PlayerVida;
    private PlayerCollect playerCollect =>playerScript.PlayerCollect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerVida.MeCambiaVida += ActualizarTextoVida;
        playerVida.MeHacenDaño += ActualizarCorazones;
        playerVida.MeCuro += ActualizarCorazones;
        playerCollect.MeCambiaCacahuete += ActualizarTextoCacahuete;
        playerCollect.MeCambiaLLave += ActualizarTextoLLave;
        ActualizarTextoVida(playerVida.ActualVidas);//seteamos las vidas  a las actuales
        ActualizarTextoCacahuete(playerCollect.ActualCacahuetes);//seteamos las cacahutes  a las actuales
        ActualizarTextoLLave(playerCollect.ActualLLaves);//seteamos las cacahutes  a las actuales

        ActualizarCorazones();
    }

    private void ActualizarTextoVida(int vida)
    {
        textoVidas.text = vida.ToString();
    }
    private void ActualizarTextoLLave(int llave)
    {
        textoLLaves.text = llave.ToString();
    }

    private void ActualizarTextoCacahuete(int cacahuete)
    {
        textoCacahuetes.text = cacahuete.ToString();
    }
    private void ActualizarCorazones()
    {
        for (int i=0; i < corazonImagenes.Length;i++)
        {
            // Activa/desactiva imágenes según corazones máximos
            corazonImagenes[i].gameObject.SetActive(i < playerVida.MaxCorazones);

            // Cambia el sprite según si tiene vida o no
            corazonImagenes[i].sprite = (i < playerVida.ActualCorazones) ? corazonLLenos : corazonVacio; 
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
