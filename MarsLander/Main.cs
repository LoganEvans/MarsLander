using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarsLander {
  public class Main {
    public static void UpdateTriggeredEventHandler_print(object sender, UpdateTriggeredEventArgs args) {
      Console.WriteLine("Height: " + args.height + " Y-Velocity: " + args.yVelocity +
                        " Position: " + args.xPosition + " X-Velocity: " + args.xVelocity + " Fuel: " + args.fuel);
    }

    [STAThread]
    static void Main(string[] args) {
      LanderBase lander = new LanderBase();
      Display display = new Display();
      lander.UpdateTriggered += UpdateTriggeredEventHandler_print;
      lander.UpdateTriggered += display.UpdateTriggeredEventHandler_paint;

      Thread displayThread = new Thread((ThreadStart)delegate {
            Application.EnableVisualStyles();
            System.Windows.Forms.Application.Run(display);
        });
      displayThread.Start();
      lander.simulate(display : true);
    }
  }
}
