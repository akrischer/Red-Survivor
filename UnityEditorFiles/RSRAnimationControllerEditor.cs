using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

[CustomEditor(typeof(RSRAnimationController))]
public class RSRAnimationControllerEditor : Editor {

    RSRAnimationController _animController; //reference to AnimationController we're currently dealing with.

    GUIStyle TitleStyle;

    public void SetupStyles()
    {
        TitleStyle = new GUIStyle();
        TitleStyle.fontSize = 12;
        TitleStyle.normal.textColor = Color.white;
        TitleStyle.contentOffset = new Vector2(0, 4f);
    }


    public void OnEnable()
    {
        /* Get ref to animController */
        _animController = (RSRAnimationController)target;
        if (_animController.GetAnimObjectHolders() == null)
        {
            _animController.SetAnimObjectHolders(new AnimObjectHolder[]{});
        }
        SetupStyles();
    }

    /* Override Inspector GUI */
    public override void OnInspectorGUI()
    {

        // TO-DO: Draw the initial button which allows you to add an animation
        DrawAddAnimationButton();

        /* This foreach loop is responsible for DRAWING the header and body of
         * each AnimObjectHolder. Each AnimObjectHolder represents an animation "type"
         * (e.g. GetHit, Death, MeleeAttack...) */
        _animController.GetAnimObjectHolders().RemoveAll(holder => holder == null);
        foreach (AnimObjectHolder aoh in _animController.GetAnimObjectHolders())
        {
            aoh.GetAnimObjects().RemoveAll(a => a == null);
        }

        foreach (AnimObjectHolder aoh in _animController.GetAnimObjectHolders())
        {
            if (aoh == null)
            {
                Debug.LogWarning("AnimObjectHolder in _animController is null");
                Debug.Log(_animController.GetAnimObjectHolders().Count);
                continue;
            }
            DrawAnimationTitle(aoh.GetAnimationEnum());

            /* Draws each AnimObject */
            if (aoh.GetAnimObjects() != null)
            {

                foreach (AnimObject ao in aoh.GetAnimObjects())
                {
                    EditorGUILayout.BeginVertical();
                    DrawFoldoutHeader(ao, aoh); // draws header, with up/down buttons
                    EditorGUI.indentLevel++;
                    ao.SetDescription(EditorGUILayout.TextField("Description: ", ao.GetDescription(), GUILayout.MinWidth(250f)));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();

                    if (ao.foldout)
                    {
                        EditorGUI.indentLevel++;
                        DrawAnimObject(ao);
                        EditorGUI.indentLevel--;
                        //EditorGUILayout.EndFadeGroup();
                    }
                }
                GUILayout.Space(15f);
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_animController);
            //foreach (AnimObjectHolder aoh in _animController.GetAnimObjectHolders())
            //{
            //    EditorUtility.SetDirty(aoh);
            //}
        }
    }

    #region Specific AnimObject Draw Methods

    /* We can't use dynamic dispatch because these classes should
     * not directly access EditorGUILayout or GUILayout */
    public static void DrawAnimObject(AnimObject ao)
    {
        if (ao is ATween)
        {
            DrawATween((ATween)ao);
        }
        else if (ao is AParticleSystem)
        {
            DrawAParticleSystem((AParticleSystem)ao);
        }
        else if (ao is ASpriteAnimation)
        {
            DrawASpriteAnimation((ASpriteAnimation)ao);
        }

            /* When adding more AnimObjects, make sure to keep 
             * this else case. Just add more else ifs */
        else
        {
            throw new System.NotImplementedException("Cannot Draw Inspector info for AnimObject " + ao.ToString() +
            ": Not implemented");
        }
    }

    public static void DrawATween(ATween at)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Move Direction:");
        at.direction = (AnimObject.AnimationDirection)EditorGUILayout.EnumPopup(
            "",
            at.direction,
            GUILayout.MaxWidth(250f));
        EditorGUILayout.EndHorizontal();

        /* Show distance if location != Target or None */
        if (at.direction != AnimObject.AnimationDirection.Target &&
            at.direction != AnimObject.AnimationDirection.None)
        {
            at.distance = EditorGUILayout.IntField("Distance (hexes):", at.distance);
        }
        at.speed = EditorGUILayout.FloatField("Speed:", at.speed);
        at.speed = AnimObject.EnsureNoNegativeFloat(at.speed);

