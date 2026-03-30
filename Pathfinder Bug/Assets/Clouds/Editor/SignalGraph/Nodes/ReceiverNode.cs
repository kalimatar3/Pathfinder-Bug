using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Clouds.SignalSystem;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Clouds.SignalSystem.Editor
{
    public class ReceiverNode : Node
    {
        public ISignalReceiver Receiver { get; private set; }
        public SerializedObject SerializedReceiver { get; private set; }

        public ReceiverNode(ISignalReceiver receiver)
        {
            Receiver = receiver;
            var mono = receiver as MonoBehaviour;
            if (mono != null)
            {
                title = mono.name + " (Receiver)";
                SerializedReceiver = new SerializedObject(mono);

                // Thêm Object Field vào title container
                var objField = new ObjectField("")
                {
                    value = mono.gameObject,
                    objectType = typeof(GameObject),
                    allowSceneObjects = true
                };
                objField.SetEnabled(false); // Chế độ chỉ đọc nhưng vẫn cho phép nhấn để chọn đối tượng
                objField.style.width = 120;
                objField.style.height = 16;
                titleContainer.Add(objField);
            }

            AddToClassList("receiver-node");

            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(SignalMessage));
            port.portName = "Receive";
            inputContainer.Add(port);

            // Nếu là IUIReceiver, hiển thị bảng Edit UI
            if (receiver is IUIReceiver)
            {
                DrawUIEditor();
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        private void DrawUIEditor()
        {
            var container = new VisualElement();
            container.style.paddingTop = 8;
            container.style.paddingBottom = 8;
            container.style.paddingLeft = 8;
            container.style.paddingRight = 8;
            container.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.95f);
            container.style.borderBottomLeftRadius = 5;
            container.style.borderBottomRightRadius = 5;

            string[] uiPropertyNames = { "AnimationGroupDics", "ContentUpdateGroupDics" };
            bool hasAny = false;

            foreach (var propName in uiPropertyNames)
            {
                var prop = SerializedReceiver.FindProperty(propName);
                if (prop != null && prop.isArray)
                {
                    var titleLabel = new Label(prop.displayName.Replace("GroupDics", "s"));
                    titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    titleLabel.style.fontSize = 11;
                    titleLabel.style.color = new Color(0.3f, 0.6f, 0.9f);
                    titleLabel.style.marginBottom = 2;
                    container.Add(titleLabel);

                    // Duyệt qua dictionary (Unity lưu SerializableDictionary dưới dạng mảng các KeyValuePair)
                    if (prop.arraySize == 0)
                    {
                        var emptyLabel = new Label("  (Empty)");
                        emptyLabel.style.fontSize = 10;
                        emptyLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                        emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                        container.Add(emptyLabel);
                    }
                    else
                    {
                        for (int i = 0; i < prop.arraySize; i++)
                        {
                            var element = prop.GetArrayElementAtIndex(i);
                            // Trong SerializableDictionary, 'key' là tên mặc định của trường khóa
                            var keyProp = element.FindPropertyRelative("key");
                            if (keyProp != null)
                            {
                                var keyLabel = new Label($"• {keyProp.enumNames[keyProp.enumValueIndex]}");
                                keyLabel.style.fontSize = 10;
                                keyLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                                keyLabel.style.marginLeft = 5;
                                container.Add(keyLabel);
                            }
                        }
                    }
                    
                    // Spacer
                    var spacer = new VisualElement();
                    spacer.style.height = 6;
                    container.Add(spacer);
                    hasAny = true;
                }
            }

            if (hasAny)
            {
                extensionContainer.Add(container);
                expanded = true;
            }
        }
    }
}