using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Clouds.SignalSystem;

namespace Clouds.SignalSystem.Editor
{
    public class SignalGraphView : GraphView
    {
        public SignalGraphView()
        {
            Insert(0, new GridBackground());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/=====MAIN======/Clouds/Editor/SignalGraph/SignalGraph.uss");
            if (styleSheet != null) styleSheets.Add(styleSheet);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());

            style.flexGrow = 1;
            
            graphViewChanged = OnGraphViewChanged;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    OnEdgeCreated(edge);
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        OnEdgeRemoved(edge);
                    }
                }
            }

            return graphViewChange;
        }

        private void OnEdgeCreated(Edge edge)
        {
            if (edge.output.node is SenderNode senderNode && edge.input.node is ReceiverNode receiverNode)
            {
                var receiver = receiverNode.Receiver as MonoBehaviour;
                if (receiver == null) return;

                var serializedSender = senderNode.SerializedSender;
                serializedSender.Update();

                // Try to find the list of receivers in the sender component
                // Common naming: targetReceivers or receivers
                var prop = serializedSender.FindProperty("targetReceivers") ?? serializedSender.FindProperty("receivers");
                
                if (prop != null && prop.isArray)
                {
                    bool exists = false;
                    for (int i = 0; i < prop.arraySize; i++)
                    {
                        if (prop.GetArrayElementAtIndex(i).objectReferenceValue == receiver)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        prop.InsertArrayElementAtIndex(prop.arraySize);
                        prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = receiver;
                        serializedSender.ApplyModifiedProperties();
                        EditorUtility.SetDirty(serializedSender.targetObject);
                    }
                }
            }
        }

        private void OnEdgeRemoved(Edge edge)
        {
            if (edge.output?.node is SenderNode senderNode && edge.input?.node is ReceiverNode receiverNode)
            {
                var receiver = receiverNode.Receiver as MonoBehaviour;
                if (receiver == null) return;

                var serializedSender = senderNode.SerializedSender;
                serializedSender.Update();

                var prop = serializedSender.FindProperty("targetReceivers") ?? serializedSender.FindProperty("receivers");
                if (prop != null && prop.isArray)
                {
                    for (int i = prop.arraySize - 1; i >= 0; i--)
                    {
                        if (prop.GetArrayElementAtIndex(i).objectReferenceValue == receiver)
                        {
                            prop.DeleteArrayElementAtIndex(i);
                        }
                    }
                    serializedSender.ApplyModifiedProperties();
                    EditorUtility.SetDirty(serializedSender.targetObject);
                }
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        public void PopulateFromPrefab(GameObject prefab)
        {
            graphViewChanged -= OnGraphViewChanged;

            try 
            {
                Dictionary<string, Rect> nodePositions = new();
                Dictionary<ISignalSender, Dictionary<ISignalReceiver, string>> graphLinks = new();

                foreach (var element in graphElements)
                {
                    if (element is Node node) nodePositions[node.title] = node.GetPosition();
                    if (element is Edge edge && edge.output?.node is SenderNode sNode && edge.input?.node is ReceiverNode rNode)
                    {
                        if (!graphLinks.ContainsKey(sNode.Sender)) graphLinks[sNode.Sender] = new();
                        graphLinks[sNode.Sender][rNode.Receiver] = edge.output.portName;
                    }
                }

                DeleteElements(graphElements);

                IEnumerable<ISignalSender> senders;
                IEnumerable<ISignalReceiver> receivers;

                if (prefab != null)
                {
                    senders = prefab.GetComponentsInChildren<ISignalSender>(true);
                    receivers = prefab.GetComponentsInChildren<ISignalReceiver>(true);
                }
                else
                {
                    var allMonos = Object.FindObjectsOfType<MonoBehaviour>(true);
                    senders = allMonos.OfType<ISignalSender>();
                    receivers = allMonos.OfType<ISignalReceiver>();
                }

                Dictionary<ISignalReceiver, ReceiverNode> receiverNodesMap = new();

                // 4. Create Receiver Nodes
                float yR = 50;
                foreach (var receiver in receivers)
                {
                    var rNode = new ReceiverNode(receiver);
                    AddElement(rNode);
                    receiverNodesMap[receiver] = rNode;

                    if (nodePositions.TryGetValue(rNode.title, out var pos) && pos.width > 0) 
                        rNode.SetPosition(pos);
                    else 
                        rNode.SetPosition(new Rect(600, yR, 250, 150));
                    yR += 180;
                }

                // 5. Create Sender Nodes
                float yS = 50;
                foreach (var sender in senders)
                {
                    var sNode = new SenderNode(sender);
                    AddElement(sNode);

                    if (nodePositions.TryGetValue(sNode.title, out var pos) && pos.width > 0) 
                        sNode.SetPosition(pos);
                    else 
                        sNode.SetPosition(new Rect(100, yS, 250, 150));
                    yS += 220;

                    if (sender.Receivers != null)
                    {
                        foreach (var target in sender.Receivers)
                        {
                            if (receiverNodesMap.TryGetValue(target, out var targetNode))
                            {
                                string portName = null;
                                if (graphLinks.TryGetValue(sender, out var links)) links.TryGetValue(target, out portName);

                                Port portToLink = sNode.Query<Port>().ToList().FirstOrDefault(p => p.portName == portName);
                                
                                // Nếu không tìm thấy bằng portName, lấy đại một port đầu tiên của Sender
                                if (portToLink == null)
                                    portToLink = sNode.Query<Port>().AtIndex(0);

                                if (portToLink != null && targetNode.inputContainer.childCount > 0)
                                {
                                    var inputPort = targetNode.inputContainer.Query<Port>().AtIndex(0);
                                    if (inputPort != null)
                                        LinkNodes(portToLink, inputPort);
                                }
                            }
                        }
                    }
                }
                
                // 6. Auto-focus on the newly created nodes
                schedule.Execute(() => FrameAll()).ExecuteLater(100);
            }
            finally 
            {
                // 7. Always re-subscribe after the refresh logic is done
                graphViewChanged += OnGraphViewChanged;
            }
        }

        private void LinkNodes(Port outPort, Port inPort)
        {
            var edge = new Edge { output = outPort, input = inPort };
            edge.input.Connect(edge); 
            edge.output.Connect(edge);
            Add(edge);
        }
    }
}