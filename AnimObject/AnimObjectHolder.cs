using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
/* An AnimObjectHolder holds a series of AnimObjects. As a whole, an AnimObjectHolder
 * represents a single animation--the individual AnimObjects are modular components of the animation */
public class AnimObjectHolder {

    [SerializeField]
    List<AnimObject> _animObjectCollection;
    [SerializeField]
    internal AnimObject.AnimationEnum animationEnum; // What animation is this?

    #region Constructors

    void OnEnable()
    {
        //hideFlags = HideFlags.HideAndDontSave;
        if (_animObjectCollection == null)
        {
            _animObjectCollection = new List<AnimObject>();
        }
    }

    /* Use these methods for setting the fields of an AnimObjectHolder */
    public void SetFields(AnimObject[] aoc) { SetFields(aoc, AnimObject.AnimationEnum.Default); }
    public void SetFields(AnimObject.AnimationEnum aEnum) { SetFields(new AnimObject[0], aEnum); }
    public void SetFields(AnimObject[] animObjectCollection, AnimObject.AnimationEnum aEnum)
    {
        _animObjectCollection = animObjectCollection.ToList();
        animationEnum = aEnum;
    }
    public void SetField(List<AnimObject> listAnimObjects, AnimObject.AnimationEnum aEnum)
    {
        _animObjectCollection = listAnimObjects;
        animationEnum = aEnum;
    }


    /* Creates and returns a new AnimObjectHolder */
    public static AnimObjectHolder CreateNewAnimObjectHolder()
    {
        return new AnimObjectHolder();
    }
    //public AnimObjectHolder(AnimObject[] animObjectCollection)
    //    {
    //        _animObjectCollection = animObjectCollection;
    //        animationEnum = AnimObject.AnimationEnum.Default;
    //    }
    //
    //public AnimObjectHolder(AnimObject[] animObjectCollection, AnimObject.AnimationEnum aEnum)
    //    {
    //        _animObjectCollection = animObjectCollection;
    //        animationEnum = aEnum;
    //    }
    //
    //public AnimObjectHolder(AnimObject.AnimationEnum aEnum)
    //    {
    //        _animObjectCollection = new AnimObject[0];
    //        animationEnum = aEnum;
    //    }

    #endregion

        #region Set/Get Methods

        public void SetAnimObject(List<AnimObject> listAnimObjects)
        {
            Debug.Log("For " + animationEnum.ToString() + ": collection now has " + listAnimObjects.Count.ToString() + " objects");
            _animObjectCollection = listAnimObjects;
        }
        public void SetAnimObjects(AnimObject[] animObjectCollection)
        {
            Debug.Log("For " + animationEnum.ToString() + ": collection now has " + animObjectCollection.Length.ToString() + " objects");
            _animObjectCollection = animObjectCollection.ToList();
        }

        public List<AnimObject> GetAnimObjects()
        {
            return _animObjectCollection;
        }

        public void SetAnimationEnum(AnimObject.AnimationEnum aEnum)
        {
            animationEnum = aEnum;
        }

        public AnimObject.AnimationEnum GetAnimationEnum()
        {
            return animationEnum;
        }
        #endregion

        #region Add To Collection Methods
        public List<AnimObject> AddToCollection(AnimObject animObject)
        {
            if (_animObjectCollection == null)
            {
                _animObjectCollection = new List<AnimObject>();
            }
            _animObjectCollection.Add(animObject);

            return _animObjectCollection;
        }

        public List<AnimObject> AddToCollection(AnimObject[] animObjects)
        {
            foreach (AnimObject ao in animObjects)
            {
                AddToCollection(ao);
            }

            return _animObjectCollection;
        }
        #endregion

        #region Reordering/Removing _animObjectCollection
        
        /* Moves the specified AnimObject up in the order of execution.
         * Since we're working with animations, order matters */
        public void MoveAnimObjectUp(AnimObject ao)
        {
            for (int i = 0; i < _animObjectCollection.Count; i++)
            {
                if (ao.Equals(_animObjectCollection[i]) && i != 0)
                {
                    AnimObject temp = _animObjectCollection[i - 1];
                    _animObjectCollection[i - 1] = ao;
                    _animObjectCollection[i] = temp;

                    break;
                }
            }
        }

        /* Moves the specified AnimObject down in the order of execution.
         * Since we're working with animations, order matters! */
        public void MoveAnimObjectDown(AnimObject ao)
        {
            for (int i = 0; i < _animObjectCollection.Count; i++)
            {
                if (ao.Equals(_animObjectCollection[i]) && i != _animObjectCollection.Count - 1)
                {
                    AnimObject temp = _animObjectCollection[i + 1];
                    _animObjectCollection[i + 1] = ao;
                    _animObjectCollection[i] = temp;

                    break;
                }
            }
        }

        /*Removes the specified AnimObject from the collection of AnimObjects, if found */
        public void RemoveAnimObject(AnimObject ao)
        {
            _animObjectCollection.Remove(ao);
            ScriptableObjectUtility.DeleteAsset(ao);
        }
        #endregion

        public override string ToString()
        {
            try
            {
                string result = "[";

                foreach (AnimObject ao in _animObjectCollection)
                {
                    result += ao.ToString() + ", ";
                }

                if (_animObjectCollection.Count > 0)
                {
                    result.Substring(0, result.Length - 2);
                }

                result += "]";

                return result;
            }
            catch (System.NullReferenceException e)
            {
                throw new System.NullReferenceException(" Null Ref Exception:" + e.ToString());
            }

        }
}
