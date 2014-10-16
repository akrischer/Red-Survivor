using UnityEngine;
using System.Collections;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;

/* Responsible for getting and setting hotkeys for all actions! */
/*
 * How to implement HotkeyController:
 *  - When you want to load hotkeys, call LoadHotkeys()
 *  - When you want to save current hotkeys, call SaveHotkeys()
 *  - When you want to see if a certain hotkey is being pressed:
 *      - Call IsHotkeyPressed(string hotkeyName)
 *  - When you want to set a new Hotkey:
        - call SetHotkeyBasedOnInput(string hotkeyName, bool isPrimary) in update
          which, as long as a key is being pressed, sets the hotkey to whatever keys
          are being pressed
 
 *  - When you want reset a hotkey, call ResetHotkey(string hotkeyName)
 *  - When you want to reset all hotkeys, call ResetAllHotkeys()
 *  
 * 
 * TO-DO:
 *  - Do not allow duplicate hotkeys
 *  - Do not allow primary hotkeys to contain SHIFT
 *  - Limit number of KeyCodes in a hotkey
 *  - 
 *  
 * */
public class HotkeyController
{
    public static HotkeyController instance = new HotkeyController();
    public List<Hotkey> allHotkeys = new List<Hotkey>();

    static float inputTimer = .2f;
    static float currentTimer = Time.time;

    #region Name of Hotkeys (as of now, must mirror XML file)
    public static string SELECT_NEXT_SQUAD = "Select Next Squad";
    public static string PHASE_CONTEXT = "Phase Context";
    public static string SCAVENGE_WORK = "Scavenge/Work";
    public static string MAIN_WEAPON_ACTION = "Main Weapon Action";
    public static string OFF_WEAPON_ACTION = "Off Weapon Action";
    public static string PAN_TO_TILE = "Pan To Tile";
    public static string WAIT = "Wait";
    #endregion

    #region Saving/Loading hotkeys

    /// <summary>
    /// Loads saved hotkeys from XML
    /// </summary>
    public static void LoadHotkeys()
    {
        instance.allHotkeys.Clear();

        string fp = Application.dataPath + "/Resources/XML/RSReHotkeys.xml";

        XmlSerializer serializer = new XmlSerializer(typeof(HotkeyController));
        using (FileStream stream = new FileStream(fp, FileMode.Open))
        {
            instance = serializer.Deserialize(stream) as HotkeyController;
        }
    }

    /// <summary>
    /// Saves current hotkey setup to XML
    /// </summary>
    public static void SaveHotkeys()
    {
        string fp = Application.dataPath + "/Resources/XML/RSReHotkeys.xml";

        XmlSerializer serializer = new XmlSerializer(typeof(HotkeyController));
        using (FileStream stream = new FileStream(fp, FileMode.Create))
        {
            serializer.Serialize(stream, instance);
        }
    }

    #endregion

    #region Get/Set Hotkeys

    /* Sets a new hotkey in allHotkeys by replacing it with a new hotkey.
     * If you need to find a specific hotkey, use GetHotkey() which you can find
     * via primary hotkey or hotkey name */
    public static bool SetHotkey(Hotkey oldHotkey, Hotkey newHotkey)
    {
        bool setNewHotkey = false;

        for (int i = 0; i < instance.allHotkeys.Count; i++)
        {
            /* If we find the hotkey, set it! */
            if (oldHotkey.Equals(instance.allHotkeys[i]))
            {
                instance.allHotkeys[i] = newHotkey;
                setNewHotkey = true;
                break;
            }
        }

        return setNewHotkey;
    }

