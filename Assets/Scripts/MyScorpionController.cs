using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{

    public class MyScorpionController
    {
        class TailTargetStuff
        {
            public MyTentacleController tail;
            public Transform endEffector, target;
            readonly Vector3 rotateAxisJoint0 = Vector3.forward, rotateAxisOtherJoints = Vector3.right;
            readonly float distanceThreshold = 0.01f, correctionVelocity = 5;
            readonly int iterations = 20;
            public void Update()
            {
                for (int j = 0; j < iterations; j++)
                    if (Vector3.Distance(endEffector.position, target.position) > distanceThreshold)
                    {
                        for (int i = 0; i < tail.Bones.Length; i++)
                        {
                            Vector3 rotateAxis;
                            if (i == 0)
                                rotateAxis = rotateAxisJoint0;
                            else
                                rotateAxis = rotateAxisOtherJoints;

                            float slope = CalculateSlope(tail.Bones[i], rotateAxis);
                            tail.Bones[i].Rotate(rotateAxis * -slope * correctionVelocity);
                        }
                    }

            }

            float CalculateSlope(Transform bone, Vector3 rotateAxis)
            {
                float deltaTheta = 0.01f;

                float distance1 = Vector3.Distance(endEffector.position, target.position);
                bone.Rotate(rotateAxis * deltaTheta);

                float distance2 = Vector3.Distance(endEffector.position, target.position);
                bone.Rotate(rotateAxis * -deltaTheta);

                return (distance2 - distance1) / deltaTheta;
            }
        }

        TailTargetStuff _tailStuff;

        bool tailLoop = false;
        float thresholdToActivateTail = 2.5f;


        class LegTargetStuff
        {
            public MyTentacleController leg;
            public Transform legTarget, legFuturBase, baseJoint; //Joint 0 has to reach futurBase               //EndEffector has to reach target
            public float distanceToStartMove, timeToReachFutureBasePos;//0.25, 0.15
            public bool lerpInProgress = false;
            float[] bonesLenghts;
            readonly int solverIteratons = 100;
            float counter = 0;
            Vector3 lerpStartPos, lerpEndPos;
            public void Start()
            {
                bonesLenghts = new float[leg.Bones.Length];
                distanceToStartMove = UnityEngine.Random.Range(0.2f, 0.3f);
                timeToReachFutureBasePos = UnityEngine.Random.Range(0.1f, 0.15f);
                //Calculem longitud de cada os
                for (int i = 0; i < leg.Bones.Length; i++)
                {
                    if (i < leg.Bones.Length - 1)
                        bonesLenghts[i] = (leg.Bones[i + 1].position - leg.Bones[i].position).magnitude;
                    else
                        bonesLenghts[i] = 0f;
                }
            }
            public void UpdateFabrik()
            {
                SolveIK();
            }
            public void UpdatePosition()
            {
                if (!lerpInProgress)
                {
                    lerpInProgress = Vector3.Distance(leg.Bones[0].position, legFuturBase.position) > distanceToStartMove;
                    lerpStartPos = leg.Bones[0].transform.position;
                    lerpEndPos = legFuturBase.transform.position;
                }
                else
                {
                    counter += Time.deltaTime;

                    if (counter < timeToReachFutureBasePos)
                    {
                        leg.Bones[0].position = Vector3.Lerp(lerpStartPos, lerpEndPos, counter / timeToReachFutureBasePos);
                    }
                    else
                    {
                        counter = 0;
                        lerpInProgress = false;
                    }
                }
            }

            void SolveIK()
            {
                Vector3[] finalBonesPositions = new Vector3[leg.Bones.Length];

                //Guardem les posicions actuals dels ossos
                for (int i = 0; i < leg.Bones.Length; i++)
                {
                    finalBonesPositions[i] = leg.Bones[i].position;
                }

                //Apliquem Fabrik les vegades que toqui
                for (int i = 0; i < solverIteratons; i++)
                {
                    finalBonesPositions = SolveForwardPositions(SolveInversePositions(finalBonesPositions));
                }

                // Apliquem resultats a cada os
                for (int i = 0; i < leg.Bones.Length; i++)
                {
                    leg.Bones[i].position = finalBonesPositions[i];

                    if (i < leg.Bones.Length - 1)
                    {
                        // Calculem la direcció del següent os i normalitzem
                        Vector3 direction = (finalBonesPositions[i + 1] - leg.Bones[i].position).normalized;

                        // Calculem la rotació basada en la direcció i normalitzem l'eix de rotació
                        Quaternion rotation = Quaternion.FromToRotation(leg.Bones[i].up, direction.normalized);

                        // Aplicar la rotació a l'objecte
                        leg.Bones[i].rotation = rotation * leg.Bones[i].rotation;
                    }
                    else
                    {
                        // Si és l'últim os, calculem la direcció al target i apliquem la rotació
                        Vector3 targetDirection = (legTarget.position - leg.Bones[i].position).normalized;
                        Quaternion targetRotation = Quaternion.FromToRotation(leg.Bones[i].up, targetDirection.normalized);
                        leg.Bones[i].rotation = targetRotation * leg.Bones[i].rotation;
                    }
                }
            }

            Vector3[] SolveInversePositions(Vector3[] forwardPositions)
            {
                Vector3[] inversePositions = new Vector3[forwardPositions.Length];

                //Calculem les posicions "ideals" desde ultim fins al primer os en base a les pos actuals
                for (int i = (forwardPositions.Length - 1); i >= 0; i--)
                {
                    if (i == forwardPositions.Length - 1) //Si es ultim os a pos prima es la mateixa q objectiu
                        inversePositions[i] = legTarget.position;
                    else
                    {
                        Vector3 nextPosPrima = inversePositions[i + 1];
                        Vector3 currentBasePos = forwardPositions[i];
                        Vector3 direction = (currentBasePos - nextPosPrima).normalized;
                        float lenght = bonesLenghts[i];
                        inversePositions[i] = nextPosPrima + (direction * lenght);
                    }
                }
                return inversePositions;
            }

            Vector3[] SolveForwardPositions(Vector3[] inversePositions)
            {
                Vector3[] forwardPositions = new Vector3[inversePositions.Length];

                //Calculem les posicions "ideals" desde ultim fins al primer os en base a les pos actuals
                for (int i = 0; i < inversePositions.Length; i++)
                {
                    if (i == 0) //Si es ultim os a pos prima es la mateixa q objectiu
                        forwardPositions[i] = leg.Bones[0].position;
                    else
                    {
                        Vector3 currentPosPrima = inversePositions[i];
                        Vector3 secondPrePrima = forwardPositions[i - 1];
                        Vector3 direction = (currentPosPrima - forwardPositions[i - 1]).normalized;
                        float lenght = bonesLenghts[i - 1];
                        forwardPositions[i] = secondPrePrima + (direction * lenght);
                    }
                }
                return inversePositions;
            }


        }

        LegTargetStuff[] _legsStuff;

        bool legsLoop = false;

        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;

        //LEGS
        Transform[] legTargets = new Transform[6];
        Transform[] legFutureBases = new Transform[6];
        MyTentacleController[] _legs = new MyTentacleController[6];


        #region public
        public void InitLegs(Transform[] LegRoots, Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            legFutureBases = new Transform[LegFutureBases.Length];
            legTargets = new Transform[LegTargets.Length];
            //Legs init
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
                legFutureBases[i] = LegFutureBases[i];
                legTargets[i] = LegTargets[i];
            }

        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation
            tailEndEffector = _tail._endEffectorSphere;
            tailTarget = GameObject.Find("Ball").transform;
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            _tailStuff = new TailTargetStuff();
            _tailStuff.tail = _tail;
            _tailStuff.target = tailTarget;
            _tailStuff.endEffector = tailEndEffector;
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {
            _legsStuff = new LegTargetStuff[6];
            for (int i = 0; i < 6; i++)
            {
                _legsStuff[i] = new LegTargetStuff();
                _legsStuff[i].leg = _legs[i];
                _legsStuff[i].legFuturBase = legFutureBases[i];
                _legsStuff[i].legTarget = legTargets[i];
                _legsStuff[i].legFuturBase = legFutureBases[i];
                _legsStuff[i].baseJoint = _legsStuff[i].leg.Bones[0];
                _legsStuff[i].Start();
            }
            legsLoop = true;
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            tailLoop = Vector3.Distance(_tailStuff.tail.Bones[0].GetComponentInParent<Transform>().position, _tailStuff.target.position) <= thresholdToActivateTail;

            if (tailLoop)
            {
                updateTail();
            }
            if (legsLoop)
            {
                updateLegs();
                updateLegPos();
            }
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            for (int i = 0; i < _legsStuff.Length; i++)
            {
                _legsStuff[i].UpdatePosition();
            }
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            _tailStuff.Update();
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {
            for (int i = 0; i < _legsStuff.Length; i++)
            {
                _legsStuff[i].UpdateFabrik();
            }
        }
        #endregion
    }
}
