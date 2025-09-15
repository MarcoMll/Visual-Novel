using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomInspector.Documentation
{
    public static class CustomInspectorAttributeDescriptions
    {
        public static Dictionary<NewPropertyD, string> descriptions = new()
        {

            { NewPropertyD.HorizontalLineAttribute,
            "A dividing line is perfect to structure your inspector" },


            { NewPropertyD.MessageBoxAttribute,
            "Sometimes you just want to write things as info in the inspector. " +
            "A [ReadOnly] string often does the trick, but these messages are more intrusive. The icon is defined by the parameter MessageType" },


            { NewPropertyD.AsRangeAttribute,
            "[AsRange] will interpret your Vector2 as a range between given minLimit and maxLimit. " +
            "It lets you choose values from a slider with changable size." },


            { NewPropertyD.AssetsOnlyAttribute,
            "Don't want scene objects in your fields? " +
            "Lock on assets and make it impossible to assign scene objects here. (Assets are saved things like e.g. a prefab or an imported Mesh)" },


            { NewPropertyD.ButtonAttribute,
            "The bigger the program gets, the more you want to be able to simply execute functions on your own without the whole thing. " +
            "Buttons can also be used very well for editor scripts: " +
            "for example, for a script that centers a UI element, adds a component or creates objects at the push of a button" },


            { NewPropertyD.ShowIfAttribute,
            "Some variables are simply not needed in certain constellations. " +
            "Instead of making your inspector unnecessarily confusing, you can simply hide them. " +
            "You can use nameof() to refer to a boolean/method's name or you can specify a string. " +
            "You can use the exclamation mark (!) to invert the string, as well as certain keywords such as \"isPlaying\" (test whether the game is currently running), \"true\" or \"false\". " +
            "\nThe opposite of ShowIfNotAttribute. Indents field automatically, but you can revert with Indent(-1)" },


            { NewPropertyD.ShowIfIsAttribute,
            "Similar to the ShowIfAttribute, but instead of passing references you pass one reference and one actual value. " +
            "It is then tested whether they have the same value. " +
            "Mostly you want to use ShowIfAttribute instead, because you cannot use functions, you are restricted to only comparing two and you can only pass constants as 2nd attribute parameter:" +
            "\nbools, numbers, strings, Types, enums, and arrays of those types" },


            { NewPropertyD.ShowIfNotAttribute,
            "Some variables are simply not needed in certain constellations. " +
            "Instead of making your inspector unnecessarily confusing, you can simply hide them. " +
            "You can use nameof() to refer to a boolean/method, or you can specify a string. " +
            "You can use the exclamation mark (!) to invert the string, as well as certain keywords such as \"isPlaying\" (test whether the game is currently running), \"true\" or \"false\"." +
            "The opposite of ShowIfAttribute" },


            { NewPropertyD.CopyPasteAttribute,
            "Makes it easy to copy values between variables (or programs)" +
            "- uses the system clipboard" },


            { NewPropertyD.DecimalsAttribute,
            "Maybe because of the inaccuracy of floats or whatever - here you can only allow numbers up to certain decimal places" },


            { NewPropertyD.DisplayAutoPropertyAttribute,
            "If you use features of c# like some syntactic sugar, you will unfortunately notice that the unity inspector doesn't like it that much and .. doesn't serialize auto-properties (Simply using SerializeField doesnt work). " +
            "With this you can still display auto-properties in the inspector. " +
            "Since unity does not save it, values can only be changed runtime in the inspector" },


            { NewPropertyD.ForceFillAttribute,
            "One of the most important attributes! " +
            "You can use it anywhere! " +
            "It clearly indicates that this field must be filled out." +
            "\nYou can define forbidden values with format: every type as ToString() (e.g. Vector3 -> (1, 1, 1))" +
            "\nCheckForceFilled function:\n" +
            "use ForceFill.CheckForceFilled(this) to test an object to see if you really haven't forgotten to fill in a field (useful in Awake)\n" +
            "The test function is excluded in build automatically"},


            { NewPropertyD.GetSetAttribute,
            "Of course, you can validate input afterwards, but you can also add a getter and setter directly here." +
            "\r\nWarning: If you don't make any changes to serialized fields on the actual object in the setter, " +
            "e.g. if you only change other objects in the scene or static fields, " +
            "then it will not be saved because unity thinks nothing has been changed" },


            { NewPropertyD.GUIColorAttribute,
            "With this you can change the color of the GUI from one field, or from this field for the whole GUI" },


            { NewPropertyD.HideFieldAttribute,
            "This hides the fields in the inspector that are serialized by default, unlike the build-in [HideInInspector] hides everything attached to the field." +
            "\nYou can see in the example below, that HideInInspector hides other attributes too but HideField keeps previous Attributes like the Header here." +
            "\nHint: HideInInspector will hide a whole list. HideField hides only the field in the element of a list."},


            { NewPropertyD.HookAttribute,
            "This calls a method, if the value was changed in the inspector. " +
            "The method can be without parameters or with 2 parameters (oldValue, newValue) of same type of the field" +
            "\nWarning: If you change values of non-serialized fields/properties, like for example statics, they wont be saved"},


            { NewPropertyD.BackgroundColorAttribute,
            "This can be used to highlight certain fields" },


            { NewPropertyD.HorizontalGroupAttribute,
            "Surely everyone has already thought about placing input fields next to each other in Unity so that they take up less space. " +
            "It is also very useful for structuring or for comparing two classes. " +
            "Note: You begin a new HorizontalGroup by setting the parameter beginNewGroup=true" +
            "\n- Doesnt work with reordable lists" },


            { NewPropertyD.IndentAttribute,
            "For indenting or un-indenting your fields."},


            { NewPropertyD.LabelSettingsAttribute,
            "Edit where the input field is and if the label is shown"},


            { NewPropertyD.LayerAttribute,
            "If an integer represents a layer, it is very difficult to tell in the inspector which number belongs to which layer. " +
            "This attribute facilitates the assignment - you can select a single layer from an enum dropdown." },


            { NewPropertyD.PreviewAttribute,
            "Filenames can be long and sometimes assets are not easy to identify in the inspector. " +
            "With a preview you can see directly what kind of asset it is" },

            { NewPropertyD.ProgressBarAttribute,
            "Use on floats and ints to show a progressbar that finishes at given max" },


            { NewPropertyD.ReadOnlyAttribute,
            "Want to see everything? Knowledge is power, but what if you don't want that variable to be edited in the inspector? " +
            "With this attribute you can easily make fields visible in the inspector without later wondering if you should add something to the field if it is null by default"},


            { NewPropertyD.RequireTypeAttribute,
            "Anyone who masters C# will eventually get to the point that they are working with inheritance. " +
            "Since c# doesn't support multi-inheritance, there are interfaces. " +
            "Unfortunately, a field with type of interface is not shown in the inspector. " +
            "With this attribute you can easily restrict object references to all types and they will still be displayed"},


            { NewPropertyD.SelfFillAttribute,
            "If you have components where you know they are on your own object and don't want to write GetComponent every time, you can now write [SelfFill] in front of it. " +
            "With this attribute, the fields are already saved in the editor and no longer consume any performance at runtime (because after the first OnValidate or OnGUI call in editor it will fill with GetComponent). " +
            "\nThe fields will hide if they are filled if you set the parameter hideIfFilled=true (they will still show an error if they didnt find themselves a suitable component). " +
            "\nYou can even use SelfFill.CheckSelfFilled to test whether all components have been found" +
            "\nTip: You can also put SelfFill on a gameObject or Transform so you get the gameObject or Transform in an inner classes" },


            { NewPropertyD.ShowMethodAttribute,
            "This attribute can be used to display return values from methods. Field is updated on each OnGUI call (e.g. when you hover over menu buttons on the left)" +
            "\nThe name shown in the inspector can be given custom or is the name of the get-function without (if found) the word \"get\"" },


            { NewPropertyD.TagAttribute,
            "Makes you select tags from an enum dropdown." },


            { NewPropertyD.ToolbarAttribute,
            "A normal toggle or enum dropdown is very small and unobtrusive. " +
            "This display is much more noticeable"},


            { NewPropertyD.UnwrapAttribute,
            "Shows the serialized fields of the class instead of it wrapped with a foldout"},


            { NewPropertyD.URLAttribute,
            "Displays a clickable url in the inspector"},


            { NewPropertyD.ValidateAttribute,
            "If you only want to allow certain values, this attribute is perfect to make it clear what is allowed or not directly when entering it in the inspector" },


            { NewPropertyD.MaskAttribute,
            "Everyone has seen the constraints on the rigidbody as 3 toggles next to each other and maybe thought of some kind of horizontal alignment, but it's a mask. " +
            "A LayerMask is also a Mask. " +
            "A mask is a number where each bit of the number is interpreted as yes/no. " +
            "Then you can pack a lot of booleans into one number. To access the 3rd bit later, you can use bitshift for example. " +
            "Now you can easily show Masks in the inspector as what they are. Note: On integers you should specify how many bits are displayed (default=3)" },


            { NewPropertyD.MaxAttribute,
            "The counterpart to unitys buildin MinAttribute: Cap the values of numbers or components of vectors to a given maximum" },


            { NewPropertyD.MultipleOfAttribute,
            "It allows only multiples of a given number. The number can be passed by value or by name/path of field" },


            { NewPropertyD.FilePath,
            "In a project I once ran DeleteAssets on a path defined by a string. " +
            "Clumsily, the string was initialized to \"Assets\". " +
            "The whole project had been deleted. That'll never happen again with this type. " +
            "If the path does not end on a specified type (which is never a Folder!), GetPath() throws a NullReferenceException" +
            "\nNote: Since type drawers are not compatible to attributes by default, you have to add [AssetPath] attribute if you add other attributes" },


            { NewPropertyD.FolderPath,
            "Since FilePath cannot hold Folders, this is a type that only holds paths leading to folders. " +
            "Invalid paths return NullReferenceExceptions.\n(Also look at FilePath)" +
            "\nNote: Since type drawers are not compatible to attributes by default, you have to add [AssetPath] attribute if you add other attributes" },


            { NewPropertyD.DynamicSlider,
            "The built-in range slider is very nice and handy; " +
            "But what if you don't want unchangable fixed min-max limits. " +
            "In this way, the designer remains flexible to change the values if necessary, but has a defined default range." +
            "\nNote: Since type drawers are not compatible to attributes by default, you have to add [DynamicSlider] attribute if you add other attributes" },


            { NewPropertyD.MessageDrawer,
            "If you want to write something in the inspector at runtime instead of in the console. For non-runtime messages use the MessageBoxAttribute" },


            { NewPropertyD.SerializableDictionary,
            "A serializable dictionary that can be shown in the inspector" +
            "\nTime complexity: add/remove/access = O(n)" +
            "\nUse SerializableSortedDictionary for better complexity/performance" },


            { NewPropertyD.SerializableSortedDictionary,
            "A serializable implementation of System.SortedDictionary that can be shown in the inspector." +
            "\nKey has to implement the interface System.IComparable." +
            "\nTime complexity: access = O(log(n)) , add/remove = O(n)" },


            { NewPropertyD.SerializableSet,
            "A list, with no duplicates possible. Adding a duplicate will lead to an ArgumentException" +
            "\nTime complexity: add/remove/access = O(n)" },

            { NewPropertyD.SerializableSortedSet,
            "The equivalent to the System.SortedSet but can be serialized and shown in the unity-inspector" +
            "\nTime complexity: access = O(log(n)), add/remove = O(n)" },


            { NewPropertyD.StaticsDrawer,
            "Static variables are all well and good, but unity doesn't show them in the inspector. " +
            "Place the serialized StaticsDrawer anywhere in your class and the inspector will show all static variables of your class. " +
            "Since unity does not save statics, values can only be changed runtime in the inspector (you can test it by entering playmode)" },

            // ------UNITYS--------

            { NewPropertyD.DelayedAttribute,
            "Unity Documentation:\n" +
            "\"When this attribute is used, the float, int, or text field will not return a new value until the user has pressed enter or focus is moved away from the field.\"" },

            { NewPropertyD.HeaderAttribute,
            "Unity Documentation:\n" +
            "\"Use this PropertyAttribute to add a header above some fields in the Inspector.\"" },

            { NewPropertyD.MinAttribute,
            "The counterpart to the MaxAttribute." +
            "\nCap the values of numbers or components of vectors to a given minimum\n" +
            "Warning: it will only cap new inputs in the inspector: not set values by script" },

            { NewPropertyD.MultilineAttribute,
            "Unity Documentation:\n" +
            "\"Attribute to make a string be edited with a multi-line textfield.\"" },

            { NewPropertyD.NonReordableAttribute,
            "Unity Documentation:\n" +
            "\"Disables reordering of an array or list in the Inspector window.\"" },

            { NewPropertyD.RangeAttribute,
            "Draws a slider in the inspector where you can choose values from\n" +
            "\nUnity Documentation:\n" +
            "Attribute used to make a float or int variable in a script be restricted to a specific range." },

            { NewPropertyD.SpaceAttribute,
            "Unity Documentation:\n" +
            "\"Use this PropertyAttribute to add some spacing in the Inspector.\"" },

            { NewPropertyD.TooltipAttribute,
            "Adds a tooltip that you appears by hovering over the given field in the inspector AND in your visual studio editor." },

            { NewPropertyD.TextAreaAttribute,
            "Unity Documentation:\n" +
            "\"Attribute to make a string be edited with a height-flexible and scrollable text area.\n" +
            "You can specify the minimum and maximum lines for the TextArea, and the field will expand according to the size of the text. A scrollbar will appear if the text is bigger than the area available.\"" },
        };
    }
}