    /// <summary>
    /// Attributes the keycodes being pressed by user at this moment to
    /// the hotkey name.
    /// </summary>
    /// <param name="hotkeyName">Valid name of a hotkey (refer to static hotkey var names above)</param>
    /// <param name="isPrimary">Is this hotkey a primary or secondary hotkey?</param>
    public static void SetHotkeyBasedOnInput(string hotkeyName, bool isPrimary)
    {
        /* Get's the keycodes of the keys currently being held down! */
        KeyCode[] newKeyCodes = GetInputKeyCodes();

        /* As long as any keys are being pressed... */
        if (newKeyCodes.Length > 0)
        {
            Hotkey oldHotkey = GetHotkey(hotkeyName); // which one we're replacing!

            Hotkey newHotkey;

            /* If it's a primary hotkey, we don't need to do anything special */
            if (isPrimary)
            {
                newHotkey = new Hotkey(hotkeyName, newKeyCodes, oldHotkey.GetSecondaryHotkey());
            }
                /* Else we need to add shift to the key codes, if it's not already there */
            else
            {
                /* Since it's a secondary hotkey, it techincally needs to also have shift in it! */
                if (!(newKeyCodes.Any(key => key == KeyCode.LeftShift)))
                {
                    List<KeyCode> list = newKeyCodes.ToList();
                    list.Add(KeyCode.LeftShift);
                    newKeyCodes = list.ToArray();
                }
                newHotkey = new Hotkey(hotkeyName, oldHotkey.GetPrimaryHotkey(), newKeyCodes);
            }

            if (!SetHotkey(oldHotkey, newHotkey))
            {
                Debug.LogWarning("No Hotkey '" + oldHotkey.ToString() + "' found for SetHotkey.");
            }
        }  
    }

