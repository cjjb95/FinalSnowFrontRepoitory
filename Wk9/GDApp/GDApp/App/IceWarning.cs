using GDLibrary;
using JigLibX.Collision;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDApp
{
    public class IceWarning : ZoneObject
    {
        private bool once = true;
        private SoundManager soundManager;
        public IceWarning(string id, ActorType actorType,
            Transform3D transform, EffectParameters effectParameters, 
            Model model, SoundManager soundManager) : base(id, actorType, transform, effectParameters, model)
        {
            this.soundManager = soundManager;
        }

        protected override bool CollisionSkin_callbackFn(CollisionSkin collider, CollisionSkin collidee)
        {

            return base.CollisionSkin_callbackFn(collider, collidee);
        }

        protected override void HandleCollisions(CollidableObject collider, CollidableObject collidee)
        {


            if (collidee.ActorType == ActorType.Player)
            {
                if (once == true)
                {
                    object[] stopParameters = { "SnowDriftWarning", 0 };
                    EventDispatcher.Publish(new EventData(EventActionType.OnStop, EventCategoryType.Sound2D, stopParameters));
                    object[] playParameters = { "IceWarning" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, playParameters));
                    once = false;
                }
            }

        }

    }
}
