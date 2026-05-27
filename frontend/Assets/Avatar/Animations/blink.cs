using UnityEngine;

public class blink : StateMachineBehaviour
{
    [SerializeField] private float blinkDuration = 0.3f;
    [SerializeField] private float blinkHoldTime = 0.05f; // Time to hold eyes closed
    [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private int eyeBlinkLeftIndex = -1;
    private int eyeBlinkRightIndex = -1;
    
    private float stateTime = 0f;
    private float closePhaseEnd;
    private float holdPhaseEnd;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SkinnedMeshRenderer smr = animator.GetComponent<SkinnedMeshRenderer>();
        if (smr != null && smr.sharedMesh != null)
        {
            // Cache blend shape indices for performance
            eyeBlinkLeftIndex = smr.sharedMesh.GetBlendShapeIndex("eyeBlinkLeft");
            eyeBlinkRightIndex = smr.sharedMesh.GetBlendShapeIndex("eyeBlinkRight");
            
            // If those don't exist, try alternative names
            if (eyeBlinkLeftIndex < 0)
                eyeBlinkLeftIndex = smr.sharedMesh.GetBlendShapeIndex("Blink_Left");
            if (eyeBlinkRightIndex < 0)
                eyeBlinkRightIndex = smr.sharedMesh.GetBlendShapeIndex("Blink_Right");
        }
        
        stateTime = 0f;
        closePhaseEnd = (blinkDuration - blinkHoldTime) * 0.5f;
        holdPhaseEnd = closePhaseEnd + blinkHoldTime;
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SkinnedMeshRenderer smr = animator.GetComponent<SkinnedMeshRenderer>();
        if (smr != null && smr.sharedMesh != null)
        {
            stateTime += Time.deltaTime;
            float blinkWeight = CalculateBlinkWeight();
            
            // Apply blink weight to both eyes
            if (eyeBlinkLeftIndex >= 0)
                smr.SetBlendShapeWeight(eyeBlinkLeftIndex, blinkWeight);
            if (eyeBlinkRightIndex >= 0)
                smr.SetBlendShapeWeight(eyeBlinkRightIndex, blinkWeight);
        }
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Ensure eyes are open when exiting
        SkinnedMeshRenderer smr = animator.GetComponent<SkinnedMeshRenderer>();
        if (smr != null && smr.sharedMesh != null)
        {
            if (eyeBlinkLeftIndex >= 0)
                smr.SetBlendShapeWeight(eyeBlinkLeftIndex, 0f);
            if (eyeBlinkRightIndex >= 0)
                smr.SetBlendShapeWeight(eyeBlinkRightIndex, 0f);
        }
    }
    
    private float CalculateBlinkWeight()
    {
        if (stateTime <= closePhaseEnd)
        {
            // Closing phase: 0 -> 100
            float t = stateTime / closePhaseEnd;
            return blinkCurve.Evaluate(t) * 100f;
        }
        else if (stateTime <= holdPhaseEnd)
        {
            // Hold phase: stay at 100
            return 100f;
        }
        else if (stateTime <= blinkDuration)
        {
            // Opening phase: 100 -> 0
            float t = (stateTime - holdPhaseEnd) / (blinkDuration - holdPhaseEnd);
            return (1f - blinkCurve.Evaluate(t)) * 100f;
        }
        else
        {
            // Blink complete
            return 0f;
        }
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}
}
