using System;
using System.Linq;


namespace SKC
{
    public class TierLoot : MobDrop
    {
        public enum LootType
        {
            Weapon,
            Ability,
            Armor,
            Ring,
            Potion
        }

        public TierLoot(byte tier, LootType type, float chance = 1, float threshold = 0, int min = 0)
        {
            ItemType[] types = [];
            switch (type)
            {
                case LootType.Weapon:
                    types = ItemDesc.WeaponTypes;
                    break;
                case LootType.Ability:
                    types = ItemDesc.AbilityTypes;
                    break;
                case LootType.Armor:
                    types = ItemDesc.ArmorTypes;
                    break;
                case LootType.Ring:
                    types = ItemDesc.RingTypes;
                    break;
                case LootType.Potion:
                    types = [ItemType.Potion];
                    break;
                default:
#if DEBUG
                    throw new NotSupportedException(type.ToString());
#endif
#if RELEASE
                    break;
#endif
            }

            var items = Resources.Type2Item
                .Where(item => Array.IndexOf(types, item.Value.SlotType) != -1)
                .Where(item => item.Value.Tier == tier)
                .Select(item => item.Value)
                .ToArray();

            foreach (var item in items)
                LootDefs.Add(new LootDef(
                    item.Id,
                    chance / items.Length,
                    threshold,
                    min));
        }
    }
}