using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// NameTagDecorator - Enhances player name display with additional styling and information
    /// This decorator already exists in the base Player class but separated for demonstration
    /// </summary>
    public class NameTagDecorator : PlayerDecorator
    {
        private readonly bool showEnhancements;
        private readonly Color nameTagColor;

        public NameTagDecorator(IPlayer player, bool showEnhancements = true, Color? customColor = null) 
            : base(player)
        {
            this.showEnhancements = showEnhancements;
            this.nameTagColor = customColor ?? Color.White;
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw the base player first
            wrappedPlayer.Draw(spriteBatch);

            // Draw enhanced name tag
            if (font != null && !string.IsNullOrEmpty(PlayerName))
            {
                DrawEnhancedNameTag(spriteBatch, font);
            }
        }

        private void DrawEnhancedNameTag(SpriteBatch spriteBatch, SpriteFont font)
        {
            string displayName = PlayerName;
            
            // Add enhancement indicators if enabled
            if (showEnhancements)
            {
                if (GetHasShield())
                {
                    displayName += " [SHIELD]";
                }
                
                float fireRate = GetFireRateMultiplier();
                if (fireRate > 1.0f)
                {
                    displayName += $" [RAPID:{fireRate:F1}x]";
                }
            }

            Vector2 nameSize = font.MeasureString(displayName);
            Vector2 namePosition = new Vector2(
                Position.X - nameSize.X / 2,
                Position.Y - Height / 2 - nameSize.Y - 5
            );

            // Draw background box for better readability
            DrawNameTagBackground(spriteBatch, namePosition, nameSize);
            
            // Draw name with player color or custom color
            Color textColor = nameTagColor == Color.White ? 
                new Color(Colour.R, Colour.G, Colour.B) : nameTagColor;
                
            spriteBatch.DrawString(font, displayName, namePosition, textColor);
        }

        private void DrawNameTagBackground(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            // Create a semi-transparent background rectangle for the name tag
            // In a real implementation, you would need a 1x1 white pixel texture
            // This is conceptual code showing the enhancement
            
            Rectangle backgroundRect = new Rectangle(
                (int)(position.X - 2),
                (int)(position.Y - 1),
                (int)(size.X + 4),
                (int)(size.Y + 2)
            );
            
            // Would draw: spriteBatch.Draw(whitePixelTexture, backgroundRect, Color.Black * 0.5f);
            // For now this is just conceptual to show the decorator pattern
        }
    }
}