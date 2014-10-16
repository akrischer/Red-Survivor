using UnityEngine;
using System.Collections;

[System.Serializable]
public class ATween : AnimObject {

    /* Members for a tween */
    public AnimationDirection direction; //In what direction should the object move?

    public int distance; //Assuming direction != target or none, how many hexes should it move in the given direction?

    public float speed = 1f;//How many hexes should it cross per second?


    public override IEnumerator PlayTarget(RSRMonoBehaviour targetObj, AnimObject.AnimationEnum animEnum)
    {
        return base.PlayTarget(targetObj, animEnum);
    }


    /* What can "play" for a tween (excluding target stuff):
     *  - moves object attached to rsrmonobehaviour in a direction, over a given distance,
     *      with a given speed */
    public override IEnumerator Play(RSRMonoBehaviour thisObj, RSRMonoBehaviour targetObj, AnimObject.AnimationEnum animEnum)
    {
        //Debug.Log("Playing Tween anim for object " + thisObj.gameObject.ToString());
        if (target && !waitForAnimObjectToEnd)
        {
            StartCoroutine(InvokePlayAnimation(targetObj, animEnum, delayForTargetAnim));//plays target's specified animation right away
        }
        if (speed <= 0) { speed = 1f; }

        /* Play actual animation *//////////////////////
        Vector3 gameEndCoord = TranslateGameCoord(
            thisObj.GetCoords(), //start pos of obj that's moving
            direction, //direction
            distance, // distance (in hexes)
            targetObj.GetCoords()); //targetObj's coords, in case the direction is TargetDirection
        //Debug.Log(thisObj.GetCoords().ToString() + ", " + direction.ToString() + ", " + distance.ToString() + ", " + targetObj.GetCoords().ToString());
        Vector3 worldStartCord = thisObj.transform.position;
        Vector3 worldEndCoord = Grid.GetWorldCoords(gameEndCoord);
        //Debug.Log("From: " + worldStartCord.ToString() + ". To: " + worldEndCoord.ToString());


        /* Forces game object to face in correct direction, if implemented */
        Vector3 directionMoving = worldEndCoord - worldStartCord;
        thisObj.MakeSpriteFace(thisObj.GetDirection(directionMoving));

        float t = 0; Vector3 newWorldCoord;
        float tIncreaseRate = (1f / 60f) * speed;
        while (t < 1f)
        {
            newWorldCoord = Vector3.Lerp(worldStartCord, worldEndCoord, t);
            thisObj.transform.position = newWorldCoord;

            yield return new WaitForFixedUpdate();
            t += tIncreaseRate;
        }
        thisObj.transform.position = worldEndCoord;
        //thisObj.MoveTo(gameEndCoord);
        /***********************/

        if (target && waitForAnimObjectToEnd)
        {
            /* plays target's specified animation after this animobj is done */
            StartCoroutine(InvokePlayAnimation(targetObj, animEnum, delayForTargetAnim));
        }
    }

    public override string ToString()
    {
        return "Tween Animation";
    }
}
