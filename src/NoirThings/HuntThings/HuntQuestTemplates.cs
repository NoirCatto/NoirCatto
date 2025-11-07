using System.Collections.Generic;
using System.Linq;

using CT = CreatureTemplate.Type;
using static CreatureTemplate.Type;
using static MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType;
using static DLCSharedEnums.CreatureTemplateType;
using static NoirCatto.HuntThings.HuntQuestThings.HuntQuest;

namespace NoirCatto.HuntThings;

public partial class HuntQuestThings
{
    public static class HuntQuestTemplates
    {

        static HuntQuestTemplates()
        {
            if (!ModManager.MSC) return;
            Karma6CreaturePool.AddRange(Karma6CreaturePoolDlc);
            Karma7CreaturePool.AddRange(Karma7CreaturePoolDlc);
            Karma9CreatureFixedList = Karma9CreatureFixedListDlc;
            Karma10CreatureFixedList = Karma10CreatureFixedListDlc;
        }

        public static List<HuntQuest> FromKarma(int karma)
        {
            var creatureList = karma switch
            {
                0 => GetFixedCreatureList(Karma2CreatureFixedList),
                1 => GetFixedCreatureList(Karma3CreatureFixedList),
                2 => GetRandomCreatureList(Karma4CreaturePool, Karma4ReqThreat, 5, 4),
                3 => GetRandomCreatureList(Karma5CreaturePool, Karma5ReqThreat, 8, 4),
                4 => GetRandomCreatureList(Karma6CreaturePool, Karma6ReqThreat, 5, 4),
                5 => GetRandomCreatureList(Karma7CreaturePool, Karma7ReqThreat, 5, 3),
                6 => GetFixedCreatureList(Karma8CreatureFixedList),
                7 => GetFixedCreatureList(Karma9CreatureFixedList),
                8 => GetFixedCreatureList(Karma10CreatureFixedList),
                _ => null
            };
            if (creatureList != null) return creatureList;
            
            //Something went wrong
            NoirCatto.LogSource.LogError("HuntQuest: Invalid karma, returning default template");
            return [new HuntQuest([CT.LanternMouse])];
        }

        private static List<HuntQuest> GetRandomCreatureList(List<Extensions.WeightedItem<CT>> creatureList, float requestedThreat, int maxCreatures = 10, int listCount = 4)
        {
            var inputList = creatureList.DeepClone(); //Don't edit the template list

            List<HuntQuest> outputList = [];
            for (var i = 0; i < listCount; i++)
            {
                var randomList = Extensions.GenerateRandomList(inputList, maxCreatures, target: requestedThreat).ToList();

                inputList.RemoveRange(inputList.Where(x => randomList.Contains(x.Item)).ToArray());

                randomList.Sort((a, b) =>
                {
                    // Move lizards to the end, so they are together
                    if (a.value.EndsWith("Lizard") && !b.value.EndsWith("Lizard"))
                        return 1;
                    if (!a.value.EndsWith("Lizard") && b.value.EndsWith("Lizard"))
                        return -1;
                    
                    // else just sort alphabetically
                    return a.value.CompareTo(b.value);
                });
                outputList.Add(new HuntQuest(randomList));
            }
            return outputList;
        }
        
        private static List<HuntQuest> GetFixedCreatureList(List<CT[]> creatureList)
        {
            return creatureList
                .Select(ingredient => new HuntQuest(ingredient.ToList()))
                .ToList();
        }


        /// Karma 1, Progression to Karma 2
        public static readonly List<CT[]> Karma2CreatureFixedList =
        [
            [HuntQuest.HuntCicada],
            [SmallCentipede],
            [HuntQuest.HuntEggBug],
            [CT.SmallNeedleWorm],
            //[CT.LanternMouse], //Nonexistent in SI
            [CT.Fly, CT.Fly, CT.Fly]
        ];

