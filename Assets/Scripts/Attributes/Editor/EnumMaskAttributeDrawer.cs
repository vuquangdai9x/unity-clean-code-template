using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Attributes
{
    [CustomPropertyDrawer(typeof(EnumMaskAttribute))]
    public class EnumMaskAttributeDrawer : PropertyDrawer
    {
        private string[] _options = null;
        private const string BUTTON_SELECT_ALL = "===ALL===";
        private const string BUTTON_CLEAR = "__CLEAR__";
        private const string BUTTON_REMOVE_SELECTION = "X";
        private const string ADD_DROPDOWN_LABEL = "Add: ";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //return base.GetPropertyHeight(property, label);
            EnumMaskAttribute enumMaskAttribute = attribute as EnumMaskAttribute;
            if (_options == null) _options = System.Enum.GetNames(enumMaskAttribute.enumType);
            float height = 0;
            height += enumMaskAttribute.buttonHeight; // label
            height += enumMaskAttribute.buttonHeight; // All/Clear button
            if (enumMaskAttribute.isShowList)
            {
                for (int i = 0; i < _options.Length && i < 32; i++)
                {
                    if (((property.intValue >> i) & 0x01) == 1)
                    {
                        height += enumMaskAttribute.buttonHeight; // each selected options
                    }
                }
                height += enumMaskAttribute.buttonHeight; // dropdown
            }
            else
            {
                height += enumMaskAttribute.buttonHeight * (_options.Length / enumMaskAttribute.numCols + ((_options.Length % enumMaskAttribute.numCols == 0) ? 0 : 1));
            }
            return height;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // First get the attribute since it contains the range for the slider
            EnumMaskAttribute enumMaskAttribute = attribute as EnumMaskAttribute;
            if (_options == null) _options = System.Enum.GetNames(enumMaskAttribute.enumType);

            Rect drawRect = position;
            drawRect.height = enumMaskAttribute.buttonHeight;

            EditorGUI.LabelField(drawRect, label);

            // draw button select all, button clear
            drawRect.y += enumMaskAttribute.buttonHeight;
            if (GUI.Button(new Rect(drawRect.x, drawRect.y, drawRect.width / 2f, drawRect.height), BUTTON_SELECT_ALL))
            {
                property.intValue = ~0x0;
            }
            if (GUI.Button(new Rect(drawRect.x + drawRect.width / 2f, drawRect.y, drawRect.width / 2f, drawRect.height), BUTTON_CLEAR))
            {
                property.intValue = 0x0;
            }
            // -----

            if (enumMaskAttribute.isShowList)
            {
                for (int i = 0; i < _options.Length && i < 32; i++)
                {
                    if (((property.intValue >> i) & 0x01) == 1)
                    {
                        drawRect.y += enumMaskAttribute.buttonHeight;
                        if (GUI.Button(new Rect(drawRect.x, drawRect.y, drawRect.height, drawRect.height), BUTTON_REMOVE_SELECTION))
                        {
                            property.intValue &= (~(1 << i));
                        }
                        EditorGUI.LabelField(new Rect(drawRect.x + drawRect.height, drawRect.y, drawRect.width - drawRect.height, drawRect.height), _options[i]);
                    }
                }

                drawRect.y += enumMaskAttribute.buttonHeight;
                int dropdownValue = EditorGUI.Popup(drawRect, ADD_DROPDOWN_LABEL, -1, _options);
                if (0 <= dropdownValue && dropdownValue < _options.Length && dropdownValue < 32)
                {
                    property.intValue |= (1 << dropdownValue);
                }
            }
            else
            {
                Color backupContentColor = GUI.contentColor;
                Color backupBgrColor = GUI.backgroundColor;

                drawRect.y += enumMaskAttribute.buttonHeight;
                drawRect.width /= enumMaskAttribute.numCols;
                for (int i = 0; i < _options.Length && i < 32; i++)
                {
                    if (((property.intValue >> i) & 0x01) == 1)
                    {
                        UseStyleToggleOn();
                        if (GUI.Button(drawRect, _options[i]))
                        {
                            property.intValue &= (~(1 << i));
                        }
                    }
                    else
                    {
                        UseStyleToggleOff();
                        if (GUI.Button(drawRect, _options[i]))
                        {
                            property.intValue |= (1 << i);
                        }
                    }

                    if (i % enumMaskAttribute.numCols == enumMaskAttribute.numCols - 1)
                    {
                        drawRect.y += enumMaskAttribute.buttonHeight;
                        drawRect.x = position.x;
                    }
                    else
                    {
                        drawRect.x += drawRect.width;
                    }

                    GUI.contentColor = backupContentColor;
                    GUI.backgroundColor = backupBgrColor;
                }
            }

            EditorGUI.EndProperty();

            void UseStyleToggleOff()
            {
                GUI.contentColor = Color.gray;
                GUI.backgroundColor = Color.gray;
            }
            void UseStyleToggleOn()
            {
                GUI.contentColor = Color.black;
                GUI.backgroundColor = Color.green;
            }
        }
    }
}