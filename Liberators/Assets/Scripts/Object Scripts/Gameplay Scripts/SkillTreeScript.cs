using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

[XmlRoot("skillTree")]
public class SkillTreeScript
{
    [XmlArray("nodes")]
    [XmlArrayItem("skillNode", Type = typeof(SkillNodeScript))]
    public List<SkillNodeScript> nodes;
}

[XmlRoot("skillNode")]
public class SkillNodeScript
{
    [XmlAttribute("id"), DefaultValue("")]
    public string id = "";

    [XmlAttribute("starting"), DefaultValue(false)]
    public bool starting = false;

    [XmlAttribute("cost"), DefaultValue(0)]
    public int cost = 0;

    [XmlArray]
    [XmlArrayItem("requirements", Type = typeof(NodeReference))]
    public List<NodeReference> nodes;

    [XmlArray]
    [XmlArrayItem("unlocks", Type = typeof(SkillReference))]
    public List<SkillReference> skills;
}

[XmlRoot("node")]
public class NodeReference
{
    [XmlAttribute("id"), DefaultValue("")]
    public string id = "";
}

[XmlRoot("skill")]
public class SkillReference
{
    [XmlAttribute("name"), DefaultValue("")]
    public string name = "";
}