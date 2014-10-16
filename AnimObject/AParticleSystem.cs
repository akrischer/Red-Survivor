using UnityEngine;
using System.Collections;

[System.Serializable]
public class AParticleSystem : AnimObject {

    /* Members of a particle system: */
    public ParticleSystem particleSystem; //particle system to play

    public bool loop = false; // should this ps loop?

    public AnimationDirection direction; // In which direction should this PS play?

    public int distance; // Assuming direction != target or none, how many hexes away does it play from this object?

    public float speed = 1f; //speed PS plays at, as a percentage of regular speed.

    #region Play Method

    public override IEnumerator Play(RSRMonoBehaviour thisObj, RSRMonoBehaviour targetObj, AnimObject.AnimationEnum animEnum)
    {
        if (particleSystem == null)
        {
            throw new System.NullReferenceException("Particle System for " + thisObj.gameObject.ToString() +
            " in animation " + animEnum.ToString() + " is null.");
        }

        Debug.Log("Playing Particle System anim for object " + thisObj.gameObject.ToString());

        if (target && !waitForAnimObjectToEnd)
        {
            /* Play target's animation right away */
            StartCoroutine(InvokePlayAnimation(targetObj, animEnum, delayForTargetAnim));
        }

        particleSystem.loop = loop; //Sets whether ps should loop or not

        /* Play actual animation */
        particleSystem.Play();

        Vector3 worldStartCord = Grid.GetWorldCoords(targetObj.GetCoords());
        Vector3 worldEndCoord = Grid.GetWorldCoords(TranslateGameCoord(
            thisObj.GetCoords(), //start pos of obj that's moving
            direction, //direction
            distance, // distance (in hexes)
            targetObj.GetCoords())); //targetObj's coords, in case the direction is TargetDirection

        float t = 0; Vector3 newWorldCoord;
        float tIncreaseRate = (1f / 60f) * speed;
        while (t < 1f)
        {
            newWorldCoord = Vector3.Lerp(worldStartCord, worldEndCoord, t);
            particleSystem.transform.position = newWorldCoord;

            yield return new WaitForFixedUpdate();
            t += tIncreaseRate;
        }
        particleSystem.transform.position = worldEndCoord;
        /**************************/

        if (target && waitForAnimObjectToEnd)
        {
            /* Play target's animation after this is now done */
            StartCoroutine(InvokePlayAnimation(targetObj, animEnum, delayForTargetAnim));
        }
    }

    #endregion

    #region ToString / Draw Method
    public override string ToString()
    {
        return "Particle System Effect";
    }
    #endregion


}
