using UnityEngine;
using UnityEngine.UI;

// Xf = Xo + Vo*t + 1/2*a*t^2


public class CustomRigidbody : MonoBehaviour
{
    [HideInInspector]
    public bool simulatePhysics = false;
    [HideInInspector]
    public bool showPredictions = true;

    [SerializeField] Transform blueTarget;

    [Header("UI")]
    [SerializeField]
    Slider forceSlider;
    [SerializeField]
    Slider effectSlider;
    [SerializeField]
    Text rotationText;      

    [Header("Arrows")]
    const int arrowsAmount = 20;
    const int pointsAmount = 40;
    [SerializeField] GameObject greyArrowPrefab;
    [SerializeField] GameObject pointsPrefab;
    [SerializeField] GameObject arrowsParent;
    [SerializeField] GameObject ballArrowsParent;

    [SerializeField] Transform greenVelocityArrow;
    [SerializeField] Transform redGravityArrow;
    [SerializeField] Transform greyStartMagnusArrow;
    [SerializeField] Transform redMagnusArrow;

    Transform[] greyArrows;
    Transform[] bluePoints;    

    Vector3 firstPos;
    Vector3 firstShootPosition;
    Vector3 firstVelocity;
    readonly Vector3 gravity = new Vector3(0,-9.8f,0);
    float timeShoot = 0f;
    float shootDuration;
    float shootStrenghtRelation = 0f;
    bool rotatingClockwise;
    readonly float ballMass = 1f;
    Vector3 currentLinearVel;
    Vector3 angularVel;
    Vector3 rotAxis;
    Vector3 magnusForce;
    Vector3 acceleration;

    void Awake()
    {
        greyArrows = new Transform[arrowsAmount];

        for (int i = 0; i < arrowsAmount; i++)
        {
            greyArrows[i] = Instantiate(greyArrowPrefab, arrowsParent.transform).transform;
        }

        bluePoints = new Transform[pointsAmount];

        for (int i = 0; i < pointsAmount; i++)
        {
            bluePoints[i] = Instantiate(pointsPrefab, arrowsParent.transform).transform;

        }
      
    }
    private void Start()
    {
        firstPos = transform.position;
        ResetArrows();
    }
    public void CalculateFirstVel()
    {
        shootDuration = Mathf.Lerp(2.5f, 0.5f, shootStrenghtRelation);

        firstShootPosition = transform.position;

        firstVelocity = blueTarget.position - firstShootPosition - (0.5f * gravity * Mathf.Pow(shootDuration, 2));
        firstVelocity /= shootDuration;
    }

