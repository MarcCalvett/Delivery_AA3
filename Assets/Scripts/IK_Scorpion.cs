using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using UnityEngine.UI;

public class IK_Scorpion : MonoBehaviour
{
    MyScorpionController _myController= new MyScorpionController();

    public IK_tentacles _myOctopus;

    [Header("Body")]
    float animTime;
    public float animDuration = 5;
    bool animPlaying = false;
    public Transform Body;
    public Transform StartPos;
    public Transform EndPos;

    [Header("Tail")]
    public Transform tailTarget;
    public Transform tail;

    [Header("Legs")]
    public Transform[] legs;
    public Transform[] legTargets;
    public Transform[] futureLegBases;

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

    // Start is called before the first frame update
    void Start()
    {
        _myController.InitLegs(legs,futureLegBases,legTargets);
        _myController.InitTail(tail);
        _myController.SaveTailState();
    }

    // Update is called once per frame
    void Update()
    {
        sliderChangeVelocity = slider.maxValue * sliderChangeVelocityFactor;

        if (animPlaying)
            animTime += Time.deltaTime;

        NotifyTailTarget();
<<<<<<< HEAD

        if (Input.GetKeyUp(KeyCode.Space) && !animPlaying)
        {
            ballShooted = true;
            startShootTime = Time.time;
        }

=======
        
>>>>>>> origin/feature/Ex2
        if (Input.GetKeyDown(KeyCode.Return))
        {
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
            ballShooted = false;
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
        }

        if (animTime < animDuration)
        {
            Body.position = Vector3.Lerp(StartPos.position, EndPos.position, animTime / animDuration);
        }
        else if (animTime >= animDuration && animPlaying)
        {
            Body.position = EndPos.position;
            animPlaying = false;
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

        _myController.NotifyStartWalk();
    }
}
