﻿using MidnightStardew.MidnightCharacters;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightInteractions
{
    public class MidnightDialogue : StardewValley.Dialogue
    {
        public readonly static Dictionary<string, string> StandardImageMap = new()
        {
            { "neutral", "0" },
            { "happy", "1" },
            { "sad", "2" },
            { "unique", "3" },
            { "love", "4" },
            { "angry", "5" }
        };

        /// <summary>
        /// The conversation for this dialogue.
        /// </summary>
        public MidnightConversation Conversation { get; set; }
        /// <summary>
        /// The current statement being displayed.
        /// </summary>
        public string CurrentStatement
        {
            get
            {
                return Parse(Conversation.Statement[StatementIndex]);
            }
        }
        /// <summary>
        /// Returns true if the statement index is the final statement of the dialogue..
        /// </summary>
        public bool AreStatementsFinished => statementIndex == LastStatementIndex;
        /// <summary>
        /// Returns true if the statement index is the final interaction with the player.
        /// </summary>
        public bool IsFinished => statementIndex == LastIndex;
        /// <summary>
        /// Returns the last statement index of the conversation.
        /// </summary>
        public int LastStatementIndex => Conversation.Responses == null ? LastIndex : LastIndex - 1;
        /// <summary>
        /// The last index of the conversation, regardless of it being a question or not.
        /// </summary>
        public int LastIndex => Conversation.Statement.Count - 1;
        public MidnightNpc Speaker { get; set; }
        private int statementIndex = 0;
        /// <summary>
        /// The index of the current statement.
        /// </summary>
        public int StatementIndex 
        {
            get => statementIndex;
            set => statementIndex = value;
        }

        public MidnightDialogue(MidnightNpc speaker, MidnightConversation currentConversation, bool isQuestion = false) 
            : base(speaker.StardewNpc, default, "")
        {
            Conversation = currentConversation;
            Speaker = speaker;

            if (currentConversation.Statement == null) return;

            parseDialogueString(currentConversation.Statement[isQuestion ? currentConversation.Statement.Count - 1 : 0]);
            
            showPortrait = true;
            if (isQuestion)
            {
                StatementIndex = currentConversation.Statement.Count - 1;
            }
        }

        /// <summary>
        /// Moves to and returns the next statement
        /// </summary>
        public string GetNextStatement()
        {
            return Parse(Conversation.Statement[++StatementIndex]);
        }

        /// <summary>
        /// Handles custom statement parsing.
        /// </summary>
        /// <param name="statement">Statement to parse</param>
        /// <returns>The statement with statement commands removed.</returns>
        protected virtual string Parse(string statement)
        {
            var parsedStatment = statement.Replace("[Farmer]", MidnightFarmer.LocalFarmer.Name);

            // Set speaker
            int speakerIndex = parsedStatment.ToLower().IndexOf("[speaker ");
            if (speakerIndex != -1)
            {
                int startSpeaker = speakerIndex + 9;
                int endSpeaker = parsedStatment.IndexOf("]", startSpeaker);
                var speaker = parsedStatment.Substring(startSpeaker, endSpeaker - startSpeaker);
                StandardImageMap.TryGetValue(speaker, out string? outSpeaker);
                if ("none" == (outSpeaker ?? speaker).ToLower())
                {
                    showPortrait = false;
                    base.speaker = null;
                } else if ("reset" == (outSpeaker ?? speaker).ToLower())
                {
                    base.speaker = Speaker;
                    showPortrait = true;
                }
                else
                {
                    base.speaker = MidnightNpc.Get[outSpeaker ?? speaker];
                    showPortrait = true;
                }
                parsedStatment = parsedStatment.Remove(speakerIndex, endSpeaker - speakerIndex + 1);
            }

            // Set emotion
            int emoteIndex = parsedStatment.ToLower().IndexOf("[image ");
            if (emoteIndex != -1)
            {
                int startEmote = emoteIndex + 7;
                int endEmote = parsedStatment.IndexOf("]", startEmote);
                var emote = parsedStatment.Substring(startEmote, endEmote - startEmote).ToLower();
                StandardImageMap.TryGetValue(emote, out string? outEmote);
                CurrentEmotion = $"${outEmote ?? emote}";
                parsedStatment = parsedStatment.Remove(emoteIndex, endEmote - emoteIndex + 1);
            }

            return parsedStatment;
        }

        protected override void parseDialogueString(string statement, string translationKey = "")
        {
            dialogues.Clear();
            dialogues.Add(Parse(statement));
        }
    }
}
