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
public enum TargetType { POINT, TILE, TARGET, BEAM, PROJECTILE, SELF, NONE }

public enum TargetCondition { RANDOMDUPE, RANDOMUNIQUE, SELECTED, ALL }

public enum TargetFilter { ENEMY, ALLY, HPPERC }

public enum EffectType { DAMAGE, HEAL, STATUS, INVOKE, MOVE }

public enum DamageElement { normal, aether, force, light, dark, fire, water, wind, energy, ice, earth, wild }

public enum ActionType { NONE, MOVE, COMBAT, MISC, WAIT, END };

public enum StatusType { BUFF, DEBUFF, NONE };

public enum AIMode { idle, attack, flee, hold };

public enum ArmorGrade { none, light, medium, heavy, superheavy };

public enum ActionPhase { INACTIVE, PREPARE, EXECUTE };

public enum TurnPhase { START, MAIN, PAUSE, END };

public enum CombatDataType { DAMAGE, HEAL, STATUS };

public enum BattleOutcome { UNSAVED, VICTORY, SUCCESS, FAILURE, ROUTED }

public enum BattleTeam { PLAYER, ENEMY, ALLY, NEUTRAL};

public enum OperationsTeam { PLAYER, ENEMY, ALLY, NEUTRAL};

public enum OperationsAI { PLAYER, WAIT, ATTACK, WANDER };

public enum OperationsMoveType { INFANTRY, VEHICLE, TRACKED, WALKED, AERIAL, NAVAL, MARINE };

public enum RosterCharacter { lana, ethan, saei, vaue, may, runli, colin, hanaei}

public enum BattleMenuPage { main, list, unit, overview };