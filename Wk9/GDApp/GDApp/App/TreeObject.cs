using GDLibrary;
using JigLibX.Collision;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDApp
{
    public class TreeObject : ZoneObject
    {
        private bool deadly;
        private bool immoveable;

        public TreeObject(string id, ActorType actorType, Transform3D transform, 
            EffectParameters effectParameters, Model model, EventDispatcher eventDispatcher) : base(id, actorType, transform, effectParameters, model)
        {
            this.immoveable = false;
            this.deadly = true;
            this.Body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
            eventDispatcher.ObstacleEvent += EventDispatcher_ObstacleEvent;

        }

        private void EventDispatcher_ObstacleEvent(EventData eventData)
        {
            if(eventData.EventType == EventActionType.OnTreeTouchGround)
            {
                this.immoveable = true;
            }
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
                    EventDispatcher.Publish(new EventData("TIMBER!!!", null, EventActionType.OnLose, EventCategoryType.GameLost));
                }
            }

            else if (collidee.ActorType == ActorType.CollidableGround)
            {
                
                if(this.immoveable)
                {
                    collider.Body.Immovable = true;
                    this.deadly = false;
                }
               
            }

        }
    }
}
