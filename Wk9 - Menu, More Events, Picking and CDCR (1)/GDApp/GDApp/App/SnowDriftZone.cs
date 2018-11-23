using GDLibrary;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDApp
{
    class SnowDriftZone : CollidableObject
    {
        public SnowDriftZone(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters, Model model) : base(id, actorType, transform, effectParameters, model)
        {
            this.Body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        private bool CollisionSkin_callbackFn(JigLibX.Collision.CollisionSkin skin0, JigLibX.Collision.CollisionSkin skin1)
        {
            return false;
        }
    }
}
