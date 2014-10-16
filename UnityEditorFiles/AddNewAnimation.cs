using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

public class AddNewAnimation : EditorWindow {

    RSRAnimationController _targetAnimController;

    AnimObject.AnimationEnum _animEnum = AnimObject.AnimationEnum.Default;

    static AddNewAnimation window; //current newanimobject window

    bool willOverride = false; //Will adding this anim override an existing anim?


    public static void ShowAddNewAnimationWindow(RSRAnimationController animController)
    {
        window = EditorWindow.GetWindow<AddNewAnimation>(false, "Add New AnimObject");
        window.SetTargetAnimController(animController);
    }

    public void SetTargetAnimController(RSRAnimationController target)
    {
        _targetAnimController = target;
    }

    /* Let user choose an AnimationEnum. If that Enum already exists for the _targetAnimController,
     * show a message warning the user that this will replace the animation.
     * 
     * User can then press OK/Cancel */
    void OnGUI()
    {
        DrawAnimationEnum();

        /* If the currently selected _animEnum already exists as an animation for the current object,
         * show a warning to the user! */
        if (_targetAnimController.GetAnimObjectHolders().Count > 0)
        {
            if (_targetAnimController.GetAnimObjectHolders().Any(holder => holder.GetAnimationEnum() == _animEnum))
            {
                GUILayout.Space(15f);
                DrawOverrideWarning();
            }
        }

        GUILayout.Space(15f);

        /* OK/Cancel buttons */
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add New Animation", GUILayout.MaxWidth(70f)))
        {
            AnimObjectHolder newAOH = AnimObjectHolder.CreateNewAnimObjectHolder();
            newAOH.SetFields(_animEnum);

            _targetAnimController.SetAnimObjectHolder(newAOH);
            this.Close();
        }
        if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50f)))
        {
            this.Close();
        }
    }


    #region Draw Methods

    void DrawAnimationEnum()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField("Choose new Animation to add");

        _animEnum = (AnimObject.AnimationEnum)EditorGUILayout.EnumPopup(
            "",
            _animEnum,
            GUILayout.MaxWidth(300f));

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }

    void DrawOverrideWarning()
    {
        EditorGUILayout.LabelField("WARNING!!! \nAdding this animation will overwrite this object's current " +
            _animEnum.ToString() + " Animation!!!", GUILayout.MinHeight(100f));
    }

    #endregion
}
