using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarsLander {
  public class PinkyLander {
    public static void UpdateTriggeredEventHandler_print(object sender, UpdateTriggeredEventArgs args) {
      Console.WriteLine("Height: " + args.height + " Y-Velocity: " + args.yVelocity +
                        " Position: " + args.xPosition + " X-Velocity: " + args.xVelocity + " Fuel: " + args.fuel);
    }

    public static NeuralLander localSearch() {
      NeuralLander champ = new NeuralLander();
      NeuralLander chump;
      double champScore = champ.simulate(false);
      double chumpScore;
      
      while (champScore > 0.0) {
        chump = new NeuralLander(champ);
        chump.mutate();
        chumpScore = chump.simulate(false);
        Console.WriteLine("champScore: " + champScore + ", chumpScore: " + chumpScore);

        if (chumpScore < champScore) {
          champ = chump;
          champScore = chumpScore;
        }
      }

      return champ;
    }

    [STAThread]
    static void Main(string[] args) {
      NeuralLander lander = localSearch();
      /*lander.testIt();
      Console.ReadKey();
      */
      for (int i = 0; i < 1000; i++) {
        lander.mutate();
      }

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
