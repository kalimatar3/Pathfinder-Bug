using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Clouds.SignalSystem;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;

namespace Clouds.SignalSystem.Editor
{
    public class SenderNode : Node
    {
        public ISignalSender Sender { get; private set; }
        public SerializedObject SerializedSender { get; private set; }

        public SenderNode(ISignalSender sender)
        {
            Sender = sender;
            var mono = sender as MonoBehaviour;
            if (mono != null)
            {
                title = mono.name + " (Sender)";
                SerializedSender = new SerializedObject(mono);

                // Thêm Object Field vào title container
                var objField = new ObjectField("")
                {
                    value = mono.gameObject,
                    objectType = typeof(GameObject),
                    allowSceneObjects = true
                };
                objField.SetEnabled(false); // Read-only nhưng vẫn có thể click
                objField.style.width = 100;
                objField.style.height = 16;
                titleContainer.Add(objField);
            }

            AddToClassList("sender-node");

            // Add button to title container
            var addSignalButton = new Button(OnAddSignalClick) { text = "+" };
            titleContainer.Add(addSignalButton);

            // Ẩn nút mũi tên mở rộng và đảm bảo nội dung luôn hiển thị
            var collapseButton = titleContainer.Q("collapse-button");
            if (collapseButton != null) collapseButton.style.display = DisplayStyle.None;

            // Ép buộc hiển thị các container chính
            outputContainer.style.display = DisplayStyle.Flex;
            inputContainer.style.display = DisplayStyle.Flex;

            RefreshPorts();
            expanded = true;
            RefreshExpandedState();
        }

        public void RefreshPorts()
        {
            outputContainer.Clear();
            extensionContainer.Clear();

            var signals = Sender.SignalsToEmit;
            var signalsProp = SerializedSender.FindProperty("signalsToEmit");

            for (int i = 0; i < signals.Count; i++)
            {
                int index = i;
                var signal = signals[i];

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.height = 24;

                // 1. Nút xóa
                var removeBtn = new Button(() => RemoveSignal(index)) { text = "x" };
                removeBtn.style.height = 16;
                removeBtn.style.width = 16;

                // 2. EnumField chỉnh sửa
                var enumField = new EnumField(signal);
                enumField.style.flexGrow = 1;
                enumField.style.marginLeft = 5;
                enumField.style.maxWidth = 150;

                // 3. Port kết nối (Nằm ngoài cùng bên phải)
                var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(SignalMessage));
                port.portName = signal.ToString();
                var portLabel = port.Q<Label>("name");
                if (portLabel != null) portLabel.style.display = DisplayStyle.None;

                enumField.RegisterValueChangedCallback(evt => 
                {
                    if (signalsProp != null && index < signalsProp.arraySize)
                    {
                        SerializedSender.Update();
                        signalsProp.GetArrayElementAtIndex(index).enumValueIndex = (int)(SignalMessage.SignalType)evt.newValue;
                        SerializedSender.ApplyModifiedProperties();
                        EditorUtility.SetDirty(SerializedSender.targetObject);
                        AssetDatabase.SaveAssets();
                        
                        port.portName = evt.newValue.ToString();
                    }
                });

                row.Add(removeBtn);
                row.Add(enumField);
                row.Add(port);
                
                outputContainer.Add(row);
            }
            
            // Thu gọn extensionContainer vì không dùng đến nữa
            expanded = false;
            RefreshExpandedState();
        }

        private void RemoveSignal(int index)
        {
            SerializedSender.Update();
            var prop = SerializedSender.FindProperty("signalsToEmit");
            if (prop != null && prop.isArray && index < prop.arraySize)
            {
                prop.DeleteArrayElementAtIndex(index);
                SerializedSender.ApplyModifiedProperties();
                
                // Save changes to the Prefab/Object
                EditorUtility.SetDirty(SerializedSender.targetObject);
                AssetDatabase.SaveAssets();
                
                RefreshPorts();
            }
        }

        private void OnAddSignalClick()
        {
            var menu = new GenericMenu();
            var existingSignals = Sender.SignalsToEmit;

            foreach (SignalMessage.SignalType type in Enum.GetValues(typeof(SignalMessage.SignalType)))
            {
                if (type == SignalMessage.SignalType.None) continue;
                
                // Show all signals, or only ones not already present
                menu.AddItem(new GUIContent(type.ToString()), existingSignals.Contains(type), () => AddSignal(type));
            }
            menu.ShowAsContext();
        }

        private void AddSignal(SignalMessage.SignalType type)
        {
            SerializedSender.Update();
            var prop = SerializedSender.FindProperty("signalsToEmit");
            if (prop != null && prop.isArray)
            {
                prop.InsertArrayElementAtIndex(prop.arraySize);
                prop.GetArrayElementAtIndex(prop.arraySize - 1).enumValueIndex = (int)type;
                SerializedSender.ApplyModifiedProperties();
                
                // Save changes to the Prefab/Object
                EditorUtility.SetDirty(SerializedSender.targetObject);
                AssetDatabase.SaveAssets();
                
                RefreshPorts();
            }
        }
    }
}