using UnityEngine;
using System.Collections;


/// <summary>
/// Class to maintain AnimObject asset creation.
/// 
/// Since AnimObjects are ScriptableObjects, they must have some
/// root, whether in the scene or in an actual asset--like a MonoBehaviour
/// they cannot simply exist and run. Creating assets lets you
/// individualize Animations for both objects and prefabs.
/// </summary>
public class AnimObjectAsset {

    public static ATween CreateTweenAsset()
    {
        return ScriptableObjectUtility.CreateAsset<ATween>();
    }

    public static ASpriteAnimation CreateSpriteAnimationAsset()
    {
        return ScriptableObjectUtility.CreateAsset<ASpriteAnimation>();
    }

    public static AParticleSystem CreateParticleSystemAsset()
    {
        return ScriptableObjectUtility.CreateAsset<AParticleSystem>();
    }
}
