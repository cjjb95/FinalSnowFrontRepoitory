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
        private bool slipping;
        private bool once;

        public SlipController(string id, ControllerType controllerType,
            PlayStatusType playStatusType, EventDispatcher eventDispatcher)
            : base(id, controllerType, playStatusType)
        {
            this.rnd = new Random();
            this.slipping = false;
            this.once = true;
            eventDispatcher.ObstacleCollision += EventDispatcher_ObstacleCollision;
        }

        private void EventDispatcher_ObstacleCollision(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnIce)
            {

                this.PlayStatusType = PlayStatusType.Play;
            }
            else if (eventData.EventType == EventActionType.OnGround)
            {
                EventDispatcher.Publish(new EventData(EventActionType.SlipOver, EventCategoryType.ObstacleEvent));
                this.slipChance = 0;
                this.slipping = false;
                this.once = true;
                this.PlayStatusType = PlayStatusType.Stop;

            }
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            PlayerObject parentActor = actor as PlayerObject;
            this.totalTimeOnIce += gameTime.ElapsedGameTime.Milliseconds;


            if (this.totalTimeOnIce % 500 == 0 && !this.slipping)
            {
                //Console.WriteLine("Calculating...." + totalTimeOnIce);
                this.slipChance++;
                this.randomNum = this.rnd.Next(1, 6);
                //Console.WriteLine("slip "+slipChance);
                //Console.WriteLine(randomNum);

            }

            if (this.randomNum < this.slipChance)
            {
                if (this.once)
                {
                    object[] additionalParameters = { "Ouch" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
                    this.once = false;
                }
                this.slipping = true;
                //set the driveable controller on the player to be paused
                //EventDispatcher.Publish(new EventData(EventActionType.OnSlip, EventCategoryType.ObstacleEvent));
                //reset slipChance
                this.totalTimeSlipping += gameTime.ElapsedGameTime.Milliseconds;
                //publish event that the player is slipping now
                EventDispatcher.Publish(new EventData(EventActionType.OnSlip, EventCategoryType.ObstacleEvent));
                //start counting timer on slipping

                //Console.WriteLine("Slipping...." + totalTimeSlipping);

                if (this.totalTimeSlipping % 4000 == 0)
                {
                    //Console.WriteLine("slip over");
                    //set the driveable controller on the player to be played again after 4 sec
                    EventDispatcher.Publish(new EventData(EventActionType.SlipOver, EventCategoryType.ObstacleEvent));
                    this.slipChance = 0;
                    this.slipping = false;
                    this.once = true;
                }

            }

            base.Update(gameTime, actor);
        }
    }
}
