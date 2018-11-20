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


        public ThermoController(string id, ControllerType controllerType, PlayStatusType playStatusType) : base(id, controllerType, playStatusType)
        {
            this.isDead = false;
            this.temperature = 30f;
            this.dropRate = 1f;
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
                    System.Console.WriteLine(this.temperature);
                    parentActor.Transform.Translation += new Vector2(0, (this.dropRate * 8));
                    parentActor.SourceRectangle =
                        new Rectangle(parentActor.SourceRectangle.X,
                        parentActor.SourceRectangle.Y + (int)(this.dropRate * 10),
                        parentActor.SourceRectangle.Width,
                        parentActor.SourceRectangle.Height - (int)(this.dropRate * 10));// TO DO CALCULATION - MATCH THE BAR WITH TEMP - must be double
                                                                                        // parentActor.SourceRectangleHeight -= (int)(this.dropRate * 20);
                }
            }

            base.Update(gameTime, actor);
        }


    }
}
