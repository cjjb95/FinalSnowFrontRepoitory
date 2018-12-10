using System;
using Microsoft.Xna.Framework;
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
        private bool onSnowDrift;
        private bool slip;
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
            this.onSnowDrift = false;
            this.slip = false;
            eventDispatcher.ObstacleCollision += EventDispatcher_ObstacleCollision;
            eventDispatcher.SnowDriftIntersected += EventDispatcher_SnowDriftIntersection;
            eventDispatcher.ObstacleEvent += EventDispatcher_ObstacleEvent;
            eventDispatcher.DebugChanged += EventDispatcher_DebugChanged;
        }


        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            
            if (eventData.EventType == EventActionType.OnStart)
                this.StatusType = StatusType.Off;
            else if (eventData.EventType == EventActionType.OnPause)
                this.StatusType = StatusType.Off;
        
    }
        private void EventDispatcher_ObstacleEvent(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnSlip)
            {
                this.slip = true;
            }
            else if (eventData.EventType == EventActionType.SlipOver)
            {
                this.slip = false;
            }
        }

        private void EventDispatcher_DebugChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnToggle)
            {
                if (this.StatusType == StatusType.Off)
                    this.StatusType = StatusType.Drawn | StatusType.Update;
                else
                    this.StatusType = StatusType.Off;
            }
        }

        private void EventDispatcher_SnowDriftIntersection(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnSnowDrift)
            {
                this.onSnowDrift = (bool)eventData.AdditionalParameters[0];
            }
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

            if (this.totalTime >= 1000) //1 second
            {
                this.fpsRate = count;
                this.totalTime = 0;
                this.count = 0;
            }
            if (this.totalTime >= 3000 && this.onSnowDrift) //1 second
            {
                this.onSnowDrift = false;
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

                //Inside snow drift

                this.spriteBatch.DrawString(this.spriteFont, "INSIDE SNOW: " + this.onSnowDrift, this.position + this.positionOffset, this.color);

                //str info

                this.spriteBatch.DrawString(this.spriteFont, this.strInfo, this.position + 2 * this.positionOffset, this.color);
                this.spriteBatch.DrawString(this.spriteFont, "On Ice: " + this.onIce, this.position + 3 * this.positionOffset, this.color);
                this.spriteBatch.DrawString(this.spriteFont, "Slip status: " + this.slip, this.position + 4 * this.positionOffset, this.color);
                this.spriteBatch.End();
            }

            base.ApplyDraw(gameTime);
        }
    }
}
