using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GDLibrary;
using Microsoft.Xna.Framework.Graphics;

namespace GDApp
{
    public class IcicleObject : CollidableObject
    {
        private bool deadly;

        public IcicleObject(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters, Model model) : base(id, actorType, transform, effectParameters, model)
        {
            this.deadly = true;
            this.Body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        private bool CollisionSkin_callbackFn(JigLibX.Collision.CollisionSkin collider, JigLibX.Collision.CollisionSkin collidee)
        {
            HandleCollisions(collider.Owner.ExternalData as CollidableObject, collidee.Owner.ExternalData as CollidableObject);

            return true;
        }

        protected virtual void HandleCollisions(CollidableObject collider, CollidableObject collidee)
        {
            if (collidee.ActorType == ActorType.Player)
            {
                if (this.deadly)
                {
                    EventDispatcher.Publish(new EventData("You Got Spiked!", null, EventActionType.OnLose, EventCategoryType.GameLost));
                }
            }

            else if (collidee.ActorType == ActorType.CollidableGround)
            {
                this.deadly = false;
                collider.Body.Immovable = true;
            }

        }

        public new object Clone()
        {
            return new IcicleObject("clone - " + ID, //deep
                this.ActorType,   //deep
                (Transform3D)this.Transform.Clone(),  //deep
                this.EffectParameters.GetDeepCopy(), //hybrid - shallow (texture and effect) and deep (all other fields) 
                this.Model); //shallow i.e. a reference
        }
    }
}
