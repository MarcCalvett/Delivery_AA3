using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BodyManager : MonoBehaviour
{
    [Header("BODY")]
    [SerializeField]
    Transform bodyHead;
    [SerializeField]
    Transform bodyThorax;
    [SerializeField]
    Transform bodyAbdomen;
    [Header("FUTURE LEGS")]
    [SerializeField]
    Transform[] headFutureLegs;
    [SerializeField]
    Transform[] thoraxFutureLegs;
    [SerializeField]
    Transform[] abdomenFutureLegs;

    public float smoothness = 5.0f;
    public float headThoraxKForce = 0.1f;
    public float thoraxAbdomenKForce = 0.1f;

    float headThoraxDefaultDistance;
    float thoraxAbdomenDefaultDistance;

    Vector3 puntoObjetivo;

    float distanceThreshold = 0.015f;
    public float rotationSpeed = 1.0f;

    public bool walking = false;
    Vector3[] startPos = new Vector3[3];
    Quaternion[] startRot = new Quaternion[3];

    void Start()
    {
        headThoraxDefaultDistance = (bodyThorax.position - bodyHead.position).magnitude;
        thoraxAbdomenDefaultDistance = (bodyAbdomen.position - bodyThorax.position).magnitude;
        startPos[0] = bodyHead.localPosition;
        startRot[0] = bodyHead.localRotation;
        startPos[1] = bodyThorax.localPosition;
        startRot[1] = bodyThorax.localRotation;
        startPos[2] = bodyAbdomen.localPosition;
        startRot[2] = bodyAbdomen.localRotation;
    }

    // Update is called once per frame

    private void FixedUpdate() //Calculs per mantenir el cos unit
    {
        if (!walking)
            return;

        puntoObjetivo = bodyHead.position + bodyHead.forward * headThoraxDefaultDistance;

        // Cálculos para el primer par de objetos (bodyHead y bodyThorax)
        Vector3 direction1 = bodyThorax.position - puntoObjetivo;
        float distance1 = direction1.magnitude;

        Debug.DrawRay(bodyHead.position, puntoObjetivo - bodyHead.position, Color.green);
        Debug.DrawRay(bodyThorax.position, puntoObjetivo - bodyThorax.position, Color.yellow);

        if (distance1 > distanceThreshold)
        {
            // Calcula la fuerza del muelle
            Vector3 force1 = direction1.normalized;

            // Aplica la fuerza y ajusta la rotación del objeto2 (bodyThorax)
            bodyThorax.position -= force1 * Time.fixedDeltaTime/** 500*/;

            //bodyThorax.rotation = Quaternion.FromToRotation(bodyThorax.forward, bodyHead.forward) * bodyThorax.rotation;

            // Calcula la rotación para que el eje de adelante apunte en la dirección opuesta
            Quaternion lookRotation = Quaternion.LookRotation(-direction1, Vector3.up);

            // Aplica la rotación al objeto
            transform.rotation = lookRotation;
        }

        puntoObjetivo = bodyThorax.position + bodyThorax.forward * thoraxAbdomenDefaultDistance;

        //// Cálculos para el segundo par de objetos (bodyThorax y bodyAbdomen)
        Vector3 direction2 = bodyAbdomen.position - puntoObjetivo;
        float distance2 = direction2.magnitude;

        Debug.DrawRay(bodyThorax.position, puntoObjetivo - bodyThorax.position, Color.green);
        Debug.DrawRay(bodyAbdomen.position, puntoObjetivo - bodyAbdomen.position, Color.yellow);

        if (distance2 > distanceThreshold)
        {
            // Calcula la fuerza del muelle
            Vector3 force1 = direction2.normalized;

            // Aplica la fuerza y ajusta la rotación del objeto2 (bodyThorax)
            bodyAbdomen.position -= force1 * Time.fixedDeltaTime/** 500*/;

            //bodyAbdomen.rotation = Quaternion.FromToRotation(bodyAbdomen.forward, bodyThorax.forward) * bodyAbdomen.rotation;            

            // Calcula la rotación para que el eje de adelante apunte en la dirección opuesta
            Quaternion lookRotation = Quaternion.LookRotation(-direction2, Vector3.up);

            // Aplica la rotación al objeto
            transform.rotation = lookRotation;
        }
    }

    void Update() //Calculs per adptar parts del cos segons les potes
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            bodyHead.localPosition = startPos[0];
            bodyHead.localRotation = startRot[0];
            bodyThorax.localPosition = startPos[1];
            bodyThorax.localRotation = startRot[1];
            bodyAbdomen.localPosition = startPos[2];
            bodyAbdomen.localRotation = startRot[2];
        }

        IK_Scorpion ikScorpion = this.GetComponentInParent<IK_Scorpion>();
        // Calcula la dirección hacia el punto objetivo
        Vector3 direction = ikScorpion.points[ikScorpion.currentEnd].position - ikScorpion.transform.position;
        // Calcula la rotación objetivo
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        // Interpola suavemente la rotación del objeto hacia la rotación objetivo
        ikScorpion.transform.rotation = Quaternion.Lerp(ikScorpion.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        //this.GetComponentInParent<IK_Scorpion>().transform.LookAt(this.GetComponentInParent<IK_Scorpion>().points[this.GetComponentInParent<IK_Scorpion>().currentEnd]);
        CheckPlacements(headFutureLegs, bodyHead, 0);
        CheckPlacements(thoraxFutureLegs, bodyThorax, 1);
        CheckPlacements(abdomenFutureLegs, bodyAbdomen, 2);
    }
    void CheckPlacements(Transform[] futureLegs, Transform bodyPart, int num)
    {
        Vector3 avgPos = Vector3.zero;
        Vector3 avgNormal = Vector3.zero;

        foreach (Transform t in futureLegs)
        {
            // Obtén la posición actual del objeto
            Vector3 startPos = t.position + Vector3.up * 10f;

            // Dirección del rayo hacia abajo
            Vector3 raycastDirection = Vector3.down;

            // Longitud del rayo
            float raycastDistance = 20f;

            // Realiza el raycast
            RaycastHit hitInfo;
            if (Physics.Raycast(startPos, raycastDirection, out hitInfo, raycastDistance, LayerMask.GetMask("ground")))
            {
                t.position = hitInfo.point;
                //Debug.DrawLine(startPos, hitInfo.point, Color.red);

                avgNormal += hitInfo.normal;
                //Debug.DrawRay(hitInfo.point, hitInfo.normal * 2f, Color.blue);
            }
            else
            {
                //Debug.DrawRay(startPos, raycastDirection * raycastDistance, Color.green);
            }

            avgPos += t.position;
        }

        avgPos /= futureLegs.Length;
        avgNormal /= futureLegs.Length;

        // Ajusta la posición de la parte del cuerpo de manera lineal (Lerp)
        bodyPart.position = Vector3.Lerp(bodyPart.position, avgPos + Vector3.up * 0.6f, Time.deltaTime * smoothness);

        // Calcula la rotación para que el vector "up" sea igual a la normal promedio
        Quaternion targetRotation = Quaternion.FromToRotation(bodyPart.up, avgNormal) * bodyPart.rotation;

        // Ajusta la rotación de la parte del cuerpo de manera lineal (Lerp)
        bodyPart.rotation = Quaternion.Lerp(bodyPart.rotation, targetRotation, Time.deltaTime * smoothness);

    }

}



