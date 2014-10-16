using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

public class AddNewAnimObject : EditorWindow {

    RSRAnimationController _targetAnimController;//The target AnimObject we're working on.

    AnimObject.AnimObjectType _animObjectType; //Whether this new AnimObject will be a tween, sprite animation, or particle effect

    static AddNewAnimObject window; //current newanimobject window

    AnimObjectHolder _targetAOH;

    #region ATween variables
    AnimObject.AnimationDirection tweenDirection; //In what direction should the object move?

    int tweenDistance; //Assuming direction != target or none, how many hexes should it move in the given direction?

    float tweenSpeed; //How many hexes should it cross per second?
    #endregion

    #region ASpriteAnimation variables
    AnimationClip spriteAnimation; // anim to play

    float spriteSpeed = 1f; //how fast to play it, as a percentage of normal speed

    bool spriteLoop = false; // should this spriteAnimation loop?

    int spriteTimesToLoop = 0; // IF loop == true, how many times should it loop?
    #endregion

    #region AParticleSystem variables
    ParticleSystem particleSystem; //particle system to play

    bool particleLoop = false; // should this ps loop?

    AnimObject.AnimationDirection particleDirection; // In which direction should this PS play?

    int particleDistance; // Assuming direction != target or none, how many hexes away does it play from this object?

    float particleSpeed = 1f; //speed PS plays at, as a percentage of regular speed.
    #endregion

    #region Common variables
    /* Concurrent? Should this AnimObject not lock other AnimObjects in the sequence from playing? */
    bool commonConcurrent;

    /* Is this info being folded out? */
    bool commonFoldout = false;

    /* Editable Description for this animation */
    string commonDescription = "";

    /* Target? Should this AnimObject play its target's animation?
     * IF THERE IS NO TARGET, nothing will happen */
    bool commonTarget;

    // IF target == true, we need to know the following...
    /* Wait for this AnimObject to end before starting target anim? */
    bool commonWaitForAnimObjectToEnd;

    /* Delay for starting target's animation.
     * IF waitForAnimObjectToEnd == true, delay applies AFTER this AnimObject finishes */
    float commonDelayForTargetAnim = 0f;

    /* (Optional): Override target animation to play. Normally the target will
     * know what animation to play via the state of the game. */
    AnimObject.AnimationEnum commonOptionalTargetAnimationOverride;
    #endregion


    /// <summary>
    /// Use this function to show a new editor window for creating a new AnimObject asset.
    /// </summary>
    /// <param name="animController">The AnimationController that this AnimObject will be added to</param>
    /// <param name="animation">Which "Animation" we're adding this AnimObject to</param>
    public static void ShowAddNewAnimObjectWindow(RSRAnimationController animController, AnimObject.AnimationEnum animation)
    {
        window = EditorWindow.GetWindow<AddNewAnimObject>(false, "Add New AnimObject");
        window.SetTargetAnimController(animController);

        /* Now we need to find which Animation we're dealing with */
        window._targetAOH = window._targetAnimController.GetAnimObjectHolders().First(aoh => aoh.GetAnimationEnum() == animation);

    }

    /// <summary>
    /// Unity Editor GUI call, which is called every update frame.
    /// </summary>
    void OnGUI()
    {
        _animObjectType = (AnimObject.AnimObjectType)EditorGUILayout.EnumPopup("AnimObject Type:", _animObjectType, GUILayout.MaxWidth(300f));

        if (_animObjectType == AnimObject.AnimObjectType.AParticleSystem)
        {
            ShowParticleSystemVariables();
        }
        else if (_animObjectType == AnimObject.AnimObjectType.ASpriteAnimation)
        {
            ShowSpriteAnimationVariables();
        }
        else if (_animObjectType == AnimObject.AnimObjectType.ATween)
        {
            ShowTweenVariables();
        }

        GUILayout.Space(100f);

        /* Show OK/Close buttons */
        if (GUILayout.Button("Create New AnimObject"))
        {
            switch (_animObjectType)
            {
                case AnimObject.AnimObjectType.ATween:
                    ATween newTween = AnimObject.CreateTweenAnimObject();
                    newTween.speed = tweenSpeed;
                    newTween.direction = tweenDirection;
                    newTween.distance = tweenDistance;
                    SetCommonVariables(newTween);

                    _targetAOH.AddToCollection(newTween);
                    break;
                case AnimObject.AnimObjectType.ASpriteAnimation:
                    ASpriteAnimation newSpriteAnimation = AnimObject.CreateSpriteAnimationAnimObject(_targetAnimController.GetComponent<Animator>());
                    newSpriteAnimation.loop = spriteLoop;
                    newSpriteAnimation.speed = spriteSpeed;
                    //newSpriteAnimation.spriteAnimation = spriteAnimation;
                    newSpriteAnimation.timesToLoop = spriteTimesToLoop;
                    SetCommonVariables(newSpriteAnimation);

                    _targetAOH.AddToCollection(newSpriteAnimation);
                    break;
                case AnimObject.AnimObjectType.AParticleSystem:
                    AParticleSystem newParticleSystem = AnimObject.CreateParticleSystemAnimObject();
                    newParticleSystem.loop = particleLoop;
                    newParticleSystem.speed = particleSpeed;
                    newParticleSystem.particleSystem = particleSystem;
                    newParticleSystem.direction = particleDirection;
                    newParticleSystem.distance = particleDistance;
                    SetCommonVariables(newParticleSystem);

                    _targetAOH.AddToCollection(newParticleSystem);
                    break;
            }

            this.Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }

    }

    public void SetTargetAnimController(RSRAnimationController target)
    {
        _targetAnimController = target;
    }

