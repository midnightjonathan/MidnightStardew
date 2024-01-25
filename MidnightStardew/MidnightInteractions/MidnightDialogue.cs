using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightInteractions
{
    public class MidnightDialogue : StardewValley.Dialogue
    {
        /// <summary>
        /// The conversation for this dialogue.
        /// </summary>
        public MidnightConversation Conversation { get; set; }
        /// <summary>
        /// The current statement being displayed.
        /// </summary>
        public string CurrentStatement => Conversation.Statement[StatementIndex];
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
                return Conversation.Statement[++StatementIndex];
            }
        }
        /// <summary>
        /// The index of the current statement.
        /// </summary>
        public int StatementIndex { get; set; } = 0;

        public MidnightDialogue(StardewValley.NPC speaker, MidnightConversation currentConversation, bool isQuestion = false) 
            : base(speaker, default, currentConversation.Statement[isQuestion ? currentConversation.Statement.Count - 1: 0])
        {
            Conversation = currentConversation;
            showPortrait = true;
            if (isQuestion)
            {
                StatementIndex = currentConversation.Statement.Count - 1;
            }
        }


    }
}
