public enum damageType { PHYSICAL, MAGIC, TRUE }

/*
 * Targeting Types
 * Point - Any arbitrary point in range
 * Tile - Any tile in range
 * Target - Any enemy in range
 * Ally - Any ally in range
 * Unit - Anyone in range
 * Beam - Beam from caster
 * Projectile - Moving hitbox, almost always has some sort of ramp up/down
 * Self - User only
 */
public enum targetType { POINT, TILE, TARGET, BEAM, PROJECTILE, SELF, NONE }

public enum targetCondition { RANDOMDUPE, RANDOMUNIQUE, SELECTED, ALL }

public enum targetFilter { ENEMY, ALLY, HPPERC }

public enum effectType { DAMAGE, HEAL, STATUS, INVOKE, MOVE }

public enum element { IMPACT, KINETIC, BALLISTIC, EXPLOSIVE, LASER, PLASMA, IONIC, AETHER, FORCE, LIGHT, DARK, FIRE, WATER, WIND, ENERGY, ICE, EARTH, WILD }

public enum actionType { NONE, MOVE, COMBAT, MISC, WAIT, END };

public enum statusType { BUFF, DEBUFF, NONE }

public enum weaponType { MELEE, GUN, FOCUS, SPECIAL };

public enum actionPhase { INACTIVE, PREPARE, EXECUTE };

public enum turnPhase { START, MAIN, PAUSE, END };