    /// <summary>
    /// Returns the current keycodes being pressed.
    /// </summary>
    /// <returns>An array of keycodes of what the user is currently pressing</returns>
    public static KeyCode[] GetInputKeyCodes()
    {
        List<KeyCode> result = new List<KeyCode>();

        int KeyCodeLength = System.Enum.GetNames(typeof(KeyCode)).Length;

        for (int i = 0; i < KeyCodeLength; i++)
        {
            if (Input.GetKey((KeyCode)i))
            {
                result.Add((KeyCode)i);
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// Attempts to get a hotkey by its string name.
    /// Throws RuntimException if no hotkey found.
    /// </summary>
    /// <param name="hotkeyName">Valid hotkey name (refer to valid names at top of file)</param>
    /// <returns>Hotkey which has the string name</returns>
    public static Hotkey GetHotkey(string hotkeyName)
    {
        Hotkey result = instance.allHotkeys.FirstOrDefault(hot => hot.GetName() == hotkeyName);

        if (result == null)
        {
            throw new System.Exception("Cannot get hotkey " + hotkeyName + "; no such name exists." +
            "\nHotkeyController.GetHotkey");
        }
        else
        {
            return result;
        }
    }

    /// <summary>
    /// Get a hotkey by inputting its primary key codes.
    /// Throws RuntimeException if no such Hotkey is found.
    /// </summary>
    /// <param name="primaryKey">The keycodes attributed to the hotkey</param>
    /// <returns>The hotkey we're looking for.</returns>
    public static Hotkey GetHotkey(KeyCode[] primaryKey)
    {
        Hotkey result = instance.allHotkeys.First(hot => primaryKey.All(t => hot.GetPrimaryHotkey().Contains(t)) &&
            hot.GetPrimaryHotkey().All(s => primaryKey.Contains(s)));

        if (result == null)
        {
            throw new System.Exception("Cannot get hotkey that is bound to " + primaryKey.ToString() + "; no such hotkey exists." +
            "\nHotkeyController.GetHotkey");
        }
        else
        {
            return result;
        }
    }


    /// <summary>
    /// Is this hotkey being pressed?
    /// </summary>
    /// <param name="hotkeyName">String name of hotkey</param>
    /// <returns>Whether or not the hotkey is being pressed</returns>
    public static bool IsHotkeyPressed(string hotkeyName)
    {
        /* Limits how fast input can be pressed! */
        if (Time.time - currentTimer < inputTimer) { return false; }
        //else { currentTimer = Time.time; }

        KeyCode[] primaryCodes = GetHotkey(hotkeyName).GetPrimaryHotkey();
        KeyCode[] secondaryCodes = GetHotkey(hotkeyName).GetSecondaryHotkey();

        bool primary = primaryCodes.All(key => Input.GetKey(key)) && (primaryCodes.Length > 0);
        bool secondary = secondaryCodes.All(sKey => Input.GetKey(sKey)) && (secondaryCodes.Length > 0);

        if (primary)
        {
            //Debug.Log("HOTKEY PRESSED");
            currentTimer = Time.time;
            return true; 
        }
        else if (secondary)
        {
            //Debug.Log("HOTKEY PRESSED");
            currentTimer = Time.time;
            return true;
        }
        else { return false; }
    }

    #endregion

    #region Reset a Hotkey

    /// <summary>
    /// Resets a hotkey by resetting its primary and secondary keycodes.
    /// </summary>
    /// <param name="hotkeyName"></param>
    public static void ResetHotkey(string hotkeyName)
    {
        Hotkey hotkey = GetHotkey(hotkeyName);
        KeyCode[] emptyArray = new KeyCode[0];

        Hotkey emptyHotkey = new Hotkey(hotkeyName, emptyArray, emptyArray);

        SetHotkey(hotkey, emptyHotkey);
    }

    /// <summary>
    /// Resets all hotkeys by resetting each primary and secondary keycodes.
    /// </summary>
    public static void ResetAllHotkeys()
    {
        Hotkey emptyHotkey;
        KeyCode[] emptyArray = new KeyCode[0];
        foreach (Hotkey hotk in instance.allHotkeys)
        {
            emptyHotkey = new Hotkey(hotk.GetName(), emptyArray, emptyArray);
            SetHotkey(hotk, emptyHotkey);
        }
    }

    #endregion


    /// <summary>
    /// Represents a single hotkey. A hotkey has a name(string),
    /// a primary hotkey(KeyCode array), and a secondary hotkey(KeyCode array--SHIFT is automatically included)
    /// </summary>
    public class Hotkey
    {

        [XmlElement("hotkeyName")]
        public string _hotkeyName { get; set; }
        [XmlElement("primaryHotkey")]
        public KeyCode[] _primaryHotkey { get; set; }
        [XmlElement("secondaryHotkey")]
        public KeyCode[] _secondaryHotkey { get; set; }

        public Hotkey(string hotkeyName, KeyCode[] defHotkey, KeyCode[] secondaryHotkey)
        {
            _hotkeyName = hotkeyName;
            _primaryHotkey = defHotkey;
            _secondaryHotkey = secondaryHotkey;
        }

        public Hotkey() { }

        public string GetName()
        {
            return _hotkeyName;
        }
        public KeyCode[] GetPrimaryHotkey()
        {
            return _primaryHotkey;
        }
        public KeyCode[] GetSecondaryHotkey()
        {
            return _secondaryHotkey;
        }
        public void SetName(string str)
        {
            _hotkeyName = str;
        }
        public void SetPrimaryHotkey(KeyCode[] key)
        {
            _primaryHotkey = key;
        }
        public void SetSecondaryHotkey(KeyCode[] key)
        {
            _secondaryHotkey = key;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Hotkey))
            {
                return false;
            }
            else
            {
                Hotkey hotkey = (Hotkey)obj;
                return
                    (this._hotkeyName == hotkey.GetName()) &&
                    (this._primaryHotkey.All(t => hotkey.GetPrimaryHotkey().Contains(t)) &&
                        (hotkey.GetPrimaryHotkey().All(h => _primaryHotkey.Contains(h)))) &&
                    (this._secondaryHotkey.All(s => hotkey.GetSecondaryHotkey().Contains(s)) &&
                        (hotkey.GetSecondaryHotkey().All(shk => _secondaryHotkey.Contains(shk))));
            }
        }

        public override int GetHashCode()
        {
            return
                _hotkeyName.GetHashCode() +
                _primaryHotkey.GetHashCode() +
                _secondaryHotkey.GetHashCode();
        }

        public override string ToString()
        {
            return "name: " + _hotkeyName + 
                ", primary: " + KCArrayToString(_primaryHotkey) + 
                ", secondary: " + KCArrayToString(_secondaryHotkey);
        }

        /// <summary>
        /// Prints out a key code array in a readable manner--for debugging
        /// </summary>
        /// <param name="kcArray">KeyCode array to print</param>
        /// <returns></returns>
        static string KCArrayToString(KeyCode[] kcArray)
        {
            string result = "[";
            foreach (KeyCode kc in kcArray)
            {
                result += kc.ToString() + ", ";
            }
            if (kcArray.Length > 0)
            {
                result.Substring(0, result.Length - 2);
            }

            result += "]";

            return result;
        }
    }


}