using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaPlayer1 : MonoBehaviour
{
    // Carga componentes y objetos
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;
    [SerializeField] GameController gameController;
    [SerializeField] BallController ball;

    // Inicializa controlador
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    // Por ahora sin utilizar
    void Update()
    {

    }

    // Si la Bola entra en su meta suma puntos y lanza sonido y resetea velocidad
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            audioSource.PlayOneShot(audioClip);
            gameController.Player2Scored();
            ball.rb.linearVelocity = new Vector2(0, 0);
        }
    }
}
