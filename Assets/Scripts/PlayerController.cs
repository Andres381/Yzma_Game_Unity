using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("VALORES CONFIGURABLES")]
    [SerializeField] private int vida = 3;
    [SerializeField] private float velocidad;
    [SerializeField] private float fuerzaSalto;
    [SerializeField] private bool saltoMejorado;
    [SerializeField] private float saltoLargo = 1.5f;
    [SerializeField] private float saltoCorto = 1f;
    [SerializeField] private Transform checkGround;
    [SerializeField] private float checkGroundRadio;
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private float addRayo;
    [SerializeField] private float anguloMax;
    [SerializeField] private PhysicsMaterial2D sinF;
    [SerializeField] private PhysicsMaterial2D maxF;
    [SerializeField] private float fuerzaToque;

    [Header("VARIABLES INFORMATIVAS")]

    [SerializeField] private bool enPendiente;
    [SerializeField] private bool TocaSuelo = false;
    [SerializeField] private bool puedoAndar;
    [SerializeField] private float anguloPendiente;

    private Rigidbody2D rPlayer;
    private Animator aPlayer;
    private CapsuleCollider2D ccPlayer;
    private SpriteRenderer sPlayer;
    private Vector2 ccSize;
    private float h;
    private Camera camara;


    private bool miraDerecha = true;
    private bool saltando = false;
    private bool puedoSaltar = false;
    private bool enPlataforma = false;
    private Vector2 nuevaVelocidad;
    private float anguloLateral;
    private float anguloAnterior;
    private Vector2 anguloPer;
    private Vector3 posIni;

    private bool tocando = false;
    private Color coloroOriginal;
    private bool muerto  = false;
    private float posPlayer, altoCam, altoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        posIni = transform.position;
        rPlayer = GetComponent<Rigidbody2D>();
        aPlayer = GetComponent<Animator>();
        ccPlayer = GetComponent<CapsuleCollider2D>();
        ccSize = ccPlayer.size;
        sPlayer = GetComponent<SpriteRenderer>();
        coloroOriginal = sPlayer.color;
        camara = Camera.main;

       
        altoCam = camara.orthographicSize * 2;
        altoPlayer = GetComponent<Renderer>().bounds.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameController.gameOn)
        {
            recibePulsaciones();
            variablesAnimador();
        }
        if (muerto)
        {
            posPlayer = camara.transform.InverseTransformDirection(transform.position - camara.transform.position).y;
            if (posPlayer < ((altoCam / 2) * -1) - (altoPlayer/2))
                {
               Invoke ("LlamaRecarga", 1);
                }
            muerto = false;
        }
        
    }

    private void LlamaRecarga()
    {
        GameController.playerMuerto = true;
    }

    void FixedUpdate()
    {
        if(GameController.gameOn)
        {
            checkTocaSuelo();
            checkPendiente();
            if (!tocando) movimientoPlayer();
        }
       
    }

    private void movimientoPlayer()
    {
        //JUGADOR EN EL SUELO
        if (TocaSuelo && !saltando && !enPendiente)
        {
            nuevaVelocidad.Set(velocidad * h, 0.0f);
            rPlayer.velocity = nuevaVelocidad;

        }//EN PENDIENTE
        else if (TocaSuelo && !saltando && puedoAndar && enPendiente)
        {
            nuevaVelocidad.Set(velocidad * anguloPer.x * -h, velocidad * anguloPer.y * -h);
            rPlayer.velocity = nuevaVelocidad;
        }
        //JUGADOR EN SALTANDO
        else
        {
            if(!TocaSuelo)
            {
                nuevaVelocidad.Set(velocidad * h, rPlayer.velocity.y);
                rPlayer.velocity = nuevaVelocidad;
            }
           
        }

    }

    private void recibePulsaciones()
    {
        if (Input.GetKey(KeyCode.R)) GameController.playerMuerto = true; /// Volver a colocar al jugador 
        h = Input.GetAxisRaw("Horizontal");
        if ((h > 0 && !miraDerecha) || h < 0 && miraDerecha) giraPlayer();
        if (Input.GetButtonDown("Jump") || puedoSaltar && TocaSuelo) Salto();
        if (saltoMejorado) SaltoMejorado();
    }

    private void Salto()
    {
        saltando = true;
        puedoSaltar = false;
        rPlayer.velocity = new Vector2(rPlayer.velocity.x, 0);
        rPlayer.AddForce(new Vector2(0, fuerzaSalto), ForceMode2D.Impulse);
    }

    private void SaltoMejorado()
    {
        if (rPlayer.velocity.y < 0)
        {
            rPlayer.velocity += Vector2.up * Physics2D.gravity.y * saltoLargo * Time.deltaTime;
        }
        else if (rPlayer.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rPlayer.velocity += Vector2.up * Physics2D.gravity.y * saltoCorto * Time.deltaTime;
        }
    }

    private void checkTocaSuelo()
    {
        TocaSuelo = Physics2D.OverlapCircle(checkGround.position, checkGroundRadio, capaSuelo);
        if (rPlayer.velocity.y <= 0f)
        {
            saltando = false;
            if (tocando && TocaSuelo)
            {
                rPlayer.velocity = Vector2.zero;
                tocando = false;
                sPlayer.color = coloroOriginal;
            }
        }
        if (TocaSuelo && !saltando)
        {
            puedoSaltar = true;
        }
    }
    //DETECCION DE PLATAFORMAS MOVILES
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "PlataformaMovil")
        {
            rPlayer.velocity = Vector3.zero;
            transform.parent = collision.transform;
            enPlataforma = true;
        }
        if (collision.gameObject.tag == "enemigoPupa" && !muerto)
        {
            tocado(collision.transform.position.x);
        }
        if (collision.gameObject.tag == "Chepaenemigo" && !tocando && !muerto)
        {
            rPlayer.velocity = Vector2.zero;
            rPlayer.AddForce(new Vector2(0.0f, 10f), ForceMode2D.Impulse);
            collision.gameObject.SendMessage("Muere");
        }
    }

    private void tocado(float posX)
    {
        if(!tocando)
        {
            if(vida > 1)
            {
                Color nuevoColor = new Color(255f / 255f, 100f / 255f, 100f / 255f);
                sPlayer.color = nuevoColor;
                tocando = true;
                float lado = Mathf.Sign(posX - transform.position.x);
                rPlayer.velocity = Vector2.zero;
                rPlayer.AddForce(new Vector2(fuerzaToque * -lado, fuerzaToque), ForceMode2D.Impulse);
                vida--;
            }
            else
            {
                muertePlayer();

            }
           
        }
       
    }

    private void muertePlayer()
    {
        aPlayer.Play("Muerto");
        GameController.gameOn = false;
        rPlayer.velocity = Vector2.zero;
        rPlayer.AddForce(new Vector2(0.0f, fuerzaSalto), ForceMode2D.Impulse);
        ccPlayer.enabled = false;
        muerto = true;
    }

    //FIN DE DETECCION DE PLATAFORMAS MOVILES
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "PlataformaMovil" && !muerto)
        {
            transform.parent = null;
            enPlataforma = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pinchos" && !muerto)
        {
            Debug.Log("Quita salud");
            muertePlayer();
  
        }
        if(collision.gameObject.tag == "CaidaAlVacio")
        {
            //Debug.Log("Muerte por caida al vacio");
            GameController.playerMuerto = true;

        }
    }
    
   

    private void checkPendiente()
    {
        if (!enPendiente)
        {
            Vector2 posPies = transform.position - (Vector3)(new Vector2(0.0f, ccSize.y / 2));
            checkePenHoriz(posPies);
            checkePendVerti(posPies);
        }

    }

    private void checkePenHoriz(Vector2 posPies)
    {
        RaycastHit2D hitDelante = Physics2D.Raycast(posPies, Vector2.right, addRayo, capaSuelo);
        RaycastHit2D hitDetras = Physics2D.Raycast(posPies, -Vector2.right, addRayo, capaSuelo);
        Debug.DrawRay(posPies, Vector2.right * addRayo, Color.cyan);
        Debug.DrawRay(posPies, -Vector2.right * addRayo, Color.red);
        if (hitDelante)
        {
            anguloLateral = Vector2.Angle(hitDelante.normal, Vector2.up);
           
        }
        else if (hitDetras)
        {
            anguloLateral = Vector2.Angle(hitDetras.normal, Vector2.up);
            if (anguloLateral > 0) enPendiente = true;
        }
        else
        {
            enPendiente = false;
            anguloLateral = 0.0f;
        }
    }
    private void checkePendVerti(Vector2 posPies)
    {
        RaycastHit2D hit = Physics2D.Raycast(posPies, Vector2.right, addRayo, capaSuelo);
        if (hit)
        {
            anguloPendiente = Vector2.Angle(hit.normal, Vector2.up);
            anguloPer = Vector2.Perpendicular(hit.normal).normalized;
            if (anguloPendiente != anguloAnterior && anguloPendiente > 0) enPendiente = true;
            anguloAnterior = anguloPendiente;
            Debug.DrawRay(hit.point, anguloPer, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.blue);

        }
        if (anguloPendiente > anguloMax || anguloLateral > anguloMax)
        {
            puedoAndar = false;
        }
        else
        {
            puedoAndar = true;
        }
        if (enPendiente && puedoAndar && h == 0.0f)
        {
            rPlayer.sharedMaterial = maxF;
        }
        else
        {
            rPlayer.sharedMaterial = sinF;
        }
    }
    private void variablesAnimador()
    {
        aPlayer.SetFloat("VelocidadX", Mathf.Abs(rPlayer.velocity.x));
        aPlayer.SetFloat("VelocidadY", rPlayer.velocity.y);
        aPlayer.SetBool("Saltando", saltando);
    }

     void giraPlayer()
    {
            miraDerecha = !miraDerecha;
            Vector3 escalaGiro = transform.localScale;
            escalaGiro.x = escalaGiro.x * -1;
            transform.localScale = escalaGiro;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(checkGround.position, checkGroundRadio);
    }
}
