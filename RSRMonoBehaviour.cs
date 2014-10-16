using UnityEngine;
using System.Collections;


/* All Game Object behavior scripts should extend this class */
public class RSRMonoBehaviour : MonoBehaviour
{
    public enum FaceDirection { Default, Left, Right, Up, Down };

    protected int movesMax = 2;
    protected int movesRemaining = 2;

    protected int sightRange = 2;

    protected int squadSize = 5;

    protected Vector3 coords;

    public RSRAnimationController _animController;

    #region Debug Variables

    static bool hasTriggeredMakeSpriteFaceWarning = false;
    static bool hasTriggeredGetDirectionWarning = false;

    #endregion

    void OnEnable()
    {
        if (_animController == null)
        {
		RSRUtility.SetupMember<RSRAnimationController>(_animController, gameObject);
            _animController = gameObject.GetComponent<RSRAnimationController>();
        }
    }


    #region Move Methods

    public virtual bool isValidMove(Tile t)
    {
        int dist = GetTile().Dist(t);
        return (movesRemaining >= dist && dist > 0) && t.isWalkable() && !t.isOccupied();
    }

    public virtual bool hasMovesRemaining()
    {
        return movesRemaining > 0;
    }

    public virtual void RefreshMoves()
    {
        movesRemaining = movesMax;
        //SwitchToStar_Yellow();
    }

    public virtual void DepleteMoves()
    {
        movesRemaining = 0;
        //SwitchToStar_Red();
    }

    public virtual void DepleteMoves(int moves)
    {
        movesRemaining -= moves;
        if (movesRemaining < 1)
        {
            DepleteMoves();
        }
    }

    //Removes this squad from the current tile and then Places at Tile t
    //Check whether moves are valid with isValidMove first
    public virtual void MoveTo(Tile t)
    {
        //Debug.Log("rsrmb MoveTo");
        GetTile().ClearSquad();
        DepleteMoves(GetTile().Dist(t));

        PlaceTo(t);
    }
    public virtual void MoveTo(Vector3 gameCoords)
    {
        //Debug.Log("MOVETO USING GAME COORDS");
        GetTile().ClearSquad();
        Tile t = Grid.Find(gameCoords);
        DepleteMoves(GetTile().Dist(t));

        PlaceTo(t);
    }

    #endregion


    #region Coordinates
    public int x;
    public int y;
    //The z coordinate is always generated programmatically when needed, as the sum of x, y, and z always equals 0

    public virtual Tile GetTile()
    {
        return Grid.Find(GetCoords());
    }

	//Places GameObject on Tile t -- doesn't move transform of game object
	//Should only be called directly when creating new squads - turn-to-turn movement should be done with MoveTo()
    public virtual void PlaceTo(Tile t, bool teleport = false)
    {
        
        SetTile(t);
        t.SetGameObject(this);
        if (teleport)
        {
            gameObject.transform.position = Grid.GetWorldCoords(t.GetCoords());
            //Debug.Log("place to TELEPORT");
        }
    }

    public virtual void SetTile(Tile t)
    {
        coords = t.GetCoords();
        x = (int)coords.x;
        y = (int)coords.y;
    }

    public static int GetZ(Vector2 vec)
    {
        return GetZ(vec.x, vec.y);
    }

    public static int GetZ(int x, int y)
    {
        return (0 - x - y);
    }

    public static int GetZ(float x, float y)
    {
        return (int)(0 - x - y);
    }

    //Returns the x, y, and z coordinates of this tile in a Vector3
    public Vector3 GetCoords()
    {
        return new Vector3(x, y, GetZ(x, y));
    }

    //Sets x, y, and z for a game object
    public void SetGridCoords(Vector3 newCoords) { SetGridCoords((Vector2)newCoords); }
    public void SetGridCoords(Vector2 newCoords) 
    {
        float newX = newCoords.x;
        float newY = newCoords.y;
        x = (int)newX;
        y = (int)newY;
        coords = new Vector3(x, y, GetZ(x, y));
    }


    //Returns the grid distance between point v1 and point v2
    public static int Dist(Vector3 v1, Vector3 v2)
    {
        int xdist = Mathf.Abs((int)v1.x - (int)v2.x);
        int ydist = Mathf.Abs((int)v1.y - (int)v2.y);
        int zdist = Mathf.Abs((int)v1.z - (int)v2.z);

        return Mathf.Max(xdist, ydist, zdist);
    }


    #endregion


    #region Animation Methods
    /* All possible animation methods a game object could ever use.
     * Note not all game objects will (or necessarily should) implement every
     * animation. */
    public void PlayAnimation(AnimObject.AnimationEnum animEnum) { PlayAnimation(animEnum, gameObject.GetComponent<RSRMonoBehaviour>(), animEnum); }
    public void PlayAnimation(AnimObject.AnimationEnum thisAnimEnum, RSRMonoBehaviour target, AnimObject.AnimationEnum targetAnimEnum)
    {
        if (_animController == null)
        {
            throw new System.NullReferenceException("PlayAnimation(" + target.ToString() + ", " +
            thisAnimEnum.ToString() + ") --> _animController null for " + gameObject.ToString());
        }
        else
        {
            StartCoroutine(PlayAnyAnimation(target, thisAnimEnum, targetAnimEnum));
        }
    }

    IEnumerator PlayAnyAnimation(RSRMonoBehaviour targetObj, AnimObject.AnimationEnum thisAnimEnum, AnimObject.AnimationEnum targetAnimEnum)
    {
        //Debug.Log("Trying to play Animation " + thisAnimEnum.ToString() + " for object " + gameObject.ToString());
        AnimObjectHolder aoh;

        if (_animController.TryGetAnimObjectHolder(thisAnimEnum, out aoh))
        {
            //Debug.Log(thisAnimEnum.ToString() + " found for " + gameObject.ToString());
            for (int i = 0; i < aoh.GetAnimObjects().Count; i++)
            {
                AnimObject ao = aoh.GetAnimObjects()[i];

                if (ao.concurrent)
                {
                    StartCoroutine(ao.Play(this, targetObj, targetAnimEnum));
                }
                else
                {
                    yield return StartCoroutine(ao.Play(this, targetObj, targetAnimEnum));
                }
            }
        }
        else
        {
            throw new System.NotImplementedException("PlayAnimation(" + targetObj.ToString() + ", " +
            thisAnimEnum.ToString() + ") --> Could not find Animation " + thisAnimEnum.ToString() + " for " + gameObject.ToString());
        }
    }


    /* This can be overriden to make the object face a certain direction */
    public virtual void MakeSpriteFace(FaceDirection dir)
    {
        if (!hasTriggeredMakeSpriteFaceWarning)
        {
            Debug.LogWarning("FaceDirection: " + dir.ToString() + " called, but " + gameObject.ToString() + " has not implemented this method.");
            hasTriggeredMakeSpriteFaceWarning = true;
        }  
    }


    /* Based on a vector, determines what "direction" this object is moving in.
     * Can be overriden since not all objects have the same directions they can move in. */
    public virtual FaceDirection GetDirection(Vector3 moveDir)
    {
        if (!hasTriggeredGetDirectionWarning)
        {
            Debug.LogWarning("Cannot get direction " + gameObject.ToString() + " is moving in; not implemented");
            hasTriggeredGetDirectionWarning = true;
        }
        return FaceDirection.Default;
    }
    #endregion
}
