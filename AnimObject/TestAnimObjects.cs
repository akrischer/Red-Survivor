using UnityEngine;
using System.Collections;

/* A simple class for testing Anim Objects */
[System.Serializable]
public class TestAnimObjects : MonoBehaviour {

    public static ATween ao1;
    public static ASpriteAnimation ao2;
    public static AParticleSystem ao3;
    public static ASpriteAnimation ao4;

    public static AnimObjectHolder aoh1;
    public static AnimObjectHolder aoh2;

    public static bool hasInited = false;

    public static void InitTests()
    {
        //ao1 = AnimObject.CreateTweenAnimObject();
        //ao1.direction = AnimObject.AnimationDirection.N;
        //ao1.distance = 2;
        //ao1.speed = 2f;
        //
        //ao2 = AnimObject.CreateSpriteAnimationAnimObject();
        //ao2.loop = false;
        //ao2.speed = 1f;
        //
        //ao3 = AnimObject.CreateParticleSystemAnimObject();
        //ao3.speed = 1f;
        //
        //ao4 = AnimObject.CreateSpriteAnimationAnimObject();
        //
        //aoh1 = AnimObjectHolder.CreateInstance<AnimObjectHolder>();
        //aoh1.SetFields(AnimObject.AnimationEnum.GetHit);
        //aoh2 = AnimObjectHolder.CreateInstance<AnimObjectHolder>();
        //aoh2.SetFields(AnimObject.AnimationEnum.Death);
        //
        //aoh1.AddToCollection(ao1);
        //aoh1.AddToCollection(ao2);
        //
        //
        //aoh2.AddToCollection(ao4);
        //aoh2.AddToCollection(ao3);
        //
        //
        //hasInited = true;
    }
}
