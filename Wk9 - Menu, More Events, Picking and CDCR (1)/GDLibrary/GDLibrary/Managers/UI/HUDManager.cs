using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class HUDManager : PausableDrawableGameComponent
    {
        private List<DrawnActor2D> hudList;
        private SpriteBatch spriteBatch;
        public HUDManager(Game game, EventDispatcher eventDispatcher,
            StatusType statusType, SpriteBatch spriteBatch) : base(game, eventDispatcher, statusType)
        {
            this.hudList = new List<DrawnActor2D>();
            this.spriteBatch = spriteBatch;
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            if (this.hudList != null)
            {
                //update all the updateable menu items (e.g. make buttons pulse etc)
                foreach (DrawnActor2D actor2D in this.hudList)
                {
                    if ((actor2D.GetStatusType() & StatusType.Update) != 0) //if update flag is set
                        actor2D.Update(gameTime);
                }
            }
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            if (this.hudList != null)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                foreach (DrawnActor2D actor2D in this.hudList)
                {
                    if ((actor2D.GetStatusType() & StatusType.Drawn) != 0) //if drawn flag is set
                    {
                        actor2D.Draw(gameTime, spriteBatch);

                    }
                }
                spriteBatch.End();
            }
        }

        public void Add(DrawnActor2D actor)
        {
            if (actor != null)
            {
                this.hudList.Add(actor);
            }
        }
    }
}
