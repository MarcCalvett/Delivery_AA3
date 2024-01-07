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

        public bool ballShooted = false;

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
            readonly int maxIterations = 20;
            readonly float distanceThreshold = 0.01f;
            float timerBall = 0;
            float timerRand = 0;
            Vector3 startPosBall = Vector3.zero;
            Vector3 startPosRand = Vector3.zero;

            public void UpdatePos(bool stopBall, Transform blueBallRegion, Transform blueBall)
            {
                ballInMyRange = stopBall && blueBallRegion != null && randomTarget.GetComponentInParent<BoxCollider>().bounds.Intersects(blueBall.GetComponent<SphereCollider>().bounds);
                Vector3 targetPos;

                if (ballInMyRange)
                {
                    startPosRand = Vector3.zero;
                    timerRand = 0;

                    if (startPosBall == Vector3.zero)
                        startPosBall = tentacleController._endEffectorSphere.position;

                    if (timerBall >= 1)
                        timerBall = 1;
                    else
                        timerBall += Time.deltaTime * 4;
                    targetPos = Vector3.Lerp(startPosBall, blueBall.position, timerBall);
                }
                else
                {
                    timerBall = 0;
                    startPosBall = Vector3.zero;

                    if (startPosRand == Vector3.zero)
                        startPosRand = tentacleController._endEffectorSphere.position;

                    if (timerRand >= 1)
                        timerRand = 1;
                    else
                        timerRand += Time.deltaTime * 6;
                    targetPos = Vector3.Lerp(startPosRand, randomTarget.position, timerRand);
                }

                ReachTarget(targetPos);
            }

            private void ReachTarget(Vector3 targetPos)
            {
                float swingZlimits = 0.01f;
                float swingXlimits = 0.001f;

                bool targetOutRegion = randomTarget.GetComponentInChildren<MovingTarget>().outRegion;

                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    for (int i = tentacleController.Bones.Length - 1; i >= 0; i--)
                    {
                        float scaleFactor = targetOutRegion ? 1 : 1.0f / Mathf.Log(i + 0.065f);
                        float scaleFactor2 = targetOutRegion ? 1 : 1.0f / Mathf.Log(i + 10);

                        Vector3 targetDir = targetPos - tentacleController.Bones[i].position;
                        Vector3 currentDir = tentacleController._endEffectorSphere.position - tentacleController.Bones[i].position;

                        float angleSwingZ = Vector3.SignedAngle(currentDir, targetDir, Vector3.forward);
                        float angleSwingX = Vector3.SignedAngle(currentDir, targetDir, Vector3.right);


                        //Debug.DrawLine(tentacleController.Bones[i].position, camDir);

                        float adjustedMaxRotationSwingZ = swingZlimits * scaleFactor;
                        float adjustedMinRotationSwingZ = -swingZlimits * scaleFactor;

                        float adjustedMaxRotationSwingX = swingXlimits * scaleFactor2;
                        float adjustedMinRotationSwingX = -swingXlimits * scaleFactor2;

                        // Limita les rotacions per evitar comportaments indesitjats
                        angleSwingZ = Mathf.Clamp(angleSwingZ, adjustedMinRotationSwingZ, adjustedMaxRotationSwingZ);
                        angleSwingX = Mathf.Clamp(angleSwingX, adjustedMinRotationSwingX, adjustedMaxRotationSwingX);
                        angleSwingX *= (targetPos.z - tentacleController.Bones[i].position.z) / 10;

                        Quaternion rotationSwingZ = Quaternion.AngleAxis(angleSwingZ, Vector3.forward);
                        Quaternion rotationSwingX = Quaternion.AngleAxis(angleSwingX, Vector3.right);

                        tentacleController.Bones[i].rotation = rotationSwingZ * rotationSwingX * tentacleController.Bones[i].rotation;

                        float distance = Vector3.Distance(tentacleController._endEffectorSphere.position, targetPos);
                        if (distance < distanceThreshold)
                        {
                            return;
                        }
                    }
                }


            }

        }

        TentacleTargetStuff[] tentaclesControllers = new TentacleTargetStuff[4];

        public void UpdateTentacles(bool saveGoal)
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd(saveGoal);
        }

        void update_ccd(bool saveGoal)
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
                t.UpdatePos(ballShooted && saveGoal, _currentRegion, _target);
            }

        }

        #endregion

    }
}
