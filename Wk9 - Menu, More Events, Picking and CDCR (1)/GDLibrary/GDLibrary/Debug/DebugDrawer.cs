﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class DebugDrawer : PausableDrawableGameComponent
    {
        #region Fields
        private SpriteBatch spriteBatch;
        private CameraManager cameraManager;
        private SpriteFont spriteFont;
        private Vector2 position;
        private Color color;
        private int fpsRate;
        private int totalTime, count;
        private string strInfo = "Drive - Numpad[8,5,4,6,1,3], O/P - pause/play controller on torus";
        private Vector2 positionOffset = new Vector2(0, 20);
        private bool onIce;
        #endregion

        public DebugDrawer(Game game, CameraManager cameraManager, 
            EventDispatcher eventDispatcher, StatusType statusType,
            SpriteBatch spriteBatch, SpriteFont spriteFont, Vector2 position, Color color) 
            : base(game, eventDispatcher, statusType)
        {
            this.spriteBatch = spriteBatch;
            this.cameraManager = cameraManager;
            this.spriteFont = spriteFont;
            this.position = position;
            this.color = color;
            this.onIce = false;
            eventDispatcher.ObstacleCollision += EventDispatcher_ObstacleCollision;
        }

        private void EventDispatcher_ObstacleCollision(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnIce)
            {
                this.onIce = (bool)eventData.AdditionalParameters[0];
            }
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            this.totalTime += gameTime.ElapsedGameTime.Milliseconds;
            this.count++;

            if(this.totalTime >= 1000) //1 second
            {
                this.fpsRate = count;
                this.totalTime = 0;
                this.count = 0;
            }

            base.ApplyUpdate(gameTime);
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            //draw the debug info for all of the cameras in the cameramanager
            foreach (Camera3D activeCamera in this.cameraManager)
            {
                //set the viewport dimensions to the size defined by the active camera
                Game.GraphicsDevice.Viewport = activeCamera.Viewport;
                this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                //frame rate
                this.spriteBatch.DrawString(this.spriteFont, "FPS: " + this.fpsRate, this.position, this.color);
                //camera info
                this.spriteBatch.DrawString(this.spriteFont, activeCamera.GetDescription(), this.position + this.positionOffset, this.color);
                //str info
                this.spriteBatch.DrawString(this.spriteFont, this.strInfo, this.position + 2 * this.positionOffset, this.color);
                this.spriteBatch.DrawString(this.spriteFont, "On Ice: " + this.onIce, this.position + 3 * this.positionOffset, this.color);
                this.spriteBatch.End();
            }

            base.ApplyDraw(gameTime);
        }
    }
}