//public float headThoraxKForce = 0.1f;
//public float thoraxAbdomenKForce = 0.1f;

//float headThoraxDefaultDistance;
//float thoraxAbdomenDefaultDistance;

//void Start()
//{
//    headThoraxDefaultDistance = (bodyThorax.position - bodyHead.position).magnitude;
//    thoraxAbdomenDefaultDistance = (bodyAbdomen.position - bodyThorax.position).magnitude;
//}

//// Update is called once per frame

//private void FixedUpdate()
//{
//    // Cálculos para el primer par de objetos (bodyHead y bodyThorax)
//    Vector3 direction1 = bodyThorax.position - bodyHead.position;
//    float distance1 = direction1.magnitude;

//    // Calcula la fuerza del muelle
//    Vector3 force1 = headThoraxKForce * (distance1 - headThoraxDefaultDistance) * direction1.normalized;

//    // Aplica la fuerza y ajusta la rotación del objeto2 (bodyThorax)
//    bodyThorax.position -= force1 * Time.fixedDeltaTime * 500;
//    bodyThorax.rotation = Quaternion.FromToRotation(bodyThorax.forward, bodyHead.forward) * bodyThorax.rotation;

//    // Cálculos para el segundo par de objetos (bodyThorax y bodyAbdomen)
//    Vector3 direction2 = bodyAbdomen.position - bodyThorax.position;
//    float distance2 = direction2.magnitude;

//    // Calcula la fuerza del muelle
//    Vector3 force2 = thoraxAbdomenKForce * (distance2 - thoraxAbdomenDefaultDistance) * direction2.normalized;

//    // Aplica la fuerza y ajusta la rotación del objeto3 (bodyAbdomen)
//    bodyAbdomen.position -= force2 * Time.fixedDeltaTime * 20;
//    bodyAbdomen.rotation = Quaternion.FromToRotation(bodyAbdomen.forward, bodyThorax.forward) * bodyAbdomen.rotation;
//}