    #region Show Specific Kind of Variables

    #region Common Stuff --- ShowCommonVariables()
    protected void ShowTargetGUI()
    {
        GUILayout.BeginVertical();

        commonWaitForAnimObjectToEnd = UnityEditor.EditorGUILayout.ToggleLeft(
            "Wait For AnimObject to End", commonWaitForAnimObjectToEnd, GUILayout.MinWidth(400f));


        commonDelayForTargetAnim = UnityEditor.EditorGUILayout.FloatField("Delay", commonDelayForTargetAnim, GUILayout.MaxWidth(200f));
        commonDelayForTargetAnim = AnimObject.EnsureNoNegativeFloat(commonDelayForTargetAnim);//ensure you can't have a negative delay


        GUILayout.BeginHorizontal(); //"(optional) Target animation play "
        GUILayout.FlexibleSpace();
        GUILayout.Label("(Optional) Target Animation to Play");
        commonOptionalTargetAnimationOverride = (AnimObject.AnimationEnum)UnityEditor.EditorGUILayout.EnumPopup(
            "",
            commonOptionalTargetAnimationOverride,
            GUILayout.MaxWidth(250f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    public virtual void ShowCommonVariables()
    {
        /* Draw the concurrency checkmark */
        commonConcurrent = UnityEditor.EditorGUILayout.ToggleLeft("Concurrently Play AnimObject", commonConcurrent, GUILayout.ExpandWidth(true));

        /* Some space */
        GUILayout.Space(10f);

        commonDescription = EditorGUILayout.TextField("Description", GUILayout.MinWidth(100f));
        /* Draw the Target Toggle + Target info, if it's toggled */
        commonTarget = UnityEditor.EditorGUILayout.ToggleLeft("Trigger Target Animation", commonTarget, GUILayout.ExpandWidth(true));
        if (commonTarget)
        {
            UnityEditor.EditorGUI.indentLevel++;
            ShowTargetGUI();
            UnityEditor.EditorGUI.indentLevel--;
        }
        GUILayout.Space(10f);
    }
    #endregion


    /// <summary>
    /// Shows variables for AParticleSystem AnimObject
    /// </summary>
    void ShowParticleSystemVariables()
    {
        particleSystem = EditorGUILayout.ObjectField("Particle System:", particleSystem, typeof(ParticleSystem), false)
    as ParticleSystem;

        particleLoop = EditorGUILayout.ToggleLeft("Loop", particleLoop);

        /* Create some space */
        GUILayout.Space(20f);

        /* Choose direction */
        GUILayout.BeginHorizontal();
        GUILayout.Label("Direction to play in:");
        particleDirection = (AnimObject.AnimationDirection)EditorGUILayout.EnumPopup(
            "",
            particleDirection,
            GUILayout.MaxWidth(250f));
        GUILayout.EndHorizontal();

        /* Show distance if location != Target or None */
        if (particleDirection != AnimObject.AnimationDirection.Target &&
            particleDirection != AnimObject.AnimationDirection.None)
        {
            particleDistance = EditorGUILayout.IntField("Distance (hexes):", particleDistance);
        }
        particleSpeed = EditorGUILayout.FloatField("Speed:", particleSpeed);
        particleSpeed = AnimObject.EnsureNoNegativeFloat(particleSpeed);

        ShowCommonVariables();
    }

    /// <summary>
    /// Shows variables for ATween AnimObject
    /// </summary>
    void ShowTweenVariables()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Move Direction:");
        tweenDirection = (AnimObject.AnimationDirection)EditorGUILayout.EnumPopup(
            "",
            tweenDirection,
            GUILayout.MaxWidth(250f));
        EditorGUILayout.EndHorizontal();

        /* Show distance if location != Target or None */
        if (tweenDirection != AnimObject.AnimationDirection.Target &&
            tweenDirection != AnimObject.AnimationDirection.None)
        {
            tweenDistance = EditorGUILayout.IntField("Distance (hexes):", tweenDistance);
        }
        tweenSpeed = EditorGUILayout.FloatField("Speed:", tweenSpeed);
        tweenSpeed = AnimObject.EnsureNoNegativeFloat(tweenSpeed);

        ShowCommonVariables();
    }

    /// <summary>
    /// Shows variables for ASpriteAnimation AnimObject
    /// </summary>
    void ShowSpriteAnimationVariables()
    {
        spriteAnimation = EditorGUILayout.ObjectField("Particle System:", spriteAnimation, typeof(AnimationClip), false)
        as AnimationClip;

        spriteSpeed = EditorGUILayout.FloatField("Speed:", spriteSpeed);
        spriteSpeed = AnimObject.EnsureNoNegativeFloat(spriteSpeed);

        spriteLoop = EditorGUILayout.ToggleLeft("Loop", spriteLoop);

        if (spriteLoop)
        {
            spriteTimesToLoop = EditorGUILayout.IntField("Times To Loop (0 for infinite)", spriteTimesToLoop);
        }

        /* Create some space */
        GUILayout.Space(20f);

        ShowCommonVariables();
    }

    #endregion


    /// <summary>
    /// Common variables are variables that are common to each AnimObject type.
    /// </summary>
    /// <param name="ao">The AnimObject we're modifying</param>
    void SetCommonVariables(AnimObject ao)
    {
        ao.waitForAnimObjectToEnd = commonWaitForAnimObjectToEnd;
        ao.target = commonTarget;
        ao.optionalTargetAnimationOverride = commonOptionalTargetAnimationOverride;
        ao.foldout = commonFoldout;
        ao.description = commonDescription;
        ao.delayForTargetAnim = commonDelayForTargetAnim;
        ao.concurrent = commonConcurrent;
    }

}
