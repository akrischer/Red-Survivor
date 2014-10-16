using UnityEngine;
using System.Collections;

[System.Serializable]
public class ASpriteAnimation : AnimObject {

    /* Members of a sprite animation */
    /* SpriteAnimation is determined by the Animation Type */
    public float speed = 1f; //how fast to play it, as a percentage of normal speed

    public bool loop = false; // should this spriteAnimation loop?

    public int timesToLoop = 0; // iff loop == true, how many times should it loop?

    public Animator _animator; //this obj's animator


    /* What can "play" for a sprite animation (excluding target stuff):
     *  - the animation */
    public override IEnumerator Play(RSRMonoBehaviour thisObj, RSRMonoBehaviour targetObj, AnimObject.AnimationEnum animEnum)
    {
        if (_animator == null)
        {
            throw new System.NullReferenceException("Sprite Animation for " + thisObj.gameObject.ToString() +
            " in animation " + animEnum.ToString() + " is null.");
        }

        if (target && !waitForAnimObjectToEnd)
        {
            /* Play target's animation right away */
            StartCoroutine(InvokePlayAnimation(targetObj, animEnum, delayForTargetAnim));
        }

        /* Play actual animation */
        _animator.SetTrigger(animEnum.ToString()); // plays actual animation
        yield return null; //gotta let it update before we get its state
        float clipLength = _animator.GetCurrentAnimatorStateInfo(0).length;

        Debug.Log("sprite clip length: " + clipLength.ToString());
        if (loop)
        {
            /* Loop the animation as many times as we need to! */

            int currentLoopCount = timesToLoop;
            /* While currentLoopCount > 0, keep playing the sprite animation! */
            while (currentLoopCount > 0)
            {
                if (timesToLoop == 0) // loop forever
                {
                    currentLoopCount = 10; //arbitrary number greater than 1
                }
                Debug.Log("playing clip");
                //thisObj.animation.Play("clip");
                yield return new WaitForSeconds(clipLength + .1f);
                _animator.SetTrigger(animEnum.ToString());
                currentLoopCount--;
            }
        }
        else
        {
            //spriteAnimation.Play();
            Debug.Log("PlayingClip no loop");
            //thisObj.animation.Play("clip");
        }
        /************************/

        if (target && waitForAnimObjectToEnd)
        {
            /* Play target's animation after this is now done */
            StartCoroutine(InvokePlayAnimation(targetObj, animEnum, delayForTargetAnim));
        }
        yield break;
    }


    public override string ToString()
    {
        return "Sprite Animation";
    }

}
