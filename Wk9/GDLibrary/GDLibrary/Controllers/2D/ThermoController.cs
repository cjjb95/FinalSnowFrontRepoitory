using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class ThermoController : Controller
    {
        private int totalTime;
        private int count;
        private float temperature;
        private bool isDead;
        private float dropRate;
        private bool bDirty;

        private bool temp1 = true;
        private bool temp2 = true;
        private bool temp3 = true;
        private bool temp4 = true;
        public ThermoController(string id, ControllerType controllerType, PlayStatusType playStatusType, EventDispatcher eventDispatcher) : base(id, controllerType, playStatusType)
        {
            this.isDead = false;
            this.temperature = 30f;
            this.dropRate = 1f;
            this.bDirty = true;
            eventDispatcher.UseItem += EventDispatcher_UseItem;
        }

        private void EventDispatcher_UseItem(EventData eventData)
        {
            //get the type of the item
            if (eventData.AdditionalParameters[0].Equals("coat"))
            {
                if (this.dropRate == 1)
                {
                    this.dropRate = 0.5f;
                }
                else
                {
                    this.dropRate = 1;
                }
            }
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            UITextureObject parentActor = actor as UITextureObject;

            this.totalTime += gameTime.ElapsedGameTime.Milliseconds;
            this.count++;
            if (this.temperature <= 0)
            {
                this.isDead = true;
            }

            if (this.totalTime % 1000 == 0)
            {
                if (!this.isDead)
                {
                    this.temperature -= this.dropRate;

                    parentActor.Transform.Translation += new Vector2(0, (this.dropRate * 8));
                    parentActor.SourceRectangle =
                        new Rectangle(parentActor.SourceRectangle.X,
                        parentActor.SourceRectangle.Y + (int)(this.dropRate * 10),
                        parentActor.SourceRectangle.Width,
                        parentActor.SourceRectangle.Height - (int)(this.dropRate * 10));// TO DO CALCULATION - MATCH THE BAR WITH TEMP - must be double
                    if (temperature <= 20 && temperature > 10 && temp1)
                    {
                        temp1 = false;
                        EventDispatcher.Publish(new EventData("tempBelow20", null, EventActionType.OnFreezeOver, EventCategoryType.LowTemp));
                        System.Console.WriteLine("once");
                    }
                    else if (temperature <= 10 && temperature > 5 && temp2)
                    {
                        EventDispatcher.Publish(new EventData("tempBelow10", null, EventActionType.OnFreezeOver, EventCategoryType.LowTemp));
                        //publish low health event
                        System.Console.WriteLine("twice");
                        temp2 = false;
                        EventDispatcher.Publish(new EventData("critical sound", EventActionType.OnHealthSet, EventCategoryType.LowTemp));
                    }
                    else if (temperature <= 5 && temperature > 1 && temp3)
                    {
                        EventDispatcher.Publish(new EventData("tempBelow5", null, EventActionType.OnFreezeOver, EventCategoryType.LowTemp));
                        temp3 = false;
                        System.Console.WriteLine("thrice");
                    }
                    else if (temperature <= 1 && temp4)
                    {
                        temp4 = false;
                        System.Console.WriteLine("four");
                        EventDispatcher.Publish(new EventData("tempBelow1", null, EventActionType.OnFreezeOver, EventCategoryType.LowTemp));
                    }




                }
                else
                {
                    //publish gameover event
                    if (this.bDirty)
                    {
                        Console.WriteLine("aa");
                        EventDispatcher.Publish(new EventData("Dead By Frosbite!", null, EventActionType.OnLose, EventCategoryType.GameLost));
                        bDirty = false;
                    }
                }
            }

            base.Update(gameTime, actor);

        }

    }
}
