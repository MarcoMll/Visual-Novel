using System;
using System.Diagnostics;
using UnityEngine;



namespace CustomInspector
{
    public enum DisabledStyle
    {
        /// <summary> greys out and forbids editing </summary>
        GreyedOut,
        /// <summary> hides the field completely </summary>
        Invisible
    }
    public enum BoolOperator
    {
        /// <summary> Check if all given bool values are true </summary>
        And,
        /// <summary> Check if all one of given bool values is true </summary>
        Or,
    }
    public enum ComparisonOp
    {
        /// <summary> Check if all given field values are equal </summary>
        Equals,
        /// <summary> Check if all given field values are not null </summary>
        NotNull,
        /// <summary> Check if all given field values are null </summary>
        Null,
    }

    /// <summary>
    /// Looks, whether the bool/method with given name(s) is/are true - Otherwise it will be greyed out or hidden.
    /// Special accepted bools: "False", "True", "isPlaying".
    /// You can write a '!' before every bool to invert. For example "!isPlaying"
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ShowIfAttribute : PropertyAttribute
    {
        public const float indentation = 15;

        public readonly BoolOperator? op;
        public readonly ComparisonOp? comOp;

        public readonly string[] conditionPaths;
        public DisabledStyle style = DisabledStyle.Invisible;
        public bool invert = false;

        protected ShowIfAttribute(params string[] conditionPaths)
        {
            order = -20;
            this.conditionPaths = conditionPaths;
        }
        /// <summary>
        /// Looks, whether the bool/method with given name/path is true
        /// </summary>
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfAttribute(string conditionPath)
        : this(new string[1] { conditionPath })
        { }
        /// <summary>
        /// Looks, whether the bools/methods with given names/paths are true
        /// </summary>
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfAttribute(BoolOperator op, params string[] conditionPaths)
        : this(conditionPaths)
        {
            this.op = op;
        }
        /// <summary>
        /// Checks values on given paths/names
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfAttribute(ComparisonOp com, params string[] conditionPaths)
        : this(conditionPaths)
        {
            this.comOp = com;
        }

        //Obsolete since 05.03.2023
        [Obsolete("Use syntax ShowIfAttribute({conditionPath}, style = {style})")]
        public ShowIfAttribute(string conditionPath, DisabledStyle style)
        {
            order = -20;
            this.conditionPaths = new string[1] { conditionPath };
            this.style = style;
        }
        [Obsolete("Use the syntax ShowIfAttribute({op}, {conditionpaths}, style = {style})")]
        public ShowIfAttribute(BoolOperator op, DisabledStyle style, params string[] conditionPaths)
        {
            order = -20;
            this.op = op;
            this.conditionPaths = conditionPaths;
            this.style = style;
        }
    }
    [Conditional("UNITY_EDITOR")]
    public class ShowIfNotAttribute : ShowIfAttribute
    {
        /// <summary>
        /// Looks, whether the bool/method with given name/path is false
        /// </summary>
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfNotAttribute(string conditionPath)
        : base(conditionPath)
        {
            this.invert = true;
        }
        /// <summary>
        /// Looks, whether the bools/methods with given names/paths are false
        /// </summary>
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfNotAttribute(BoolOperator op, params string[] conditionPaths)
        : base(op, conditionPaths)
        {
            this.invert = true;
        }
        /// <summary>
        /// Checks values on given paths/names
        /// <param name="conditionPath">Name/path of an bool/method in scope</param>
        public ShowIfNotAttribute(ComparisonOp com, params string[] conditionPaths)
        : base(com, conditionPaths)
        {
            this.invert = true;
        }


        //Obsolete since 05.03.2023
        [Obsolete("Use syntax ShowIfNotAttribute({conditionPath}, style = {style})")]
        public ShowIfNotAttribute(string conditionPath, DisabledStyle style)
        : base(conditionPath, style)
        {
            this.invert = true;
        }
        [Obsolete("Use syntax ShowIfNotAttribute({op}, {conditionPaths}, style = {style})")]
        public ShowIfNotAttribute(BoolOperator op, DisabledStyle style, params string[] conditionPaths)
        : base(op, style, conditionPaths)
        {
            this.invert = true;
        }
    }
}