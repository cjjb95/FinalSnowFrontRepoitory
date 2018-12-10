using GDLibrary;
using JigLibX.Collision;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDApp
{
    public class TreeWarning : ZoneObject
    {
        bool once = true;
        public TreeWarning(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters, Model model) : base(id, actorType, transform, effectParameters, model)
        {

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
                    object[] stopParameters = { "IceWarning", 0 };
                    EventDispatcher.Publish(new EventData(EventActionType.OnStop, EventCategoryType.Sound2D, stopParameters));
                    object[] additionalParameters = { "TreeWarning" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
                    once = false;
                }
            }

        }

    }
}
