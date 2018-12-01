/*
Function: 		Represents a collidable zone within the game - subclass to make TriggerCameraChangeZone, WinLoseZone etc 
Author: 		NMCG
Version:		1.0
Date Updated:	
Bugs:			None
Fixes:			None
*/
using JigLibX.Collision;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class ZoneObject : CollidableObject
    {
        #region Fields
        #endregion

        #region Properties
        #endregion

        public ZoneObject(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters,
            Model model)
            : base(id, actorType, transform, effectParameters, model)
        {
            //register for callback on CDCR
            this.Body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        #region Event Handling
        protected virtual bool CollisionSkin_callbackFn(CollisionSkin collider, CollisionSkin collidee)
        {
            HandleCollisions(collider.Owner.ExternalData as CollidableObject, collidee.Owner.ExternalData as CollidableObject);

            //can walk through this object BUT it will still detect a collision
            return false;
        }

        //how do we want this object to respond to collisions?
        protected virtual void HandleCollisions(CollidableObject collider, CollidableObject collidee)
        {
            //if player hits me then remove me

        }
        #endregion
    }
}
