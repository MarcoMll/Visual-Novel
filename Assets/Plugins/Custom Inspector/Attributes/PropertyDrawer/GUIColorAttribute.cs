using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    public enum FixedColor
    {
        CloudWhite, IceWhite, Black, Gray, DarkGray, Blue, DustyBlue, BabyBlue, Purple, Red, CherryRed, Orange, Cyan, Green, Magenta, Yellow
    }
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class GUIColorAttribute : PropertyAttribute
    {
        public readonly string colorString = null;
        public FixedColor? fixedColor = null;
        public readonly bool colorWholeUI;

        public GUIColorAttribute(string color = "(0.9, 0.0, 0, 1)", bool colorWholeUI = false)
        {
            order = -10;
            this.colorString = color;
            this.colorWholeUI = colorWholeUI;
        }
        public GUIColorAttribute(FixedColor fixedColor, bool colorWholeUI = false)
        {
            order = -10;
            this.fixedColor = fixedColor;
            this.colorWholeUI = colorWholeUI;
        }
    }
    public static class FixedColorConvert
    {
        public static Color ToColor(FixedColor c)
        {
            return c switch
            {
                FixedColor.CloudWhite => new Color(.93f, .93f, .93f, 1),
                FixedColor.IceWhite => Color.white,
                FixedColor.Black => Color.black,
                FixedColor.Gray => Color.gray,
                FixedColor.DarkGray => new Color(.1f, .1f, .1f, 1),
                FixedColor.Blue => Color.blue,
                FixedColor.DustyBlue => new Color(.31f, .4f, .5f, 1),
                FixedColor.BabyBlue => new Color(.73f, .89f, .96f, 1),
                FixedColor.Purple => new Color(.44f, .13f, .51f, 1),
                FixedColor.Red => Color.red,
                FixedColor.CherryRed => new Color(.8f, 0, .1f, 1),
                FixedColor.Orange => new Color(.95f, .55f, .09f, 1),
                FixedColor.Cyan => Color.cyan,
                FixedColor.Green => Color.green,
                FixedColor.Magenta => Color.magenta,
                FixedColor.Yellow => Color.yellow,
                _ => throw new System.NotImplementedException($"{c} currently not supported")
            };
        }
    }
}
