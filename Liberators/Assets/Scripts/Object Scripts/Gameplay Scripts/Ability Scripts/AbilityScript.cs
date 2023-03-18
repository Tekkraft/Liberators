using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

[XmlRoot("abilityScript")]
public class AbilityScript
{
    [XmlArray("targeting")]
    [XmlArrayItem("unit", Type = typeof(UnitTargeting))]
    [XmlArrayItem("beam", Type = typeof(BeamTargeting))]
    public List<TargetingType> targeting;

    [XmlArray("effects")]
    [XmlArrayItem("damage", Type = typeof(DamageEffect))]
    [XmlArrayItem("status", Type = typeof(StatusEffect))]
    [XmlArrayItem("heal", Type = typeof(HealEffect))]
    [XmlArrayItem("select", Type = typeof(SelectEffect))]
    public List<EffectTypeA> effects;
}

public class TargetingType { }

[XmlRoot("unit")]
public class UnitTargeting : TargetingType
{
    [XmlElement("range")]
    public RangeElement range;

    [XmlElement("team")]
    public TeamElement team;

    [XmlElement("count")]
    public CountElement count;

    //TODO: Add filter and custom conditions
}

[XmlRoot("beam")]
public class BeamTargeting : TargetingType
{
    [XmlElement("range")]
    public RangeElement range;

    [XmlElement("team")]
    public TeamElement team;

    [XmlElement("count")]
    public CountElement count;

    [XmlElement("block")]
    public BlockElement block;

    //TODO: Add filter and custom conditions
}

[XmlRoot("self")]
public class SelfTargeting : TargetingType
{
    //TODO: Add filter and custom conditions
}

[XmlRoot("tile")]
public class TileTargeting : TargetingType
{
    [XmlElement("range")]
    public RangeElement range;

    [XmlElement("team")]
    public TeamElement team;

    [XmlElement("count")]
    public CountElement count;

    [XmlElement("mode")]
    public ModeElement mode;

    //TODO: Add filter and custom conditions
}

[XmlRoot("range")]
public class RangeElement
{
    [XmlAttribute("min"), DefaultValue(0)]
    public int min = 0;

    [XmlAttribute("max"), DefaultValue(0)]
    public int max = 0;

    [XmlAttribute("extendMax"), DefaultValue(true)]
    public bool extendMaxRange = true;

    [XmlAttribute("sight"), DefaultValue(true)]
    public bool sightRequired = true;

    [XmlAttribute("melee"), DefaultValue(false)]
    public bool melee = false;
}

[XmlRoot("team")]
public class TeamElement
{
    [XmlAttribute("filter"), DefaultValue("all")]
    public string filter = "all";
}

[XmlRoot("count")]
public class CountElement
{
    [XmlAttribute("targets"), DefaultValue(1)]
    public int targets = 1;

    [XmlAttribute("selectManual"), DefaultValue(true)]
    public bool selectManual = true;

    [XmlAttribute("infinite"), DefaultValue(false)]
    public bool infinite = false;

    [XmlAttribute("duplicate"), DefaultValue(false)]
    public bool duplicate = false;
}

[XmlRoot("block")]
public class BlockElement
{
    [XmlAttribute("terrain"), DefaultValue(true)]
    public bool terrain = true;

    [XmlAttribute("units"), DefaultValue(true)]
    public bool units = true;
}

[XmlRoot("mode")]
public class ModeElement
{
    [XmlAttribute("filter"), DefaultValue("none")]
    public string filter = "none";

    [XmlAttribute("state"), DefaultValue("any")]
    public string state = "any";
}

public class EffectTypeA { }

[XmlRoot("damage")]
public class DamageEffect : EffectTypeA
{
    [XmlAttribute("value")]
    public int value;

    [XmlAttribute("element"), DefaultValue("weapon")]
    public string element = "weapon";

    [XmlAttribute("source"), DefaultValue("neutral")]
    public string source = "neutral";

    [XmlAttribute("type"), DefaultValue("true")]
    public string type = "true";

    [XmlAttribute("melee"), DefaultValue(false)]
    public bool melee = false;

    [XmlAttribute("hit"), DefaultValue(100)]
    public int hit = 100;

    [XmlAttribute("multiplier"), DefaultValue(1)]
    public float multimplier = 1;

    [XmlAttribute("trueHit"), DefaultValue(false)]
    public bool trueHit = false;

    [XmlArray("effective")]
    [XmlArrayItem("target")]
    public List<string> effective = new List<string>();
}

[XmlRoot("status")]
public class StatusEffect : EffectTypeA
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("intensity"), DefaultValue(1)]
    public int intensity;

    [XmlAttribute("duration"), DefaultValue(1)]
    public int duration;

    [XmlAttribute("hit"), DefaultValue(100)]
    public int hit = 100;

    [XmlAttribute("trueHit"), DefaultValue(false)]
    public bool trueHit = false;
}

[XmlRoot("heal")]
public class HealEffect : EffectTypeA
{
    [XmlAttribute("value")]
    public int value;

    [XmlAttribute("hit"), DefaultValue(100)]
    public int hit = 100;

    [XmlAttribute("trueHit"), DefaultValue(false)]
    public bool trueHit = false;
}

//TODO: NEEDS TESTING
[XmlRoot("select")]
public class SelectEffect : EffectTypeA
{
    [XmlElement("aoe", Type = typeof(AOESelector))]
    public SelectType selector;
}

public class SelectType { }

[XmlRoot("aoe")]
public class AOESelector : SelectType
{
    [XmlElement("area")]
    public AreaElement area;

    [XmlElement("range")]
    public RangeElement range;

    [XmlElement("team")]
    public TeamElement team;

    [XmlElement("count")]
    public CountElement count;

    [XmlArray("effects")]
    [XmlArrayItem("damage", Type = typeof(DamageEffect))]
    [XmlArrayItem("status", Type = typeof(StatusEffect))]
    [XmlArrayItem("heal", Type = typeof(HealEffect))]
    [XmlArrayItem("select", Type = typeof(SelectEffect))]
    public List<EffectTypeA> effects;
    //TODO: Add filter and custom conditions
}

[XmlRoot("area")]
public class AreaElement
{
    [XmlAttribute("shape"), DefaultValue("circle")]
    public string shape;

    [XmlAttribute("direction"), DefaultValue("none")]
    public string direction;
}