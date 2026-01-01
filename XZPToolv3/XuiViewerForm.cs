using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using XZPToolv3.XUI;

namespace XZPToolv3
{
    public partial class XuiViewerForm : Form
    {
        private string filePath;
        private bool isXurFile;
        private XmlDocument xmlDoc;
        private XUIOBJECTDATA xurData;
        private TimelineData timelineData;
        private bool displayAsXui;
        private string tempXuiPath;

        public XuiViewerForm(string xuiXurPath)
        {
            InitializeComponent();
            filePath = xuiXurPath;
            isXurFile = Path.GetExtension(xuiXurPath).ToLower() == ".xur";
            xmlDoc = new XmlDocument();
        }

        private void XuiViewerForm_Load(object sender, EventArgs e)
        {
            this.Text = $"XUI/XUR Viewer - {Path.GetFileName(filePath)}";

            LoadFile();
        }

        private void LoadFile()
        {
            try
            {
                if (isXurFile)
                {
                    // Convert XUR to XUI for viewing
                    tempXuiPath = CreateTempXuiPath(filePath);
                    if (XuiConverter.ConvertXurToXui(filePath, tempXuiPath))
                    {
                        xmlDoc = XuiConverter.LoadXuiFile(tempXuiPath);
                        displayAsXui = true;
                        BuildTreeViewFromXui();
                    }
                    else
                    {
                        xurData = XuiConverter.LoadXurFile(filePath);
                        displayAsXui = false;
                        BuildTreeViewFromXur(xurData);
                    }
                }
                else
                {
                    // Load XML XUI file
                    xmlDoc = XuiConverter.LoadXuiFile(filePath);
                    displayAsXui = true;
                    BuildTreeViewFromXui();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load file:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuildTreeViewFromXur(XUIOBJECTDATA rootObject)
        {
            treeElements.Nodes.Clear();

            if (rootObject != null)
            {
                TreeNode rootNode = CreateTreeNodeFromXur(rootObject);
                treeElements.Nodes.Add(rootNode);
                rootNode.Expand();
                treeElements.SelectedNode = rootNode;
            }
        }

        private TreeNode CreateTreeNodeFromXur(XUIOBJECTDATA obj)
        {
            // Create node with class name
            string nodeName = obj.ClassName;

            // Add Id if available
            object idValue = obj.GetPropVal("Id");
            if (idValue != null && !string.IsNullOrEmpty(idValue.ToString()))
            {
                nodeName += $" [{idValue}]";
            }

            TreeNode treeNode = new TreeNode(nodeName);
            treeNode.Tag = obj;

            // Add child nodes recursively
            foreach (XUIOBJECTDATA child in obj.ChildrenArray)
            {
                treeNode.Nodes.Add(CreateTreeNodeFromXur(child));
            }

            return treeNode;
        }

        private void BuildTreeViewFromXui()
        {
            treeElements.Nodes.Clear();

            XmlElement root = xmlDoc.DocumentElement;
            if (root != null)
            {
                TreeNode rootNode = CreateTreeNodeFromXui(root);
                treeElements.Nodes.Add(rootNode);
                rootNode.Expand();
                treeElements.SelectedNode = rootNode;
            }
        }

        private TreeNode CreateTreeNodeFromXui(XmlNode xmlNode)
        {
            // Create node with element name
            string nodeName = xmlNode.Name;

            // Check for Id in Properties child
            foreach (XmlNode child in xmlNode.ChildNodes)
            {
                if (child.Name == "Properties")
                {
                    foreach (XmlNode prop in child.ChildNodes)
                    {
                        if (prop.Name == "Id" && !string.IsNullOrEmpty(prop.InnerText))
                        {
                            nodeName += $" [{prop.InnerText}]";
                            break;
                        }
                    }
                    break;
                }
            }

            TreeNode treeNode = new TreeNode(nodeName);
            treeNode.Tag = xmlNode;

            // Add child nodes recursively (skip Properties node)
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element &&
                    childNode.Name != "Properties" &&
                    childNode.Name != "Timelines")
                {
                    treeNode.Nodes.Add(CreateTreeNodeFromXui(childNode));
                }
            }

            return treeNode;
        }

        private void treeElements_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null)
                return;

            if (displayAsXui || !isXurFile)
            {
                XmlNode selectedNode = (XmlNode)e.Node.Tag;
                DisplayXuiNodeDetails(selectedNode);
                UpdateTimelineViewFromXui(selectedNode);
            }
            else
            {
                XUIOBJECTDATA selectedObject = (XUIOBJECTDATA)e.Node.Tag;
                DisplayXurObjectDetails(selectedObject);
                UpdateTimelineViewFromXur(selectedObject);
            }
        }

