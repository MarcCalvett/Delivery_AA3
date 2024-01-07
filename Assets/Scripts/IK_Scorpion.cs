using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using UnityEngine.UI;

public class IK_Scorpion : MonoBehaviour
{
    MyScorpionController _myController = new MyScorpionController();

    public IK_tentacles _myOctopus;

    [Header("Body")]
    float animTime;
    [SerializeField]
    public float[] animDurations;
    bool animPlaying = false;
    public Transform Body;
    public Transform StartPos;
    public Transform EndPos;
    [SerializeField]
    public Transform[] points;
    public int currentEnd = 1;
    [Header("Tail")]
    public Transform tailTarget;
    public Transform tail;

    [Header("Legs")]
    public Transform[] legs;
    public Transform[] legTargets;
    public Transform[] futureLegBases;

<<<<<<< HEAD
<<<<<<< HEAD
    public Slider slider;
    [SerializeField]
    float sliderChangeVelocityFactor = 3f;
    float sliderChangeVelocity;
    int sliderSignChange = 1;

    public bool ballShooted = false;
    public float startShootTime;
=======
    public Slider slider;    
>>>>>>> origin/feature/Ex2

=======
    Vector3 checkPoint;
    [SerializeField]
    BodyManager futureLegsManager;
    public bool ballShooted = false;
    //[SerializeField]
    //Transform head;    
    bool initializeLegs = true;
    public float startShootTime;
>>>>>>> origin/feature/Ex3
    // Start is called before the first frame update
    void Start()
    {
        _myController.InitLegs(legs, futureLegBases, legTargets);
        _myController.InitTail(tail);
<<<<<<< HEAD
        _myController.SaveTailState();
=======
        checkPoint = Body.position;
        NotifyTailTarget();
>>>>>>> origin/feature/Ex3
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< HEAD
        sliderChangeVelocity = slider.maxValue * sliderChangeVelocityFactor;
=======
        futureLegsManager.walking = animPlaying;        
>>>>>>> origin/feature/Ex3

        if (animPlaying)
            animTime += Time.deltaTime;

<<<<<<< HEAD
        NotifyTailTarget();
<<<<<<< HEAD

        if (Input.GetKeyUp(KeyCode.Space) && !animPlaying)
        {
            ballShooted = true;
            startShootTime = Time.time;
        }

=======
        
>>>>>>> origin/feature/Ex2
=======

>>>>>>> origin/feature/Ex3
        if (Input.GetKeyDown(KeyCode.Return))
        {
            currentEnd = 1;
            checkPoint = points[0].position;
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
            ballShooted = false;
<<<<<<< HEAD
            _myController.RestartTail();

        }

        if (!ballShooted)
        {
            if (slider.value == slider.maxValue && sliderSignChange > 0)
                sliderSignChange = -1;
            else if (slider.value == slider.minValue && sliderSignChange < 0)
                sliderSignChange = 1;
            else
                slider.value += Time.deltaTime * sliderChangeVelocity * sliderSignChange;
=======
>>>>>>> origin/feature/Ex3
        }

        if (Input.GetKeyUp(KeyCode.Space) && !animPlaying && currentEnd >= 3)
        {
            ballShooted = true;
            startShootTime = Time.time;
        }        

        if (animTime < animDurations[currentEnd - 1])
        {
            Body.position = Vector3.Lerp(checkPoint, points[currentEnd].position, animTime / animDurations[currentEnd - 1]);
        }
        else if (animTime >= animDurations[currentEnd - 1] && animPlaying)
        {
            //Body.position = points[currentEnd].position;
            if (currentEnd >= 3)
            {
                animPlaying = false;
            }
            else
            {
                animTime = 0;
                checkPoint = Body.position;
                currentEnd++;
            }
        }

        _myController.UpdateIK(ballShooted);
    }

    //Function to send the tail target transform to the dll
    public void NotifyTailTarget()
    {
        _myController.NotifyTailTarget(tailTarget);
    }

    //Trigger Function to start the walk animation
    public void NotifyStartWalk()
    {
        _myController.NotifyStartWalk(initializeLegs);
        initializeLegs = false;
    }
}
