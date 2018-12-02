using GDLibrary;
using JigLibX.Collision;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDApp
{
    public class IcicleZone : ZoneObject
    {
        public IcicleZone(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters, Model model) : base(id, actorType, transform, effectParameters, model)
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
                EventDispatcher.Publish(new EventData(EventActionType.OnIcicleZone, EventCategoryType.Obstacle));
                HeroPlayerObject hero = collidee as HeroPlayerObject;
                //hero.MovementSpeed = 0.3f;
            }

        }

    }
}
