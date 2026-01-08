# 中文客户端适配说明 (Chinese Client Localization)

## 概述

本次修改使 Wholesome TBC AIO Fightclasses 项目支持中文WoW客户端（以及其他非英文客户端）。

## 主要问题

原项目中存在以下依赖英文字符串的问题：

1. **Buff/Debuff 检测** - 使用英文技能名称字符串检查 buff（如 "Soul Link", "Shadow Trance"）
2. **物品名称** - 使用英文名称检查物品（如 "Soul Shard", "Healthstone"）
3. **生物类型** - `CreatureTypeTarget` 返回客户端语言的字符串

## 解决方案

### 新增文件

1. **`Helpers/SpellIds.cs`** - 包含所有技能的Spell ID常量
   - 覆盖所有职业的buff、debuff和技能
   - 使用Spell ID进行语言无关的检测

2. **`Helpers/ItemIds.cs`** - 包含物品ID常量
   - 术士灵魂碎片、治疗石、灵魂石
   - 盗贼毒药 (Instant Poison, Deadly Poison)
   - 药水和弹药
   - `CreatureTypes` 类包含生物类型常量

3. **`Helpers/AuraHelper.cs`** - 提供语言无关的辅助方法
   - `PlayerHasAuraById(uint spellId)` - 通过Spell ID检查玩家buff
   - `PlayerHasDrinkAura()` / `PlayerHasFoodAura()` - 检查饮食buff
   - `GetCreatureTypeIdFromString()` - 将本地化生物类型名称转换为ID
   - 支持中文、英文、德语、法语、西班牙语、俄语的生物类型名称

### 修改的接口和实现

1. **`IWoWUnit` 接口新增方法:**
   - `HasAuraById(uint spellId)` - 通过Spell ID检查buff
   - `IsHumanoid` - 检查是否为人型生物
   - `IsUndead` - 检查是否为亡灵
   - `IsDemon` - 检查是否为恶魔
   - `IsBeast` - 检查是否为野兽
   - `IsGiant` - 检查是否为巨人

2. **`CachedWoWUnit` 实现了所有新属性**

### 修改的旋转文件

**所有 `CreatureTypeTarget` 检查已替换为语言无关属性：**

| 文件 | 修改内容 |
|------|----------|
| `Paladin/Retribution.cs` | Exorcism 检查改为 `Target.IsUndead \|\| Target.IsDemon` |
| `Paladin/RetributionParty.cs` | Exorcism 检查改为语言无关 |
| `Paladin/PaladinProtectionParty.cs` | Exorcism 检查改为语言无关 |
| `Rogue/Combat.cs` | `CanDismantle()` 和 Riposte 检查改为语言无关 |
| `Rogue/Rogue.cs` | 毒药 DoNotSellList 改为动态获取名称，字典改用 ID |
| `Rogue/RogueCombatParty.cs` | Riposte 检查改为 `Target.IsHumanoid` |
| `Hunter/BeastMastery.cs` | Concussive Shot 和 Wing Clip 检查改为语言无关 |
| `Hunter/BeastMasteryParty.cs` | Wing Clip 检查改为语言无关 |
| `Warrior/Fury.cs` | Hamstring 检查改为 `Target.IsHumanoid` |
| `Warrior/FuryParty.cs` | Hamstring 检查改为语言无关 |
| `Shaman/Enhancement.cs` | Frost Shock 检查改为语言无关 |
| `Shaman/Elemental.cs` | Frost Shock 检查改为语言无关 |
| `Shaman/TotemManager.cs` | 图腾buff检查改为Spell ID |
| `Mage/Arcane.cs` | Slow 检查改为 `Target.IsHumanoid` |
| `Mage/ArcaneParty.cs` | Slow 检查改为语言无关 |
| `Mage/Mage.cs` | Polymorph 目标检查改为 `enemy.IsBeast \|\| enemy.IsHumanoid` |
| `Warlock/Warlock.cs` | Soul Shard 检查改为ID方法 |
| `Warlock/Demonology.cs` | Soul Link, Shadow Trance, Fire Ward 检查改为Spell ID |
| `Warlock/Affliction.cs` | 同上 |
| `Warlock/WarlockPetAndConsumables.cs` | 物品检查改为ID方法 |
| `Helpers/StatusChecker.cs` | Ghost Wolf, Drink, Food, Earth Shield 检查改为Spell ID |
| `Helpers/Cast.cs` | Spirit of Redemption 检查改为Spell ID |
| `Helpers/ToolBox.cs` | 新增ID方法 |
| `Managers/RacialsManager/RacialManager.cs` | 种族技能buff检查改为Spell ID |

## 使用方法

### 检查Buff (使用AIOSpell)

```csharp
// 推荐方式：使用AIOSpell对象（已包含Spell ID）
if (!Me.HasAura(FelArmor))
    cast.OnSelf(FelArmor);
```

### 检查Buff (使用Spell ID)

```csharp
// 使用Spell ID常量
if (!Me.HasAuraById(SpellIds.SoulLink))
    cast.OnSelf(SoulLink);
```

### 检查生物类型

```csharp
// 语言无关的生物类型检查
if (Target.IsHumanoid)
    // 目标是人型生物
    
if (Target.IsUndead || Target.IsDemon)
    // 圣骑士驱魔/圣光审判适用
```

### 检查物品

```csharp
// 使用物品ID
if (WarlockPetAndConsumables.CountSoulShards() > 5)
    WarlockPetAndConsumables.DeleteOneSoulShard();

// 检查治疗石
if (WarlockPetAndConsumables.HaveHealthstone())
    WarlockPetAndConsumables.UseHealthstone();
```

## 支持的语言

生物类型检测支持以下语言的客户端：
- 英语 (English)
- 简体中文 (Simplified Chinese)
- 繁体中文 (Traditional Chinese)
- 德语 (Deutsch)
- 法语 (Français)
- 西班牙语 (Español)
- 俄语 (Русский)

## 注意事项

1. **AIOSpell 已支持本地化** - `AIOSpell` 类已经通过 `SpellId` 属性和 `LocalizedName` 属性支持本地化技能释放

2. **图腾名称** - `GetTotemInfo()` 返回的图腾名称仍然是本地化的，检查方式保持 `.Contains("Totem")` 方式（因为所有语言的图腾名称都包含"Totem"或类似字样）

3. **宠物技能** - 宠物技能通过 `PetSpellIds.cs` 和 `Cast.PetSpellById()` 方法已支持本地化

4. **编译** - 项目需要 .NET Framework 4.8 来编译，在 Windows 环境下使用 Visual Studio

## 测试清单

- [ ] 术士灵魂碎片计数和删除
- [ ] 术士治疗石创建和使用
- [ ] 术士灵魂石使用
- [ ] 圣骑士驱魔技能对亡灵/恶魔生物
- [ ] 战士/猎人/萨满对人型生物的减速技能
- [ ] 法师变形技能目标选择
- [ ] 盗贼的Riposte技能
- [ ] 萨满图腾buff检测
- [ ] 种族技能的条件检查

## 版本历史

- **v1.0** (2026-01-08): 初始本地化支持
  - 添加 SpellIds.cs, ItemIds.cs, AuraHelper.cs
  - 更新所有职业旋转文件
  - 添加语言无关的生物类型检测