        private static string CreateTempXuiPath(string xurPath)
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "XZPTool", "ViewerXurToXui", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);
            string name = Path.GetFileNameWithoutExtension(xurPath) + ".xui";
            return Path.Combine(tempRoot, name);
        }

        private void UpdateTimelineViewFromXui(XmlNode node)
        {
            timelineData = BuildTimelineDataFromXui(node);
            timelineView.SetData(timelineData);
        }

        private void UpdateTimelineViewFromXur(XUIOBJECTDATA obj)
        {
            timelineData = BuildTimelineDataFromXur(obj);
            timelineView.SetData(timelineData);
        }

        private TimelineData BuildTimelineDataFromXui(XmlNode node)
        {
            TimelineData data = new TimelineData();
            string elementId = GetXuiNodeId(node);
            data.Title = string.IsNullOrEmpty(elementId) ? $"Timelines: {node.Name}" : $"Timelines: {node.Name} [{elementId}]";

            XmlNode timelines = node.SelectSingleNode("Timelines");
            if (timelines == null)
            {
                data.Message = "No timelines for this element.";
                return data;
            }

            XmlNodeList namedFrames = timelines.SelectNodes("NamedFrames/NamedFrame");
            if (namedFrames != null)
            {
                foreach (XmlNode frame in namedFrames)
                {
                    NamedFrame nf = new NamedFrame();
                    nf.Name = GetChildInnerText(frame, "Name");
                    nf.Command = GetChildInnerText(frame, "Command");
                    if (uint.TryParse(GetChildInnerText(frame, "Time"), out uint t))
                        nf.Time = t;
                    data.NamedFrames.Add(nf);
                }
            }

            XmlNodeList timelineNodes = timelines.SelectNodes("Timeline");
            if (timelineNodes != null)
            {
                foreach (XmlNode timelineNode in timelineNodes)
                {
                    TimelineTrack track = new TimelineTrack();
                    track.Id = GetChildInnerText(timelineNode, "Id");
                    if (string.IsNullOrEmpty(track.Id))
                        track.Id = "Timeline";

                    XmlNodeList keyFrames = timelineNode.SelectNodes("KeyFrame");
                    if (keyFrames != null)
                    {
                        foreach (XmlNode keyFrame in keyFrames)
                        {
                            string timeText = GetChildInnerText(keyFrame, "Time");
                            if (uint.TryParse(timeText, out uint time))
                                track.Keyframes.Add(time);
                        }
                    }
                    data.Tracks.Add(track);
                }
            }

            data.MaxTime = GetMaxTime(data);
            if (data.Tracks.Count == 0 && data.NamedFrames.Count == 0)
                data.Message = "No timeline data for this element.";

            return data;
        }

        private TimelineData BuildTimelineDataFromXur(XUIOBJECTDATA obj)
        {
            TimelineData data = new TimelineData();
            object idValue = obj.GetPropVal("Id");
            string idText = idValue != null ? idValue.ToString() : "";
            data.Title = string.IsNullOrEmpty(idText) ? $"Timelines: {obj.ClassName}" : $"Timelines: {obj.ClassName} [{idText}]";

            if (obj.NamedFrameArray.Count == 0 && obj.SubTimelines.Count == 0)
            {
                data.Message = "Timeline data not available for XUR view. Open as XUI to see full timelines.";
                return data;
            }

            foreach (XUINAMEDFRAMEDATA frame in obj.NamedFrameArray)
            {
                data.NamedFrames.Add(new NamedFrame
                {
                    Name = frame.Name,
                    Time = frame.Time,
                    Command = frame.Command.ToString()
                });
            }

            foreach (XUISUBTIMELINEDATA timeline in obj.SubTimelines)
            {
                TimelineTrack track = new TimelineTrack();
                track.Id = string.IsNullOrEmpty(timeline.ElementId) ? "Timeline" : timeline.ElementId;
                foreach (XUIKEYFRAMEDATA keyFrame in timeline.KeyframeDataArray)
                    track.Keyframes.Add(keyFrame.Frame);
                data.Tracks.Add(track);
            }

            data.MaxTime = GetMaxTime(data);
            return data;
        }

        private static string GetChildInnerText(XmlNode node, string childName)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == childName)
                    return child.InnerText ?? "";
            }
            return "";
        }

        private static string GetXuiNodeId(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Properties")
                {
                    foreach (XmlNode prop in child.ChildNodes)
                    {
                        if (prop.Name == "Id")
                            return prop.InnerText ?? "";
                    }
                }
            }
            return "";
        }

        private static uint GetMaxTime(TimelineData data)
        {
            uint max = 0;
            foreach (NamedFrame nf in data.NamedFrames)
                if (nf.Time > max) max = nf.Time;
            foreach (TimelineTrack track in data.Tracks)
                foreach (uint kf in track.Keyframes)
                    if (kf > max) max = kf;
            return max;
        }

        private void DisplayXurObjectDetails(XUIOBJECTDATA obj)
        {
            listViewDetails.Items.Clear();

            // Add class name
            AddDetailItem("Class Name", obj.ClassName, true);

            // Add properties
            if (obj.PropertyArray.Count > 0)
            {
                AddDetailItem("", "", false); // Separator
                AddDetailItem("PROPERTIES", "", true);

                foreach (XUIPROPERTYDATA prop in obj.PropertyArray)
                {
                    string propName = prop.PropDef.PropName ?? "Unknown";
                    string propValue = prop.PropValue != null ? prop.PropValue.ToString() : "";
                    AddDetailItem(propName, propValue, false);
                }
            }

            // Add child count
            if (obj.ChildrenArray.Count > 0)
            {
                AddDetailItem("", "", false); // Separator
                AddDetailItem("CHILD ELEMENTS", obj.ChildrenArray.Count.ToString(), true);
            }

            // Add timelines info
            if (obj.NumNamedFrames > 0)
            {
                AddDetailItem("", "", false); // Separator
                AddDetailItem("NAMED FRAMES", obj.NumNamedFrames.ToString(), true);

                foreach (XUINAMEDFRAMEDATA frame in obj.NamedFrameArray)
                {
                    AddDetailItem($"  {frame.Name}", $"Time: {frame.Time}, Command: {frame.Command}", false);
                }
            }

            if (obj.NumSubTimelines > 0)
            {
                AddDetailItem("", "", false); // Separator
                AddDetailItem("TIMELINES", obj.NumSubTimelines.ToString(), true);
            }
        }

        private void DisplayXuiNodeDetails(XmlNode node)
        {
            listViewDetails.Items.Clear();

            // Add element name
            AddDetailItem("Element Type", node.Name, true);

            // Add properties from Properties child node
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Properties")
                {
                    AddDetailItem("", "", false); // Separator
                    AddDetailItem("PROPERTIES", "", true);

                    foreach (XmlNode prop in child.ChildNodes)
                    {
                        if (prop.NodeType == XmlNodeType.Element)
                        {
                            AddDetailItem(prop.Name, prop.InnerText, false);
                        }
                    }
                    break;
                }
            }

            // Add child elements count
            int childElementCount = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element &&
                    child.Name != "Properties" &&
                    child.Name != "Timelines")
                    childElementCount++;
            }

            if (childElementCount > 0)
            {
                AddDetailItem("", "", false); // Separator
                AddDetailItem("CHILD ELEMENTS", childElementCount.ToString(), true);
            }

            // Check for Timelines
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element && child.Name == "Timelines")
                {
                    AddDetailItem("", "", false); // Separator
                    AddDetailItem("TIMELINES", "", true);
                    break;
                }
            }
        }

        private void AddDetailItem(string property, string value, bool isBold)
        {
            ListViewItem item = new ListViewItem(property);
            item.SubItems.Add(value);

            if (isBold)
            {
                item.Font = new System.Drawing.Font(listViewDetails.Font, System.Drawing.FontStyle.Bold);
            }

            listViewDetails.Items.Add(item);
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    if (isXurFile)
                    {
                        sfd.Filter = "XUI Files|*.xui|All Files|*.*";
                        sfd.DefaultExt = "xui";
                        sfd.FileName = Path.GetFileNameWithoutExtension(filePath) + ".xui";
                    }
                    else
                    {
                        sfd.Filter = "XUR Files|*.xur|All Files|*.*";
                        sfd.DefaultExt = "xur";
                        sfd.FileName = Path.GetFileNameWithoutExtension(filePath) + ".xur";
                    }

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        bool result;
                        if (isXurFile)
                            result = XuiConverter.ConvertXurToXui(filePath, sfd.FileName);
                        else
                            result = XuiConverter.ConvertXuiToXur(filePath, sfd.FileName);

                        if (result)
                        {
                            MessageBox.Show("Conversion completed successfully.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Conversion returned no data.",
                                "Conversion Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Conversion error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenExternal_Click(object sender, EventArgs e)
        {
            try
            {
                // Open file directly
                System.Diagnostics.Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isXurFile)
                {
                    xmlDoc.Save(filePath);
                    MessageBox.Show("File saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Saving XUR files not yet supported.",
                        "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
