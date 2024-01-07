using OctopusController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEngine.GraphicsBuffer;

namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController
    {


        MyTentacleController[] _tentacles = new MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];


        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        bool ballShooted = false;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin { set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }


        public void TestLogging(string objectName)
        {
            Debug.Log("hello, I am initializing my Octopus Controller in object Modified " + objectName);
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for (int i = 0; i < tentacleRoots.Length; i++)
            {

                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i], TentacleMode.TENTACLE);
                //TODO: initialize any variables needed in ccd
            }

            _randomTargets = randomTargets;
            //TODO: use the regions however you need to make sure each tentacle stays in its region

        }

        public void NotifyTarget(Transform target, Transform region)
        {
            if (_currentRegion != region)
                _currentRegion = region;
            if (_target != target)
                _target = target;
        }

        public void NotifyShoot()
        {
            //TODO. what happens here?
            Debug.Log("Shoot");
            if (!ballShooted)
                ballShooted = true;
        }

        #endregion

        #region private and internal methods
        //todo: add here anything that you need

        class TentacleTargetStuff
        {
            public Transform randomTarget;
            public MyTentacleController tentacleController;
            public bool ballInMyRange = false;
            readonly int maxIterations = 100;
            readonly float distanceThreshold = 0.01f;
            public void UpdatePos(bool ballShooted, Transform blueBallRegion, Transform blueBall)
            {
                ballInMyRange = ballShooted && blueBallRegion != null && randomTarget.GetComponentInParent<BoxCollider>().bounds.Intersects(blueBall.GetComponent<SphereCollider>().bounds);

                if (!ballInMyRange)
                {
                    ReachRandomTarget();
                }
                else
                {
                    ReachBlueTarget(blueBall);
                }
            }

            private void ReachRandomTarget()
            {
                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    for (int i = tentacleController.Bones.Length - 1; i >= 0; i--)
                    {
                        Vector3 targetDir = randomTarget.position - tentacleController.Bones[i].position;
                        Vector3 currentDir = tentacleController._endEffectorSphere.position - tentacleController.Bones[i].position;

                        float angleSwing = Vector3.SignedAngle(currentDir, targetDir, Vector3.up);
                        float angleTwist = Vector3.SignedAngle(currentDir, targetDir, Vector3.forward);

                        Quaternion rotationSwing = Quaternion.AngleAxis(angleSwing, Vector3.up);
                        Quaternion rotationTwist = Quaternion.AngleAxis(angleTwist, Vector3.forward);

                        // Aplica el swing i el twist als joints
                        tentacleController.Bones[i].rotation = rotationSwing * rotationTwist * tentacleController.Bones[i].rotation;

                        // Comprova si l'end effector és prou a prop de l'objectiu
                        float distance = Vector3.Distance(tentacleController._endEffectorSphere.position, randomTarget.position);
                        if (distance < distanceThreshold)
                        {
                            return; // Atura les iteracions si està prou a prop de l'objectiu
                        }
                    }
                }

            }
            private void ReachBlueTarget(Transform blueBall)
            {
                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    for (int i = tentacleController.Bones.Length - 1; i >= 0; i--)
                    {
                        Vector3 targetDir = blueBall.position - tentacleController.Bones[i].position;
                        Vector3 currentDir = tentacleController._endEffectorSphere.position - tentacleController.Bones[i].position;

                        float angleSwing = Vector3.SignedAngle(currentDir, targetDir, Vector3.up);
                        float angleTwist = Vector3.SignedAngle(currentDir, targetDir, Vector3.forward);

                        Quaternion rotationSwing = Quaternion.AngleAxis(angleSwing, Vector3.up);
                        Quaternion rotationTwist = Quaternion.AngleAxis(angleTwist, Vector3.forward);

                        // Aplica el swing i el twist als joints
                        tentacleController.Bones[i].rotation = rotationSwing * rotationTwist * tentacleController.Bones[i].rotation;

                        // Comprova si l'end effector és prou a prop de l'objectiu
                        float distance = Vector3.Distance(tentacleController._endEffectorSphere.position, blueBall.position);
                        if (distance < distanceThreshold)
                        {
                            return; // Atura les iteracions si està prou a prop de l'objectiu
                        }
                    }
                }
            }

        }

        TentacleTargetStuff[] tentaclesControllers = new TentacleTargetStuff[4];

        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
        }

        void update_ccd()
        {

            if (tentaclesControllers.Length == 0) { }
            for (int i = 0; i < 4; i++)
            {
                tentaclesControllers[i] = new TentacleTargetStuff();
                tentaclesControllers[i].randomTarget = _randomTargets[i];
                tentaclesControllers[i].tentacleController = _tentacles[i];
            }


            foreach (TentacleTargetStuff t in tentaclesControllers)
            {
                t.UpdatePos(ballShooted, _currentRegion, _target);
            }

        }

        #endregion

    }
}
