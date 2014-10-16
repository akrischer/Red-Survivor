using UnityEngine;
using System.Collections;


/* Parent class for the 3 types of anim objects: Tween, Sprite Animation, and Particle System */
//[System.Serializable]
public abstract class AnimObject : ScriptableObject {

    /* Reference to the coroutine starter */
    [SerializeField]
    CoroutineStarter _coroutineStarter;
    #region Enums
    /* Enums */
    
    /* This holds the list of all possible animations an object COULD have.
     * Note, just because an animation exists does not mean each object implements it. */
    public enum AnimationEnum {Death, GetHit, RangedAttack, MeleeAttack, Move, Default, Spawn};

    /* The 3 basic types of AnimObjects */
    public enum AnimObjectType {ATween, ASpriteAnimation, AParticleSystem};

    /* Special directions; mostly used for tweens.
     * Note: Numerical lengths for these "directions" is in terms of HEX SPACES
     *  - Target: *SPECIAL* location = target; there is no length to set in this case
     *  - None: *SPECIAL* location = where object is; there is not length to set in this case
     *  - TargetDirection: Direction of the target. If there's no target object, direction is E.
     *  - NE, NW SE, SW, E, W: Compass Directions. */
    public enum AnimationDirection {Target, TargetDirection, N, S, NE, NW, SE, SW, None};

    #endregion
    /* Common behaviors */

    /* Concurrent? Should this AnimObject not lock other AnimObjects in the sequence from playing? */
    public bool concurrent;

    /* Is this info being folded out? */
    public bool foldout = false;

    /* Editable Description for this animation */
    public string description = "";

    void OnEnable()
    {
    }

    #region Target
    /* Target? Should this AnimObject play its target's animation?
     * IF THERE IS NO TARGET, nothing will happen */
    public bool target;

    // IF target == true, we need to know the following...

    /* Wait for this AnimObject to end before starting target anim? */
    public bool waitForAnimObjectToEnd;

    /* Delay for starting target's animation.
     * IF waitForAnimObjectToEnd == true, delay applies AFTER this AnimObject finishes */
    public float delayForTargetAnim = 0f;

    /* (Optional): Override target animation to play. Normally the target will
     * know what animation to play via the state of the game. */
    public AnimationEnum optionalTargetAnimationOverride;
    #endregion 

    #region Play Methods
    /* 
     * thisObj : the obj that's doing the animation
     * targetObj: "Target" for the obj
     * animEnum: Animation for the target to play */
    public virtual IEnumerator Play(RSRMonoBehaviour thisObj, RSRMonoBehaviour targetObj, AnimationEnum animEnum)
    {
        throw new System.NotImplementedException("Play(GameObject,AnimationEnum) not implemented for this object");
    }

    /*
     * targetObj: in this case, the object that will be doing an animation
     * animEnum: Animation for targetObj to play */
    public virtual IEnumerator PlayTarget(RSRMonoBehaviour targetObj, AnimObject.AnimationEnum animEnum)
    {
        RSRAnimationController targetAnimController = targetObj.GetComponent<RSRAnimationController>();
        if (targetAnimController == null)
        {
            Debug.LogWarning("targetObj " + targetObj.ToString() + "does not have an AnimationController attached!");
            yield break;
        }

        yield return new WaitForSeconds(delayForTargetAnim); //DELAY

        if (optionalTargetAnimationOverride == AnimationEnum.Default)
        {
            targetObj.PlayAnimation(animEnum);
        }
        else
        {
            targetObj.PlayAnimation(optionalTargetAnimationOverride);
        }
    }

    #endregion

    #region Factory Method Create

    public static ATween CreateTweenAnimObject()
    {
        //return ATween.CreateInstance<ATween>();
        //return new ATween();
        return ScriptableObjectUtility.CreateAsset<ATween>();
    }

    public static AParticleSystem CreateParticleSystemAnimObject()
    {
        //return AParticleSystem.CreateInstance<AParticleSystem>();
        //return new AParticleSystem();
        return ScriptableObjectUtility.CreateAsset<AParticleSystem>();
    }
        
    public static ASpriteAnimation CreateSpriteAnimationAnimObject(Animator thisAnimator)
    {
        //return ASpriteAnimation.CreateInstance<ASpriteAnimation>();
        //return new ASpriteAnimation();
        ASpriteAnimation result = ScriptableObjectUtility.CreateAsset<ASpriteAnimation>();
        result._animator = thisAnimator;
        return result;
    }

    #endregion

