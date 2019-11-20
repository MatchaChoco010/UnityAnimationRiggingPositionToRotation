using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent, AddComponentMenu ("Animation Rigging/Custom/Position To Rotation Constraint")]
public class PositionToRotationConstraint : RigConstraint<PositionToRotationConstraintJob, PositionToRotationConstraintData, PositionToRotationConstraintBinder> { }

[BurstCompile]
public struct PositionToRotationConstraintJob : IWeightedAnimationJob {
    public ReadWriteTransformHandle constrained;
    public ReadWriteTransformHandle source;

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion (AnimationStream stream) { }

    public void ProcessAnimation (AnimationStream stream) {
        float w = jobWeight.Get (stream);

        var sourcePos = source.GetLocalPosition (stream);
        sourcePos = new Vector3 (sourcePos.x, sourcePos.y, 0);
        source.SetLocalPosition (stream, sourcePos);

        if (w > 0f) {
            var rot = constrained.GetLocalRotation (stream);
            rot *= Quaternion.AngleAxis (sourcePos.y * Mathf.PI * Mathf.Rad2Deg, Vector3.forward);
            rot *= Quaternion.AngleAxis (sourcePos.x * Mathf.PI * Mathf.Rad2Deg, Vector3.right);
            constrained.SetLocalRotation (
                stream,
                Quaternion.Lerp (constrained.GetLocalRotation (stream), rot, w)
            );
        }
    }
}

[System.Serializable]
public struct PositionToRotationConstraintData : IAnimationJobData {
    public Transform constrainedObject;
    [SyncSceneToStream] public Transform sourceObject;

    public bool IsValid () => !(constrainedObject == null || sourceObject == null);

    public void SetDefaultValues () {
        constrainedObject = null;
        sourceObject = null;
    }
}

public class PositionToRotationConstraintBinder : AnimationJobBinder<PositionToRotationConstraintJob, PositionToRotationConstraintData> {
    public override PositionToRotationConstraintJob Create (Animator animator, ref PositionToRotationConstraintData data, Component component) {
        var job = new PositionToRotationConstraintJob ();
        job.constrained = ReadWriteTransformHandle.Bind (animator, data.constrainedObject);
        job.source = ReadWriteTransformHandle.Bind (animator, data.sourceObject);
        return job;
    }
    public override void Destroy (PositionToRotationConstraintJob job) { }
}
