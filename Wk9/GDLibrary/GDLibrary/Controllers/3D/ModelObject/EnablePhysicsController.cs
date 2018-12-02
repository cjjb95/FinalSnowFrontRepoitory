using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class EnablePhysicsController : Controller
    {


        public EnablePhysicsController(string id, ControllerType controllerType,
            PlayStatusType playStatusType)
            : base(id, controllerType, playStatusType)
        {

        }



        public override void Update(GameTime gameTime, IActor actor)
        {
            CollidableObject parentActor = actor as CollidableObject;

            parentActor.Body.Immovable = false;




        }
    }
}
