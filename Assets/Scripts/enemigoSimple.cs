using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemigoSimple : MonoBehaviour
{
    [SerializeField] private Transform[] puntosMov;
    [SerializeField] private float velocidad;
    [SerializeField] private GameObject padre;
    [SerializeField] private GameObject parte1, parte2;
    [SerializeField] private GameObject player;

    private BoxCollider2D boxCol1, boxCol2;
    private SpriteRenderer spr1, spr2;

    private float velocidadIni;
    private int i = 0;

    private Vector3 escalaIni, escalaTemp;
    private float miraDer = 1;

    // Start is called before the first frame update
    void Start()
    {
        escalaIni = transform.localScale;
        boxCol1 = parte1.GetComponent<BoxCollider2D>();
        boxCol2 = parte2.GetComponent<BoxCollider2D>();
        spr1 = parte1.GetComponent<SpriteRenderer>();
        spr2 = parte2.GetComponent<SpriteRenderer>();
        velocidadIni = velocidad;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, puntosMov[i].transform.position, velocidad * Time.deltaTime);
        if(Vector2.Distance(transform.position, puntosMov[i].transform.position) < 0.1f)
        {
            if (puntosMov[i] != puntosMov[puntosMov.Length - 1]) i++;
            else i = 0;
            miraDer = Mathf.Sign(puntosMov[i].transform.position.x - transform.position.x);
            gira(miraDer);
        }
    }

    private void FixedUpdate()
    {
        float lado = Mathf.Sign(player.transform.position.x - transform.position.x);
        if ((Mathf.Abs(transform.position.x - player.transform.position.x) < 3) && lado == miraDer)
        {
            Ataca();
        }
        else
        {
            Relaja();
        }
    }

    private void Ataca()
    {
        parte2.transform.rotation = Quaternion.Lerp(parte2.transform.rotation, Quaternion.Euler(0, 0, -45), 10 * Time.deltaTime);
        velocidad *= 1.2f;
    }
    private void Relaja()
    {
        parte2.transform.rotation = Quaternion.Lerp(parte2.transform.rotation, Quaternion.Euler(0, 0, 0), 10 * Time.deltaTime);
        velocidad = velocidadIni;
    }



    private void gira (float lado)
    {
        if (miraDer == -1)
        {
            escalaTemp = transform.localScale;
            escalaTemp.x = escalaTemp.x * -1;
        }
        else escalaTemp = escalaIni;
        transform.localScale = escalaTemp;
    }

    public void Muere()
    {
        boxCol1.enabled = false;
        boxCol2.enabled = false;
        StartCoroutine("FadeOut");
    }
    IEnumerator FadeOut()
    {
        for(float f = 1f; f >= 0; f -=0.2f )
        {
            Color c1 = spr1.material.color;
            c1.a = f;
            spr1.material.color = c1;

            Color c2 = spr2.material.color;
            c2.a = f;
            spr2.material.color = c2;

            yield return new WaitForSeconds(0.025f);
        }
        Destroy(padre);
    }
}
