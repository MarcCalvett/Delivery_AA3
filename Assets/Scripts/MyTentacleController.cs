using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{


    internal class MyTentacleController

    //MAINTAIN THIS CLASS AS INTERNAL
    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        public Transform _endEffectorSphere;

        public Transform[] Bones { get => _bones; }
        List<Transform> joints = new List<Transform>();


        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            //TODO: add here whatever is needed to find the bones forming the tentacle for all modes
            //you may want to use a list, and then convert it to an array and save it into _bones
            tentacleMode = mode;

            switch (tentacleMode)
            {
                case TentacleMode.LEG:
                    //TODO: in _endEffectorsphere you keep a reference to the base of the leg
                    joints.AddRange(root.GetComponentsInChildren<Transform>().Where((trans) => trans.childCount != 0));
                    joints.Add(joints.Last().GetChild(1));
                    _endEffectorSphere = joints.Last();
                    break;
                case TentacleMode.TAIL:
                    //TODO: in _endEffectorsphere you keep a reference to the red sphere 
                    joints.AddRange(root.GetComponentsInChildren<Transform>().Where((trans) => trans.childCount != 0));
                    _endEffectorSphere = joints.Last().GetChild(0);
                    break;
                case TentacleMode.TENTACLE:
                    //TODO: in _endEffectorphere you  keep a reference to the sphere with a collider attached to the endEffector
                    _endEffectorSphere = root.GetComponentInChildren<SphereCollider>().transform;
                    joints.AddRange(root.GetComponentsInChildren<Transform>().Where((trans) => trans != _endEffectorSphere));
                    break;
            }

            _bones = joints.ToArray();

            return Bones;
        }
    }
}
