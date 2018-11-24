using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class SlipController : Controller
    {
        private int totalTimeOnIce;
        private int slipChance;
        private Random rnd;
        private int randomNum;
        private int totalTimeSlipping;

        public SlipController(string id, ControllerType controllerType, PlayStatusType playStatusType) : base(id, controllerType, playStatusType)
        {
            this.rnd = new Random();
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            PlayerObject parentActor = actor as PlayerObject;
            this.totalTimeOnIce += gameTime.ElapsedGameTime.Milliseconds;


            if (this.totalTimeOnIce % 1000 == 0)
            {
                this.slipChance++;
                this.randomNum = this.rnd.Next(3, 8);
                if (this.randomNum < this.slipChance)
                {
                    //set the driveable controller on the player to be paused
                    parentActor.SetControllerPlayStatus(PlayStatusType.Pause,
                        controller => controller.GetControllerType() == ControllerType.Drive);
                    //reset slipChance
                    this.slipChance = 0;
                    //publish event that the player is slipping now
                    EventDispatcher.Publish(new EventData(EventActionType.OnSlip, EventCategoryType.Obstacle));
                    //start counting timer on slipping
                    this.totalTimeSlipping += gameTime.ElapsedGameTime.Milliseconds;

                    if (this.totalTimeSlipping % 4000 == 0)
                    {
                        //set the driveable controller on the player to be played again after 4 sec
                        parentActor.SetControllerPlayStatus(PlayStatusType.Play,
                        controller => controller.GetControllerType() == ControllerType.Drive);
                    }

                }
            }

            base.Update(gameTime, actor);
        }
    }
}
