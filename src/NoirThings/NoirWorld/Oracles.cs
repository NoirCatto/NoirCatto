using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    public static void PebblesConversationOnAddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
    {
        if (self.owner.oracle.room.game.StoryCharacter != Const.NoirName)
        {
            orig(self);
            return;
        }

        #region Helpers
        void Say(string text)
        {
            self.events.Add(new Conversation.TextEvent(self, 0, text, 0));
        }
        void Wait(int pauseFrames)
        {
            self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, pauseFrames));
        }
        #endregion

        if (!self.owner.playerEnteredWithMark)
        {
            Say(".  .  .");
            Say("...is this reaching you?");
            Wait(4);
        }
        else
        {
            self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 210));
        }

        Say("A... Feline...?");
        Say("Standing or.. Should I say, sitting, at the bottom of my chamber.");
        Wait(20);
        Say("Did...<LINE>" +
            ". . .<LINE>" +
            "Did NSH send you?");
        Say("Is this another one of his... Arguably NOT funny jokes?");
        Say("Or... Perhaps you're from the same mystery supplier... I'm getting distracted.");
        
        Say("It appears my Rotund visitor didnt get the message across.<LINE>" +
            "If it is as it seems and your kind is going to incessantly bother me for the known future...");
        Say("I may aswell do my job as an iterator and push you in the right direction.");

        Say("You're stuck in a cycle little feline, a repeating pattern. You want a way out.");
        Say("Know that this does not make you special - every living thing shares that same frustration.<LINE>" +
            "From the microbes in the processing strata to me, who am, if you excuse me, godlike in comparison.");
        Say("The good news first. In a way, I am what you are searching for. Me and my kind have as our<LINE>" +
            "purpose to solve that very oscillating clawstrophobia in the chests of you and countless others.<LINE>" +
            "A strange charity - you the unknowing recipient, I the reluctant gift. The noble benefactors?<LINE>" +
            "Gone.");
        Say("The bad news is that no definitive solution has been found. And every moment the equipment erodes to a new state of decay.<LINE>" +
            "I can't help you collectively, or individually. I can't even help myself.");
        Wait(10);

        if (self.owner.playerEnteredWithMark)
        {
            Say("For you though, there is another way. The old path. Go to the west past the Farm Arrays, and then down into the earth<LINE>" +
                "where the land fissures, as deep as you can reach, where the ancients built their temples and danced their silly rituals.");
        }
        else
        {
            Say("For you though, there is another way. The old path. Go to the west past the Farm Arrays, and then down into the earth<LINE>" +
                "where the land fissures, as deep as you can reach, where the ancients built their temples and danced their silly rituals.<LINE>" +
                "The mark I gave you will let you through.");
        }
        Say("Not that it solves anyone's problem but yours.");
        Wait(10);

        Say("I am afraid to say due to your..<LINE>" +
            "Feral... ways....<LINE>" +
            "I cannot raise your karmic level.");
        Say("Although, if i am correct, our world still houses the last remaining members of my creators.<LINE>"+
            "Be it in a...<LINE>" +
            "Less physical form.");
        Say("If you may, pay them each a visit. Maybe they will be willing to help you out further.");
        Wait(20);

        if (self.owner.oracle.room.game.IsStorySession && self.owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.memoryArraysFrolicked)
        {
            Say("At the end of time none of this will matter I suppose, but it would be nice if you took another way out.<LINE>" +
                "One free of... Clawing through my memory arrays. There is a perfectly good access shaft right here.");
        }

        if (ModManager.MSC && self.owner.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
        {
            Say("Best of luck to you, and your companion. There is nothing else I can do.");
            Say("I must resume my work.");
            self.owner.CreatureJokeDialog();
            return;
        }

        Say("Best of luck to you, little feline. I must resume my work.");
    }

    public static void MoonConversationOnAddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        if (self.currentSaveFile != Const.NoirName)
        {
            orig(self);
            return;
        }

        #region Helpers
        void Say(string text)
        {
            self.events.Add(new Conversation.TextEvent(self, 0, text, 0));
        }
        void Say2(string text, int initialWait,  int textLinger)
        {
            self.events.Add(new Conversation.TextEvent(self, initialWait, text, textLinger));
        }
        void Wait(int initialWait)
        {
            self.events.Add(new Conversation.WaitEvent(self, initialWait));
        }
        #endregion

        if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
        {
            switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
            {
                case 2:
                    Say2("Get... get away... black... thing.", 30, 10);
                    Say2("Please... thiss all I have left.", 0, 10);
                    return;
                case 3:
                    Say2("YOU!", 30, 10);
                    Say2("...You ate... me. Please go away. I won't speak... to you.<LINE>" +
                         "I... CAN'T speak to you... because... you ate...me...", 60, 0);
                    return;
                case 5:
                    Say("Hello <PlayerName>.");
                    Say("What are you? If I had my memories I would know...");
                    if (self.State.playerEncounters > 0 && self.State.playerEncountersWithMark == 0)
                    {
                        Say("Perhaps... I saw you before?");
                    }
                    Say("You must be very brave to have made it all the way here.<LINE>" +
                        "You dont seem suited to the ocean around my remains.");
                    Say("I am sorry to say, that I have nothing for you. Not even my memories.");
                    Say2("Or did I say that already?", 0, 5);
                    Say("You have been given the gift of communication.<LINE>" +
                        "Did you meet my neighbour, Five Pebbles?");
                    Say("If so.. You must already understand how little of me remains functional.");
                    Wait(10);
                    Say("Your.. biology... feels familiar.");
                    Say2("Almost like a distant memory..<LINE>" +
                        "Be it a very fluffy memory!", 0, 5);

                    Say("Speaking of which, my previous visitor wasnt quite as fluffy as you.<LINE>" +
                        "More... Round. They also didnt seem to walk or crawl the same as you.");
                    Say2("Maybe you're distantly related..?",0, 2);
                    Say("I must admit your kind's visits help keep what remains of me busy!");
                    Say("I can only hope to remember them all for as long as I can in my current state.");
                    Say("Maybe you know the round one? They left some time ago.<LINE>" +
                        "My last overseer watched them head far to the west, past the...");
                    Say2("...", 0, 2);
                    Wait(5);
                    Say("It appears I dont remember the locales name. I'm sorry little one.");
                    Say("You are welcome to stay as long as you desire. It is nice to have someone to talk to!");
                    Say("I apologize I don't fully understand the noises you make...<LINE>" +
                        "They are very cute though.");
                    return;
                default:
                    orig(self);
                    return;
            }
        }

        if (self.id == Conversation.ID.MoonSecondPostMarkConversation)
        {
            switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
            {
                case 4:
                    if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                    {
                        Say2("Hello there! You again!", 30, 0);
                        Say("I wonder what it is that you want?");
                        return;
                    }

                    if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                    {
                        Say2("Oh. You.", 30, 0);
                        Say("I wonder what it is that you want?");
                        Say("I have had scavengers come by before. Scavengers!<LINE>" +
                            "And they left me alive!<LINE>" +
                            "But... I have told you that already, haven't I?");
                        Say("You must excuse me if I repeat myself. My memory is bad.<LINE>" +
                            "I used to have a pathetic five neurons... And then you ate one.<LINE>" +
                            "Maybe I've told you that before as well.");
                        return;
                    }

                    Say2("Oh. You.", 30, 0);
                    return;

                case 5:
                    if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                    {
                        Say2("You again.", 0, 10);
                        return;
                    }

                    if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                    {
                        Say2("Oh, hello!", 0, 10);
                        Say("I wonder what it is that you want?");
                        Say("There is nothing here. Not even my memories remain.");
                        Say2("Even the scavengers that come here from time to time leave with nothing. But... I have told you that already, haven't I?", 30, 0);
                        if (ModManager.MSC && self.myBehavior.CheckSlugpupsInRoom())
                        {
                            Say2("I do enjoy the company though. You and your family are always welcome here.", 0, 5);
                            return;
                        }
                        if (ModManager.MMF && self.myBehavior.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                        {
                            Say2("I do enjoy the company of you and your friend though, <PlayerName>.", 0, 5);
                            Say2("You're welcome to stay a while, fluffy little thing.", 0, 5);
                            return;
                        }
                        Say2("I do enjoy the company though. You're welcome to stay a while, fluffy little thing.", 0, 5);
                        return;
                    }

                    Say2("Oh, hello.", 0, 10);
                    return;

                default:
                    orig(self);
                    return;
            }
        }

        if (self.id == Conversation.ID.Moon_Misc_Item)
        {
            if (self.describeItem == SLOracleBehaviorHasMark.MiscItemType.Spear)
            {
                Say2("It's a piece of sharpened rebar... Others of your kind seemed very proficient at using it,<LINE>" +
                     "but given your anatomy, I believe you use different ways to catch your meal!", 10, 0);
                return;
            }
        }

        orig(self);
    }

    public static void SSOracleBehaviorOnUpdate(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<RainWorldGame>("get_StoryCharacter"),
                i => i.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>("Artificer"),
                i => i.MatchCallOrCallvirt(out _),
                i => i.MatchBrfalse(out label)
            );
            c.GotoPrev(MoveType.AfterLabel,
                i => i.MatchLdsfld<ModManager>(nameof(ModManager.MSC))
            );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((SSOracleBehavior beh) =>
            {
                if (beh.oracle.room.game.StoryCharacter == Const.NoirName)
                {
                    UnityEngine.Debug.Log("Refusing giving Noir karma, he's chubby enough as is.");
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            LogSource.LogError(ex);
        }
    }

    public static void SSOracleBehaviorILSeePlayer(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SSOracleBehaviorAction>("MeetGourmand_Init")
                );
            label = il.DefineLabel(c.Next);
            c.GotoPrev(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<OracleBehavior>("oracle")
            );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((SSOracleBehavior beh) =>
            {
                if (beh.player.SlugCatClass == Const.NoirName)
                {
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            LogSource.LogError(ex);
        }
    }
}