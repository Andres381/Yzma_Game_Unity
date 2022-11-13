using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camaraController : MonoBehaviour
{
    [SerializeField] private GameObject fondoLejos;
    [SerializeField] private GameObject fondoMedio;
    [SerializeField] private float velocidadScrollo;

    private Renderer fondoLejosR, fondoMedioR;
    private float iniCamX, difCamX;

    public Vector2 minCamPos, maxCamPos;
    public GameObject seguir;
    public float movSuave;

    private Vector2 velocidad;

    // Start is called before the first frame update
    void Start()
    {
        fondoLejosR = fondoLejos.GetComponent<Renderer>();
        fondoMedioR = fondoMedio.GetComponent<Renderer>();
        iniCamX = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        difCamX = iniCamX - transform.position.x;
        fondoLejosR.material.mainTextureOffset = new Vector2(difCamX * velocidadScrollo * -1, 0.0f);
        fondoMedioR.material.mainTextureOffset = new Vector2(difCamX * (velocidadScrollo * 1.5f) * -1, 0.0f);
        fondoLejos.transform.position = new Vector3(transform.position.x, fondoLejos.transform.position.y, fondoLejos.transform.position.z);
        fondoMedio.transform.position = new Vector3(transform.position.x, fondoMedio.transform.position.y, fondoMedio.transform.position.z);

        float posX = Mathf.SmoothDamp(transform.position.x, seguir.transform.position.x, ref velocidad.x, movSuave);
        float posY = Mathf.SmoothDamp(transform.position.y, seguir.transform.position.y, ref velocidad.y, movSuave);

        transform.position = new Vector3(
            Mathf.Clamp(posX, minCamPos.x, maxCamPos.x),
            Mathf.Clamp(posY, minCamPos.y, maxCamPos.y),
            transform.position.z);
    }
}

