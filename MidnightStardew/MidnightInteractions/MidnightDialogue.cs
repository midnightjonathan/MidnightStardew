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
        public string CurrentStatement => Parse(Conversation.Statement[StatementIndex]);
        /// <summary>
        /// Returns true if the statement index is the final statement of the dialogue.
        /// </summary>
        public bool IsFinished => Conversation.Responses == null ? StatementIndex == Conversation.Statement.Count - 1 : StatementIndex == Conversation.Statement.Count - 2;
        /// <summary>
        /// Moves to and returns the next statement
        /// </summary>
        public string NextStatement
        {
            get
            {
                return Parse(Conversation.Statement[++StatementIndex]);
            }
        }
        /// <summary>
        /// The index of the current statement.
        /// </summary>
        public int StatementIndex { get; set; } = 0;

        public MidnightDialogue(StardewValley.NPC speaker, MidnightConversation currentConversation, bool isQuestion = false) 
            : base(speaker, default, currentConversation.Statement[isQuestion ? currentConversation.Statement.Count - 1 : 0])
        {
            Conversation = currentConversation;
            showPortrait = true;
            if (isQuestion)
            {
                StatementIndex = currentConversation.Statement.Count - 1;
            }
        }

        protected virtual string Parse(string statement)
        {
            var parsedStatment = statement.Replace("[Farmer]", MidnightFarmer.LocalFarmer.Name);

            // Set emotion
            int emoteIndex = parsedStatment.IndexOf("[Image ");
            if (emoteIndex != -1)
            {
                int startEmote = emoteIndex + 7;
                int endEmote = parsedStatment.IndexOf("]", startEmote);
                var emote = parsedStatment.Substring(startEmote, endEmote - startEmote);
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
