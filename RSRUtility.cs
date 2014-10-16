using UnityEngine;
using System.Collections;

/* Contains a bunch of static utility functions */
public class RSRUtility {

    public static void SetupMember<T>(ref T member, GameObject gameObject) where T : UnityEngine.Component
    {
        T component = gameObject.GetComponent<T>();

        if (component == null)
        {
            if (member == null)
            {
                member = gameObject.AddComponent<T>();
            }
            else
            {
                Debug.LogWarning("gameObject.GetComponent<" + typeof(T).ToString() + "> returned null, but " +
                    member.ToString() + " is not!", gameObject);
            }
        }
        else
        {
            if (member != component)
            {
                member = component;
            }
        }
    }

}
