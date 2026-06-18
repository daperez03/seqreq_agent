
using System;
using UnityEngine;

public enum AvatarAction
{
    Idle,
    Talking,
    Head_Nod_Yes,
    Acknowleding
}

public class ActionManager : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private AvatarAction currentAction =
        AvatarAction.Idle;

    public static event Action<AvatarAction> OnPlayAction;

    public static void PlayAction(AvatarAction action)
    {
        OnPlayAction?.Invoke(action);
    }

    private void OnEnable()
    {
        OnPlayAction += HandleAction;
    }

    private void OnDisable()
    {
        OnPlayAction -= HandleAction;
    }

    private void HandleAction(AvatarAction newAction)
    {
        if (currentAction == newAction)
            return;

        currentAction = newAction;

        animator.CrossFade(
            newAction.ToString(),
            0.15f
        );
    }

    //private void StopCurrentAction()
    //{
    //    animator.ResetTrigger("Base Layer." + currentAction.ToString());
    //}
}