        DrawBaseAnimObject(at);
    }

    public static void DrawAParticleSystem(AParticleSystem aps)
    {
        aps.particleSystem = EditorGUILayout.ObjectField("Particle System:", aps.particleSystem, typeof(ParticleSystem), false)
    as ParticleSystem;

        aps.loop = EditorGUILayout.ToggleLeft("Loop", aps.loop);

        /* Create some space */
        GUILayout.Space(20f);

        /* Choose direction */
        GUILayout.BeginHorizontal();
        GUILayout.Label("Direction to play in:");
        aps.direction = (AnimObject.AnimationDirection)EditorGUILayout.EnumPopup(
            "",
            aps.direction,
            GUILayout.MaxWidth(250f));
        GUILayout.EndHorizontal();

        /* Show distance if location != Target or None */
        if (aps.direction != AnimObject.AnimationDirection.Target &&
            aps.direction != AnimObject.AnimationDirection.None)
        {
            aps.distance = EditorGUILayout.IntField("Distance (hexes):", aps.distance);
        }
        aps.speed = EditorGUILayout.FloatField("Speed:", aps.speed);
        aps.speed = AnimObject.EnsureNoNegativeFloat(aps.speed);

        /* Create some space */
        GUILayout.Space(20f);


        /* Draws concurrency/target stuff */
        DrawBaseAnimObject(aps);
    }

    public static void DrawASpriteAnimation(ASpriteAnimation asa)
    {
        //asa.spriteAnimation = EditorGUILayout.ObjectField("Sprite Animation:", asa.spriteAnimation, typeof(AnimationClip), false)
        //    as AnimationClip;

        //    spriteAnimation = EditorGUILayout.PropertyField(spriteAnimation, "Sprite Animation:", false, GUILayout.MaxWidth(300f))
        //as Animation;

        asa.speed = EditorGUILayout.FloatField("Speed:", asa.speed);
        asa.speed = AnimObject.EnsureNoNegativeFloat(asa.speed);

        asa.loop = EditorGUILayout.ToggleLeft("Loop", asa.loop);

        if (asa.loop)
        {
            EditorGUILayout.LabelField("Times To Loop (0 for infinite)");
            asa.timesToLoop = EditorGUILayout.IntField(asa.timesToLoop, GUILayout.MaxWidth(100f));
            asa.timesToLoop = AnimObject.EnsureNoNegativeInt(asa.timesToLoop);
        }

        /* Create some space */
        GUILayout.Space(20f);

        DrawBaseAnimObject(asa);
    }

    public static void DrawBaseAnimObject(AnimObject ao)
    {
        /* Draw the concurrency checkmark */
        ao.concurrent = EditorGUILayout.ToggleLeft("Concurrently Play AnimObject", ao.concurrent, GUILayout.ExpandWidth(true));

        /* Some space */
        GUILayout.Space(10f);

        /* Draw the Target Toggle + Target info, if it's toggled */
        ao.target = EditorGUILayout.ToggleLeft("Trigger Target Animation", ao.target, GUILayout.ExpandWidth(true));
        if (ao.target)
        {
            UnityEditor.EditorGUI.indentLevel++;
            ShowTargetGUI(ao);
            UnityEditor.EditorGUI.indentLevel--;
        }

        GUILayout.Space(10f);
    }

    public static void ShowTargetGUI(AnimObject ao)
    {
        GUILayout.BeginVertical();

        ao.waitForAnimObjectToEnd = UnityEditor.EditorGUILayout.ToggleLeft("Wait For AnimObject to End", ao.waitForAnimObjectToEnd, GUILayout.MinWidth(400f));


        ao.delayForTargetAnim = UnityEditor.EditorGUILayout.FloatField("Delay", ao.delayForTargetAnim, GUILayout.MaxWidth(200f));
        if (ao.delayForTargetAnim < 0f) { ao.delayForTargetAnim = 0f; } //ensure you can't have a negative delay


        GUILayout.BeginHorizontal(); //"(optional) Target animation play "
        GUILayout.FlexibleSpace();
        GUILayout.Label("(Optional) Target Animation to Play");
        ao.optionalTargetAnimationOverride = (AnimObject.AnimationEnum)UnityEditor.EditorGUILayout.EnumPopup(
            "",
            ao.optionalTargetAnimationOverride,
            GUILayout.MaxWidth(250f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    #endregion


    #region GUI Draw Methods

    void DrawAddAnimationButton()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add New Animation", GUILayout.MaxWidth(200f)))
        {
            //AnimObjectHolder[] testAohs = new AnimObjectHolder[] { TestAnimObjects.aoh1, TestAnimObjects.aoh2 };
            //_animController.SetAnimObjectHolders(testAohs);
            //TO DO: Window which let's you add a new animation!
            AddNewAnimation.ShowAddNewAnimationWindow(_animController);
        }
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
    }

    /* Draw the "header" for a single animation type you can create a sequence for.
     * EXAMPLE:
     * 'Animation 1...               (button)[Add AnimObject]' */
    void DrawAnimationTitle(AnimObject.AnimationEnum animEnum)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(animEnum.ToString() + " Animation", TitleStyle, GUILayout.Height(20f)); //show title of animation

        if (GUILayout.Button("X", GUILayout.MaxWidth(25f)))
        {
            _animController.RemoveAnimObjectHolder(animEnum);
        }
        GUILayout.FlexibleSpace(); // fill up the middle
        GUILayout.Label(". . .");
        GUILayout.FlexibleSpace();

        //TO-DO: Add in a Button for adding a new AnimObject
        if (GUILayout.Button("Add AnimObject"))
        {
            //TO-DO: Implement adding a new AnimObject
            AddNewAnimObject.ShowAddNewAnimObjectWindow(_animController, animEnum);
        }

        GUILayout.EndHorizontal();
    }

    /* Draws the header for each foldout for each AnimObject */
    void DrawFoldoutHeader(AnimObject ao, AnimObjectHolder aoh)
    {
        EditorGUILayout.BeginHorizontal();
        ao.foldout = EditorGUILayout.Foldout(ao.foldout, ao.ToString());

        

        if (GUILayout.Button("UP", GUILayout.Width(50f)))
        {
            aoh.MoveAnimObjectUp(ao);
        }   
        if (GUILayout.Button("DOWN", GUILayout.Width(50f)))
        {
            aoh.MoveAnimObjectDown(ao);
        }
        GUILayout.Space(10f);
        if (GUILayout.Button("X", GUILayout.Width(25f)))
        {
            aoh.RemoveAnimObject(ao);
        }

        GUILayout.Space(60f);

        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region Create New Windows

    void CreateNewAddNewAnimObjectWindow(AnimObject.AnimationEnum animationType)
    {
        AddNewAnimObject.ShowAddNewAnimObjectWindow(_animController, animationType);
    }

    #endregion
}