        /// Karma 2, Progression to Karma 3
        public static readonly List<CT[]> Karma3CreatureFixedList =
        [
            [SmallCentipede, SmallCentipede, SmallCentipede],
            [HuntCicada, HuntCicada],
            [HuntEggBug, HuntCicada],
            [HuntEggBug, SmallCentipede],
            [YellowLizard]
        ];
        
        // Beginning of Random Creature Pools
        /// Karma 3, Progression to Karma 4
        public const float Karma4ReqThreat = 1.0f;

        public static readonly List<Extensions.WeightedItem<CT>> Karma4CreaturePool =
        [
            new() { Limit = 1, Weight = 5, Contribution = 0.33f, Item = CT.JetFish },
            new() { Limit = 5, Weight = 20, Contribution = 0.2f, Item = CT.LanternMouse },
            new() { Limit = 4, Weight = 10, Contribution = 0.2f, Item = HuntCicada },
            new() { Limit = 1, Weight = 10, Contribution = 0.33f, Item = HuntSpider },
            new() { Limit = 2, Weight = 10, Contribution = 0.6f, Item = CT.BigNeedleWorm },
            new() { Limit = 4, Weight =  5, Contribution = 0.3f, Item = HuntCentipede },
            new() { Limit = 3, Weight = 10, Contribution = 0.4f, Item = YellowLizard },
            new() { Limit = 3, Weight = 10, Contribution = 0.4f, Item = PinkLizard },
            new() { Limit = 2, Weight = 10, Contribution = 0.5f, Item = BlueLizard },
            new() { Limit = 1, Weight = 5, Contribution = 0.6f, Item = WhiteLizard },
        ];
        
        /// Karma 4, Progression to Karma 5
        public const float Karma5ReqThreat = 2.0f;
        public static readonly List<Extensions.WeightedItem<CT>> Karma5CreaturePool =
        [
            new() { Limit = 1, Weight = 100, Contribution = 2f, Item = CT.Vulture },
            new() { Limit = 8, Weight = 5, Contribution = 0.2f, Item = CT.LanternMouse },
            new() { Limit = 2, Weight = 10, Contribution = 1f, Item = CT.DropBug },
            new() { Limit = 2, Weight = 10, Contribution = 1f, Item = CT.Scavenger },
            new() { Limit = 4, Weight =  5, Contribution = 0.4f, Item = HuntSpider },
            new() { Limit = 4, Weight = 10, Contribution = 0.4f, Item = CT.Centipede },
            new() { Limit = 2, Weight = 10, Contribution = 0.6f, Item = CT.BigNeedleWorm },
            //new() { Limit = 1, Weight = 10, Contribution = 0.40f, Item = YellowLizard },
            new() { Limit = 4, Weight = 10, Contribution = 0.35f, Item = PinkLizard },
            new() { Limit = 4, Weight = 10, Contribution = 0.45f, Item = BlueLizard },
            new() { Limit = 2, Weight = 5, Contribution = 0.6f, Item = BlackLizard },
            new() { Limit = 2, Weight = 10, Contribution = 0.6f, Item = WhiteLizard },
            new() { Limit = 2, Weight = 1, Contribution = 0.33f, Item = CT.JetFish },
            new() { Limit = 2, Weight = 1, Contribution = 1f, Item = Salamander },
        ];
        
        /// Karma 5, Progression to Karma 6
        public const float Karma6ReqThreat = 3.0f;
        public static readonly List<Extensions.WeightedItem<CT>> Karma6CreaturePool =
        [
            new() { Limit = 2, Weight = 10, Contribution = 0.70f, Item = CT.Vulture },
            new() { Limit = 2, Weight = 10, Contribution = 0.40f, Item = CT.DropBug },
            new() { Limit = 3, Weight = 5, Contribution = 0.65f, Item = CT.Scavenger },
            new() { Limit = 2, Weight = 10, Contribution = 0.5f, Item = CT.Centiwing },
            new() { Limit = 2, Weight = 10, Contribution = 0.6f, Item = CT.BigNeedleWorm },
            new() { Limit = 2, Weight = 30, Contribution = 1f, Item = GreenLizard },
            new() { Limit = 2, Weight = 10, Contribution = 0.5f, Item = WhiteLizard },
            new() { Limit = 4, Weight = 10, Contribution = 0.4f, Item = YellowLizard },
            new() { Limit = 2, Weight = 10, Contribution = 0.65f, Item = CyanLizard },
            new() { Limit = 2, Weight = 10, Contribution = 0.6f, Item = Salamander },
        ];
        public static readonly List<Extensions.WeightedItem<CT>> Karma6CreaturePoolDlc =
        [
            new() { Limit = 1, Weight = 20, Contribution = 0.50f, Item = ZoopLizard },
            new() { Limit = 1, Weight = 10, Contribution = 1f, Item = MotherSpider }
        ];
        
