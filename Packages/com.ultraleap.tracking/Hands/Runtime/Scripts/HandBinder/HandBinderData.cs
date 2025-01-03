/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2024.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Leap.HandsModule
{
    /// <summary>
    /// A data structure to define all the fingers in a hand, the wrist and elbow
    /// </summary>
    [System.Serializable]
    public class BoundHand
    {
        public BoundFinger[] fingers = new BoundFinger[5];
        public BoundBone wrist = new BoundBone();
        public BoundBone elbow = new BoundBone();
        public float baseScale;
        public Vector3 startScale;
        [Range(-1, 3)] public float scaleOffset = 1;
        [Range(-1, 3)] public float elbowOffset = 1;
    }

    /// <summary>
    /// A data structure to define a finger
    /// </summary>
    [System.Serializable]
    public class BoundFinger
    {
        public BoundBone[] boundBones = new BoundBone[4];
        public float fingerTipBaseLength;
        [Range(-1, 3)] public float fingerTipScaleOffset = 1;
    }

    /// <summary>
    /// A data structure to define starting position, an offset and the Transform reference found in the scene
    /// </summary>
    [System.Serializable]
    public class BoundBone
    {
        public Transform boundTransform;
        public TransformStore startTransform = new TransformStore();
        public TransformStore offset = new TransformStore();
    }

    /// <summary>
    /// A data structure to store a transforms position and rotation
    /// </summary>
    [System.Serializable]
    public class TransformStore
    {
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.zero;
    }

    /// <summary>
    /// A data structure to store information about a transform and a Gameobject
    /// </summary>
    [System.Serializable]
    public class SerializedTransform
    {
        public TransformStore transform;
        public GameObject reference;
    }

    /// <summary>
    /// ENUM types for bones of the hand the hand binder can attach to
    /// </summary>
    public enum BoundTypes
    {
        THUMB_METACARPAL,
        THUMB_PROXIMAL,
        THUMB_INTERMEDIATE,
        THUMB_DISTAL,

        INDEX_METACARPAL,
        INDEX_PROXIMAL,
        INDEX_INTERMEDIATE,
        INDEX_DISTAL,

        MIDDLE_METACARPAL,
        MIDDLE_PROXIMAL,
        MIDDLE_INTERMEDIATE,
        MIDDLE_DISTAL,

        RING_METACARPAL,
        RING_PROXIMAL,
        RING_INTERMEDIATE,
        RING_DISTAL,

        PINKY_METACARPAL,
        PINKY_PROXIMAL,
        PINKY_INTERMEDIATE,
        PINKY_DISTAL,

        WRIST,
        ELBOW,
    }

    public static class HandBinderUtilities
    {

        /// <summary>
        /// The mapping that allows a BoundType and Leap FingerType/BoneType to map back to the HandBinders Data structure
        /// </summary>
        public readonly static Dictionary<BoundTypes, (Finger.FingerType, Bone.BoneType)> boundTypeMapping = new Dictionary<BoundTypes, (Finger.FingerType, Bone.BoneType)>
            {
            {BoundTypes.THUMB_METACARPAL, (Finger.FingerType.THUMB, Bone.BoneType.METACARPAL)},
            {BoundTypes.THUMB_PROXIMAL, (Finger.FingerType.THUMB, Bone.BoneType.PROXIMAL)},
            {BoundTypes.THUMB_INTERMEDIATE, (Finger.FingerType.THUMB, Bone.BoneType.INTERMEDIATE)},
            {BoundTypes.THUMB_DISTAL, (Finger.FingerType.THUMB, Bone.BoneType.DISTAL)},
            {BoundTypes.INDEX_METACARPAL, (Finger.FingerType.INDEX, Bone.BoneType.METACARPAL)},
            {BoundTypes.INDEX_PROXIMAL, (Finger.FingerType.INDEX, Bone.BoneType.PROXIMAL)},
            {BoundTypes.INDEX_INTERMEDIATE, (Finger.FingerType.INDEX, Bone.BoneType.INTERMEDIATE)},
            {BoundTypes.INDEX_DISTAL, (Finger.FingerType.INDEX, Bone.BoneType.DISTAL)},
            {BoundTypes.MIDDLE_METACARPAL, (Finger.FingerType.MIDDLE, Bone.BoneType.METACARPAL)},
            {BoundTypes.MIDDLE_PROXIMAL, (Finger.FingerType.MIDDLE, Bone.BoneType.PROXIMAL)},
            {BoundTypes.MIDDLE_INTERMEDIATE, (Finger.FingerType.MIDDLE, Bone.BoneType.INTERMEDIATE)},
            {BoundTypes.MIDDLE_DISTAL, (Finger.FingerType.MIDDLE, Bone.BoneType.DISTAL)},
            {BoundTypes.RING_METACARPAL, (Finger.FingerType.RING, Bone.BoneType.METACARPAL)},
            {BoundTypes.RING_PROXIMAL, (Finger.FingerType.RING, Bone.BoneType.PROXIMAL)},
            {BoundTypes.RING_INTERMEDIATE, (Finger.FingerType.RING, Bone.BoneType.INTERMEDIATE)},
            {BoundTypes.RING_DISTAL, (Finger.FingerType.RING, Bone.BoneType.DISTAL)},
            {BoundTypes.PINKY_METACARPAL, (Finger.FingerType.PINKY, Bone.BoneType.METACARPAL)},
            {BoundTypes.PINKY_PROXIMAL, (Finger.FingerType.PINKY, Bone.BoneType.PROXIMAL)},
            {BoundTypes.PINKY_INTERMEDIATE, (Finger.FingerType.PINKY, Bone.BoneType.INTERMEDIATE)},
            {BoundTypes.PINKY_DISTAL, (Finger.FingerType.PINKY, Bone.BoneType.DISTAL)},
        };

        /// <summary>
        /// Calculate a Hand from a BoundHand
        /// </summary>
        public static Hand GenerateLeapHand(this BoundHand boundHand, Hand leapHand, float fingerTipScale = 0.8f)
        {
            if (leapHand == null)
            {
                return leapHand;
            }

            //Loop through all the fingers of the hand to calculate where the leap data should be in relation to the Bound Hand
            for (int leapFingerID = 0; leapFingerID < leapHand.fingers.Length; leapFingerID++)
            {
                //Get the leap Finger
                Finger leapFinger = leapHand.fingers[leapFingerID];

                for (int leapBoneID = 0; leapBoneID < leapFinger.bones.Length; leapBoneID++)
                {
                    //Get the leapBone
                    Bone leapBone = leapFinger.bones[leapBoneID];

                    //If this bone is the distal bone, calculate a finger tip position
                    if (leapBoneID == (int)Bone.BoneType.DISTAL)
                    {
                        BoundBone distalBoundBone = boundHand.fingers[leapFingerID].boundBones[leapBoneID];
                        BoundBone intermediateBoundBone = boundHand.fingers[leapFingerID].boundBones[leapBoneID - 1];

                        if (intermediateBoundBone.boundTransform && distalBoundBone.boundTransform)
                        {
                            //Get the positions of the rigged joints
                            Vector3 distalBoundBonePosition = distalBoundBone.boundTransform.position;
                            Vector3 previousBoundBonePosition = intermediateBoundBone.boundTransform.position;

                            //Calculate the length of the previous bone
                            Vector3 previousBone = previousBoundBonePosition - distalBoundBonePosition;
                            float length = previousBone.magnitude;

                            //Get the direction of the bone
                            Vector3 direction = leapBone.Direction;

                            //Calculate the tip position
                            Vector3 tipPosition = (distalBoundBonePosition + direction * (length * fingerTipScale));
                            leapFinger.TipPosition = tipPosition;

                            //Calculate the center of the finger
                            Vector3 center = Vector3.Lerp(distalBoundBonePosition, tipPosition, 0.5f);

                            //Get the direction of distal to tip
                            Vector3 distalDirection = (distalBoundBonePosition - tipPosition);

                            //Set the leap finger
                            leapFinger.bones[leapBoneID] = new Bone(distalBoundBonePosition, tipPosition, center, distalDirection, distalDirection.magnitude, leapBone.Width, leapBone.Type, leapBone.Rotation);
                        }
                    }
                    else
                    {
                        BoundBone previousBoundBone = boundHand.fingers[leapFingerID].boundBones[leapBoneID];
                        BoundBone nextBoundBone = boundHand.fingers[leapFingerID].boundBones[leapBoneID + 1];

                        //If the bones are not null, calculate the data needed for this bone
                        if (previousBoundBone.boundTransform && nextBoundBone.boundTransform)
                        {
                            Vector3 previousBoundJointPosition = previousBoundBone.boundTransform.position;
                            Vector3 nextBoundJointPosition = nextBoundBone.boundTransform.position;
                            Vector3 direction = previousBoundJointPosition - nextBoundJointPosition;
                            float length = direction.magnitude;
                            Vector3 center = Vector3.Lerp(previousBoundJointPosition, nextBoundJointPosition, 0.5f);

                            //Set the data for a new leap bone
                            leapFinger.bones[leapBoneID] = new Bone(previousBoundJointPosition, nextBoundJointPosition, center, direction, length, leapBone.Width, leapBone.Type, leapBone.Rotation);
                        }

                        //If the bone is a metacarpal, use the wrist bone as the previous joint
                        else if (leapBoneID == (int)Bone.BoneType.METACARPAL)
                        {
                            BoundBone proximalBoundBone = boundHand.fingers[leapFingerID].boundBones[(int)Bone.BoneType.PROXIMAL];
                            BoundBone wristBoundBone = boundHand.wrist;

                            if (proximalBoundBone.boundTransform && wristBoundBone.boundTransform)
                            {
                                Vector3 nextBoundJointPosition = proximalBoundBone.boundTransform.position;
                                Vector3 previousBoundJointPosition = Vector3.Lerp(wristBoundBone.boundTransform.position, nextBoundJointPosition, 0.5f);
                                Vector3 direction = previousBoundJointPosition - nextBoundJointPosition;
                                float length = direction.magnitude;
                                Vector3 center = Vector3.Lerp(previousBoundJointPosition, nextBoundJointPosition, 0.5f);

                                //Set the data for the leap finger
                                leapFinger.bones[leapBoneID] = new Bone(previousBoundJointPosition, nextBoundJointPosition, center, direction, length, leapBone.Width, leapBone.Type, leapBone.Rotation);
                            }
                        }
                    }
                }
            }

            //Calculate the data for the wrist
            if (boundHand.wrist.boundTransform != null)
            {
                leapHand.WristPosition = boundHand.wrist.boundTransform.position;
                leapHand.Arm.NextJoint = boundHand.wrist.boundTransform.position;

                //Sor the elbow data
                if (boundHand.elbow.boundTransform != null)
                {
                    Vector3 elbowPos = boundHand.elbow.boundTransform.position;
                    Vector3 wristPos = boundHand.wrist.boundTransform.position;
                    Vector3 center = Vector3.Lerp(elbowPos, wristPos, 0.5f);
                    Vector3 dir = (elbowPos - wristPos);
                    float length = dir.magnitude;

                    //Set the data on the leap Arm
                    leapHand.Arm = new Arm(elbowPos, wristPos, center, dir, length, leapHand.Arm.Width, leapHand.Arm.Rotation);
                    leapHand.Arm.PrevJoint = elbowPos;
                }
            }

            //Calculate the palm position half way between the wrist and the middle proximal bone
            Vector3 palmPos = Vector3.Lerp(leapHand.WristPosition, leapHand.Middle.bones[(int)Bone.BoneType.PROXIMAL].PrevJoint, 0.5f);

            //Set the data on the leap hand
            leapHand.PalmPosition = palmPos;
            leapHand.StabilizedPalmPosition = leapHand.PalmPosition;
            leapHand.PalmWidth = (leapHand.Pinky.bones[1].PrevJoint - leapHand.Index.bones[1].PrevJoint).magnitude;
            leapHand.Arm.NextJoint = leapHand.WristPosition;
            return leapHand;
        }
    }

    /// <summary>
    /// Used to define what bones names are valid for each finger
    /// </summary>
    [System.Serializable]
    public static class BoneNameDefinitions
    {
        public static string[] DefinitionThumb = { "thumb" };
        public static string[] DefinitionIndex = { "index" };
        public static string[] DefinitionMiddle = { "middle" };
        public static string[] DefinitionRing = { "ring" };
        public static string[] DefinitionPinky = { "pinky", "little" };
        public static string[] DefinitionWrist = { "wrist", "hand", "palm" };
        public static string[] DefinitionElbow = { "elbow", "lowerArm", "forearm" };
    }
}