    public Vector3 CalculateAcc(Vector3 _angularVel, Vector3 _currentLinearVel)
    {
        magnusForce = ComputeMagnusForce(_angularVel, _currentLinearVel);
        return (gravity + magnusForce) / ballMass;
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.I))
        {
            showPredictions = !showPredictions;
            arrowsParent.SetActive(showPredictions);
            ballArrowsParent.SetActive(showPredictions);
        }

        shootStrenghtRelation = forceSlider.value / 100;

        if (simulatePhysics)
        {           
            transform.position = CheckPos(transform.position, currentLinearVel, Time.deltaTime);
            currentLinearVel = CheckVel(currentLinearVel, acceleration, Time.deltaTime);

            acceleration = CalculateAcc(angularVel, currentLinearVel);
            timeShoot += Time.deltaTime;

            BallSpin();

            if (timeShoot <= shootDuration)
            {                
                CorrectPoints();
            }

            if (showPredictions) 
                UpdateBallArrows();
        }
        else
        {

            transform.rotation = Quaternion.identity;

            acceleration = gravity;

            CalculateFirstVel();

            angularVel = CalculateAngVel();
            CalculateRotAxis();

            currentLinearVel = firstVelocity;
            CorrectPoints();

            if (showPredictions) 
                UpdateMovementInfo();
        }
        
    }    

    public Vector3 CheckPos(Vector3 position, Vector3 velocity, float timeStep)
    {
        return position + velocity * timeStep;
    }

    public Vector3 CheckVel(Vector3 velocity, Vector3 acceleration, float timeStep)
    {
        return velocity + acceleration * timeStep;
    }

    public Vector3 GetPositionInTime(float time, Vector3 acceleration)
    {
        return firstShootPosition + (firstVelocity * time) + (0.5f * acceleration * Mathf.Pow(time, 2));
    }

    private void UpdateBallArrows()
    {
        SetGreenArrowsTransform();
        SetGravityArrowTransform();
        SetMagnusArrowTransform();
    }

    private void UpdateMovementInfo()
    {
        SetGreyArrowsTransforms();
        CorrectPoints();
        UpdateBallArrows();
        SetStartMagnusArrowTransform();
    }

    private void SetGreyArrowsTransforms()
    {
        float timeStep = shootDuration / arrowsAmount;
        float accumulatedTime = 0;
        Vector3 futurePosition = GetPositionInTime(accumulatedTime, gravity);

        for (int i = 0; i < greyArrows.Length; i++)
        {
            greyArrows[i].position = futurePosition;

            futurePosition = GetPositionInTime(accumulatedTime + timeStep, gravity);
            greyArrows[i].rotation = Quaternion.LookRotation((futurePosition - greyArrows[i].position).normalized, Vector3.up);

            accumulatedTime += timeStep;
        }
    }

    private void SetGreenArrowsTransform()
    {
        greenVelocityArrow.rotation = Quaternion.LookRotation(currentLinearVel.normalized, Vector3.up);
    }

    private void SetGravityArrowTransform()
    {
        redGravityArrow.rotation = Quaternion.LookRotation(gravity.normalized, Vector3.up);
    }

    private void ResetArrows()
    {
        arrowsParent.SetActive(showPredictions);
        ballArrowsParent.SetActive(showPredictions);

    }

    private void BallSpin()
    {
        float angleRotationPerSecond = angularVel.magnitude * Mathf.Rad2Deg;

        transform.Rotate(angularVel.normalized, angleRotationPerSecond * Time.deltaTime);

        if (!rotatingClockwise)
            angleRotationPerSecond *= -1;

        rotationText.text = "Rotation: " + angleRotationPerSecond.ToString();
    }

    private void CorrectPoints()
    {
        float timeStep = shootDuration / pointsAmount;
        Vector3 position = firstPos;
        Vector3 velocity = firstVelocity;
        Vector3 acceleration = CalculateAcc(angularVel, velocity);


        for (int i = 0; i < pointsAmount; ++i)
        {
            bluePoints[i].position = position;

            position = CheckPos(position, velocity, timeStep);
            velocity = CheckVel(velocity, acceleration, timeStep);
            acceleration = CalculateAcc(angularVel, velocity);

        }
    }

    private Vector3 CalculateAngVel()
    {
        return new Vector3(0, 2f, 0.17f) * effectSlider.value / 10000; //Valores elegidos a base de prueba y error         
    }

    private void CalculateRotAxis()
    {
        Vector3 ballToGoalTargetDir = (blueTarget.transform.position - this.transform.position).normalized;
        Vector3 ballHitToCenterDir = -Vector3.right;

        float dot = Vector3.Dot(ballToGoalTargetDir, ballHitToCenterDir);
        rotatingClockwise = Vector3.Dot(-ballHitToCenterDir, this.transform.right) >= 0;


        if (dot > 0.999f)
        {
            rotAxis = Vector3.zero;
        }
        else
        {
            rotAxis = Vector3.Cross(ballToGoalTargetDir, ballHitToCenterDir);
        }

    }

    private Vector3 ComputeMagnusForce(Vector3 angularVelocity, Vector3 instantLinearVelocity)
    {
        return Vector3.Cross(angularVelocity, instantLinearVelocity);
    }

    private void SetStartMagnusArrowTransform()
    {
        if (magnusForce.sqrMagnitude > 0.01f)
            greyStartMagnusArrow.rotation = Quaternion.LookRotation(magnusForce.normalized, Vector3.up);
    }

    private void SetMagnusArrowTransform()
    {
        if (magnusForce.sqrMagnitude > 0.01f)
            redMagnusArrow.rotation = Quaternion.LookRotation(magnusForce.normalized, Vector3.up);
    }

}