using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBall : MonoBehaviour
{
    public bool isGoal = false;

    [SerializeField]
    IK_tentacles _myOctopus;

    //movement speed in units per second
    [Range(-1.0f, 1.0f)]
    [SerializeField]
    private float _movementSpeed = 5f;
    Vector3 startPosition;

    Vector3 _dir;
    CustomRigidbody _myRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        _myRigidbody = GetComponent<CustomRigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< HEAD
        //transform.rotation = Quaternion.identity;
=======
        if (!isGoal && _myRigidbody.simulatePhysics && transform.position.z < -68.5f && _myRigidbody.enabled)
        {
            _myRigidbody.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            this.transform.position = startPosition;
            _myRigidbody.simulatePhysics = false;
            _myRigidbody.enabled = true;
        }

        transform.rotation = Quaternion.identity;
>>>>>>> origin/feature/Ex2

        ////get the Input from Horizontal axis
        //float horizontalInput = Input.GetAxis("Horizontal");
        ////get the Input from Vertical axis
        //float verticalInput = Input.GetAxis("Vertical");

<<<<<<< HEAD
        ////update the position
=======
        //update the position
>>>>>>> origin/feature/Ex2
        //transform.position = transform.position + new Vector3(-horizontalInput * _movementSpeed * Time.deltaTime, verticalInput * _movementSpeed * Time.deltaTime, 0);

    }

    private void OnTriggerEnter(Collider other)
    {
        IK_Scorpion ikScorpion = other.transform.GetComponentInParent<IK_Scorpion>();
        if (ikScorpion == null)
            return;

        _myRigidbody.simulatePhysics = true;
        //ikScorpion.ballMoving = true;
        //Debug.Log("Ball shooted with force of " + ikScorpion.slider.value + " % in " + (Time.time - ikScorpion.startShootTime) + " seconds");
        _myOctopus.NotifyShoot();
    }
}
