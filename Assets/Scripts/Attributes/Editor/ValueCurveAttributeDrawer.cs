using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// IngredientDrawerUIE
[CustomPropertyDrawer(typeof(Game.Attributes.ValueCurveAttribute))]
public class ValueCurveAttributeDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // First get the attribute since it contains the range for the slider
        //ValueCurveAttribute curveAttr = attribute as ValueCurveAttribute;

        AnimationCurve curve = property.animationCurveValue;
        if (curve != null && curve.length > 0)
        {
            EditorGUI.PropertyField(position, property, new GUIContent(label.text + " [" + curve.keys[0].value + "," + curve.keys[curve.length - 1].value + "]"));
        }
        else
        {
            EditorGUI.PropertyField(position, property, new GUIContent(label.text + " [?,?]"));
        }
    }
}