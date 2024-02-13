using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MidnightStardew.MidnightCharacters;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightInteractions
{
    public class MidnightDialogueBox : DialogueBox
    {
        /// <summary>
        /// The Stardew NPC that should be displayed.
        /// </summary>
        public MidnightNpc Speaker { get; set; }

        /// <summary>
        /// Tracks if a dialogue has been set.
        /// </summary>
        private bool hasSetDialogueIcon = false;
        /// <summary>
        /// The dialogue icon to be used, resets the underlying DialogueBox dialogueIcon every update.
        /// No it's not clean - but it works.
        /// </summary>
        public TemporaryAnimatedSprite? DialogueIcon { get; set; }

        /// <summary>
        /// The conversation that the NPC is having with the player.
        /// </summary>
        public MidnightConversation Conversation { get; set; }
        /// <summary>
        /// The current dialogue within the conversation that the player is having.
        /// </summary>
        public MidnightDialogue CharacterDialogue
        {
            get => (MidnightDialogue)characterDialogue;
            set
            {
                dialogues.Clear();
                characterDialoguesBrokenUp.Clear();
                characterDialogue = value;
            }
                
        }
           
        /// <summary>
        /// Creates a dialogue box object that can be used to display dialogue to the player.
        /// </summary>
        /// <param name="speaker">The NPC that the player is speaking to.</param>
        /// <param name="conversation">The conversation that is being had.</param>
        public MidnightDialogueBox(MidnightNpc speaker, MidnightConversation conversation) : base(100, 100, 100, 100)
        {
            Speaker = speaker;
            Conversation = conversation;
        }

        /// <summary>
        /// Set up the dialog box.
        /// </summary>
        protected void SetupForDisplay()
        {
            if (Game1.options.SnappyMenus)
            {
                Game1.mouseCursorTransparency = 0f;
            }

            width = 1200;
            height = 384;
            x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
            y = Game1.uiViewport.Height - height - 64;

            friendshipJewel = new Rectangle(x + width - 64, y + 256, 44, 44); // Not sure if this is needed

            setUpForGamePadMode();
        }

        /// <summary>
        /// Check if the conversation has a question that needs to be presented to the player.
        /// </summary>
        public void CheckDisplay()
        {
            if (Conversation.Responses != null && Conversation.Responses.Count != 0)
            {
                DisplayQuestion();
            }
        }

        /// <summary>
        /// Displays the current conversation.
        /// </summary>
        public void Display()
        {
            hasSetDialogueIcon = false;

            if (Conversation.Statement == null)
            {
                Conversation.ApplyEffects(MidnightFarmer.LocalFarmer);
            }
            else if (Conversation.Responses == null || Conversation.Statement.Count > 1)
            {
                DisplayStatement();
            }
            else
            {
                DisplayQuestion();
            }
        }

        /// <summary>
        /// Display a statement to the player.
        /// </summary>
        private void DisplayStatement()
        {
            isQuestion = false;

            CharacterDialogue = new MidnightDialogue(Speaker, Conversation);

            SetupForDisplay();
            newPortaitShakeTimer = ((characterDialogue.getPortraitIndex() == 1) ? 250 : 0); // Shakes the image of the NPC

            Game1.activeClickableMenu?.emergencyShutDown();
            Game1.afterDialogues = CheckDisplay;
            Game1.activeClickableMenu = this;
            Game1.player.CanMove = false;
            Game1.dialogueUp = true;
        }
        
        /// <summary>
        /// Display a question to the player.
        /// </summary>
        private void DisplayQuestion()
        {
            isQuestion = true;
            
            CharacterDialogue = new MidnightDialogue(Speaker, Conversation, true);

            var availableOptions = GetOptions();

            responses = new Response[availableOptions.Count];
            
            int i = 0;
            foreach ((string option, MidnightConversation nextDialogue) in availableOptions)
            {
                responses[i++] = new Response(nextDialogue.Key, option);
            }

            SetupForDisplay();
            SetQuestionHeight();

            Game1.activeClickableMenu?.emergencyShutDown();
            Game1.afterDialogues = CheckDisplay;
            Game1.activeClickableMenu = this;
            Game1.player.CanMove = false;
            Game1.dialogueUp = true;
        }

        /// <summary>
        /// Gets the options that are available to the player.
        /// </summary>
        /// <returns>Set of conversations with their response keys.</returns>
        private Dictionary<string, MidnightConversation> GetOptions()
        {
            Dictionary<string, MidnightConversation> available = new();

            foreach (var conversation in Conversation.Responses)
            {
                if (conversation.Value.MeetsRequirements())
                {
                    available[conversation.Key] = conversation.Value;
                }
            }

            return available;
        }

        /// <summary>
        /// Determine how tall the question dialogue needs to be.
        /// </summary>
        private void SetQuestionHeight()
        {
            int tempWidth = base.width - 16;
            this.heightForQuestions = SpriteText.getHeightOfString(this.getCurrentString(), tempWidth);
            Response[] array = this.responses;
            foreach (Response r in array)
            {
                this.heightForQuestions += SpriteText.getHeightOfString(r.responseText, tempWidth) + 16;
            }
            this.heightForQuestions += 40;
        }

        /// <summary>
        /// Handles a left click when a statement is displayed.
        /// </summary>
        public void HandleStatementClick()
        {
            if (transitioning || safetyTimer > 0) return; // Ignore clicks happens too quickly.
            if (CharacterDialogue.AreStatementsFinished && !isQuestion)
            {
                closeDialogue();
                Conversation.ApplyEffects(MidnightFarmer.LocalFarmer);
                return;
            }

            characterDialoguesBrokenUp.Clear();
            characterDialoguesBrokenUp.Push(CharacterDialogue.GetNextStatement());
            safetyTimer = 750;
        }

        /// <summary>
        /// Sets up the close dialogue icon.
        /// </summary>
        private void SetUpCloseDialogueIcon()
        {
            Vector2 iconPosition = new Vector2(this.x + base.width - 40, this.y + base.height - 44);
            if (isPortraitBox())
            {
                iconPosition.X -= 492f;
            }
            DialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(289, 342, 11, 12), 80f, 11, 999999, iconPosition, false, false, 0.89f, 0f, Color.White, 4f, 0f, 0f, 0f, true);
            dialogueIcon = DialogueIcon;
        }

        /// <summary>
        /// Sets up the next page icon.
        /// </summary>
        private void SetUpNextPageIcon()
        {
            Vector2 iconPosition = new Vector2(this.x + base.width - 40, this.y + base.height - 40);
            if (isPortraitBox())
            {
                iconPosition.X -= 492f;
            }
            DialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(232, 346, 9, 9), 90f, 6, 999999, iconPosition, false, false, 0.89f, 0f, Color.White, 4f, 0f, 0f, 0f, true)
            {
                yPeriodic = true,
                yPeriodicLoopTime = 1500f,
                yPeriodicRange = 8f
            };
            dialogueIcon = DialogueIcon;
        }

        /// <summary>
        /// Handles a left click when a question is displayed.
        /// </summary>
        public void HandleQuestionClick()
        {
            if (selectedResponse < 0) return;

            Conversation = Conversation.Responses[responses[selectedResponse].responseText];
            closeDialogue();
            Display();
        }

        /// <summary>
        /// Handles a left click.
        /// </summary>
        /// <param name="x">x location clicked.</param>
        /// <param name="y">y location clicked.</param>
        /// <param name="playSound">whether or not to play sounds.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (isQuestion) 
            { 
                HandleQuestionClick();
            }
            else
            {
                HandleStatementClick();
            }

        }

        /// <summary>
        /// Updates the Dialogue Box.
        /// </summary>
        /// <param name="time">Time since last tick.</param>
        public override void update(GameTime time)
        {
            base.update(time);
            if (!hasSetDialogueIcon)
            {
                if (CharacterDialogue.IsFinished) SetUpCloseDialogueIcon(); else SetUpNextPageIcon();
                hasSetDialogueIcon = true;
            }
            dialogueIcon = DialogueIcon;
        }
    }
}