        /// Karma 6, Progression to Karma 7
        public const float Karma7ReqThreat = 3.0f;
        public static readonly List<Extensions.WeightedItem<CT>> Karma7CreaturePool =
        [
            new() { Limit = 2, Weight = 10, Contribution = 0.6f, Item = CT.Vulture },
            new() { Limit = 3, Weight = 10, Contribution = 0.6f, Item = CT.Scavenger },
            new() { Limit = 5, Weight = 10, Contribution = 0.6f, Item = CyanLizard },
            new() { Limit = 3, Weight = 10, Contribution = 0.5f, Item = WhiteLizard },
            new() { Limit = 2, Weight = 10, Contribution = 0.8f, Item = GreenLizard },
            new() { Limit = 2, Weight = 10, Contribution = 0.6f, Item = Salamander },
        ];
        public static readonly List<Extensions.WeightedItem<CT>> Karma7CreaturePoolDlc =
        [
            new() { Limit = 4, Weight = 10, Contribution = 0.5f, Item = ZoopLizard },
            new() { Limit = 1, Weight = 10, Contribution = 2f, Item = EelLizard },
            new() { Limit = 2, Weight = 10, Contribution = 1.25f, Item = AquaCenti },
        ];
        
        /// Karma 7, Progression to Karma 8
        public static readonly List<CT[]> Karma8CreatureFixedList =
        [
            [KingVulture, KingVulture],
            [RedCentipede],
            [SpitterSpider, SpitterSpider]
        ];
        
        /// Karma 8, Progression to Karma 9
        public static readonly List<CT[]> Karma9CreatureFixedList =
        [
            [RedLizard],
            [CT.Deer],
        ];
        public static readonly List<CT[]> Karma9CreatureFixedListDlc =
        [
            [RedLizard],
            [CT.Deer],
            //[MirosVulture] //Doesn't appear in our worldstate
        ];
        
        /// Karma 9, Progression to Karma 10
        public static readonly List<CT[]> Karma10CreatureFixedList =
        [
            [CT.DaddyLongLegs],
            [CT.MirosBird, CT.MirosBird],
            [CT.BigEel],
            [RedLizard, CyanLizard, WhiteLizard, BlackLizard, GreenLizard, PinkLizard, YellowLizard, BlueLizard, Salamander]
        ];
        public static readonly List<CT[]> Karma10CreatureFixedListDlc =
        [
            [CT.DaddyLongLegs],
            [CT.MirosBird, CT.MirosBird],
            [CT.BigEel],
            [RedLizard, CyanLizard, WhiteLizard, BlackLizard, GreenLizard, PinkLizard, YellowLizard, BlueLizard, Salamander, EelLizard, SpitLizard, ZoopLizard]
        ];
    }
}

/*
Sky Islands (Gourmand)
Centiwing x6
Minipede x5
Squidcada x6+
Eggbug x2
Noodlefly x1
Noodlechild x3
Vulture x3
KVulture x1
Scavs x6
Lizards:
Blue x3
Cyan x3
Orange x6
Pink x2
White x3

Sky Islands (Hunter)
Centiwing x6
Minipede x5
Sqiodcada x6+
Eggbug x4
Noodlefly x1
Noodlechild x3
Vulture x3
KVulture x3
Scavs x8
Lizards:
Blue x1
Cyan x8
Orange x6
Pink x0
White x3
*/