    //protected void ShowTargetGUI()
    //{
    //    GUILayout.BeginVertical();
    //
    //    waitForAnimObjectToEnd = UnityEditor.EditorGUILayout.ToggleLeft("Wait For AnimObject to End", waitForAnimObjectToEnd, GUILayout.MinWidth(400f));
    //
    //
    //    delayForTargetAnim = UnityEditor.EditorGUILayout.FloatField("Delay", delayForTargetAnim, GUILayout.MaxWidth(200f));
    //    if (delayForTargetAnim < 0f) { delayForTargetAnim = 0f; } //ensure you can't have a negative delay
    //
    //
    //    GUILayout.BeginHorizontal(); //"(optional) Target animation play "
    //    GUILayout.FlexibleSpace();
    //    GUILayout.Label("(Optional) Target Animation to Play");
    //    optionalTargetAnimationOverride = (AnimationEnum)UnityEditor.EditorGUILayout.EnumPopup(
    //        "",
    //        optionalTargetAnimationOverride,
    //        GUILayout.MaxWidth(250f));
    //    GUILayout.FlexibleSpace();
    //    GUILayout.EndHorizontal();
    //
    //    GUILayout.EndVertical();
    //}

    #region Draw Method

    //public virtual void DrawAnimObject()
    //{
    //    /* Draw the concurrency checkmark */
    //    concurrent = UnityEditor.EditorGUILayout.ToggleLeft("Concurrently Play AnimObject", concurrent, GUILayout.ExpandWidth(true));
    //
    //    /* Some space */
    //    GUILayout.Space(10f);
    //
    //    /* Draw the Target Toggle + Target info, if it's toggled */
    //    target = UnityEditor.EditorGUILayout.ToggleLeft("Trigger Target Animation", target, GUILayout.ExpandWidth(true));
    //    if (target)
    //    {
    //        UnityEditor.EditorGUI.indentLevel++;
    //        ShowTargetGUI();
    //        UnityEditor.EditorGUI.indentLevel--;
    //    }
    //
    //    GUILayout.Space(10f);
    //}

    #endregion

    #region Set/Get Methods

    public void SetDescription(string descr)
    {
        description = descr;
    }

    public string GetDescription()
    {
        return description;
    }

    #endregion

    #region Utility Methods

    /* Given a startin game coordinate, direction, and distance,
     * return game coordinates that reflect this translation. */
    public static Vector3 TranslateGameCoord(Vector3 startCoord, AnimationDirection direction, int dist)
    {
        if (direction == AnimationDirection.TargetDirection)
        {
            throw new System.FormatException("Error in TranslateGameCoord. If you're using TargetDirection you MUST " +
                "input a target coordinates (as game world coordinates");
        }
        else
        {
            return TranslateGameCoord(startCoord, direction, dist, Vector3.zero);
        }
    }
    public static Vector3 TranslateGameCoord(Vector3 startCoord, AnimationDirection direction, int dist, Vector3 targetCoord)
    {
        /* First and foremost, translate direction and distance into a coordinates */
        Vector3 gameDelta = Vector3.zero;
        switch (direction)
        {
            case AnimationDirection.N:
                gameDelta = new Vector3(0, 1, -1).normalized;
                break;
            case AnimationDirection.S:
                gameDelta = new Vector3(0, -1, 1).normalized;
                break;
            case AnimationDirection.NE:
                gameDelta = new Vector3(1, 0, -1).normalized;
                break;
            case AnimationDirection.NW:
                gameDelta = new Vector3(-1, 1, 0).normalized;
                break;
            case AnimationDirection.SE:
                gameDelta = new Vector3(1, -1, 0).normalized;
                break;
            case AnimationDirection.SW:
                gameDelta = new Vector3(-1, 0, 1).normalized;
                break;
            case AnimationDirection.TargetDirection:
                Vector3 worldStartCoord = Grid.GetWorldCoords(startCoord);
                Vector3 worldTargetCoord = Grid.GetWorldCoords(targetCoord);
                gameDelta += (worldTargetCoord - worldStartCoord).normalized;
                break;
            case AnimationDirection.Target:
                return targetCoord;
        }

        /* Now multiple gameDelta by dist */
        gameDelta *= dist;

        return startCoord + gameDelta;
    }

    public static float EnsureNoNegativeFloat(float t)
    {
        if (t < 0f) { t = 0f; }
        return t;
    }

    public static int EnsureNoNegativeInt(int i)
    {
        if (i < 0) { i = 0; }
        return i;
    }

    protected UnityEngine.Coroutine StartCoroutine(IEnumerator coroutine)
    {
        if (_coroutineStarter == null)
        {
            _coroutineStarter = CoroutineStarter.instance;
        }

        return _coroutineStarter.StartCoroutineDelegate(coroutine);
    }

    protected IEnumerator InvokePlayAnimation(RSRMonoBehaviour targetObj, AnimationEnum animEnum, float time)
    {
        yield return new WaitForSeconds(time);

        PlayTarget(targetObj, animEnum);
    }

    #endregion



}
